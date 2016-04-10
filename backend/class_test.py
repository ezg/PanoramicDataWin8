from dataprovider import SequentialDataProvider

from databinner import DataBinner
import json

import numpy as np
import time
from sklearn.metrics import roc_curve
from sklearn.metrics import confusion_matrix
from sklearn.utils import compute_class_weight
from sklearn.metrics import auc
from sklearn.metrics import classification_report
from sklearn.metrics import roc_curve
from sklearn.linear_model import SGDClassifier
from sklearn.linear_model import PassiveAggressiveClassifier
from sklearn.linear_model import Perceptron
from sklearn.naive_bayes import MultinomialNB
from sklearn.feature_extraction.text import HashingVectorizer

def getClassifier(classifier):
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
    
def classifyStats( cm, y_test, y_prob, tile_size):
    #print cm
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
             'auc': roc_auc}
    return stats    
    
    
job = {
  "type": "execute",
  "dataset": "cars.csv_100000",
  "task": {
    "type": "classify",
    "classifier": "passive_aggressive",
    "chunkSize": 1000,
    "features": [
      "mpg", "model_year", "horsepower", "displacement" 
    ],
    "label": "mpg < 15",
    "filter": ""
  }
}

print json.dumps(job)

task = job['task']

dp = SequentialDataProvider(job['dataset'], 'C:\\data\\', task['chunkSize'], 0)
cls = getClassifier(task['classifier'])

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
            df['test'] = df.apply(lambda x: 0, axis=1).astype(object)
            X_test =  np.array(df[task['features']])
            X_test =  np.array(df['test'])
            y_test =  np.array([1 if x else 0 for x in np.array(df.eval(task['label']))])
            
        else:
            y_train = np.array([1 if x else 0 for x in np.array(df.eval(task['label']))])
            X_train = np.array(df[task['features']])
            df['test'] = df.apply(lambda x: 0, axis=1).astype(object)
            X_train = np.array(df['test'])
            print X_train
            
            
            #print len(X_train), len(y_train)
            cls.partial_fit(X_train, y_train, classes=np.array([0, 1]))
            

            y_prob = None
            y_pred = None
            if cls_name in ['sgd', 'perceptron', 'passive_aggressive']:
                y_pred = cls.predict(X_test)
                y_prob = np.array([[0,y] for y in y_pred])
            else:
                y_prob = cls.predict_proba(X_test)
                print y_prob
                y_pred = [1 if t[0] >= 0.5 else 0 for t in y_prob]

            cm = confusion_matrix(y_test, y_pred)
            stats = classifyStats(cm, y_test, y_prob, len(y_test))
            print stats
            
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