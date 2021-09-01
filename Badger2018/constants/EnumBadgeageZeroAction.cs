using System;
using System.Collections.Generic;
using System.Linq;
using BadgerCommonLibrary.utils;

namespace Badger2018.constants
{
    public sealed class EnumBadgeageZeroAction : IEnumSerializableWithIndex<EnumBadgeageZeroAction>
    {

        public static readonly EnumBadgeageZeroAction NO_CHOICE = new EnumBadgeageZeroAction(0, "Demander à l'utilisateur");
        public static readonly EnumBadgeageZeroAction BADGER = new EnumBadgeageZeroAction(1, "Badger");
        public static readonly EnumBadgeageZeroAction REPORT_HEURE = new EnumBadgeageZeroAction(2, "Reporter l'heure");

        public static IEnumerable<EnumBadgeageZeroAction> Values
        {
            get
            {
                yield return NO_CHOICE;
                yield return BADGER;
                yield return REPORT_HEURE;

            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumBadgeageZeroAction(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;

        }


        public static EnumBadgeageZeroAction GetFromIndex(int index)
        {
            if (index < 0) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }



        public static EnumBadgeageZeroAction GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

        EnumBadgeageZeroAction IEnumSerializableWithIndex<EnumBadgeageZeroAction>.GetFromIndex(int index)
        {
            return GetFromIndex(index);
        }

        public int GetIndex()
        {
            return Index;
        }
    }
}
