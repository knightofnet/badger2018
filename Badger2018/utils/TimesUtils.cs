using System;
using System.Collections.Generic;
using System.Linq;
using AryxDevLibrary.extensions;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using BadgerCommonLibrary.utils;

namespace Badger2018.utils
{
    public static class TimesUtils
    {

        private static readonly Dictionary<string, DateTime> _cacheDatesByKeys = new Dictionary<string, DateTime>(2);

        public static DateTime TransformDateTimeWith(DateTime dateTime, TimeSpan minSpan, TimeSpan minusTs,
            bool minusBeforeMin = true)
        {

            String keyCache = "TransformDateTimeWith#" + dateTime.ToString("O") + minSpan + minusTs +
                              minusBeforeMin;

            // Retourne la date mise en cache si existe (evite les recalculs)
            if (_cacheDatesByKeys.ContainsKey(keyCache))
            {
                return _cacheDatesByKeys[keyCache];
            }

            DateTime retDateTime = dateTime.Clone();

            if (minusBeforeMin)
            {
                retDateTime -= minusTs;
                if (retDateTime.TimeOfDay.CompareTo(minSpan) < 0)
                {
                    retDateTime = AppDateUtils.ChangeTime(retDateTime, minSpan);
                }
            }
            else
            {
                if (retDateTime.TimeOfDay.CompareTo(minSpan) < 0)
                {
                    retDateTime = AppDateUtils.ChangeTime(retDateTime, minSpan);
                }
                retDateTime -= minusTs;
            }

            _cacheDatesByKeys.Add(keyCache, retDateTime);
            return retDateTime;

        }

        public static DateTime GetDateTimeEndTravTheoriqueBis(DateTime startTime, AppOptions appOptions, EnumTypesJournees tyJournee)
        {

            String keyCache = "GetDateTimeEndTravTheoriqueBis#" + startTime.ToString("O") + appOptions.TempsDemieJournee + appOptions.TempsMinPause +
                              tyJournee.Index;

            // Retourne la date mise en cache si existe (evite les recalculs)
            if (_cacheDatesByKeys.ContainsKey(keyCache))
            {
                return _cacheDatesByKeys[keyCache];
            }

            DateTime retDt = new DateTime();
            // Si le type de journée est une journée complète
            if (EnumTypesJournees.Complete == tyJournee)
            {
                retDt = startTime + appOptions.TempsDemieJournee + appOptions.TempsDemieJournee + appOptions.TempsMinPause;
                //return retDt;
            }

            if (EnumTypesJournees.ApresMidi == tyJournee)
            {
                retDt = startTime + appOptions.TempsDemieJournee;
                //return retDt;
            }

            if (EnumTypesJournees.Matin == tyJournee)
            {
                // Matin
                retDt = startTime + appOptions.TempsDemieJournee;
            }

            _cacheDatesByKeys.Add(keyCache, retDt);
            return retDt;

        }

