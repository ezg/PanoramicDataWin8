import time
import timeit
import sys
import json
import numpy as np
import pandas as pd
import threading
import math
import multiprocessing
from SimpleWebSocketServer import WebSocket, SimpleWebSocketServer, SimpleSSLWebSocketServer
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
        self.resultsPerComputation = self.manager.dict()
        self.isRunningPerComputation = self.manager.dict()
        self.modelsPerComputation = self.manager.dict()
        self.dataFolder = ''
        self.updator = None

class ExecutorWebSocket(WebSocket):

    def handleMessage(self):
        job = json.loads(self.data)
        logging.info('Request : ' + job['type'])
        if job['type'] == 'execute':            
            if Shared().updator == None:
                Shared().updator = Updator(Shared().resultsPerComputation)
                Shared().updator.start()
            # already full result available:
            
            if self.data in Shared().resultsPerComputation and Shared().resultsPerComputation[self.data]['progress'] == 1.0:
                self.sendMessage(json.dumps(Shared().resultsPerComputation[self.data], indent=2, default=default))
                self.close()

            elif self.data in Shared().updator.clientsPerComputation and self.data in Shared().isRunningPerComputation and Shared().isRunningPerComputation[self.data]:
                print 'existing comp ' + job['task']['type']
                clientList = Shared().updator.clientsPerComputation[self.data]
                clientList.append(self)
                Shared().updator.clientsPerComputation[self.data] = clientList
            else:   
                if job['task']['type'] == 'visualization':
                    print 'new comp visualization'
                    print "1", str(time.time())
                    exe = VisualizationExecutor(self.data, job['task'], job['dataset'], Shared().resultsPerComputation, Shared().isRunningPerComputation)
                    if self.data in Shared().updator.clientsPerComputation:
                        Shared().updator.clientsPerComputation[self.data].append(self)
                    else:
                        Shared().updator.clientsPerComputation[self.data] = [self]
                    exe.start()  
                    
                elif job['task']['type'] == 'classify':
                    print 'new comp classify'
                    exe = ClassificationExecutor(self.data, job['task'], job['dataset'], Shared().resultsPerComputation, Shared().isRunningPerComputation, Shared().modelsPerComputation)
                    if self.data in Shared().updator.clientsPerComputation:
                        Shared().updator.clientsPerComputation[self.data].append(self)
                    else:
                        Shared().updator.clientsPerComputation[self.data] = [self]
                    exe.start()  

                
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
            self.sendMessage(json.dumps(schemas, indent=2, default=default))
            
        
            
        elif job['type'] == 'tasks':
            self.sendMessage(json.dumps([['classify',[
                                      'sgd',
                                      'naive_bayes',
                                      'perceptron',
                                      'passive_aggressive']]
                                ], default=default))

    def handleConnected(self):
        print (self.address, 'connected')
        
    def handleClose(self):
        print (self.address, 'closed')
        if Shared().updator != None:
            for comp in Shared().updator.clientsPerComputation.keys():
                if self in Shared().updator.clientsPerComputation[comp]:
                    clientsList = Shared().updator.clientsPerComputation[comp]
                    clientsList.remove(self)
                    Shared().updator.clientsPerComputation[comp] = clientsList
                    if len(Shared().updator.clientsPerComputation[comp]) == 0:
                        Shared().isRunningPerComputation[comp] = False

class Updator(threading.Thread):    
                 
    def __init__(self, resultsPerComputation):
        threading.Thread.__init__(self)    
        self.clientsPerComputation = {}
        self.lastProgressUpdatePerCompuationAndClient = {}
        self.resultsPerComputation = resultsPerComputation
        self.running = True
        
    def run(self):
        while self.running:
            time.sleep(0.05)
            for comp in self.clientsPerComputation:
                for client in self.clientsPerComputation[comp]:
                    if not comp in self.lastProgressUpdatePerCompuationAndClient:
                        self.lastProgressUpdatePerCompuationAndClient[comp] = {}
                    if not client in self.lastProgressUpdatePerCompuationAndClient[comp]:
                        self.lastProgressUpdatePerCompuationAndClient[comp][client] = 0
                    
                    if comp in self.resultsPerComputation:
                        if self.lastProgressUpdatePerCompuationAndClient[comp][client] < self.resultsPerComputation[comp]['progress']:
                            client.sendMessage(json.dumps(self.resultsPerComputation[comp], indent=2, default=default))
                            self.lastProgressUpdatePerCompuationAndClient[comp][client] = self.resultsPerComputation[comp]['progress']
                            print "3", str(time.time()), str(self.resultsPerComputation[comp]['progress'])
                        if self.lastProgressUpdatePerCompuationAndClient[comp][client] == 1.0:
                            client.close()
                    
