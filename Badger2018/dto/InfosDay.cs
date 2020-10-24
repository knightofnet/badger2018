using Badger2018.constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.dto
{
    public class InfosDay
    {

        public int EtatBadger { get; set; }
        public EnumTypesJournees TypesJournees { get; set; }
        public TimesBadgerDto Times { get; set; }

    }
}
