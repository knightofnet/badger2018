using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BadgerUpdater.dto
{
    public class EnumExitCodes
    {

        public static readonly EnumExitCodes OK = new EnumExitCodes(0, "Traitements terminés avec succés");
        public static readonly EnumExitCodes OK_NO_UPDATE_NEEDED = new EnumExitCodes(1, "Aucune mise  à jour à effectuer");
        public static readonly EnumExitCodes ERROR_IN_PARAMS = new EnumExitCodes(10, "Erreur lors de la lecture des paramètres en entrée");
        public static readonly EnumExitCodes ERROR_IN_PARAMS_UPDXML_FILM = new EnumExitCodes(11, "Erreur lors de la lecture des paramètres en entrée : Impossible de trouver le fichier contenant les mises à jours");
        public static readonly EnumExitCodes ERROR_WAIT_PROGRAM_CLOSE = new EnumExitCodes(20, "Erreur lors de la mise à jour : le programme Badger2018 n'était pas terminé");


        public static IEnumerable<EnumExitCodes> Values
        {
            get
            {
                yield return OK;
                yield return OK_NO_UPDATE_NEEDED;
                yield return ERROR_IN_PARAMS;
                yield return ERROR_IN_PARAMS_UPDXML_FILM;
                yield return ERROR_WAIT_PROGRAM_CLOSE;



            }
        }

        public int ExitCodeInt { get; private set; }
        public String Libelle { get; private set; }


        private EnumExitCodes(int exitCodeInt, String libelle)
        {
            ExitCodeInt = exitCodeInt;
            Libelle = libelle;


        }


        public static EnumExitCodes GetFromExitCodeInt(int exitCodeInt)
        {
            if (exitCodeInt < 0) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.ExitCodeInt == exitCodeInt);
        }



        public static EnumExitCodes GetFromLibelle(string modeBadgeSeleted)
        {
            if (modeBadgeSeleted == null) return null;

            return Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }


    }
}
