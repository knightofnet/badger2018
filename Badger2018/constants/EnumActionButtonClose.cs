using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    public sealed class EnumActionButtonClose : IEnumSerializableWithIndex<EnumActionButtonClose>
    {

        public static readonly EnumActionButtonClose CLOSE = new EnumActionButtonClose(0, "Fermer le programme");
        public static readonly EnumActionButtonClose MINIMIZE = new EnumActionButtonClose(1, "Réduire le programme");

        public static IEnumerable<EnumActionButtonClose> Values
        {
            get
            {
                yield return CLOSE;
                yield return MINIMIZE;


            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumActionButtonClose(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;


        }


        public static EnumActionButtonClose GetFromIndex(int index)
        {
            if (index < 0) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }



        public static EnumActionButtonClose GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

        EnumActionButtonClose IEnumSerializableWithIndex<EnumActionButtonClose>.GetFromIndex(int index)
        {
            return GetFromIndex(index);
        }

        public int GetIndex()
        {
            return Index;
        }

    }
}
