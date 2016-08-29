import sys
import random

if __name__ == '__main__':
    flist = []
    with open(sys.argv[1], 'r') as f:
        for line in f:
            flist.append(line.strip())
        
    print float(sys.argv[2])
    print len(flist)
    while len(flist) < float(sys.argv[2]):
        flist.append(random.choice(flist))
        
    random.shuffle(flist)
    with open(sys.argv[1]+'_c', 'w') as f:
        f.write('mpg,cyl,dis,hp,weight,acc,year,origin\n')
        for line in flist:
            f.write(line + '\n')