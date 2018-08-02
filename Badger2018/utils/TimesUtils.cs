using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Badger2018.constants;
using Badger2018.dto;

namespace Badger2018.utils
{
    public static class TimesUtils
    {

        public static DateTime GetDateTimeEndTravTheorique(DateTime startTime, AppOptions appOptions, EnumTypesJournees tyJournee)
        {
            DateTime retDt = startTime;

            if (EnumTypesJournees.Complete == tyJournee)
            {

                retDt = startTime + appOptions.TempsDemieJournee + appOptions.TempsDemieJournee + appOptions.TempsMinPause;
                if (retDt.TimeOfDay.CompareTo(appOptions.PlageFixeApremFin) < 0)
                {
                    retDt = retDt.ChangeTime(appOptions.PlageFixeApremFin);
                }
                return retDt;
            }

            if (EnumTypesJournees.ApresMidi == tyJournee)
            {
                retDt = startTime + appOptions.TempsDemieJournee;
                if (retDt.TimeOfDay.CompareTo(appOptions.PlageFixeApremFin) < 0)
                {
                    retDt = retDt.ChangeTime(appOptions.PlageFixeApremFin);
                }
                return retDt;
            }

            retDt = startTime + appOptions.TempsDemieJournee;
            if (retDt.TimeOfDay.CompareTo(appOptions.PlageFixeMatinFin) < 0)
            {
                retDt = retDt.ChangeTime(appOptions.PlageFixeMatinFin);
            }
            return retDt;


        }

        public static TimeSpan GetTimeEndTravTheorique(DateTime startTime, AppOptions appOptions, EnumTypesJournees tyJournee)
        {
            return GetDateTimeEndTravTheorique(startTime, appOptions, tyJournee).TimeOfDay;
        }

        // Donne le temps de travaille théorique pour une journée
        public static TimeSpan GetTpsTravTheoriqueOneDay(AppOptions appOptions, EnumTypesJournees tyJournees)
        {
            TimeSpan tTravTheo = appOptions.TempsDemieJournee;
            if (tyJournees == EnumTypesJournees.Complete)
            {
                tTravTheo += appOptions.TempsDemieJournee;
            }
            return tTravTheo;
        }





    }
}
