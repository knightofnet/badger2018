using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ExceptionHandlingUtils = BadgerCommonLibrary.utils.ExceptionHandlingUtils;

namespace Badger2018.dto
{
    public class LicenceInfo
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public string Username { get; set; }
        public int VersionLicence { get; set; }
        public DateTime DateExpiration { get; set; }
        public String NiceName { get; set; }
        public int TypeUser { get; set; }
        public string ReArmMail { get; set; }



        public static LicenceInfo GetFromString(String licenceRaw)
        {
            try
            {
                if (StringUtils.IsNullOrWhiteSpace(licenceRaw))
                {
                    _logger.Debug("Licence non lisible");
                    return null;

                }

                string[] splitted = licenceRaw.Split('#');
                if (splitted.Length < 6)
                {

                    _logger.Debug("Licence non lisible, ou ne contient pas les éléments nécessaires");
                    return null;
                }

                LicenceInfo licenceInfo = new LicenceInfo()
                {
                    Username = splitted[0],
                    VersionLicence = Int32.Parse(splitted[1]),
                    DateExpiration = DateTime.ParseExact(splitted[2], "dd/MM/yyyy", CultureInfo.CurrentCulture),
                    NiceName = splitted[3],
                    TypeUser = Int32.Parse(splitted[4]),
                    ReArmMail = splitted[5]

                };

                return licenceInfo;
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "Erreur lors de la lecture de la licence");
                return null;
            }
        }


        public Dictionary<string, string> ToDictionnary()
        {
            Dictionary<String, String> retDico = new Dictionary<string, string>();
            retDico.Add("NiceName", NiceName);
            retDico.Add("VersionLicence", VersionLicence.ToString());
            retDico.Add("DateExpiration", DateExpiration.ToString("g"));
            retDico.Add("TypeUser", this.TypeUser.ToString());
            retDico.Add("Username", Username);
            retDico.Add("ReArmMail", ReArmMail);

            return retDico;

        }
    }
}
