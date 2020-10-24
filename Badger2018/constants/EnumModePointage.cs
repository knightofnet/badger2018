using System;
using System.Collections.Generic;
using System.Linq;
using BadgerCommonLibrary.utils;

namespace Badger2018.constants
{
    public sealed class EnumModePointage : IEnumSerializableWithIndex<EnumModePointage>
    {

        public static readonly EnumModePointage FORM = new EnumModePointage(0, "Par validation du formulaire", "Id du formulaire :");
        public static readonly EnumModePointage ELEMENT = new EnumModePointage(1, "Par clic sur élément HTML", "Id de l'élément HTML :");

        public static IEnumerable<EnumModePointage> Values
        {
            get
            {
                yield return FORM;
                yield return ELEMENT;


            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }

        public String UiLibelle { get; private set; }

        private EnumModePointage(int index, String libelle, String uiLibelle)
        {
            Index = index;
            Libelle = libelle;
            UiLibelle = uiLibelle;

        }


        public static EnumModePointage GetFromIndex(int index)
        {
            if (index < 0) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }



        public static EnumModePointage GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

        EnumModePointage IEnumSerializableWithIndex<EnumModePointage>.GetFromIndex(int index)
        {
            return GetFromIndex(index);
        }

        public int GetIndex()
        {
            return Index;
        }
    }
}
