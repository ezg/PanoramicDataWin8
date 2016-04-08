#!/usr/bin/python
import json
import time
import numpy as np
import pandas as pd
import math
from binrange import *
import sys

class DataBinner():

    def __init__(self, dimensions, dimensionAggregateFunctions, nrOfBins, toAggregate, toAggregateFunctions, brushes):
        self.dimensions = dimensions
        self.dimensionAggregateFunctions = dimensionAggregateFunctions
        self.toAggregate = toAggregate
        self.toAggregateFunctions = toAggregateFunctions
        self.nrOfBins = nrOfBins
        self.brushes = brushes
        self.binStructure = None

    def bin(self, df, progress):
        dataMins = [df[dimension].min() for dimension in self.dimensions]
        dataMaxs  = [df[dimension].max() for dimension in self.dimensions]
        binRanges = []
        
        if not self.binStructure is None:
            binRanges = self.binStructure.binRanges
            for idx, val in enumerate(self.dimensions):
                dataMins[idx] = min(binRanges[idx].dataMinValue, dataMins[idx])
                dataMaxs[idx] = max(binRanges[idx].dataMaxValue, dataMaxs[idx])
        else:
            binRanges = self.__createBinRanges(dataMins, dataMaxs, df)
            self.binStructure = self.__initializeBinStructure(binRanges)
            
        for idx, val in enumerate(self.binStructure.binRanges):
            self.binStructure.binRanges[idx] = self.binStructure.binRanges[idx].getUpdatedBinRange(dataMins[idx], dataMaxs[idx], df, self.dimensions[idx])
        
        tempBinStructure = self.__initializeBinStructure(binRanges)
        tempBinStructure.map(self.binStructure)
        self.binStructure = tempBinStructure
        self.__binSamples(df, progress)

    def __binSamples(self, df, progress):
        #b1, b2, overlap, all
        currentBrushes = range(0, len(self.brushes) + 2)
        self.binStructure.initializeBatchAggregation(self.dimensions, self.toAggregate, self.toAggregateFunctions, currentBrushes, df)
    
        brushDataFrames = []
        mt = pd.Series([False] * len(df), index=df.index)
        fu = pd.Series([True] * len(df), index=df.index)
        if len(self.brushes) > 0:
            overlapMask = fu
            brushMasks = []
            
            restMask = fu
            for brush in self.brushes:
                brushMask = fu
                if brush != '':
                    brushMask = df.eval(brush)
                brushMasks.append(brushMask)
                overlapMask = (overlapMask & brushMask)                
                restMask = (restMask & ~brushMask)
            
            if len(self.brushes) <= 1:
                overlapMask = mt
            for brushMask in brushMasks:
                brushDataFrames.append(df[(brushMask & ~overlapMask)])

            brushDataFrames.append(df[overlapMask])
            brushDataFrames.append(df[restMask])
        else:
            brushDataFrames = [df[mt], df]
        
        start = time.time()
        for brushIndex, brushDataFrame in enumerate(brushDataFrames):
            toAggregateBrushes = [brushIndex, currentBrushes[-1]]
            dfDTypes = brushDataFrame.dtypes
            
            if len(brushDataFrame):
                #brushDataFrame['binIndex'] = brushDataFrame.apply(lambda x: self.__createAggregateKey(x), axis=1).astype(object)
                #grouped = brushDataFrame.groupby('binIndex', axis=1)
                #print grouped.first()
                #print grouped.apply(lambda x: len(x), axis=1)
                brushDataFrame.apply(lambda x: self.__runAggregation(x, dfDTypes, toAggregateBrushes, currentBrushes, progress, brushDataFrame), axis=1).astype(object)
                
        end = time.time()
        print (end - start)
        
        self.binStructure.endBatchAggregation(self.dimensions, self.toAggregate, self.toAggregateFunctions, currentBrushes, df, progress)
    
    def __runAggregation(self, row, dTypes, brushes, currentBrushes, progress, brushDataFrame):
        binIndex = []
        for idx, binRange in enumerate(self.binStructure.binRanges):
            value = row[self.dimensions[idx]]
            if isinstance(binRange, NominalBinRange):
                binIndex.append(self.binStructure.binRanges[idx].getIndexFromValue(value))    
            else:
                binIndex.append(self.binStructure.binRanges[idx].getIndex(value))        
        
        maxCount = sys.float_info.max;
        #bin = self.binStructure.bins[row['binIndex'].index]
        bin = self.binStructure.bins[tuple(binIndex)]
        bin.count += 1
        for brushIndex in brushes:
            for idx, aggregate in enumerate(self.toAggregate):
                aggregateFunction = self.toAggregateFunctions[idx]
                aggregateKey = (aggregate, aggregateFunction, brushIndex)
                dType = dTypes[aggregate]
                
                currentValue = bin.values[aggregateKey]
                value = row[aggregate]
                
                if aggregateFunction == 'Max':
                    currentValue = max(currentValue, value)
                    
                elif aggregateFunction == 'Min':
                    currentValue = min(currentValue, value)    
                    
                elif aggregateFunction == 'Sum':
                    currentValue = currentValue + value
                    
                elif aggregateFunction == 'Avg':
                    currentValue = ((currentValue * bin.counts[aggregateKey]) + value) / (bin.counts[aggregateKey] + 1)
                    n = bin.ns[aggregateKey] + 1
                    bin.ns[aggregateKey] = n
                    x = value
                    bin.means[aggregateKey] = bin.means[aggregateKey] + (x - bin.means[aggregateKey])/n
                    bin.powerSumAverage[aggregateKey] = bin.powerSumAverage[aggregateKey] + (x*x - bin.powerSumAverage[aggregateKey])/n
                    if (bin.ns[aggregateKey] - 1 > 0):
                        mean = bin.means[aggregateKey]
                        toSqrt = (bin.powerSumAverage[aggregateKey]*n - n*mean*mean)/(n - 1)
                        bin.sampleStandardDeviations[aggregateKey] = math.sqrt(max(0, toSqrt))
                    
                elif aggregateFunction == 'Count':
                   
                    currentValue = bin.count / progress
                
                bin.values[aggregateKey] = currentValue   
                bin.counts[aggregateKey] += 1.0          
    
    def __createAggregateKey(self, x):
        binIndex = []
        for idx, binRange in enumerate(self.binStructure.binRanges):
            value = x[self.dimensions[idx]]
            if isinstance(binRange, NominalBinRange):
                binIndex.append(self.binStructure.binRanges[idx].getIndexFromValue(value))    
            else:
                binIndex.append(self.binStructure.binRanges[idx].getIndex(value))
        
        return AggregateKey(tuple(binIndex))

        
    def __initializeBinStructure(self, binRanges):
        binStructure = BinStructure(binRanges)
        binStructure.createBins();
        return binStructure;
            
    def __createBinRanges(self, dataMins, dataMaxs, df):
        binRanges = []
        for idx, val in enumerate(self.dimensions):
            if self.dimensionAggregateFunctions[idx] != 'None':
                binRanges.append(AggregatedBinRange.initialize())
            elif df.dtypes[val] == 'float64' or df.dtypes[val] == 'int64':
                binRanges.append(QuantitativeBinRange.initialize(dataMins[idx], dataMaxs[idx], self.nrOfBins[idx], df.dtypes[val] == 'int64'))
            else:
                binRanges.append(NominalBinRange.initialize(df, val))
        return binRanges
 
 
 
        
