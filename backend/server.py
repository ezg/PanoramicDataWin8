#!/usr/bin/python
import json
import re
import traceback
import uuid
from BaseHTTPServer import BaseHTTPRequestHandler
from BaseHTTPServer import HTTPServer
from multiprocessing import Manager
from SocketServer import ThreadingMixIn

class Server(ThreadingMixIn, HTTPServer):
    def __init__(self, addr, handler, file_dir):
        HTTPServer.__init__(self, addr, handler)

        self.mgr = Manager()
        self.tasks = {}
        self.executors = {}
        self.catalog = self.mgr.dict()
        self.results = self.mgr.dict()

        self.id = 0

class RequestHandler(BaseHTTPRequestHandler):
    def do_POST(self):
        response = 200
        result = None
        try:
            content_length = int(self.headers.getheader('content-length'))
            req_msg = json.loads(self.rfile.read(content_length))
            print req_msg

            tasks = self.server.tasks
            executors = self.server.executors
            catalog = self.server.catalog
            results = self.server.results

            req_type = req_msg['type']
            print req_type
            #request = req_msg['request']
            if req_type == 'catalog':
                result = json.dumps(dict(catalog))
            elif req_type == 'cancel':
                req_uuid = req_msg['uuid']
                result = json.dumps({'uuid': req_uuid})
                #executors[req_uuid].terminate()
            elif req_type == 'execute':
                task_msg = req_msg['task']
                print task_msg['type']
                if str(task_msg) in tasks:
                    req_uuid = tasks[str(task_msg)]
                    result = json.dumps({'uuid': req_uuid})
                    pass
                else:
                    req_uuid = str(self.server.id)
                    self.server.id += 1
                    #req_uuid = str(uuid.uuid4())
                    tasks[str(task_msg)] = req_uuid
                    task = parse_task(req_uuid, task_msg)
                    results[req_uuid] = EmptyResult(task)
                    executor = BasicExecutor(catalog, results, task)
                    executors[req_uuid] = executor
                    executor.start()
                    result = json.dumps({'uuid': req_uuid})
            elif req_type == 'lookup':
                req_uuid = req_msg['uuid']
                result = results[req_uuid]
                #print req_uuid, ':', result.to_json()
                if isinstance(result, TransformResult):
                    page_size = int(req_msg['page_size'])
                    page_num = int(req_msg['page_num'])

                    i = page_size * page_num
                    j = i + page_size
                    #print i, j
                    result = result.data[i:j].to_json(orient='records')
                else:
                    result = result.to_json()
                    #print result
            elif req_type == 'tasks':
                result = json.dumps([['supervised',[
                                          'logistic_regression',
                                          'naive_bayes',
                                          'perceptron',
                                          'svm']],
                                     ['unsupervised',[
                                          'correlate',
                                          'frequent_itemsets',
                                          'kmeans',
                                          'regression']]
                                    ])
                # include additional info
                # what are the available params for panoD to display?
                #   -learning rate
                #   -number of iters
                #   -etc
            elif req_type == 'codegen':
                req_uuid = req_msg['uuid']
                code = BODY.replace('XXX', codegen('', req_uuid, results))
                code = code.replace('\n', '\r\n')
                result = json.dumps({'code': code})
            else:
                result = 'not implemented'
                #result = ErrorResult('not implemented')
                raise NotImplementedError()
        except:
            print traceback.format_exc()
            response = 500
            result = 'malformed request\n'
            #result = ErrorResult('malformed request')

        self.send_response(response)
        self.send_header('Content-type','application/json')
        self.end_headers()
        self.wfile.write(result)
