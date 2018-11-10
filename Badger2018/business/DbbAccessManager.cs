using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils.logger;
using Badger2018.services;
using Badger2018.utils;
using BadgerCommonLibrary.utils;

namespace Badger2018.business
{
    public class DbbAccessManager
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private bool isEnabled = true;

        private static DbbAccessManager _instance;
        public static DbbAccessManager Instance
        {
            get { return _instance ?? (_instance = new DbbAccessManager()); }
            private set { _instance = value; }
        }

        public SQLiteConnection Connection { get; private set; }

        private DbbAccessManager()
        {
            Connection = null;
            if (!isEnabled) return;

            String dbFileName = Properties.Resources.dbbFile;
            try
            {
                if (!File.Exists(dbFileName))
                {
                    _logger.Info("Création de la base de données SQLITE");
                    SqlLiteUtils.CreateDb(dbFileName);
                }

                _logger.Debug("Initialisation de la connexion");
                Connection = SqlLiteUtils.InitAndGetConnection(Properties.Resources.dbbFile);


            }
            catch (Exception ex)
            {
                _logger.Error("Erreur fatale lors de l'initiation de la connexion : {0}", ex);
                if (Connection != null && Connection.State == ConnectionState.Open)
                {
                    Connection.Close();
                }
                throw ex;
            }
        }



        public void StartTransaction()
        {
            if (!isEnabled) return;

            if (CurrentTransaction != null)
            {
                _logger.Error("Une transaction SQLite est déjà ouverte");
                throw new Exception("Une transaction SQLite est déjà ouverte");

            }
            CurrentTransaction = Connection.BeginTransaction();
        }

        public SQLiteTransaction CurrentTransaction { get; set; }

        public void StopAndCommitTransaction()
        {
            if (!isEnabled) return;

            if (CurrentTransaction != null)
            {
                CurrentTransaction.Commit();
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }
        }

        public void StopAndRollbackTransaction()
        {
            if (!isEnabled) return;

            if (CurrentTransaction != null)
            {
                CurrentTransaction.Rollback();
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }
        }
    }
}