class BinStructure():

    def __init__(self, binRanges):
        self.binRanges = binRanges     
        self.bins = {}
        self.nullCount = 0
        self.z = 1.96;
    
    def toJson(self):
        data = {
            'binRanges' : [br.__dict__ for br in self.binRanges],
            'bins' : {str(list(k)): v.toJson() for k, v in self.bins.iteritems()},
            'nullCount': self.nullCount
        }
        return data
    
    def createBins(self):
        self.__recursiveCreateBins(self.binRanges, [])
            
    def __recursiveCreateBins(self, previousBinRangesLeft, previousSpans):
        binRangesLeft = []
        binRangesLeft.extend(previousBinRangesLeft);
        
        binRange = binRangesLeft[0];
        del binRangesLeft[0]
        
        for idx, val in enumerate(binRange.getBins()):
            span = Span(val, val, idx)
            spans = []
            spans.extend(previousSpans)
            spans.append(span);
            if len(binRangesLeft) == 0:          
                bin = Bin(spans, tuple([s.index for s in spans]))
                self.bins[bin.binIndex] = bin
            else:
                self.__recursiveCreateBins(binRangesLeft, spans);
    
    def initializeBatchAggregation(self, dimensions, toAggregate, toAggregateFunctions, brushes, df):
        for brushIndex in brushes:
            for idx, aggregate in enumerate(toAggregate):
                aggregateFunction = toAggregateFunctions[idx]
                aggregateKey = (aggregate, aggregateFunction, brushIndex)
                dType = df.dtypes[aggregate]
                
                for bin in self.bins.values():
                    if not aggregateKey in bin.counts:
                        bin.counts[aggregateKey] = 0.0
                        
                        if not aggregateKey in bin.values:
                            if dType == 'float64' or dType == 'int64' or aggregateFunction == 'Count':
                                bin.values[aggregateKey] = 0.0
                            else:
                                bin.values[aggregateKey] = None
                                
                    if not aggregateKey in bin.margins:
                        bin.margins[aggregateKey] = 0
                        bin.marginsAbsolute[aggregateKey] = 0
                        bin.powerSumAverage[aggregateKey] = 0
                        bin.ns[aggregateKey] = 0
                        bin.sampleStandardDeviations[aggregateKey] = 0
                        bin.means[aggregateKey] = 0
                            
    def endBatchAggregation(self, dimensions, toAggregate, toAggregateFunctions, brushes, df, progress):   
        for brushIndex in brushes:
            for idx, aggregate in enumerate(toAggregate):
                aggregateFunction = toAggregateFunctions[idx]
                aggregateKey = (aggregate, aggregateFunction, brushIndex)
                dType = df.dtypes[aggregate]
                
                sumCount = sum([b.counts[aggregateKey] for b in self.bins.values()])
                for bin in self.bins.values():
                    bin.countsInterpolated[aggregateKey] = bin.counts[aggregateKey] / progress
                    totalCountInterpolated = bin.countsInterpolated[aggregateKey]
                    
                    if bin.counts[aggregateKey] > 1:
                        if aggregateFunction == 'Count':
                            toSqrt = (totalCountInterpolated - bin.counts[aggregateKey])/(totalCountInterpolated - 1)                        
                            fpc = math.sqrt(max(0, toSqrt))

                            probability = bin.counts[aggregateKey] / sumCount;
                            margin = ((probability*(1.0 - probability))/ sumCount) * fpc
                            margin = math.sqrt(margin) * self.z
                            if not math.isnan(margin) and not math.isnan(totalCountInterpolated):
                                bin.marginsAbsolute[aggregateKey] = margin*totalCountInterpolated
                                bin.margins[aggregateKey] = margin
                                
                        elif aggregateFunction == 'Avg':
                            fpc = math.sqrt((totalCountInterpolated - bin.counts[aggregateKey]) / (totalCountInterpolated - 1))
                            bin.margins[aggregateKey] = ((bin.sampleStandardDeviations[aggregateKey] / math.sqrt(bin.counts[aggregateKey])) * fpc)*self.z
                            bin.marginsAbsolute[aggregateKey] = ((bin.sampleStandardDeviations[aggregateKey] / math.sqrt(bin.counts[aggregateKey])) * fpc) * self.z
                    
    def aggregate(self, binIndexTuple, dimensions, toAggregate, toAggregateFunctions, brushes, df, row, dfDtypes, progress):
        maxCount = sys.float_info.max;
        bin = self.bins[binIndexTuple]
        bin.count += 1

        #row = df.loc[rowIndex]
        for brushIndex in brushes:
            for idx, aggregate in enumerate(toAggregate):
                aggregateFunction = toAggregateFunctions[idx]
                aggregateKey = (aggregate, aggregateFunction, brushIndex)
                dType = dfDtypes[aggregate]
                        
                currentValue = bin.values[aggregateKey]
                value = row[aggregate]
                
                if aggregateFunction == 'Max':
                    currentValue = max(currentValue, value)
                    
                elif aggregateFunction == 'Min':
                    currentValue = min(currentValue, value)    
                    
                elif aggregateFunction == 'Sum':
                    currentValue = currentValue + value
                    
                elif aggregateFunction == 'Avg':
                    currentValue = ((currentValue * bin.counts[aggregateKey]) + value) / (bin.counts[aggregateKey] + 1)
                    n = bin.ns[aggregateKey] + 1
                    bin.ns[aggregateKey] = n
                    x = value
                    bin.means[aggregateKey] = bin.means[aggregateKey] + (x - bin.means[aggregateKey])/n
                    bin.powerSumAverage[aggregateKey] = bin.powerSumAverage[aggregateKey] + (x*x - bin.powerSumAverage[aggregateKey])/n
                    if (bin.ns[aggregateKey] - 1 > 0):
                        mean = bin.means[aggregateKey]
                        toSqrt = (bin.powerSumAverage[aggregateKey]*n - n*mean*mean)/(n - 1)
                        bin.sampleStandardDeviations[aggregateKey] = math.sqrt(max(0, toSqrt))
                    
                elif aggregateFunction == 'Count':
                    currentValue = bin.count / progress
                
                bin.values[aggregateKey] = currentValue   
                bin.counts[aggregateKey] += 1.0   
                
    def map(self, binStructure):
        self.nullCount += binStructure.nullCount;
        
        for oldBinIndex in binStructure.bins.keys():
            oldBin = binStructure.bins[oldBinIndex];
            newBinIndex = [];

            for d, val in enumerate(self.binRanges):
                newBinIndex.append(self.binRanges[d].getIndex(oldBin.spans[d].min))

            newBin = self.bins[tuple(newBinIndex)];

            if newBin.containsBin(oldBin):
                newBin.map(oldBin)
        
