from dataprovider import SequentialDataProvider
from databinner import DataBinner
import json
import time
import numpy as np


job = {
  "type": "execute",
  "dataset": "facts.csv_5000",
  "task": {
    "filter": "",
    "aggregateFunctions": [
      "Count"
    ],
    "type": "visualization",
    "chunkSize": 10000,
    "aggregateDimensions": [
      "obama"
    ],
    "nrOfBins": [
      10.0,
      10.0
    ],
    "brushes": [],
    "dimensionAggregateFunctions": [
      "None",
      "Count"
    ],
    "dimensions": [
      "obama",
      "obama"
    ]
  }
}
print json.dumps(job)

task = job['task']

dp = SequentialDataProvider(job['dataset'], 'C:\\data\\', task['chunkSize'], 0)
db = DataBinner(task['dimensions'], task['dimensionAggregateFunctions'], task['nrOfBins'], task['aggregateDimensions'], task['aggregateFunctions'], task['brushes'])

aggName = 'mpg'
aggFunc = 'Count'

aggKey1 = (aggName, aggFunc, 0)
aggKey2 = (aggName, aggFunc, 1)
aggKey3 = (aggName, aggFunc, 1)
aggKey4 = (aggName, aggFunc, 1)

def default(o):
    if isinstance(o, np.integer): return int(o)
    raise TypeError

while True:
    start = time.time()

    c, df = dp.getDataFrame()
    if not task['filter'] == '':
        df = df.query(task['filter'])
    print 'progress', str(c * 100.0) + '%'
    if not df is None:
        db.bin(df, c)
        
        br0 = db.binStructure.binRanges[0]        
        br1 = db.binStructure.binRanges[1]
       
       
        
        
        for b0 in br0.getBins():
            print '  ' + br0.getLabel(b0)
            bkey = (br0.getIndex(b0),)
        
            for b1 in br1.getBins():  
                bkey = (br0.getIndex(b0), br1.getIndex(b1))
                print '    ' + br1.getLabel(b1)
                print '      ',
                for k in [aggKey1, aggKey2, aggKey3, aggKey4]:
                    print str(db.binStructure.bins[bkey].values[k]) +',',
                print
                print '      ',
                for k in [aggKey1, aggKey2, aggKey3, aggKey4]:
                    print str(db.binStructure.bins[bkey].counts[k]) +',',
                print

        
        #print len(db.binStructure.bins)
        #for g in db.binStructure.bins:
        #    print g
    
    if df is None or c == 1.0:
        break    
        
    end = time.time()
    print "time", (end - start)
    print