using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.constants;

namespace Badger2018
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger _logger = new Logger(CommonCst.AppLogFile, CommonCst.ConsoleLogLvl, CommonCst.FileLogLvl, "1 Mo");

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _logger.Info("/***********************");
            _logger.Info("*  Application lancée (v. {0})", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            _logger.Info("***********************/");

            Cst.ApplicationDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();

            AppArgsDto argsDto = new AppArgsDto();

            _logger.Debug("Chargement des options du programme");
            AppOptions prgOptions = OptionManager.LoadOptions();
            if (prgOptions == null)
            {
                _logger.Error("Erreur lors du chargement des options du programme. Impossible de démarrer le programme");
                Environment.Exit(1);
            }


            argsDto = LoadArgs(e, argsDto);

            PreLoadActions(argsDto, prgOptions);

            UpdaterManager updaterManager = new UpdaterManager();
            try
            {
               
                updaterManager.XmlUpdFilePath = prgOptions.UpdateXmlUri;
                updaterManager.CheckForUpdates("launch");

            } catch (Exception ex)
            {
                _logger.Warn("Erreur lors de la recherche de mise à jour");
                _logger.Debug("{0} : {1}", ex.GetType().Name, ex.Message);
                _logger.Debug("{0}", ex.StackTrace);
            }
            /*
            UpdateChecker.Instance.UpdateUri = prgOptions.UpdateXmlUri;
            UpdateChecker.Instance.CheckForNewUpdate("launch");

             */
            //_logger.Debug(DbbAccessManager.Instance.Connection.ConnectionString);

            MainWindow mainWindow = new MainWindow(prgOptions, updaterManager);
            mainWindow.ShowDialog();

            _logger.Info("Fin du programme");
            Environment.Exit(0);

        }

        private static void PreLoadActions(AppArgsDto argsDto, AppOptions prgOptions)
        {
            if (argsDto.IsForceLogDebug)
            {
                _logger.Info("Passage de log en debug");
                _logger = new Logger(_logger.FileLog.FullName, Logger.LogLvl.DEBUG, Logger.LogLvl.DEBUG);
            }

            if (argsDto.ExportConfFilePath != null)
            {
                _logger.Info("Exportation de la configuration");

                OptionManager.SaveOptionsToXml(prgOptions, argsDto.ExportConfFilePath);

            }

            if (argsDto.ImportConfFilePath != null)
            {
                _logger.Info("Importation de la configuration");
                _logger.Debug(" Lecture du fichier");
                AppOptions optionImported = OptionManager.LoadFromXml(argsDto.ImportConfFilePath);

                _logger.Debug(" Prise en compte des nouvelles options");
                OptionManager.ChangeOptions(optionImported, prgOptions, argsDto.ImportConfFilePath);

            }

            if (File.Exists("oneShotUpdateConf.xml"))
            {
                _logger.Info("Importation de la configuration à partir de oneShotUpdateConf.xml");
                _logger.Debug(" Lecture du fichier");
                AppOptions optionImported = OptionManager.LoadFromXml("oneShotUpdateConf.xml");

                _logger.Debug(" Prise en compte des nouvelles options");
                OptionManager.ChangeOptions(optionImported, prgOptions, "oneShotUpdateConf.xml");

                File.Delete("oneShotUpdateConf.xml");
            }

            OptionManager.SaveOptions(prgOptions);

            if (!argsDto.LoadAfterImportExport && (argsDto.ExportConfFilePath != null || argsDto.ImportConfFilePath != null))
            {
                _logger.Info(
                    "Fin du traitement. Utiliser l'option -l pour démarrer l'application après une tâche d'export/import de la configuration");

                Environment.Exit(3);
            }

        }

        private static AppArgsDto LoadArgs(StartupEventArgs e, AppArgsDto argsDto)
        {
            try
            {
                if (e.Args.Length > 0)
                {
                    _logger.Debug("Lecteurs des options et paramètres");
                    AppArgsParser argsParser = new AppArgsParser();
                    argsDto = argsParser.ParseDirect(e.Args);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors du chargement des paramètres en entrée du programme.");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);

                Environment.Exit(2);
            }
            return argsDto;
        }
    }
}
