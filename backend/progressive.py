import time
import timeit
import sys
import json
import numpy as np
import pandas as pd
import threading
import math
import multiprocessing
import uuid
import traceback

from BaseHTTPServer import BaseHTTPRequestHandler
from BaseHTTPServer import HTTPServer
from multiprocessing import Manager
from SocketServer import ThreadingMixIn
from dataprovider import SequentialDataProvider
from databinner import DataBinner
from optparse import OptionParser

from sklearn.metrics import roc_curve
from sklearn.metrics import confusion_matrix
from sklearn.utils import compute_class_weight
from sklearn.metrics import auc
import sklearn.metrics 
from sklearn.metrics import classification_report
from sklearn.metrics import roc_curve
from sklearn.linear_model import SGDClassifier
from sklearn.linear_model import PassiveAggressiveClassifier
from sklearn.linear_model import Perceptron
from sklearn.naive_bayes import MultinomialNB

import logging
import os.path
import uuid

import signal

#https://github.com/dpallot/simple-websocket-server/tree/master/SimpleWebSocketServer

def default(o):
    if isinstance(o, np.integer): return int(o)
    raise TypeError
    
class Singleton(type):
    _instances = {}
    def __call__(cls, *args, **kwargs):
        if cls not in cls._instances:
            cls._instances[cls] = super(Singleton, cls).__call__(*args, **kwargs)
        return cls._instances[cls]
        
class Shared():
    __metaclass__ = Singleton
    def __init__(self):
        self.manager = multiprocessing.Manager()
        self.computationToUUIDLookup = self.manager.dict()
        self.resultsPerUUID = self.manager.dict()
        self.isRunningPerUUID = self.manager.dict()
        self.modelsPerUUID = self.manager.dict()
        self.dataFolder = ''
        
class Server(ThreadingMixIn, HTTPServer):
    def __init__(self, addr, handler):
        HTTPServer.__init__(self, addr, handler)        

class RequestHandler(BaseHTTPRequestHandler):

    def do_POST(self):
        response = 200
        result = None     
        try:
            content_length = int(self.headers.getheader('content-length'))
            job = json.loads(self.rfile.read(content_length))
            print job
            logging.info('Request : ' + job['type'])
            
            key = json.dumps(job, indent=2, default=default)
            
            if job['type'] == 'execute':            
                if key in Shared().computationToUUIDLookup:
                    print 'existing comp ' + job['task']['type']
                    result = {'uuid' : Shared().computationToUUIDLookup[key]}
                    
                else:   
                    if job['task']['type'] == 'visualization':
                        print 'new comp visualization'
                        newUUID = str(uuid.uuid4())
                        exe = VisualizationExecutor(newUUID, job['task'], job['dataset'], Shared().resultsPerUUID, Shared().isRunningPerUUID)
                        Shared().isRunningPerUUID[newUUID] = True
                        exe.start()  
                        Shared().computationToUUIDLookup[key] = newUUID
                        result = {'uuid' : newUUID}
                        
                    elif job['task']['type'] == 'classify':
                        print 'new comp classify'
                        newUUID = str(uuid.uuid4())
                        exe = ClassificationExecutor(newUUID, job['task'], job['dataset'], Shared().resultsPerUUID, Shared().isRunningPerUUID, Shared().modelsPerUUID)
                        Shared().isRunningPerUUID[newUUID] = True
                        exe.start()  
                        Shared().computationToUUIDLookup[key] = newUUID
                        result = {'uuid' : newUUID}
                        
            elif job['type'] == 'test':
                print 'new comp test'
                reqUuid = job['uuid']
                print reqUuid
                print  pd.DataFrame(job['features'])
                
                if reqUuid in Shared().modelsPerUUID:
                    model = Shared().modelsPerUUID[reqUuid]
                    #print np.array(pd.DataFrame(job['features']))[0]
                    prediction = model.predict(np.array(pd.DataFrame(job['features'])[job['feature_dimensions']]))[0]
                    print np.array(pd.DataFrame(job['features'])[job['feature_dimensions']])
                    print 'test prediction : ', prediction
                    result = json.dumps({'result' : prediction}, default=default)
                else:
                    print ">>> not found"   
                    result = json.dumps({'result' : 0}, default=default)
                    
            elif job['type'] == 'catalog':
                subdirs = [name for name in os.listdir(Shared().dataFolder) if os.path.isdir(os.path.join(Shared().dataFolder, name))]
                schemas = {}
                print subdirs
                for subdir in subdirs:
                    dp = SequentialDataProvider(subdir, Shared().dataFolder, 100, 0)
                    p, df = dp.getDataFrame()
                    attrs = df.columns
                    dtypes = df.dtypes
                    schema = { 'uuid' : -1, 'schema': [(attr,str(dtypes[attr])) for attr in attrs] }
                    schemas[subdir] = schema
                result = json.dumps(schemas, indent=2, default=default)
                
            elif job['type'] == 'lookup':
                if job['uuid'] in Shared().resultsPerUUID:
                    result = json.dumps(Shared().resultsPerUUID[job['uuid']], indent=2, default=default)          
            
            elif job['type'] == 'halt':
                if job['uuid'] in Shared().isRunningPerUUID:
                    Shared().isRunningPerUUID[job['uuid']] = False      
                
            elif job['type'] == 'tasks':
                result = json.dumps([['classify',[
                                          'sgd',
                                          'naive_bayes',
                                          'perceptron',
                                          'passive_aggressive']]
                                    ], default=default)
                                    
        except:
            print traceback.format_exc()
            response = 500
            result = 'malformed request\n'

        self.send_response(response)
        self.send_header('Content-type','application/json')
        self.end_headers()
        self.wfile.write(result)