        [Obsolete("Use GetDateTimeEndTravTheoriqueBis()")]
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
                    retDt = AppDateUtils.ChangeTime(retDt, appOptions.PlageFixeApremFin);
                }
                return retDt;
            }

            if (EnumTypesJournees.ApresMidi == tyJournee)
            {
                retDt = startTime + appOptions.TempsDemieJournee;

                actionsCommunes(retDt, appOptions);

                if (retDt.TimeOfDay.CompareTo(appOptions.PlageFixeApremFin) < 0)
                {
                    retDt = AppDateUtils.ChangeTime(retDt, appOptions.PlageFixeApremFin);
                }
                return retDt;
            }


            // Matin
            retDt = startTime + appOptions.TempsDemieJournee;

            actionsCommunes(retDt, appOptions);

            if (retDt.TimeOfDay.CompareTo(appOptions.PlageFixeMatinFin) < 0)
            {
                retDt = AppDateUtils.ChangeTime(retDt, appOptions.PlageFixeMatinFin);
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

        public static DateTime GetMaxDateTimeForOneDay(DateTime endTheoDateTime, AppOptions prgOptions, EnumTypesJournees typeJournee)
        {
            TimeSpan maxTpsAutorise = prgOptions.TempsMaxJournee - (prgOptions.TempsDemieJournee + prgOptions.TempsDemieJournee);
            TimeSpan tsFinJourneeReglementaire = endTheoDateTime.TimeOfDay + maxTpsAutorise -
                                                 (prgOptions.IsAdd5minCpt ? new TimeSpan(0, 5, 0) : TimeSpan.Zero);

            return DateUtilsExt.ChangeTime(AppDateUtils.DtNow(), tsFinJourneeReglementaire);

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


        public static DateTime ClassicTransform(DateTime dateTime, AppOptions prgOptions, EnumTypesJournees typeJournee)
        {
            return TransformDateTimeWith(dateTime, EnumTypesJournees.ApresMidi.Equals(typeJournee) || EnumTypesJournees.Complete.Equals(typeJournee) ?
                prgOptions.PlageFixeApremFin : prgOptions.PlageFixeMatinFin,
                prgOptions.IsAdd5minCpt ? new TimeSpan(0, 5, 0) : TimeSpan.Zero);
        }


        public static TimesBadgerDto HydrateTimesLogicalOrder(EnumTypesJournees typesJournees, int etatBadger, List<BadgeageEntryDto> badgeageEntryDtos)
        {
            DateTime dftDt = DateTime.MinValue;
            dftDt = AppDateUtils.ChangeTime(dftDt, TimeSpan.Zero);
            TimesBadgerDto retTimes = new TimesBadgerDto();

            if (typesJournees == EnumTypesJournees.Complete)
            {
                retTimes.PlageTravMatin.Start = NullDateTimeOrDft(badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_MATIN_START)?.DateTime, dftDt);
                retTimes.PlageTravMatin.End = badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_MATIN_END)?.DateTime;
                retTimes.PlageTravAprem.Start = NullDateTimeOrDft(badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_START)?.DateTime, dftDt);
                retTimes.PlageTravAprem.End = badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_END)?.DateTime;
            }
            else if (typesJournees == EnumTypesJournees.Matin)
            {
                retTimes.PlageTravAprem.Start = dftDt;
                retTimes.PlageTravAprem.End = dftDt;

                retTimes.PlageTravMatin.Start = NullDateTimeOrDft(badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_MATIN_START)?.DateTime, dftDt);
                retTimes.PlageTravMatin.End = badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_END)?.DateTime;
            }
            else if (typesJournees == EnumTypesJournees.ApresMidi)
            {
                retTimes.PlageTravMatin.Start = dftDt;
                retTimes.PlageTravMatin.End = dftDt;

                retTimes.PlageTravAprem.Start = NullDateTimeOrDft(badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_MATIN_START)?.DateTime, dftDt);
                retTimes.PlageTravAprem.End = badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_END)?.DateTime;
            }
            else if (etatBadger == -1)
            {
                retTimes.PlageTravMatin.Start = dftDt;
                retTimes.PlageTravMatin.End = dftDt;

                retTimes.PlageTravAprem.Start = dftDt;
                retTimes.PlageTravAprem.End = dftDt;
            }

            return retTimes;
        }


        public static TimesBadgerDto HydrateTimesApplicationOrder(EnumTypesJournees typesJournees, int etatBadger, List<BadgeageEntryDto> badgeageEntryDtos)
        {
            DateTime dftDt = DateTime.MinValue;
            dftDt = AppDateUtils.ChangeTime(dftDt, TimeSpan.Zero);
            TimesBadgerDto retTimes = new TimesBadgerDto();

            if (typesJournees == EnumTypesJournees.Complete)
            {
                retTimes.PlageTravMatin.Start = NullDateTimeOrDft(badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_MATIN_START)?.DateTime, dftDt);
                retTimes.PlageTravMatin.End = badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_MATIN_END)?.DateTime;
                retTimes.PlageTravAprem.Start = NullDateTimeOrDft(badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_START)?.DateTime, dftDt);
                retTimes.PlageTravAprem.End = badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_END)?.DateTime;
            }
            else if (typesJournees == EnumTypesJournees.Matin || typesJournees == EnumTypesJournees.ApresMidi)
            {
                retTimes.PlageTravMatin.Start = dftDt;
                retTimes.PlageTravAprem.End = dftDt;

                retTimes.PlageTravMatin.Start = NullDateTimeOrDft(badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_MATIN_START)?.DateTime, dftDt);
                retTimes.PlageTravAprem.End = badgeageEntryDtos
                    .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_END)?.DateTime;
            }
            else if (etatBadger == -1)
            {
                retTimes.PlageTravMatin.Start = dftDt;
                retTimes.PlageTravMatin.End = dftDt;

                retTimes.PlageTravAprem.Start = dftDt;
                retTimes.PlageTravAprem.End = dftDt;
            }


            return retTimes;
        }

        private static DateTime NullDateTimeOrDft(DateTime? dt, DateTime dft)
        {
            if (dt.HasValue) return dt.Value;
            return dft;
        }
    }
}
