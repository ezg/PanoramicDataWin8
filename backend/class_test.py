from dataprovider import SequentialDataProvider
from sklearn.metrics import roc_curve
from databinner import DataBinner
import json
from sklearn.utils import compute_class_weight
import numpy as np

def classify_stats(cm, y_test, y_prob, tile_size):
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
                       total_vect_time + cls_stats[cls_name]['total_fit_time'])
        cls_stats[cls_name]['runtime_history'].append(run_history)


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

def get_classifier(classifier):
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
  "dataset": "cars_small.csv_1000",
  "task": {
    "type": "classify",
    "classifier": "sgd",
    "chunkSize": 2000,
    "features": [
      "mpg"
    ],
    "label": "mpg > 25"
  }
}

print json.dumps(job)

task = job['task']

dp = SequentialDataProvider(job['dataset'], 'C:\\data\\', task['chunkSize'], 0)
b = get_classifier(task.classifier)

def default(o):
    if isinstance(o, np.integer): return int(o)
    raise TypeError

while True:
    c, df = dp.getDataFrame()
    if not task['filter'] == '':
        df = df.query(task['filter'])
    print 'progress', str(c * 100.0) + '%'
    if not df is None:
        y_data = self.results[self.task.label].data
        X_data = self.results[self.task.features].data
        y = np.array(y_data)
        X = np.array(X_data)



        tile_size = 1000
        num_tiles = y.size / tile_size
        for i in range(num_tiles):
            pos = i * tile_size
            X_sub = X[pos : pos + tile_size]
            y_sub = y[pos : pos + tile_size]

            y_prob = None
            y_pred = None
            if self.task.classifier == 'svm':
                y_pred = b.predict(X_sub)
                y_prob = np.array([[0,y] for y in y_pred])
            else:
                y_prob = b.predict_proba(X_sub)
                y_pred = [1 if y[1] >= 0.5 else 0 for y in y_prob]

            cm = confusion_matrix(y_sub, y_pred)
            stats = classify_stats(cm, y_test, y_prob)

            y_pred = pd.DataFrame(y_pred, columns=y_data.columns)
            result = ClassifyResult(self.task, 1.0, b, stats)
            self.results[self.task.uuid] = result

            b.partial_fit(X_sub, y_sub)
        
        
    
    if df is None or c == 1.0:
        break    
    print