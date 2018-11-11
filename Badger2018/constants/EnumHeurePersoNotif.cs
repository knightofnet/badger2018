using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    public class EnumHeurePersoNotif
    {

        public static readonly EnumHeurePersoNotif HEURE_PERSO = new EnumHeurePersoNotif("HEURE_PERSO", "une heure personnalisée");
        public static readonly EnumHeurePersoNotif END_PF_MATIN = new EnumHeurePersoNotif("END_PF_MATIN", "l'heure de fin de la plage fixe du matin");
        public static readonly EnumHeurePersoNotif START_PF_APREM = new EnumHeurePersoNotif("START_PF_APREM", "l'heure du début de la plage fixe de l'après-midi");
        public static readonly EnumHeurePersoNotif END_PF_APREM = new EnumHeurePersoNotif("APREM", "l'heure de fin de la plage fixe de l'après-midi");
        public static readonly EnumHeurePersoNotif TPS_TRAV_THEO = new EnumHeurePersoNotif("TPS_TRAV_THEO", "l'heure de fin de travail théorique");



        public static IEnumerable<EnumHeurePersoNotif> Values
        {
            get
            {
                yield return HEURE_PERSO;
                yield return END_PF_MATIN;
                yield return START_PF_APREM;
                yield return END_PF_APREM;
                yield return TPS_TRAV_THEO;


            }
        }


        public String EnumRef { get; private set; }
        public String Libelle { get; private set; }


        private EnumHeurePersoNotif(String enumRef, String libelle)
        {

            Libelle = libelle;
            EnumRef = enumRef;

        }


        public static EnumHeurePersoNotif GetFromEnumRef(string enumRef)
        {
            if (enumRef == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.EnumRef == enumRef);
        }

        public static EnumHeurePersoNotif GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }



    }
}
