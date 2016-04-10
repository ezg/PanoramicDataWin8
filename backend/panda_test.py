import pandas as pd
import numpy as np
import math
import tornado.httpserver
import tornado.websocket
import tornado.ioloop
import tornado.web
import json
from binrange import QuantitativeBinRange

d = [{'dataMaxValue': 230, 'maxValue': 240.0, 'minValue': 40.0, 'step': 20.0, 'dataMinValue': 46, 'maxMin': 0, 'targetBinNumber': 10.0, 'isIntegerRange': True, 'type': 'QuantitativeBinRange'}]
print json.dumps(d)
c = 0

chunksize = 50
data  = pd.read_table("original_data\\cars_small.csv", sep=",", chunksize=chunksize)

a = [3, 45,5, 6]

#print range(len((1,2,2)))
df = pd.DataFrame({'col1':[1,2,3,4,5,6], 'col2':[1,2,1,1,1,1]})

sub1 = 'col1 > 4'
sub2 = 'col1 <= 5'
#print len(df)

tt = df.query('col2 == 1').eval(sub1)
mt = pd.Series([False] * len(tt), index=tt.index)
#print mt

class Test():
    def __init__(self,index):
        self.index = index  
    def __eq__(self, other):
        return other and self.index == other.index

    def __ne__(self, other):
        return not self.__eq__(other)

    def __hash__(self):
        return index
        
tt = tt[(mt | tt)]


def f(x):
    print x.dtypes
    ret = []
    ret.extend(x['col1'])
    ret.extend([0,0])
    return ret



def vecorize(df):
    ret = []
    
ret = []
ret.append('a')
ret.extend([0,0])
np.array(ret).flatten()

df = pd.DataFrame({'col1':["a","B","a","a","C","C"], 'col2':[1,2,1,1,1,1]})
print df.dtypes['col2']
df['Flag'] = df.apply(lambda x: f(x), axis=1).astype(object)
print df
print np.array(df)


#df['Flag'] = df.applymap(f).astype(bool)
#print df

#print df[~(df.eval(sub1) | df.eval(sub2))]




#print df2[~df2.isin(df1).all(1)]

#print df2[(df2!=df1)]
#print df2[~(df2==df1)].dropna(how='all')


for chunk in data:
    #print chunk
    #print chunk['mpg'][0]
    #print chunk['mpg'].max()
    #print chunk.query('mpg > 20')
    #print chunk.isin(chunk.query('mpg > 20').all(1))
    break
    
    for index, row in chunk.iterrows():   
        print row.query("mpg > 35")
        print chunk.iloc[index]['mpg']
        
    #print chunk.columns.values
    #print chunk
    #print chunk.query("mpg > 35")
    #print chunk.max(axis=chunk['mpg'])
    c += 1
    

