using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    public sealed class EnumBrowser : IEnumSerializableWithIndex<EnumBrowser>
    {

        public static readonly EnumBrowser IE = new EnumBrowser(0, "Internet Explorer");
        public static readonly EnumBrowser FF = new EnumBrowser(1, "Firefox");

        public static IEnumerable<EnumBrowser> Values
        {
            get
            {
                yield return IE;
                yield return FF;


            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumBrowser(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;


        }


        public static EnumBrowser GetFromIndex(int index)
        {
            if (index < 0) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }



        public static EnumBrowser GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

        EnumBrowser IEnumSerializableWithIndex<EnumBrowser>.GetFromIndex(int index)
        {
            return GetFromIndex(index);
        }

        public int GetIndex()
        {
            return Index;

        }
    }
}
