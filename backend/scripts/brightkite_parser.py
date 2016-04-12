import pandas as pd
from dateutil.parser import parse
import calendar
import random

def weighted_choice(choices):
   total = sum(w for c, w in choices)
   r = random.uniform(0, total)
   upto = 0
   for c, w in choices:
      if upto + w >= r:
         return c
      upto += w
   assert False, "Shouldn't get here"



chunksize = 50000
data  = pd.read_table("C:\\Temp\\test\\Brightkite_totalCheckins.txt", sep="\t", chunksize=chunksize)


lunch_weekend_summer = [('entertainment', 0.15), ('food', 0.7), ('outdoor', 0.4), ('business', 0.05)]
lunch_weekend_winter = [('entertainment', 0.15), ('food', 0.7), ('outdoor', 0.2), ('business', 0.05)]
lunch_work_winter    = [('entertainment', 0.15), ('food', 0.7), ('outdoor', 0.01), ('business', 0.05)]
lunch_work_summer    = [('entertainment', 0.15), ('food', 0.7), ('outdoor', 0.04), ('business', 0.05)]


rest_weekend_summer  = [('entertainment', 0.1), ('food', 0.1), ('outdoor', 0.4), ('business', 0.65)]
rest_weekend_winter  = [('entertainment', 0.1), ('food', 0.1), ('outdoor', 0.2), ('business', 0.65)]
rest_work_winter     = [('entertainment', 0.1), ('food', 0.1), ('outdoor', 0.01), ('business', 0.05)]
rest_work_summer     = [('entertainment', 0.1), ('food', 0.1), ('outdoor', 0.04), ('business', 0.05)]

dinner_weekend_summer = [('entertainment', 0.35), ('food', 0.7), ('outdoor', 0.3), ('business', 0.06)]
dinner_weekend_winter = [('entertainment', 0.35), ('food', 0.7), ('outdoor', 0.1), ('business', 0.06)]
dinner_work_winter    = [('entertainment', 0.35), ('food', 0.7), ('outdoor', 0.01), ('business', 0.06)]
dinner_work_summer    = [('entertainment', 0.35), ('food', 0.7), ('outdoor', 0.04), ('business', 0.06)]

new_rows = []

for df in data:
    df.columns = ['user', 'ts', 'lat' , 'long', 'location_id']
    for r in df.itertuples():
        print r
        dt = parse(r[2])    
        
        time_of_day = 2
        if dt.hour >= 11 and dt.hour <= 14:
            time_of_day = 0
        elif dt.hour >= 18 and dt.hour <= 23:
            time_of_day = 1
           
        weekend = True
        if dt.weekday() >= 0 and dt.weekday() <= 4:
            weekend = False
        
        toChooseFrom = []
        # summer
        if dt.month > 5 and dt.month < 9:
            if time_of_day == 0:
                if weekend:
                    toChooseFrom = lunch_weekend_summer
                else:
                    toChooseFrom = lunch_work_summer
            elif time_of_day == 1:
                if weekend:
                        toChooseFrom = dinner_weekend_summer
                else:
                    toChooseFrom = dinner_work_summer
            else:
                if weekend:
                        toChooseFrom = rest_weekend_summer
                else:
                    toChooseFrom = rest_work_summer
        # winter
        else:
            if time_of_day == 0:
                if weekend:
                    toChooseFrom = lunch_weekend_winter
                else:
                    toChooseFrom = lunch_work_winter
            elif time_of_day == 1:
                if weekend:
                        toChooseFrom = dinner_weekend_winter
                else:
                    toChooseFrom = dinner_work_winter
            else:
                if weekend:
                        toChooseFrom = rest_weekend_winter
                else:
                    toChooseFrom = rest_work_winter
        
        #['user', 'ts', 'lat' , 'long', 'location_id']
        #lat, long, year, month, day, hour, minute, type
        newRow = [r[3], r[4], dt.year, calendar.month_abbr[dt.month], calendar.day_abbr[dt.weekday()], dt.hour, dt.minute, weighted_choice(toChooseFrom)]
        #print newRow
        new_rows.append(newRow)

    
random.shuffle(new_rows)
with open("C:\\Temp\\test\\out.txt", 'w') as f:
    f.write('lat,long,year,month,day,hour, minute,type\n')
    for line in new_rows:
        f.write(','.join(map(str, line))  + '\n')

