using System;
using System.Collections.Generic;

namespace Badger2018.dto
{
    public class PointageElt
    {

        public bool IsComplete { get; set; }

        public String DateDay { get; set; }
        public int EtatBadger { get; set; }

        public int OldEtatBadger { get; set; }

        public String B0 { get; set; }
        public String B1 { get; set; }
        public String B2 { get; set; }
        public String B3 { get; set; }

        public int TypeJournee { get; set; }

        public bool IsNotif1Showed { get; set; }

        public bool IsNotif2Showed { get; set; }

        public List<IntervalTemps> Pauses { get; set; }

        public double WorkAtHomeCpt { get; set; }


        public override string ToString()
        {
            return string.Format("IsComplete: {0}, DateDay: {1}, EtatBadger: {2}, OldEtatBadger: {3}, B0: {4}, B1: {5}, B2: {6}, B3: {7}, TypeJournee: {8}, IsNotif1Showed: {9}, IsNotif2Showed: {10}, Pauses: {11}, WorkAtHomeCpt: {12}", IsComplete, DateDay, EtatBadger, OldEtatBadger, B0, B1, B2, B3, TypeJournee, IsNotif1Showed, IsNotif2Showed, Pauses, WorkAtHomeCpt);
        }
    }
}
