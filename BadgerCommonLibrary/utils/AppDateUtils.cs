using System;

namespace BadgerCommonLibrary.utils
{
    public static class AppDateUtils
    {
        public static TimeSpan DecallageDtNowTimeSpan { get; private set; }

        public static DateTime DtNow()
        {
            return DateTime.Now - DecallageDtNowTimeSpan;
        }

        public static void ForceDtNow(DateTime? dtTime)
        {
            if (dtTime.HasValue)
            {
                DecallageDtNowTimeSpan = DateTime.Now - dtTime.Value;
            }
            else
            {
                DecallageDtNowTimeSpan = TimeSpan.Zero;
            }
        }



        public static DateTime ChangeDate(this DateTime dateTime, DateTime newDate)
        {
            return new DateTime(
                newDate.Year,
                newDate.Month,
                newDate.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond,
                dateTime.Kind);
        }



        public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }

        public static DateTime ChangeTime(this DateTime dateTime, TimeSpan ts)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                ts.Hours,
                ts.Minutes,
                ts.Seconds,
                ts.Milliseconds,
                dateTime.Kind);
        }

        public static DateTime AtSec(this DateTime dateTime, int seconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                seconds,
                dateTime.Millisecond,
                dateTime.Kind);
        }



        public static DateTime WithLastDayOfMonth(this DateTime date)
        {

            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

        }

        public static DateTime WithFirstDayOfMonth(this DateTime date)
        {

            return new DateTime(date.Year, date.Month, 1);

        }


    }
}
