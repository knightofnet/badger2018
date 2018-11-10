using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Badger2018.constants;

namespace Badger2018.dto.bdd
{
    class JourEntryDto
    {
        public bool IsHydrated { get; set; }

        public DateTime DateJour { get; set; }

        public EnumTypesJournees TypeJour { get; set; }

        public bool IsComplete { get; set; }

        public int EtatBadger { get; set; }
        public int OldEtatBadger { get; set; }

        public TimeSpan? TpsTravaille { get; set; }



    }
}
