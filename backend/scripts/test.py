from multiprocessing import Pool, TimeoutError
import time
import os

def f(x):
    time.sleep(1)
    return x

if __name__ == "__main__":
    pool = Pool(processes=4)
    while True:
        for i in pool.imap_unordered(f, range(10)):
            print "tr", i