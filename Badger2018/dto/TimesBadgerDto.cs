using System;
using System.Collections.Generic;
using System.Linq;
using AryxDevLibrary.utils;

namespace Badger2018.dto
{
    public class TimesBadgerDto
    {

        public DateTime TimeRef { get; set; }

        public IntervalTemps PlageTravMatin { get; set; }
        public IntervalTemps PlageTravAprem { get; set; }

        //public DateTime StartDateTime { get; set; }
        public DateTime EndTheoDateTime { get; set; }
        //public DateTime EndDateTime { get; set; }

        //public DateTime PauseStartDateTime { get; set; }
        //public DateTime PauseEndDateTime { get; set; }

        public List<IntervalTemps> PausesHorsDelai { get; set; }

        public TimeSpan TpsTravMatin { get; set; }
        public TimeSpan EndMoyPfMatin { get; set; }
        public TimeSpan EndMoyPfAprem { get; set; }


        public TimesBadgerDto()
        {
            PlageTravMatin = new IntervalTemps();
            PlageTravAprem = new IntervalTemps();
        }

        public TimeSpan GetTpsTravMatin()
        {
            TimeSpan tpsReelTrav = PlageTravMatin.GetDuration();
            return tpsReelTrav - GetTpsPauseMatin();
        }



        public TimeSpan GetTpsTravMatinOrMax(TimeSpan tempsMaxDemieJournee)
        {
            TimeSpan tpsTravMatin = GetTpsTravMatin();
            if (tpsTravMatin.CompareTo(tempsMaxDemieJournee) > 0)
            {
                return tempsMaxDemieJournee;
            }
            return tpsTravMatin;
        }

        public TimeSpan GetTpsTravAprem()
        {
            TimeSpan tpsReelTrav = PlageTravAprem.GetDuration();
            return tpsReelTrav - GetTpsPauseAprem();
        }

        public TimeSpan GetTpsTravApremOrMax(TimeSpan tempsMaxDemieJournee)
        {
            TimeSpan tpsTravMatin = GetTpsTravAprem();
            if (tpsTravMatin.CompareTo(tempsMaxDemieJournee) > 0)
            {
                return tempsMaxDemieJournee;
            }
            return tpsTravMatin;
        }

        public TimeSpan GetTpsPause()
        {
            TimeSpan ts = TimeSpan.Zero;
            if (PausesHorsDelai.Any(r => r.IsIntervalComplet()))
            {
                ts = PausesHorsDelai.Where((r => r.IsIntervalComplet())).Aggregate(ts, (current, pause) => current + pause.GetDuration());
            }

            return ts;
        }

        private TimeSpan GetTpsPauseMatin()
        {
            TimeSpan tsRet = TimeSpan.Zero;
            foreach (IntervalTemps intervalTemps in PausesHorsDelai.Where((r => r.IsIntervalComplet() && r.Start.CompareTo(PlageTravMatin.Start) >= 0 && r.EndOrDft.CompareTo(PlageTravMatin.EndOrDft) <= 0)))
            {
                tsRet += intervalTemps.GetDuration();
            }
            return tsRet;

        }

        private TimeSpan GetTpsPauseAprem()
        {

            TimeSpan tsRet = TimeSpan.Zero;
            foreach (IntervalTemps intervalTemps in PausesHorsDelai.Where((r => r.IsIntervalComplet() && r.Start.CompareTo(PlageTravAprem.Start) >= 0 && r.EndOrDft.CompareTo(PlageTravAprem.EndOrDft) <= 0)))
            {
                tsRet += intervalTemps.GetDuration();
            }
            return tsRet;

        }

        public bool IsTherePauseMatin()
        {
            return
                PausesHorsDelai.Any(
                    (r =>
                        r.Start.CompareTo(PlageTravMatin.Start) >= 0));
        }

        public bool IsTherePauseAprem()
        {
            return
                PausesHorsDelai.Any(
                    (r =>
                        r.Start.CompareTo(PlageTravAprem.Start) >= 0));
        }

        public bool IsStartMatinBadged()
        {
            return !PlageTravMatin.Start.Equals(ReflexionUtils.GetDefaultValue(typeof(DateTime)));
        }

        public bool IsStartApremBadged()
        {
            return !PlageTravAprem.Start.Equals(ReflexionUtils.GetDefaultValue(typeof(DateTime)));
        }

        public bool IsEndMatinBadged()
        {
            return PlageTravMatin.End.HasValue;
        }

        public bool IsEndApremBadged()
        {
            return PlageTravAprem.End.HasValue;
        }
    }
}
