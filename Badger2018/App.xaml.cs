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
using Badger2018.views;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;

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
            _logger.Info("*  Application lancée (v. {0})", Assembly.GetExecutingAssembly().GetName().Version);
            _logger.Info("***********************/");
            if (ProcessUtils.CountAppInstanceOf(Assembly.GetExecutingAssembly().Location) > 1)
            {
                _logger.Error("Une autre instance de Badger2018 est déjà chargée...");
                Environment.Exit(EnumExitCodes.M_ALREADY_RUNNING_INSTANCE.ExitCodeInt);
            }

            AppOptions prgOptions = null;
            UpdaterManager updaterManager = null;

            /*
             * Bloc chargement de la configuration et divers traitement pré-interface
             */
            try
            {

                Cst.ApplicationDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();

                AppArgsDto argsDto = new AppArgsDto();

                _logger.Debug("Chargement des options du programme");
                prgOptions = OptionManager.LoadOptions();
                if (prgOptions == null)
                {
                    ExceptionHandlingUtils.LogAndNewException("Erreur lors du chargement des options du programme. Impossible de démarrer le programme");

                }


                argsDto = LoadArgs(e, argsDto);

                PreLoadActions(argsDto, prgOptions);

                updaterManager = new UpdaterManager();
                try
                {

                    updaterManager.XmlUpdFilePath = prgOptions.UpdateXmlUri;
                    updaterManager.CheckForUpdates("launch", Assembly.GetEntryAssembly().GetName().Version.ToString());

                }
                catch (Exception ex)
                {
                    ExceptionHandlingUtils.LogAndHideException(ex, "Erreur lors de la recherche de mise à jour", true);

                }


            }
            catch (Exception ex)
            {
                ExceptionMsgBoxView.ShowException(ex);
                ExceptionHandlingUtils.LogAndEndsProgram(
                    ex,
                    EnumExitCodes.M_ERROR_LOADING_APP.ExitCodeInt,
                    "Erreur lors de la phase de chargement de la configuration et des divers traitements pré-interface");

            }

            /*
             * Bloc chargeant l'interface
             */
            try
            {

                MainWindow mainWindow = new MainWindow(prgOptions, updaterManager);
                mainWindow.ShowDialog();

            }
            catch (Exception ex)
            {
                ExceptionMsgBoxView.ShowException(ex);
                ExceptionHandlingUtils.LogAndEndsProgram(
                    ex,
                   EnumExitCodes.M_ERROR_UNKNOW_IN_APP.ExitCodeInt,
                    "Erreur lors du programme");
            }
            _logger.Info("Fin du programme");

            Environment.Exit(EnumExitCodes.OK.ExitCodeInt);

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

            if (File.Exists(CommonCst.OneShotUpdateConfFilename))
            {
                _logger.Info("Importation de la configuration à partir de {0}", CommonCst.OneShotUpdateConfFilename);
                _logger.Debug(" Lecture du fichier");
                AppOptions optionImported = OptionManager.LoadFromXml(CommonCst.OneShotUpdateConfFilename);

                _logger.Debug(" Prise en compte des nouvelles options");
                OptionManager.ChangeOptions(optionImported, prgOptions, CommonCst.OneShotUpdateConfFilename);

                File.Delete(CommonCst.OneShotUpdateConfFilename);
            }

            OptionManager.SaveOptions(prgOptions);

            if (!argsDto.LoadAfterImportExport && (argsDto.ExportConfFilePath != null || argsDto.ImportConfFilePath != null))
            {
                _logger.Info(
                    "Fin du traitement. Utiliser l'option -l pour démarrer l'application après une tâche d'export/import de la configuration");

                Environment.Exit(EnumExitCodes.M_OK_IMPORT_EXPORT_OK.ExitCodeInt);
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
                ExceptionHandlingUtils.LogAndRethrows(ex, "Erreur lors du chargement des paramètres en entrée du programme.");

            }
            return argsDto;
        }
    }
}
