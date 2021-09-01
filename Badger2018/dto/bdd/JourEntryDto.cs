using System;
using Badger2018.constants;

namespace Badger2018.dto.bdd
{
    public class JourEntryDto
    {
        public bool IsHydrated { get; set; }

        public DateTime DateJour { get; set; }

        public EnumTypesJournees TypeJour { get; set; }

        public bool IsComplete { get; set; }

        public int EtatBadger { get; set; }
        public int OldEtatBadger { get; set; }

        public TimeSpan? TpsTravaille { get; set; }

        public double WorkAtHomeCpt { get; set; }



    }
}
