import requests
import time
import json

def run(command):
    #print command
    url = 'http://localhost:8080'
    #url = 'http://10.116.60.7:8080'
    headers = {'Content-Type':'application/json'}
    t = time.time()
    r = requests.post(url, headers=headers, data=command)
    j = None
    if r.text != 'None':
        j = json.loads(r.text)
    elapsed = time.time() - t;
    print 'roundtrip time : ', elapsed, 'status : ', r.status_code
    return j

def load(schema_file, data_file, name):
    cmd = json.dumps({'type':'execute','task':{'type':'load','name':name,'schema_file':schema_file, 'data_file':data_file}})
    return run(cmd)

def load2(data_file, name):
    cmd = json.dumps({'type':'execute','task':{'type':'load','name':name,'data_file':data_file}})
    return run(cmd)    
    

def select(source, predicate):
    cmd = json.dumps({'type':'execute','task':{'type':'select','source':source,'predicate':predicate}})
    return run(cmd)

def project(source, attributes):
    cmd = json.dumps({'type':'execute','task':{'type':'project','source':source,'attributes':attributes}})
    return run(cmd)

def frequent(source, support):
    cmd = json.dumps({'type':'execute','task':{'type':'frequent_itemsets','source':source, 'support':support}})
    return run(cmd)

def union(sources):
    cmd = json.dumps({'type':'execute','task':{'type':'union','sources':sources}})
    return run(cmd)

def intersect(sources):
    cmd = json.dumps({'type':'execute','task':{'type':'intersect','sources':sources}})
    return run(cmd)
    
def correlate(source):
    #cmd = json.dumps({'type':'execute','task':{'type':'correlate','source':source}})
    cmd = json.dumps({"type":"execute","task":{"type":"correlate","source":source}})
    
    
    return run(cmd)

def classify(label, features):
    cmd = json.dumps({'type':'execute','task':{'type':'classify','params':{},'classifier':'svm','label':label,'features':features}})
    return run(cmd)

def lookup(uuid, page_size, page_num):
    cmd = json.dumps({'type':'lookup','uuid':uuid,'page_size':page_size,'page_num':page_num})
    return run(cmd)

def catalog():
    cmd = json.dumps({'type':'catalog'})
    return run(cmd)

def tasks():
    cmd = json.dumps({'type':'tasks'})
    return run(cmd)

'''
curl -H "Content-Type: application/json" -X POST -d '{"type":"execute","task":{"type":"load","schema_file":"/data/demo_data/mimic2/mimic2_schema.json","data_file":"/data/demo_data/mimic2/mimic2.csv"}}' localhost:8080

curl -H "Content-Type: application/json" -X POST -d '{"type":"execute","task":{"type":"select","source":"0","predicate":"age>18"}}' localhost:8080

curl -H "Content-Type: application/json" -X POST -d '{"type":"execute","task":{"type":"project","source":"1","attributes":["metabolic"]}}' localhost:8080

curl -H "Content-Type: application/json" -X POST -d '{"type":"execute","task":{"type":"project","source":"1","attributes":["sex","height"]}}' localhost:8080

curl -H "Content-Type: application/json" -X POST -d '{"type":"execute","task":{"type":"classify","params":{},"classifier":"naive_bayes","labels":"2", "features":"3",}}' localhost:8080
'''

# get the current files loaded in the server
#curl -H "Content-Type: application/json" -X POST -d '{"type":"catalog"}' http://10.116.60.7:8080

# get the data for a uuid from the cache (uuid = 100)
#curl -H "Content-Type: application/json" -X POST -d '{"type":"lookup", "args":{"uuid":"3"}}' http://localhost:8080

# do a project on uuid 100 and store result in uuid 0
#curl -H "Content-Type: application/json" -X POST -d '{"type":"execute","args":{"task":{"uuid":"1", "type":"project", "args":{"source":"0", "attributes":["age"]}}}}' http://localhost:8080

# do a project on uuid 100 and store result in uuid 1
#curl -H "Content-Type: application/json" -X POST -d '{"type":"execute","args":{"task":{"uuid":"2", "type":"project", "args":{"source":"0", "attributes":["metabolic"]}}}}' http://localhost:8080

# classify using age as a feature (uuid 0) and metabolic as labels (uuid 1).  store result to uuid = 2
#curl -H "Content-Type: application/json" -X POST -d '{"type":"execute", "args":{"task":{"uuid":"3","type":"classify", "args":{"classifier": {"type":"logistic_regression", "params":{},"labels":"2", "features":"1"}}}}}' http://localhost:8080

# lookup result of classify
#curl -H "Content-Type: application/json" -X POST -d '{"type":"lookup", "args":{"uuid":"2"}}' http://10.116.60.7:8080

# do a select on the base data (uuid = 100) for people who are 21 and store result in uuid 5
#curl -H "Content-Type: application/json" -X POST -d '{"type":"execute", "task":{"uuid":"5","type":"select", "args":{"data":"0", "predicate":"((age > 20) and (age < 22))"}}}' http://10.116.60.7:8080

#print load('examples//mimic2_schema.json', 'C://ez_projects//data//mimic2.csv')
print load('examples//mimic2_schema_old.json', 'examples//mimic2_new.csv', 'mimic')
print load2('examples//tweets_copy.txt', 'tweets')
#time.sleep(1)
print catalog()

#print lookup("1",2000, 1)
#classifiy = classify(label, feats)['uuid']
#classifiy = classify(label, feats)['uuid']
#time.sleep(2)

print tasks()

label = project('0', ["neurologic", "digestive"])['uuid']

#feats = project('0', ['age', 'sex'])['uuid']
feats = project('0',  ['blood'])['uuid']

#feats = '0'

#feats = project(feats, ['metabolic'])['uuid']
#print "ii"
cc = correlate(label)['uuid']
#time.sleep(1)
#print cc
print lookup(cc, 10, 0)


#print lookup('0', 100, 10)
#print project(1, 0, ['metabolic'])
#print project(2, 0, ['age','sex'])

#print lookup(3, 10, 1)
#$base_uuid = run(catalog_com)['mimic2']['uuid']
#print base_uuid

#project(1, base_uuid, ['age'])
#project(2, base_uuid, ['age', 'sex'])
#project(3, base_uuid, ['sex'])

#union(4, [1,2])
#print lookup(4, 100, 0)

#union(5, [1,3])
#print lookup(5, 100, 0)


#f = open('C:\\Temp\\test.txt','w')
#for c in j:
#f.write(r.text)
#f.flush()
#f.close()
