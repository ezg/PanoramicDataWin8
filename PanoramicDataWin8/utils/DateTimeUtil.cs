using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.utils
{
    public class DateTimeUtil
    {
        private static List<DateTimeStep> _dateTimeSteps = new List<DateTimeStep>(
            new DateTimeStep[] 
            {
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Second, DateTimeStepValue = 1, DateTimeStepMaxValue = TimeSpan.FromSeconds(1).Ticks }, // 1-second
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Second, DateTimeStepValue  = 5, DateTimeStepMaxValue = TimeSpan.FromSeconds(5).Ticks }, // 5-second
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Second, DateTimeStepValue  = 15, DateTimeStepMaxValue = TimeSpan.FromSeconds(15).Ticks },  // 15-second
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Second, DateTimeStepValue  = 30, DateTimeStepMaxValue = TimeSpan.FromSeconds(30).Ticks },  // 30-second
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Minute, DateTimeStepValue  = 1, DateTimeStepMaxValue = TimeSpan.FromMinutes(1).Ticks },  // 1-minute
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Second, DateTimeStepValue  = 5, DateTimeStepMaxValue = TimeSpan.FromMinutes(5).Ticks }, // 5-minute
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Second, DateTimeStepValue  = 15, DateTimeStepMaxValue = TimeSpan.FromMinutes(15).Ticks }, // 15-minute
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Second, DateTimeStepValue  = 30, DateTimeStepMaxValue = TimeSpan.FromMinutes(30).Ticks }, // 30-minute
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Hour, DateTimeStepValue  = 1, DateTimeStepMaxValue = TimeSpan.FromHours(1).Ticks }, // 1-hour
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Hour, DateTimeStepValue  = 3, DateTimeStepMaxValue = TimeSpan.FromHours(3).Ticks }, // 3-hour
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Hour, DateTimeStepValue  = 6, DateTimeStepMaxValue = TimeSpan.FromHours(6).Ticks }, // 6-hour
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Hour, DateTimeStepValue  = 12, DateTimeStepMaxValue = TimeSpan.FromHours(12).Ticks }, // 12-hour
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Day, DateTimeStepValue  = 1, DateTimeStepMaxValue = TimeSpan.FromDays(1).Ticks }, // 1-day
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Day, DateTimeStepValue  = 2, DateTimeStepMaxValue = TimeSpan.FromDays(2).Ticks }, // 2-day
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Day, DateTimeStepValue  = 7, DateTimeStepMaxValue = TimeSpan.FromDays(7).Ticks }, // 1-week
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Month, DateTimeStepValue  = 1, DateTimeStepMaxValue = TimeSpan.FromDays(31).Ticks }, // 1-month
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Month, DateTimeStepValue  = 3, DateTimeStepMaxValue = TimeSpan.FromDays(31 * 3).Ticks }, // 3-month
                new DateTimeStep() {DateTimeStepGranularity = DateTimeStepGranularity.Year, DateTimeStepValue  = 1, DateTimeStepMaxValue = TimeSpan.FromDays(31 * 12).Ticks } // 1-year
            });

        public static DateTime RoundDateTimeTo(long ticks, DateTimeStep dateTimeStep)
        {
            return RoundDateTimeTo(new DateTime(ticks), dateTimeStep);
        }

        public static DateTime RoundDateTimeTo(DateTime dt, DateTimeStep dateTimeStep)
        {
            if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Second)
            {
                return new DateTime((long)Math.Floor((double)(dt.Ticks / TimeSpan.FromSeconds(dateTimeStep.DateTimeStepValue).Ticks)) * TimeSpan.FromSeconds(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Minute)
            {
                return new DateTime((long)Math.Floor((double)(dt.Ticks / TimeSpan.FromMinutes(dateTimeStep.DateTimeStepValue).Ticks)) * TimeSpan.FromMinutes(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Hour)
            {
                return new DateTime((long)Math.Floor((double)(dt.Ticks / TimeSpan.FromHours(dateTimeStep.DateTimeStepValue).Ticks)) * TimeSpan.FromHours(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Day)
            {
                return new DateTime((long)Math.Floor((double)(dt.Ticks / TimeSpan.FromDays(dateTimeStep.DateTimeStepValue).Ticks)) * TimeSpan.FromDays(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Month)
            {
                if (dateTimeStep.DateTimeStepValue == 1)
                {
                    return new DateTime(dt.Year, dt.Month, 1);
                }
                else
                {
                    var ttt = (int)(Math.Floor((dt.Month - 1) / dateTimeStep.DateTimeStepValue) * dateTimeStep.DateTimeStepValue) + 1;
                    return new DateTime(dt.Year, (int)(Math.Floor((dt.Month - 1) / dateTimeStep.DateTimeStepValue) * dateTimeStep.DateTimeStepValue) + 1, 1);
                }
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Year)
            {
                return new DateTime(dt.Year, 1, 1);
            }
            else
            {
                return DateTime.Now;
            }
        }

        public static DateTime IncludeDateTime(double from, double to, DateTimeStep dateTimeStep, bool inclusive, out int stepsTaken)
        {
            return IncludeDateTime(new DateTime((long)from), new DateTime((long)to), dateTimeStep, inclusive, out stepsTaken);
        }

        public static DateTime IncludeDateTime(DateTime from, DateTime to, DateTimeStep dateTimeStep, bool inclusive, out int stepsTaken)
        {
            stepsTaken = 0;
            DateTime min = from < to ? from : to;
            DateTime max = from > to ? from : to;

            DateTime current = new DateTime(min.Ticks);

            if (inclusive)
            {
                while (current <= max)
                {
                    if (from <= to)
                    {
                        current = AddToDateTime(current, dateTimeStep);
                    }
                    else
                    {
                        current = RemoveFromDateTime(current, dateTimeStep);
                    }
                    stepsTaken++;
                }
            }
            else
            {
                while (current < max)
                {
                    if (from < to)
                    {
                        current = AddToDateTime(current, dateTimeStep);
                    }
                    else
                    {
                        current = RemoveFromDateTime(current, dateTimeStep);
                    }
                    stepsTaken++;
                }
            }
            return current;
        }

        public static DateTime RemoveFromDateTime(double ticks, DateTimeStep dateTimeStep)
        {
            return RemoveFromDateTime(new DateTime((long)ticks), dateTimeStep);
        }

        public static DateTime RemoveFromDateTime(DateTime dt, DateTimeStep dateTimeStep)
        {
            return AddToDateTime(dt, new DateTimeStep()
            {
                DateTimeStepGranularity = dateTimeStep.DateTimeStepGranularity,
                DateTimeStepValue = -dateTimeStep.DateTimeStepValue
            });
        }

        public static DateTime AddToDateTime(double ticks, DateTimeStepGranularity granularity, double value)
        {
            return AddToDateTime(new DateTime((long)ticks), new DateTimeStep() { DateTimeStepGranularity = granularity, DateTimeStepValue = value });
        }

        public static DateTime AddToDateTime(double ticks, DateTimeStep dateTimeStep)
        {
            return AddToDateTime(new DateTime((long)ticks), dateTimeStep);
        }

        public static DateTime AddToDateTime(DateTime dt, DateTimeStep dateTimeStep)
        {
            if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Second)
            {
                return new DateTime(dt.Ticks + TimeSpan.FromSeconds(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Minute)
            {
                return new DateTime(dt.Ticks + TimeSpan.FromMinutes(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Hour)
            {
                return new DateTime(dt.Ticks + TimeSpan.FromHours(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Day)
            {
                return new DateTime(dt.Ticks + TimeSpan.FromDays(dateTimeStep.DateTimeStepValue).Ticks);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Month)
            {
                return dt.AddMonths((int)dateTimeStep.DateTimeStepValue);
            }
            else if (dateTimeStep.DateTimeStepGranularity == DateTimeStepGranularity.Year)
            {
                return dt.AddYears((int)dateTimeStep.DateTimeStepValue);
            }
            else
            {
                return DateTime.Now;
            }
        }

        public static long[] GetDataTimeExtent(double min, double max, double m, out DateTimeStep dateTimeStep)
        {
            return GetDataTimeExtent(new DateTime((long)min), new DateTime((long)max), m, out dateTimeStep);
        }

        public static long[] GetDataTimeExtent(DateTime min, DateTime max, double m, out DateTimeStep dateTimeStep)
        {
            double span = max.Ticks - min.Ticks;
            double target = span / m;

            int count = 0;
            int i = _dateTimeSteps.Count - 1;
            double lastStepValue = 0;
            foreach (var step in _dateTimeSteps)
            {
                if (target > lastStepValue && target < step.DateTimeStepMaxValue)
                {
                    i = count;
                    break;
                }
                lastStepValue = step.DateTimeStepMaxValue;
                count++;
            }

            long[] ret = new long[2];
            ret[0] = RoundDateTimeTo(min, _dateTimeSteps[i]).Ticks;
            ret[1] = AddToDateTime(RoundDateTimeTo(max, _dateTimeSteps[i]), _dateTimeSteps[i]).Ticks;
            dateTimeStep = _dateTimeSteps[i];
            return ret;
        }
    }

    public enum DateTimeStepGranularity { Second, Minute, Hour, Day, Month, Year }

    public class DateTimeStep
    {
        public DateTimeStepGranularity DateTimeStepGranularity { get; set; }
        public double DateTimeStepValue { get; set; }
        public double DateTimeStepMaxValue { get; set; }
    }
}
