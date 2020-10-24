using System;
using Badger2018.constants;
using Badger2018.dto;
using BadgerCommonLibrary.utils;

namespace Badger2018.utils
{
    public static class TimesUtils
    {

        public static DateTime GetDateTimeEndTravTheorique(DateTime startTime, AppOptions appOptions, EnumTypesJournees tyJournee)
        {
            DateTime retDt = startTime;

            Action<DateTime, AppOptions> actionsCommunes = (dateTimeRet, locAppOption) =>
            {
                // Retire 5 min au temps travaillé.
                if (appOptions.IsAdd5minCpt)
                {
                    retDt -= new TimeSpan(0, 5, 0);
                }
            };


            // Si le type de journée est une journée complète
            if (EnumTypesJournees.Complete == tyJournee)
            {              
                retDt = startTime + appOptions.TempsDemieJournee + appOptions.TempsDemieJournee + appOptions.TempsMinPause;
                actionsCommunes(retDt, appOptions);
                if (retDt.TimeOfDay.CompareTo(appOptions.PlageFixeApremFin) < 0)
                {
                    retDt = retDt.ChangeTime(appOptions.PlageFixeApremFin);
                }
                return retDt;
            }

            if (EnumTypesJournees.ApresMidi == tyJournee)
            {
                retDt = startTime + appOptions.TempsDemieJournee;
                actionsCommunes(retDt, appOptions);
                if (retDt.TimeOfDay.CompareTo(appOptions.PlageFixeApremFin) < 0)
                {
                    retDt = retDt.ChangeTime(appOptions.PlageFixeApremFin);
                }
                return retDt;
            }


            // Matin
            retDt = startTime + appOptions.TempsDemieJournee;
            actionsCommunes(retDt, appOptions);
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


        public static TimeSpan GetTempsTravaille(DateTime now, int etatBadger, TimesBadgerDto times, AppOptions appOptions, EnumTypesJournees tyJournee, bool isGetPausesInCalcul, ref bool isMaxDepass)
        {

            TimeSpan retTsTpsTrav = TimeSpan.Zero;

            if (etatBadger == 0)
            {
                retTsTpsTrav = now.TimeOfDay - times.PlageTravMatin.Start.TimeOfDay;

                if (isGetPausesInCalcul)
                {
                    retTsTpsTrav -= times.GetTpsPause();
                }

                if (appOptions.IsStopCptAtMaxDemieJournee && retTsTpsTrav.CompareTo(appOptions.TempsMaxDemieJournee) >= 0)
                {
                    retTsTpsTrav = appOptions.TempsMaxDemieJournee;
                    isMaxDepass = true;
                }


            }
            else if (etatBadger == 1)
            {
                retTsTpsTrav = times.PlageTravMatin.EndOrDft.TimeOfDay - times.PlageTravMatin.Start.TimeOfDay;
                if (isGetPausesInCalcul)
                {
                    retTsTpsTrav -= times.GetTpsPause();
                }
                if (appOptions.IsStopCptAtMaxDemieJournee && retTsTpsTrav.CompareTo(appOptions.TempsMaxDemieJournee) >= 0)
                {
                    retTsTpsTrav = appOptions.TempsMaxDemieJournee;
                    isMaxDepass = true;
                }


            }
            else if (etatBadger == 2)
            {
                TimeSpan matin = TimeSpan.Zero;
                TimeSpan current = TimeSpan.Zero;

                if (tyJournee == EnumTypesJournees.Complete)
                {
                    current = now.TimeOfDay;

                    if (appOptions.TempsMinPause.CompareTo(times.PlageTravAprem.Start.TimeOfDay - times.PlageTravMatin.EndOrDft.TimeOfDay) > 0)
                    {
                        current -= (times.PlageTravMatin.EndOrDft.TimeOfDay + appOptions.TempsMinPause);
                    }
                    else
                    {
                        current -= times.PlageTravAprem.Start.TimeOfDay;
                    }

                }

                if (tyJournee == EnumTypesJournees.Complete)
                {
                    matin = times.PlageTravMatin.EndOrDft.TimeOfDay - times.PlageTravMatin.Start.TimeOfDay;
                }
                else if (EnumTypesJournees.IsDemiJournee(tyJournee))
                {
                    current = now.TimeOfDay - times.PlageTravMatin.Start.TimeOfDay;
                }

                if (appOptions.IsStopCptAtMaxDemieJournee && matin.CompareTo(appOptions.TempsMaxDemieJournee) >= 0)
                {
                    matin = appOptions.TempsMaxDemieJournee;
                }
                if (appOptions.IsStopCptAtMaxDemieJournee && current.CompareTo(appOptions.TempsMaxDemieJournee) >= 0)
                {
                    current = appOptions.TempsMaxDemieJournee;
                    isMaxDepass = true;
                }

                retTsTpsTrav = current + matin;
                if (isGetPausesInCalcul)
                {
                    retTsTpsTrav -= times.GetTpsPause();
                }

            }
            else if (etatBadger == 3)
            {

                if (tyJournee == EnumTypesJournees.Complete)
                {
                    TimeSpan matin = times.PlageTravMatin.EndOrDft.TimeOfDay - times.PlageTravMatin.Start.TimeOfDay;
                    TimeSpan aprem = times.PlageTravAprem.EndOrDft.TimeOfDay;

                    if (appOptions.TempsMinPause.CompareTo(times.PlageTravAprem.Start.TimeOfDay - times.PlageTravMatin.EndOrDft.TimeOfDay) > 0)
                    {
                        aprem -= (times.PlageTravMatin.EndOrDft.TimeOfDay + appOptions.TempsMinPause);
                    }
                    else
                    {
                        aprem -= times.PlageTravAprem.Start.TimeOfDay;
                    }




                    if (appOptions.IsStopCptAtMaxDemieJournee && matin.CompareTo(appOptions.TempsMaxDemieJournee) >= 0)
                    {
                        matin = appOptions.TempsMaxDemieJournee;
                    }
                    if (appOptions.IsStopCptAtMaxDemieJournee && aprem.CompareTo(appOptions.TempsMaxDemieJournee) >= 0)
                    {
                        aprem = appOptions.TempsMaxDemieJournee;
                    }

                    retTsTpsTrav = aprem + matin;
                    if (isGetPausesInCalcul)
                    {
                        retTsTpsTrav -= times.GetTpsPause();
                    }

                }
                else
                {
                    retTsTpsTrav = times.PlageTravAprem.EndOrDft.TimeOfDay - times.PlageTravMatin.Start.TimeOfDay;
                }
            }

            if (appOptions.IsAdd5minCpt && !isMaxDepass)
            {
                retTsTpsTrav = retTsTpsTrav + new TimeSpan(0, 5, 0);
            }


            if (appOptions.IsStopCptAtMax && retTsTpsTrav.CompareTo(appOptions.TempsMaxJournee) >= 0)
            {
                retTsTpsTrav = appOptions.TempsMaxJournee;
                isMaxDepass = true;
            }

            return retTsTpsTrav;
        }

        //public TimeSpan parseTimeSpanStringWithTags()

    }
}
