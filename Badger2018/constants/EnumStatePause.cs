using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    public sealed class EnumStatePause
    {

        public static readonly EnumStatePause NONE = new EnumStatePause("Pas de pause en cours", 0);
        public static readonly EnumStatePause IN_PROGRESS = new EnumStatePause("Pause en cours", 1);
        public static readonly EnumStatePause HAVE_PAUSES = new EnumStatePause("Pas de pause en cours, mais des pauses ont eu lieu", 2);


        public static IEnumerable<EnumStatePause> Values
        {
            get
            {
                yield return NONE;
                yield return IN_PROGRESS;
                yield return HAVE_PAUSES;


            }
        }


        public String Libelle { get; private set; }
        public int State { get; private set; }
        public bool BoolState { get; private set; }

        private EnumStatePause(String libelle, int boolThreeStateForImgWrapper)
        {

            Libelle = libelle;
            State = boolThreeStateForImgWrapper;
            BoolState = boolThreeStateForImgWrapper == 2;

        }




        public static EnumStatePause GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

    }
}
