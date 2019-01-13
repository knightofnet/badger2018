using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Badger2018.constants;

namespace Badger2018.dto.bdd
{
    public class BadgeageEntryDto
    {

        public DateTime DateTime { get; set; }

        public EnumBadgeageType TypeBadge { get; set; }

        public string RelationKey { get; set; }

        public DateTime DateAdded { get; set; }

        public TimeSpan CdAtTime { get; set; }
    }
}
