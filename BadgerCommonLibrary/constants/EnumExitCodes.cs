using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BadgerCommonLibrary.constants
{
    public class EnumExitCodes
    {
        /*
         * Codes exit :
         * sur 3 positions : XYZ (exemple 103)
         * - X Identifiant de l'application : 1 appplication principale (Badger2018), 2 programme de mise à jour (BadgerUpdater)
         * - Y Catégorie de gravité : 0 OK (sorties normales, sans erreurs), 1à4 Erreurs prévues, >5 Erreur non prévu ou graves
         * - Z Sous code erreur
         */

        /// <summary>
        /// Code 0, "Traitement terminé avec succés"
        /// </summary>
        public static readonly EnumExitCodes OK = new EnumExitCodes(0, "Traitement terminé avec succés");

        /// <summary>
        /// Code 103, "Tâches d'import/export des options terminées avec succés"
        /// </summary>
        public static readonly EnumExitCodes M_OK_IMPORT_EXPORT_OK = new EnumExitCodes(103, "Tâches d'import/export des options terminées avec succés");

        /// <summary>
        /// Code 111, "Une instance de badger2018 est déjà lancée"
        /// </summary>
        public static readonly EnumExitCodes M_ALREADY_RUNNING_INSTANCE = new EnumExitCodes(111, "Une instance de badger2018 est déjà lancée");


        /// <summary>
        /// Code 151, "Erreur lors du chargement des options ou lors des tâches de pré-chargement de l'interface"
        /// </summary>
        public static readonly EnumExitCodes M_ERROR_LOADING_APP = new EnumExitCodes(151, "Erreur lors du chargement des options ou lors des tâches de pré-chargement de l'interface");

        /// <summary>
        /// Code 199, "Erreur inconnue"
        /// </summary>
        public static readonly EnumExitCodes M_ERROR_UNKNOW_IN_APP = new EnumExitCodes(199, "Erreur inconnue");


        /// <summary>
        /// Code 201, "Aucune mise  à jour à effectuer"
        /// </summary>
        public static readonly EnumExitCodes U_OK_NO_UPDATE_NEEDED = new EnumExitCodes(201, "Aucune mise  à jour à effectuer");
        /// <summary>
        /// Code 209, "Arrêt du programme de mise à jour pour prise en compte de la mise à jour de l'application de mise à jour"
        /// </summary>
        public static readonly EnumExitCodes U_OK_UPD_UPDATE_RELAUNCH = new EnumExitCodes(209, "Arrêt du programme de mise à jour pour prise en compte de la mise à jour de l'application de mise à jour");

        /// <summary>
        /// Code 210, "Erreur lors de la lecture des paramètres en entrée"
        /// </summary>
        public static readonly EnumExitCodes U_ERROR_IN_PARAMS = new EnumExitCodes(210, "Erreur lors de la lecture des paramètres en entrée");
        /// <summary>
        /// Code 211, "Erreur lors de la lecture des paramètres en entrée : Impossible de trouver le fichier contenant les mises à jours"
        /// </summary>
        public static readonly EnumExitCodes U_ERROR_IN_PARAMS_UPDXML_FILM = new EnumExitCodes(211, "Erreur lors de la lecture des paramètres en entrée : Impossible de trouver le fichier contenant les mises à jours");
        /// <summary>
        /// Code 220, "Erreur lors de la mise à jour : le programme Badger2018 n'était pas terminé"
        /// </summary>
        public static readonly EnumExitCodes U_ERROR_WAIT_PROGRAM_CLOSE = new EnumExitCodes(220, "Erreur lors de la mise à jour : le programme Badger2018 n'était pas terminé");

        /// <summary>
        /// Code 251, "Erreur lors de la mise à jour : la mise a été annulée et l'application est revenu à son état avant mise à jour"
        /// </summary>
        public static readonly EnumExitCodes U_ERROR_UPD_ROOLBACK_OK = new EnumExitCodes(251, "Erreur lors de la mise à jour : la mise a été annulée et l'application est revenu à son état avant mise à jour");
        /// <summary>
        /// Code 252, "Erreur lors de la mise à jour : la mise a été annulée. L'application n'a pas réussi à revenir à son état d'avant mise à jour"
        /// </summary>
        public static readonly EnumExitCodes U_ERROR_UPD_ROOLBACK_KO = new EnumExitCodes(252, "Erreur lors de la mise à jour : la mise a été annulée. L'application n'a pas réussi à revenir à son état d'avant mise à jour");

        /// <summary>
        /// Code 299, "Erreur inconnue"
        /// </summary>
        public static readonly EnumExitCodes U_ERROR_UNKNOW_IN_APP = new EnumExitCodes(299, "Erreur inconnue");

        public static IEnumerable<EnumExitCodes> Values
        {
            get
            {
                yield return OK;

                yield return M_OK_IMPORT_EXPORT_OK;
                yield return M_ERROR_LOADING_APP;
                yield return M_ERROR_UNKNOW_IN_APP;

                yield return U_OK_NO_UPDATE_NEEDED;
                yield return U_ERROR_IN_PARAMS;
                yield return U_ERROR_IN_PARAMS_UPDXML_FILM;
                yield return U_ERROR_WAIT_PROGRAM_CLOSE;
                yield return U_ERROR_UNKNOW_IN_APP;



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