class Bin():
    def __init__(self, spans, binIndex):
        self.spans = spans
        self.binIndex = binIndex
        self.count = 0
        self.counts = {}
        self.values = {}
        self.countsInterpolated = {}
        self.margins = {}
        self.marginsAbsolute = {}
        self.means = {}
        self.powerSumAverage = {}
        self.sampleStandardDeviations = {}
        self.ns = {}
    
    def toJson(self):
        data = {
            'counts' : self.__reHash(self.counts),
            'countsInterpolated' : self.__reHash(self.countsInterpolated),
            'values' : self.__reHash(self.values),
            'margins' : self.__reHash(self.margins),
            'marginsAbsolute' : self.__reHash(self.marginsAbsolute)            
        }
        return data
        
    def __reHash(self, d):
        newD = {}
        for k, v in d.iteritems():
            subD = newD
            for i in range(len(k)):
                subK = k[i]
                if i != len(k)-1:
                    if not subK in subD:
                        tempD = {}
                        subD[subK] = tempD
                    subD = subD[subK]
                else:
                    subD[subK] = v
        return newD
    
    def containsBin(self, bin):
        for d, val in enumerate(self.spans):
            if (not ((bin.spans[d].min >= self.spans[d].min or __aboutEqual(bin.spans[d].min, self.spans[d].min)) and
                     (bin.spans[d].max <= self.spans[d].max or __aboutEqual(bin.spans[d].max, self.spans[d].max)))):
                return False
        return True
    
    def __aboutEqual(x, y):
        epsilon = max(abs(x), abs(y)) * 1E-15;
        return abs(x - y) <= epsilon;
        
    def map(self, bin):
        self.binIndex = bin.binIndex
        self.count = bin.count
        self.spans = bin.spans
        self.counts = bin.counts
        self.values = bin.values
        self.countsInterpolated = bin.countsInterpolated
        self.margins = bin.margins
        self.marginsAbsolute = bin.marginsAbsolute
        self.means = bin.means
        self.powerSumAverage = bin.powerSumAverage
        self.sampleStandardDeviations = bin.sampleStandardDeviations
        self.ns = bin.ns

class AggregateKey():
    def __init__(self,index):
        self.index = index  
    def __eq__(self, other):
        return other and self.index == other.index

    def __ne__(self, other):
        return not self.__eq__(other)

    def __hash__(self):
        return index
        
class Span():
    def __init__(self, min, max, index):
        self.min = min     
        self.max = max  
        self.index = index  
        
    def __str__(self):
        return str(self.__dict__)
    def __unicode__(self):
        return 'u'+str(self.__dict__)
    def __repr__(self):
        return str(self.__dict__)