class Executor(multiprocessing.Process): 

    def __init__(self, uuid):
        multiprocessing.Process.__init__(self)
        self.uuid = uuid    
        self.count = 0
        
    def run(self):
        raise NotImplementedError()
                                

class VisualizationExecutor(Executor): 

    def __init__(self, uuid, task, dataset, resultsPerUUID, isRunningPerUUID):
        Executor.__init__(self, uuid)
        
        self.task = task
        self.dataset = dataset
        self.resultsPerUUID = resultsPerUUID
        self.isRunningPerUUID = isRunningPerUUID
        
        #do first batch fast
        dp = SequentialDataProvider(self.dataset, 'C:\\data', self.task['chunkSize'], 0)
        db = DataBinner(self.task['dimensions'], self.task['dimensionAggregateFunctions'], self.task['nrOfBins'], self.task['aggregateDimensions'], self.task['aggregateFunctions'], self.task['brushes'])
        self.step(dp, db)

    def step(self, dp, db):
        start = time.time()
            
        progress, df = dp.getDataFrame()
        if not self.task['filter'].strip() == '':
            df = df.query(self.task['filter'].strip())
            
        if not df is None: 
            db.bin(df, progress)
            data = {
                'binStructure' : db.binStructure.toJson(), 
                'progress' : progress
            }
            jsonMessage = json.dumps(data, indent=2, default=default)
            self.resultsPerUUID[self.uuid] = data
            
        if df is None or progress >= 1.0:
            self.isRunningPerUUID[self.uuid] = False
            
        end = time.time()
        print 'VisualizationExecutor : p= ' + '{0:.2f}'.format(progress) + ', t= ' + str(end - start)    
        return df, progress
        
    def run(self):
        dp = SequentialDataProvider(self.dataset, 'C:\\data', self.task['chunkSize'], self.task['chunkSize'])
        db = DataBinner(self.task['dimensions'], self.task['dimensionAggregateFunctions'], self.task['nrOfBins'], self.task['aggregateDimensions'], self.task['aggregateFunctions'], self.task['brushes'])

        while True:
            if not self.isRunningPerUUID[self.uuid]:
                break
            time.sleep(0.2)
            self.step(dp, db)
                
        print 'VisualizationExecutor END'
        
