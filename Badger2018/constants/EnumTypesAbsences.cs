using System;
using System.Collections.Generic;
using System.Linq;

namespace Badger2018.constants
{
    public sealed class EnumTypesAbsences
    {

        public static readonly EnumTypesAbsences Rtt = new EnumTypesAbsences(0, "RTT");
        public static readonly EnumTypesAbsences DispPlageFixe = new EnumTypesAbsences(1, "Récupération horaire variable");
        public static readonly EnumTypesAbsences Ca = new EnumTypesAbsences(2, "CA");

        public static IEnumerable<EnumTypesAbsences> Values
        {
            get
            {
                yield return Rtt;
                yield return DispPlageFixe;
                yield return Ca;

            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumTypesAbsences(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;

        }


        public static EnumTypesAbsences GetFromIndex(int index)
        {
            return index < 0 ? null : Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }

        public static EnumTypesAbsences GetFromLibelle(string modeBadgeSeleted)
        {
            return modeBadgeSeleted == null ? null : Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }


    }
}
