using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.utils;

namespace Badger2018.business.dbb
{
    class DbbUpdateManager
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private readonly Regex regexExtractVers = new Regex(@"sqlUpd-(\d{1,2}\.\d{1,2}\.\d{1,4}\.\d{1,4})\.sql$", RegexOptions.Compiled);

        private SQLiteConnection _connexionRef;
        private readonly string _lastSqlUpdateVersion;

        private readonly List<string> _fileUpd = new List<string>();

        public DbbUpdateManager(SQLiteConnection connection, string lastSqlUpdateVersion)
        {
            this._connexionRef = connection;
            _lastSqlUpdateVersion = lastSqlUpdateVersion;
        }

        public bool CheckUpdateRequired()
        {
            bool isUpdateRequired = false;

            Version currentVersionApp = Assembly.GetExecutingAssembly().GetName().Version;
            Version lastUpdVersion;
            if (StringUtils.IsNullOrWhiteSpace(_lastSqlUpdateVersion) || Version.TryParse(_lastSqlUpdateVersion, out lastUpdVersion))
            {
                lastUpdVersion = new Version(1, 0, 0, 0);
            }


            string[] filesCandidates = Directory.GetFiles(".", "sqlUpd-*.sql", SearchOption.TopDirectoryOnly);
            foreach (string candidate in filesCandidates)
            {

                Match m = regexExtractVers.Match(candidate);
                if (m.Success)
                {
                    string candidateVersRaw = m.Groups[1].ToString();
                    Version cdtitVersion = Version.Parse(candidateVersRaw);

                    if (lastUpdVersion < cdtitVersion && cdtitVersion <= currentVersionApp)
                    {
                        _fileUpd.Add(candidate);
                        isUpdateRequired = true;
                    }
                }
            }


            return isUpdateRequired;
        }

        public void UpdateDbb()
        {

            if (_fileUpd != null && _fileUpd.Count > 0)
            {
                foreach (string fileUpd in _fileUpd)
                {
                    _logger.Info("Fichier de mise à jour BDD detecté ({0}). Exécution", fileUpd);

                    SqlLiteUtils.ExecuteContentFile(DbbAccessManager.Instance.Connection, fileUpd);
                    File.Delete(fileUpd);
                }
            }

        }

        public void BackupDbb()
        {

        }
    }
}
