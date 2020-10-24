using System;
using System.Collections.Generic;
using System.Linq;

namespace BadgerCommonLibrary.constants
{
    public class EnumWaveCompModeTraitement
    {

        public static readonly EnumWaveCompModeTraitement ShowDevicesMode = new EnumWaveCompModeTraitement("SHOWDEVICES", "Récupère la liste des périphériques");
        public static readonly EnumWaveCompModeTraitement PlayEnumWaveCompSoundMode = new EnumWaveCompModeTraitement("PLAYSOUND", "Joue un son parmis :");
        public static readonly EnumWaveCompModeTraitement FullDumpMode = new EnumWaveCompModeTraitement("FULLDUMP", "Affiche plus d'information sur les périphériques de lecture");
        public static readonly EnumWaveCompModeTraitement TestAllDeviceMode = new EnumWaveCompModeTraitement("TEST", "Joue un son sur tous les périphériques de lecture");
        public static readonly EnumWaveCompModeTraitement DaemonWaitingOrders = new EnumWaveCompModeTraitement("DEMON", "Mode démon. Attends les ordres par fichier signal.");


        public static IEnumerable<EnumWaveCompModeTraitement> Values
        {
            get
            {
                yield return ShowDevicesMode;
                yield return PlayEnumWaveCompSoundMode;
                yield return FullDumpMode;
                yield return TestAllDeviceMode;
                yield return DaemonWaitingOrders;

            }
        }

        public string LaunchModeOption { get; private set; }
        public String Libelle { get; private set; }


        private EnumWaveCompModeTraitement(string launchModeOption, String libelle)
        {
            LaunchModeOption = launchModeOption;
            Libelle = libelle;

        }


        public static EnumWaveCompModeTraitement GetFromLaunchModeOption(string launchModeOption)
        {

            return launchModeOption == null ? null : Values.FirstOrDefault(enumModeP => enumModeP.LaunchModeOption == launchModeOption);
        }



        public static EnumWaveCompModeTraitement GetFromLibelle(string modeBadgeSeleted)
        {
            return modeBadgeSeleted == null ? null : Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }

        public static string LibelleJoined(string joinStr = ", ")
        {
            if (joinStr == null)
            {
                return null;
            }
            List<String> strRet = Values.Select(value => value.LaunchModeOption).ToList();

            return String.Join(joinStr, strRet);
        }

    }
}
