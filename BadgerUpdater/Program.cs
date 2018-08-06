using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.constants;
using BadgerUpdater.dto;

namespace BadgerUpdater
{
    class Program
    {
        private static Logger _logger = new Logger(CommonCst.UpdaterLogFile, CommonCst.ConsoleLogLvl, CommonCst.FileLogLvl, "1 Mo");

        static void Main(string[] args)
        {

            _logger.Info("/***********************");
            _logger.Info("*  Application Updater lancée (v. {0})", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            _logger.Info("***********************/");

            AppArgsDto argsDto = new AppArgsDto();

            FileInfo badgerExeFi = new FileInfo("./Badger2018.exe");

            Version badgerExeVersion = Version.Parse(FileVersionInfo.GetVersionInfo(badgerExeFi.FullName).ProductVersion);

            _logger.Debug("Exe version : {0}", badgerExeVersion.ToString());


            //UpdaterManager updMgr = new UpdaterManager();
            // Récupérer les mise à jours jusqu'à la cible

            /* Avant (vérif) :
             * - Archiver le dossier (backup)
             * - sauvegarder les options (backup)
             * - Pour chaque MAJ - Vérifier la présence des exe archives
             * 
             * Mise à jour :
             * - Pour chaque MAJ :
             * -- Si NeedLaunch : Export options vers tmpFile.xml
             * -- Décompression
             * -- Si NeedLaunch : Import options depuis tmpFile.xml (== lancement intermédiaire. Chargera aussi oneshotUpd)
             * >> En cas d'erreur, arrêt (utilisation du backup)
             * 
             * - import options backups
             * - lancement
             * - RockNRoll !
             */

        }
    }
}
