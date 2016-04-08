from dataprovider import SequentialDataProvider
from sklearn.metrics import roc_curve
from databinner import DataBinner
import json
from sklearn.utils import compute_class_weight
import numpy as np
import time
from sklearn.linear_model import SGDClassifier
from sklearn.linear_model import PassiveAggressiveClassifier
from sklearn.linear_model import Perceptron
from sklearn.naive_bayes import MultinomialNB

def get_classifier(classifier):
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
    
    
    
job = {
  "type": "execute",
  "dataset": "cars_small.csv_5000",
  "task": {
    "type": "classify",
    "classifier": "passive_aggressive",
    "chunkSize": 2500,
    "features": [
      "acceleration", "model_year", "horsepower", "displacement" 
    ],
    "label": "mpg < 25",
    "filter": ""
  }
}

print json.dumps(job)

task = job['task']

dp = SequentialDataProvider(job['dataset'], 'C:\\data\\', task['chunkSize'], 0)
cls = get_classifier(task['classifier'])

def default(o):
    if isinstance(o, np.integer): return int(o)
    raise TypeError

cls_stats = {}
cls_name = task['classifier']    

stats = {'n_train': 0, 'n_train_pos': 0,
         'accuracy': 0.0, 'accuracy_history': [(0, 0)], 't0': time.time(),
         'runtime_history': [(0, 0)], 'total_fit_time': 0.0}
cls_stats[cls_name] = stats

X_test = None
y_test = None
while True:
    c, df = dp.getDataFrame()
    tick = time.time()
    
    if not task['filter'] == '':
        df = df.query(task['filter'])
    print 'progress', str(c * 100.0) + '%'
    
    if not df is None:
        # retain first as test
        if X_test == None:
            X_test =  np.array(df[task['features']])
            y_test =  np.array([1 if x else 0 for x in np.array(df.eval(task['label']))])
            
        else:
            y_train = np.array([1 if x else 0 for x in np.array(df.eval(task['label']))])
            X_train = df[task['features']]
            cls.partial_fit(X_train, y_train, classes=np.array([0, 1]))
            
            y_pred = cls.predict(X_train).sum()
            print y_pred, np.array([1 if x else 0 for x in np.array(df.eval(task['label']))]).sum()

            # accumulate test accuracy stats
            cls_stats[cls_name]['total_fit_time'] += time.time() - tick
            cls_stats[cls_name]['n_train'] += X_train.shape[0]
            cls_stats[cls_name]['n_train_pos'] += sum(y_train)
            tick = time.time()
            cls_stats[cls_name]['accuracy'] = cls.score(X_test, y_test)
            cls_stats[cls_name]['prediction_time'] = time.time() - tick
            acc_history = (cls_stats[cls_name]['accuracy'],
                           cls_stats[cls_name]['n_train'])
            cls_stats[cls_name]['accuracy_history'].append(acc_history)
            run_history = (cls_stats[cls_name]['accuracy'],
                           cls_stats[cls_name]['total_fit_time'])
            cls_stats[cls_name]['runtime_history'].append(run_history)
        
            print cls_stats[cls_name]['accuracy']
        
    
    if df is None or c == 1.0:
        break    
    print