class Executor(multiprocessing.Process): 

    def __init__(self, computation):
        multiprocessing.Process.__init__(self)
        self.computation = computation    
        self.count = 0
        
    def run(self):
        raise NotImplementedError()
                                

class VisualizationExecutor(Executor): 

    def __init__(self, computation, task, dataset, resultsPerComputation, isRunningPerComputation):
        Executor.__init__(self, computation)
        
        self.task = task
        self.dataset = dataset
        self.resultsPerComputation = resultsPerComputation
        self.isRunningPerComputation = isRunningPerComputation
        self.isRunningPerComputation[self.computation] = True
        
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
            self.resultsPerComputation[self.computation] = data
            
        if df is None or progress >= 1.0:
            self.isRunningPerComputation[self.computation] = False
            
        end = time.time()
        print 'VisualizationExecutor : p= ' + '{0:.2f}'.format(progress) + ', t= ' + str(end - start)    
        return df, progress
        
    def run(self):
        dp = SequentialDataProvider(self.dataset, 'C:\\data', self.task['chunkSize'], self.task['chunkSize'])
        db = DataBinner(self.task['dimensions'], self.task['dimensionAggregateFunctions'], self.task['nrOfBins'], self.task['aggregateDimensions'], self.task['aggregateFunctions'], self.task['brushes'])

        while True:
            if not self.isRunningPerComputation[self.computation]:
                break
            self.step(dp, db)
                
        print 'VisualizationExecutor END'
        
class ClassificationExecutor(Executor): 

    def __init__(self, computation, task, dataset, resultsPerComputation, isRunningPerComputation, modelsPerComputation):
        Executor.__init__(self, computation)
        self.task = task
        self.dataset = dataset
        self.resultsPerComputation = resultsPerComputation
        self.isRunningPerComputation = isRunningPerComputation
        self.modelsPerComputation = modelsPerComputation
        self.isRunningPerComputation[self.computation] = True
        
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
        
        dataBinners = {}
        for feature in self.task['features']:
            db = DataBinner([feature, feature], ['None', 'Count'], [10,10], [feature], ['Count'], 
                ['actual and predicted', 'not actual and predicted', 'not actual and not predicted', 'actual and not predicted'])
            dataBinners[feature] = db

        cls_name = self.task['classifier']
        cls = self.getClassifier(cls_name)
                
        progressList = []
        f1List = []
        
        X_test = []
        y_test = []
        while True:
            if not self.isRunningPerComputation[self.computation]:
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
                    
                    dfTest = dfTest.copy(deep=True)
                    dfTest['actual'] = dfTest.eval(self.task['label'])
                    dfTest['predicted'] = [True if t == 1.0 else False for t in y_pred[len(y_test):]]
                    
                    histograms = {}
                    for feature in self.task['features']:
                        db = dataBinners[feature]
                        
                        db.bin(dfTest, progress)
                        data = {
                            'binStructure' : db.binStructure.toJson(), 
                            'progress' : progress
                        }
                        histograms[feature] = data
                    
                    jsonMessage = json.dumps(stats, indent=2, default=default)
                    self.resultsPerComputation[self.computation] = {self.task['label'] : stats, 'progress' : progress, 'histograms' : histograms}
                    
                    self.modelsPerComputation = cls
                    
            end = time.time()
            print 'ClassificationExecutor : p= ' + '{0:.2f}'.format(progress) + ', t= ' + str(end - start)
            if df is None or progress >= 1.0:
                self.isRunningPerComputation[self.computation] = False
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
    
    cls = ExecutorWebSocket
    server = SimpleWebSocketServer('localhost', options.port, cls)
    
    def close_sig_handler(signal, frame):
        print "close"
        if not Shared().updator == None:
            Shared().updator.running = False        
        for comp in Shared().isRunningPerComputation.keys():
            Shared().isRunningPerComputation[comp] = False

        server.close()
        sys.exit()

    signal.signal(signal.SIGINT, close_sig_handler)

    server.serveforever()