import pandas as pd
from optparse import OptionParser
import ntpath
import os
import numpy as np
import math


def shuffle(df, n=1, axis=0):     
    df = df.copy()
    for _ in range(n):
        df.apply(np.random.shuffle, axis=axis)
        return df        

if __name__ == "__main__":

    parser = OptionParser(usage="usage: %dataprovider [options]", version="%dataprovider 1.0")
    parser.add_option("-f", "--file", action="store", type="string", dest="filename")
    parser.add_option("-d", "--data", action="store", type="string", dest="datafolder")
    parser.add_option("-c", "--chunk", action="store", type="int", dest="chunksize")
    (options, args) = parser.parse_args()
    
    output_dir = os.path.join(options.datafolder, ntpath.basename(options.filename + '_' + str(options.chunksize)))
    if os.path.exists(output_dir):
        for name in os.listdir(output_dir):
            os.remove(os.path.join(output_dir, name))
        os.rmdir(output_dir)
    os.makedirs(output_dir)
    
    count = 0    
    data = pd.read_table(options.filename, sep=",", chunksize=options.chunksize)
    
    print ":" + ntpath.basename(options.filename) +":"
    
    for chunk in data:
        shuffle(chunk).to_csv(os.path.join(output_dir, str(count)), sep=",", headers=True)
        #print chunk['mpg'][0]
        #print chunk['mpg'].max()
        #print chunk.columns.values
        #print chunk
        #print chunk.query("mpg > 35")
        #print chunk.max(axis=chunk['mpg'])
        count += 1
    print options.datafolder    
    subdirs = [name for name in os.listdir(options.datafolder) if os.path.isdir(os.path.join(options.datafolder, name))]
    datafolder = [name for dir in subdirs if dir.startswith(ntpath.basename(options.filename))][0]
    print datafolder
    
   