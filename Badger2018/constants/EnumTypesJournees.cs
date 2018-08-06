using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    public sealed class EnumTypesJournees
    {

        public static readonly EnumTypesJournees Complete = new EnumTypesJournees(0, "Journée complète");
        public static readonly EnumTypesJournees Matin = new EnumTypesJournees(1, "Matin travaillé");
        public static readonly EnumTypesJournees ApresMidi = new EnumTypesJournees(2, "Après-midi travaillée");

        public static IEnumerable<EnumTypesJournees> Values
        {
            get
            {
                yield return Complete;
                yield return Matin;
                yield return ApresMidi;

            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumTypesJournees(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;


        }


        public static EnumTypesJournees GetFromIndex(int index)
        {
            return index < 0 ? null : Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }

        public static EnumTypesJournees GetFromLibelle(string modeBadgeSeleted)
        {
            return modeBadgeSeleted == null ? null : Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

        public static bool IsDemiJournee(EnumTypesJournees tyJournee)
        {
            return tyJournee == ApresMidi || tyJournee == Matin;
        }
    }
}
