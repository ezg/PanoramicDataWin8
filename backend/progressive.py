import time
import timeit
import sys
import json
import numpy as np
import threading
import multiprocessing
from SimpleWebSocketServer import WebSocket, SimpleWebSocketServer, SimpleSSLWebSocketServer
from dataprovider import SequentialDataProvider
from databinner import DataBinner
from optparse import OptionParser

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
        self.dataFolder = ''
        self.updator = None

class ExecutorWebSocket(WebSocket):

    def handleMessage(self):
        job = json.loads(self.data)
        logging.info('Request : ' + job['type'])
        
        if job['type'] == 'execute':
            if job['task']['type'] == 'visualization':
                if Shared().updator == None:
                    Shared().updator = Updator(Shared().resultsPerComputation)
                    Shared().updator.start()
               
                # already full result available:
                if self.data in Shared().resultsPerComputation and Shared().resultsPerComputation[self.data]['progress'] == 1.0:
                    self.sendMessage(json.dumps(Shared().resultsPerComputation[self.data], indent=2, default=default))
                    self.close()
               
                if self.data in Shared().updator.clientsPerComputation and self.data in Shared().isRunningPerComputation and Shared().isRunningPerComputation[self.data]:
                    print 'existing comp'
                    clientList = Shared().updator.clientsPerComputation[self.data]
                    clientList.append(self)
                    Shared().updator.clientsPerComputation[self.data] = clientList
                else:
                    print 'new comp'
                    exe = VisualizationExecutor(self.data, job['task'], job['dataset'], Shared().resultsPerComputation, Shared().isRunningPerComputation)
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
            self.sendMessage(json.dumps([['supervised',[
                                      'logistic_regression',
                                      'naive_bayes',
                                      'perceptron',
                                      'svm']],
                                 ['unsupervised',[
                                      'correlate',
                                      'frequent_itemsets',
                                      'kmeans',
                                      'regression']]
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
            time.sleep(0.1)
            for comp in self.clientsPerComputation:
                for client in self.clientsPerComputation[comp]:
                    if not comp in self.lastProgressUpdatePerCompuationAndClient:
                        self.lastProgressUpdatePerCompuationAndClient[comp] = {}
                    if not client in self.lastProgressUpdatePerCompuationAndClient[comp]:
                        self.lastProgressUpdatePerCompuationAndClient[comp][client] = 0
                    
                    if comp in self.resultsPerComputation:
                        client.sendMessage(json.dumps(self.resultsPerComputation[comp], indent=2, default=default))
                        self.lastProgressUpdatePerCompuationAndClient[comp][client] = self.resultsPerComputation[comp]['progress']
                        
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
        
    def run(self):
        dp = SequentialDataProvider(self.dataset, 'C:\\data', self.task['chunkSize'], 0)
        db = DataBinner(self.task['dimensions'], self.task['dimensionAggregateFunctions'], self.task['nrOfBins'], self.task['aggregateDimensions'], self.task['aggregateFunctions'], self.task['brushes'])

        while True:
            if not self.isRunningPerComputation[self.computation]:
                break
            start = time.time()
            
            progress, df = dp.getDataFrame()
            if not self.task['filter'] == '':
                print "f", ':' + self.task['filter'] + ':'
                df = df.query(self.task['filter'])
                
            if not df is None: 
                db.bin(df, progress)
                data = {
                    'binStructure' : db.binStructure.toJson(), 
                    'progress' : progress
                }
                jsonMessage = json.dumps(data, indent=2, default=default)
                self.resultsPerComputation[self.computation] = data
                    
            end = time.time()
            print 'VisualizationExecutor : p= ' + '{0:.2f}'.format(progress) + ', t= ' + str(end - start)
            if df is None or progress == 1.0:
                self.isRunningPerComputation[self.computation] = False
                break 
                
        print 'VisualizationExecutor END'

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