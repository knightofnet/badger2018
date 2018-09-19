using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BadgerCommonLibrary.utils;

namespace Badger2018.constants
{
    public sealed class EnumActionBtnBadgeM : IEnumSerializableWithIndex<EnumActionBtnBadgeM>
    {

        public static readonly EnumActionBtnBadgeM BadgerSeulement = new EnumActionBtnBadgeM(0, "Badger uniquement");
        public static readonly EnumActionBtnBadgeM BadgerInterval = new EnumActionBtnBadgeM(1, "Badger un interval");


        public static IEnumerable<EnumActionBtnBadgeM> Values
        {
            get
            {
                yield return BadgerSeulement;
                yield return BadgerInterval;

            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumActionBtnBadgeM(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;

        }


        public static EnumActionBtnBadgeM GetFromIndex(int index)
        {
            return index < 0 ? null : Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }

        public int GetIndex()
        {
            return Index;
        }

        public static EnumActionBtnBadgeM GetFromLibelle(string modeBadgeSeleted)
        {
            return modeBadgeSeleted == null ? null : Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }


        EnumActionBtnBadgeM IEnumSerializableWithIndex<EnumActionBtnBadgeM>.GetFromIndex(int index)
        {
            return GetFromIndex(index);
        }
    }
}
