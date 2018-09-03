using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.dto
{
    public class TimesBadgerDto
    {

        public DateTime TimeRef { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndTheoDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public DateTime PauseStartDateTime { get; set; }
        public DateTime PauseEndDateTime { get; set; }

        public TimeSpan TpsTravMatin { get; set; }

        public TimeSpan GetTpsTravMatin()
        {
            return PauseStartDateTime - StartDateTime;
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
            return EndDateTime - PauseEndDateTime;
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
    }
}
