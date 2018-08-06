using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    public sealed class EnumErrorCodeRetour
    {

        public static readonly EnumErrorCodeRetour RETRY = new EnumErrorCodeRetour(0, "Réessayer");
        public static readonly EnumErrorCodeRetour OPEN_BADGE_PAGE = new EnumErrorCodeRetour(1, "Ouvrir la page pour badger");
        public static readonly EnumErrorCodeRetour CONSULTER_POINTAGE = new EnumErrorCodeRetour(2, "Consulter mes pointages");
        public static readonly EnumErrorCodeRetour ANNULER = new EnumErrorCodeRetour(3, "Annuler");
        public static readonly EnumErrorCodeRetour OPEN_SIRHIUS = new EnumErrorCodeRetour(3, "Ouvrir Sirhius");

        public static IEnumerable<EnumErrorCodeRetour> Values
        {
            get
            {
                yield return RETRY;
                yield return OPEN_BADGE_PAGE;
                yield return CONSULTER_POINTAGE;
                yield return ANNULER;
                yield return OPEN_SIRHIUS;


            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumErrorCodeRetour(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;


        }


        public static EnumErrorCodeRetour GetFromIndex(int index)
        {
            if (index < 0) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }

        public static EnumErrorCodeRetour GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

    }
}
