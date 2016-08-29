#!/usr/bin/python
import json
import numpy as np
import pandas as pd
import math

class BinRange():
    def __init__(self, dataMinValue, dataMaxValue, targetBinNumber):
        self.dataMinValue = float(dataMinValue)
        self.dataMaxValue = float(dataMaxValue)
        self.targetBinNumber = float(targetBinNumber)
        self.maxValue = 0
        self.minValue = 0
    
    def getIndex(self, value):
        raise NotImplementedError()
    
    def addStep(self, value):
        raise NotImplementedError()
       
    def getLabel(self, value):
        return str(value)
        
    def getBins(self):
        raise NotImplementedError()
        
    def getUpdatedBinRange(self, dataMin, dataMax, df, dimension):
        raise NotImplementedError()

    def getLabels(self):
        labels = []
        for b in self.getBins():
            labels.append((bin, bin, self.addStep(bin), self.getLabel(bin)))
        return labels

class AggregatedBinRange(BinRange):        
    
    def __init__(self):
        BinRange.__init__(self, 0, 0, 0)
        self.type = 'AggregatedBinRange'
    
    @staticmethod
    def initialize():
        scale = AggregatedBinRange()
        return scale
    
    def getIndex(self, value):
        return 0       
    
    def addStep(self, value):
        return value + 1
        
    def getBins(self):
        scale = [0]
        return scale
        
    def getUpdatedBinRange(self, dataMin, dataMax, df, dimension):
        return AggregatedBinRange()

class NominalBinRange(BinRange):        
    def __init__(self):
        BinRange.__init__(self, 0, 0, 0)
        self.labelsValue = {} #string, index
        self.valuesLabel = {} #index, string
        self.type = 'NominalBinRange'
    
    @staticmethod
    def initialize(df, val):
        uniqueValues = df[val].unique()
    
        scale = NominalBinRange()
        for u in uniqueValues:
            if not u in scale.labelsValue:
                index = len(scale.labelsValue.keys())
                scale.labelsValue[u] = index
                scale.valuesLabel[index] = u
        return scale
    
    def getIndexFromValue(self, value):
        return self.labelsValue[value]  
    
    def getIndex(self, value):
        return value
    
    def addStep(self, value):
        return value
        
    def getLabel(self, value):
       return self.valuesLabel[value]      
        
    def getBins(self):
        scale = []
        for idx, label in enumerate(self.labelsValue):
            scale.append(idx)
        return scale
        
    def getUpdatedBinRange(self, dataMin, dataMax, df, val):
        newRange = NominalBinRange()
        newRange.labelsValue = self.labelsValue
        newRange.valuesLabel = self.valuesLabel
        
        uniqueValues = df[val].unique()
    
        for u in uniqueValues:
            if not u in newRange.labelsValue:
                index = len(newRange.labelsValue.keys())
                newRange.labelsValue[u] = index
                newRange.valuesLabel[index] = u
        return newRange
        
class QuantitativeBinRange(BinRange):        
    def __init__(self, dataMinValue, dataMaxValue, targetBinNumber, isIntegerRange):
        BinRange.__init__(self, dataMinValue, dataMaxValue, targetBinNumber)
        self.isIntegerRange = isIntegerRange
        self.step = 0
        self.type = 'QuantitativeBinRange'
    
    @staticmethod
    def initialize(dataMinValue, dataMaxValue, targetBinNumber, isIntegerRange):
        scale = QuantitativeBinRange(dataMinValue, dataMaxValue, targetBinNumber, isIntegerRange)
        extent = scale.__getExtent(scale.dataMinValue, scale.dataMaxValue, scale.targetBinNumber)
        scale.minValue = extent[0]
        scale.maxValue = extent[1]
        scale.step = extent[2]
        return scale
    
    def getIndex(self, value):
        return int(math.floor(round((value - self.minValue) / self.step, 8)))        
    
    def addStep(self, value):
        return value + self.step
        
    def getBins(self):
        scale = []
        idx = 0
        for v in np.arange(self.minValue, self.maxValue, self.step):
            scale.append(v)
            idx += 1
        return scale
        
    def getUpdatedBinRange(self, dataMin, dataMax, df, dimension):
        newMin = self.minValue
        newMax = self.maxValue

        if dataMin < self.minValue:
            while dataMin < newMin:
                newMin -= self.step
                
        if dataMax >= self.maxValue:
            while dataMax >= newMax:
                newMax += self.step

        multiplier = int(len(self.getBins()) / self.targetBinNumber);
        newStep = self.step
        if multiplier > 1:
            pass
            #newStep = Step * (double)multiplier

        newRange = QuantitativeBinRange(dataMin, dataMax, self.targetBinNumber, self.isIntegerRange)
        
        newRange.minValue = newMin
        newRange.maxValue = newMax
        newRange.dataMinValue = min(dataMin, self.dataMinValue)
        newRange.dataMaxValue = min(dataMax, self.dataMaxValue)
        newRange.step = self.step
        return newRange
        
    def __getExtent(self, dataMin, dataMax, m):
        if (dataMin == dataMax):
            dataMax += 0.1
            
        span = dataMax - dataMin

        step = math.pow(10, math.floor(math.log10(span / m)))
        err = m / span * step

        if (err <= .15):
            step *= 10
        elif (err <= .35):
            step *= 5
        elif (err <= .75):
            step *= 2

        if (self.isIntegerRange):
            step = math.ceil(step)
        
        ret = [0,0,0]
        ret[0] = (math.floor(round(dataMin, 8) / step) * step)
        ret[1] = (math.floor(round(dataMax, 8) / step) * step + step)
        ret[2] = step

        return ret
        
