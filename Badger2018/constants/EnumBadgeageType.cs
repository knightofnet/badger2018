using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    public sealed class EnumBadgeageType
    {

        public static readonly EnumBadgeageType PLAGE_TRAV_MATIN_START = new EnumBadgeageType(0, "Premier badgeage de la journée");
        public static readonly EnumBadgeageType PLAGE_TRAV_MATIN_END = new EnumBadgeageType(1, "Badgeage de fin de matinée");
        public static readonly EnumBadgeageType PLAGE_TRAV_APREM_START = new EnumBadgeageType(2, "Badgeage du début de l'après-midi");
        public static readonly EnumBadgeageType PLAGE_TRAV_APREM_END = new EnumBadgeageType(3, "Badgeage de fin de la journée");
        public static readonly EnumBadgeageType PLAGE_START = new EnumBadgeageType(10, "Début de la pause");
        public static readonly EnumBadgeageType PLAGE_END = new EnumBadgeageType(11, "Fin de la pause");

        public static IEnumerable<EnumBadgeageType> Values
        {
            get
            {
                yield return PLAGE_TRAV_MATIN_START;
                yield return PLAGE_TRAV_MATIN_END;
                yield return PLAGE_TRAV_APREM_START;
                yield return PLAGE_TRAV_APREM_END;
                yield return PLAGE_START;
                yield return PLAGE_END;


            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumBadgeageType(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;


        }

        public static EnumBadgeageType GetFromIndex(int index)
        {
            if (index < 0)
            {
                return null;
            }

            if (index < 0) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }
    }
}
