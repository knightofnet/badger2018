using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.utils;
using Badger2018.utils.sqlite;
using BadgerCommonLibrary.constants;

namespace Badger2018.business.dbb
{
    class DbbUpdateManager
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;


        private readonly string _lastSqlUpdateVersion;

        public Version LastVersion { get; private set; }


        public DbbUpdateManager(string lastSqlUpdateVersion)
        {
            _lastSqlUpdateVersion = lastSqlUpdateVersion;
        }

        private readonly List<AbstractUpdateDbActions> _updatesToDo = new List<AbstractUpdateDbActions>(0);

        public bool CheckUpdateRequired()
        {
            bool isUpdateRequired = false;

            Version currentVersionApp = Assembly.GetExecutingAssembly().GetName().Version;
            Version lastUpdVersion;
            if (StringUtils.IsNullOrWhiteSpace(_lastSqlUpdateVersion) || Version.TryParse(_lastSqlUpdateVersion, out lastUpdVersion))
            {
                lastUpdVersion = new Version(1, 0, 0, 0);
            }

            AbstractUpdateDbActions ua = new Update_DianumDublin_1_3_0901_1510();
            if (ua.IsUpdateNeeded())
            {
                _updatesToDo.Add(ua);

                return true;
            }

            return false;
        }

        public void UpdateDbb()
        {

            if (_updatesToDo.Any())
            {
                try
                {
                    DbbAccessManager.Instance.StartTransaction();
                    foreach (AbstractUpdateDbActions update in _updatesToDo)
                    {
                        _logger.Info("Mise à jour BDD : {0}. Exécution", update.GetType().Name);

                        update.DoUpdate();
                        LastVersion = update.VersionMinForUpdate;
                    }

                    DbbAccessManager.Instance.StopAndCommitTransaction();
                }
                catch (Exception ex)
                {
                    DbbAccessManager.Instance.StopAndRollbackTransaction();
                    ExceptionHandlingUtils.LogAndEndsProgram(ex, EnumExitCodes.M_ERROR_UPDATE_BDD.ExitCodeInt, "Erreur lors de la mise à jour schema");
                }
            }

        }

        public void BackupDbb()
        {

        }

        class Update_DianumDublin_1_3_0901_1510 : AbstractUpdateDbActions
        {
            public sealed override bool IsEnabled { get; set; }
            public sealed override Version VersionMinForUpdate { get; set; }

            public Update_DianumDublin_1_3_0901_1510()
            {
                IsEnabled = true;
                VersionMinForUpdate = new Version("1.3.0901.1510");
            }
            public override bool IsUpdateNeeded()
            {
                List<TableDef> tableDefs = SqlLiteUtils.GetTableDefinition(DbbAccessManager.Instance.Connection, "JOURS");

                return !tableDefs.Any(r => r.Name.Equals("WORK_AT_HOME_CPT"));
            }

            public override void DoUpdate()
            {
              
                try
                {
                    String[] sqls = {
                        "ALTER TABLE JOURS ADD COLUMN WORK_AT_HOME_CPT NUMERIC DEFAULT 0;"
                    };
                    SqlLiteUtils.ExecuteSqlOrdersArray(DbbAccessManager.Instance.Connection, sqls);

                   
                }
                catch (Exception ex)
                {

                    ExceptionHandlingUtils.LogAndRethrows(ex, "Erreur lors de la mise à jour schema Update_DianumDublin_1_3_0901_1510");
                }
            }
        }

        abstract class AbstractUpdateDbActions
        {
            public abstract bool IsEnabled { get; set; }
            public abstract Version VersionMinForUpdate { get; set; }

            public abstract bool IsUpdateNeeded();

            public abstract void DoUpdate();
        }
    }
}
