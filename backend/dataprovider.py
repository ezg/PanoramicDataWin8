import pandas as pd
from binrange import QuantitativeBinRange
from optparse import OptionParser
import ntpath
import os
import numpy as np
import math

class SequentialDataProvider(): 

    def __init__(self, datasetName, dataFolder, requestChunkSize, lineStartFrom):
        self.datasetName = datasetName
        subdirs = [name for name in os.listdir(dataFolder) if os.path.isdir(os.path.join(dataFolder, name))]
        datasetNameFolder = [name for dir in subdirs if dir.startswith(datasetName)][0]
        for d in subdirs:
            if d.startswith(self.datasetName):
                datasetNameFolder = d
                break
        
        self.datasetDir = os.path.join(dataFolder, datasetNameFolder)
        self.fileChunkSize = int(datasetNameFolder.split('_')[-1])
        self.requestChunkSize = requestChunkSize
        self.currentFileIndex = self.calculateFileIndex(lineStartFrom)
        self.currentChunkIndex = 0
        self.dataReader = self.getDataReader()        
        self.progress = 0
        self.numberOfFiles = len([name for name in os.listdir(self.datasetDir) if os.path.isfile(os.path.join(self.datasetDir, name))])
        self.perFileProgress = 1.0 / float(self.numberOfFiles)
        self.perChunkProgress = (1.0 / (float(self.fileChunkSize) / float(requestChunkSize))) * self.perFileProgress
                
        chunksToSkip = int((lineStartFrom - (self.currentFileIndex * self.fileChunkSize)) / requestChunkSize)
        for i in range(chunksToSkip):
            self.getDataFrame()
        
    def calculateFileIndex(self, lineIndexFrom):
        return int(math.floor(lineIndexFrom / self.fileChunkSize))
            
    def getDataReader(self):
        self.currentChunkIndex = 0
        if os.path.exists(os.path.join(self.datasetDir, str(self.currentFileIndex))):
            return pd.read_table(os.path.join(self.datasetDir, str(self.currentFileIndex)) , sep=",", chunksize=self.requestChunkSize)
        else:
            return None
            
    def getDataFrame(self):
        if self.dataReader == None:
            self.progress = 1.0
            return self.progress, None
        
        try:
            df = self.dataReader.get_chunk()
            self.currentChunkIndex += 1
            
            self.progress = (float(self.currentFileIndex) * self.perFileProgress) + (self.currentChunkIndex * self.perChunkProgress)
            
            return self.progress, df
        except StopIteration:
            self.currentFileIndex += 1
            self.dataReader = self.getDataReader()
            return self.getDataFrame()
            
