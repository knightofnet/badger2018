using System;

namespace BadgerCommonLibrary.utils
{
    public static class AppDateUtils
    {

        private static readonly String[] _monthsFr = new string[] { "janvier", "février", "mars", "avril", "mai", "juin", "juillet", "aout", "septembre", "octobre", "novembre", "décembre" };

        public static TimeSpan DecallageDtNowTimeSpan { get; private set; }

        public static DateTime DtNow()
        {
            return (DateTime.Now - DecallageDtNowTimeSpan).NoLowerThanSec();
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


        public static DateTime Clone(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond,
                dateTime.Kind);
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

        public static DateTime NoLowerThanSec(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                0,
                0);
        }

        public static TimeSpan AtSec(this TimeSpan ts, int seconds)
        {
            return new TimeSpan(ts.Hours, ts.Minutes, seconds);
        }




        public static DateTime WithLastDayOfMonth(this DateTime date)
        {

            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

        }

        public static string StrDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "lundi";
                case DayOfWeek.Tuesday:
                    return "mardi";
                case DayOfWeek.Wednesday:
                    return "mercredi";
                case DayOfWeek.Thursday:
                    return "jeudi";
                case DayOfWeek.Friday:
                    return "vendredi";
                case DayOfWeek.Saturday:
                    return "samedi";
                case DayOfWeek.Sunday:
                    return "dimanche";
            }

            return null;
        }

        public static DateTime WithFirstDayOfMonth(this DateTime date)
        {

            return new DateTime(date.Year, date.Month, 1);

        }

        public static DateTime WithFirstDayOfWeek(this DateTime date)
        {


            return date.AddDays(DayOfWeek.Monday - date.DayOfWeek);

        }


        public static string StrMonthOfYear(int month)
        {
            int m = (month - 1) % 12;
            return _monthsFr[m];
        }
    }
}