class ClassificationExecutor(Executor): 

    def __init__(self, uuid, task, dataset, resultsPerUUID, isRunningPerUUID, modelsPerUUID):
        Executor.__init__(self, uuid)
        self.task = task
        self.dataset = dataset
        self.resultsPerUUID = resultsPerUUID
        self.isRunningPerUUID = isRunningPerUUID
        self.modelsPerUUID = modelsPerUUID
        
    def getClassifier(self, classifier):
        print classifier
        if classifier == 'sgd':
            return SGDClassifier()
        if classifier == 'naive_bayes':
            return MultinomialNB(alpha=0.01)
        elif classifier == 'perceptron':
            return Perceptron()
        elif classifier == 'passive_aggressive':
            return PassiveAggressiveClassifier()
        raise NotImplementedError()
        
    def classifyStats(self, cm, y_test, y_prob, tile_size, progress):
        #print classification_report(y_test, y_pred)
        tp = float(cm[1][1])
        fp = float(cm[0][1])
        tn = float(cm[0][0])
        fn = float(cm[1][0])
        #precision = tp / (tp + fp)
        #recall = tp / (tp + fn)
        #f1 = 2 * tp / (2 * tp + fp + fn)
        p_support = (tp + fn) / tile_size
        n_support = (tn + fp) / tile_size
        precision = tp / max((tp + fp), 1) * p_support + tn / max((tn + fn), 1) * n_support
        recall = tp / max((tp + fn), 1) * p_support + tn / max((tn + fp), 1) * n_support
        f1 = 2 * precision * recall / (precision + recall)
        fpr, tpr, thresholds = roc_curve(y_test, y_prob[:, 1])
        roc_auc = auc(fpr, tpr)
        stats = {'tp': tp,
                 'fp': fp,
                 'tn': tn,
                 'fn': fn,
                 'precision': precision,
                 'recall': recall,
                 'f1': f1,
                 'fpr': fpr.tolist(),
                 'tpr': tpr.tolist(),
                 'auc': roc_auc,
                 'progress' : progress}
        return stats            
        
    def run(self):
        dp = SequentialDataProvider(self.dataset, 'C:\\data', self.task['chunkSize'], 0)
        
        cls_name = self.task['classifier']
        cls = self.getClassifier(cls_name)
                
        progressList = []
        f1List = []
        
        X_test = []
        y_test = []
        df_test = None
        while True:
            if not self.isRunningPerUUID[self.uuid]:
                break
            start = time.time()
            tick = time.time()
            
            progress, df = dp.getDataFrame()
            if not self.task['filter'].strip() == '':
                df = df.query(self.task['filter'].strip())
                
            if not df is None: 
                split = int(math.ceil(len(df) * 0.3))
            
                 # retain first as test
                if len(X_test) == 0:
                    X_test = df[self.task['features']]
                    y_test = df.eval(self.task['label'])
                    df_test = df
                    #X_test =  np.array(df[self.task['features']])
                    #y_test =  np.array([1 if x else 0 for x in np.array(df.eval(self.task['label']))])
                    
                else:
                    dfTest = df[:split]
                    dfTrain = df[split:]
                
                    y_train = dfTrain.eval(self.task['label'])
                    X_train = dfTrain[self.task['features']]
                    
                    y_test_current = dfTest.eval(self.task['label'])
                    X_test_current = dfTest[self.task['features']]
                    
                    cls.partial_fit(np.array(X_train), np.array([1 if x else 0 for x in np.array(y_train)]), classes=np.array([0, 1]))
                    
                    y_prob = None
                    y_pred = None
                    
                    if cls_name in ['sgd', 'perceptron', 'passive_aggressive']:
                        y_pred = cls.predict(np.array(pd.concat([X_test, X_test_current])))
                        y_prob = np.array([[0,y] for y in y_pred])
                    else:
                        y_prob = cls.predict_proba(np.array(pd.concat([X_test, X_test_current])))
                        y_pred = [1 if t[0] >= 0.5 else 0 for t in y_prob]
                        
                    y_test_concat = np.array([1 if x else 0 for x in np.array(pd.concat([y_test, y_test_current]))])
                    cm = confusion_matrix(y_test_concat, y_pred)
                    stats = self.classifyStats(cm, y_test_concat, y_prob, len(y_test_concat), progress)
                    progressList.append(progress)
                    f1List.append(stats['f1'])
                    stats['f1'] = f1List
                    stats['progress'] = progressList
                    
                    dfTest = df_test.copy(deep=True)
                    dfTest['actual'] = df_test.eval(self.task['label'])
                    dfTest['predicted'] = [True if t == 1.0 else False for t in y_pred[:len(y_test)]]
                    
                    histograms = {}
                    for feature in self.task['features']:
                        db = DataBinner([feature, feature], ['None', 'Count'], [10,10], [feature], ['Count'], 
                            ['actual and predicted', 'not actual and predicted', 'not actual and not predicted', 'actual and not predicted'])
                        
                        db.bin(dfTest, 1.0)
                        data = {
                            'binStructure' : db.binStructure.toJson(), 
                            'progress' : 1.0
                        }
                        histograms[feature] = data
                    
                    jsonMessage = json.dumps(stats, indent=2, default=default)
                    self.resultsPerUUID[self.uuid] = {self.task['label'] : stats, 'progress' : progress, 'histograms' : histograms}
                    
                    self.modelsPerUUID[self.uuid] = cls
                    
            end = time.time()
            print 'ClassificationExecutor : p= ' + '{0:.2f}'.format(progress) + ', t= ' + str(end - start)
            if df is None or progress >= 1.0:
                self.isRunningPerUUID[self.uuid] = False
                break 
                
        print 'ClassificationExecutor END'        

if __name__ == "__main__":

    parser = OptionParser(usage="usage: %prog [options]", version="%prog 1.0")
    parser.add_option("-p", "--port", default=8888, type='int', action="store", dest="port", help="port (8000)")
    parser.add_option("-d", "--data", default='C:\\data', type='string', action="store", dest="data", help="folder where data is stored")
    
    logging.getLogger().setLevel(logging.INFO)
        
    (options, args) = parser.parse_args()
    Shared().dataFolder = options.data
    logging.info('Starting server on port: ' + str(options.port))
    logging.info('Data folder: ' + options.data)
 

    def close_sig_handler(signal, frame):
        print "close"   
        for comp in Shared().isRunningPerUUID.keys():
            Shared().isRunningPerUUID[comp] = False

        server.close()
        sys.exit()

    signal.signal(signal.SIGINT, close_sig_handler)

       
    server = Server(('', options.port), RequestHandler)
    server.serve_forever()