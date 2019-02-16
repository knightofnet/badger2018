using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.Properties;
using Badger2018.services;
using Badger2018.utils;
using Badger2018.views;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;
using BadgerPluginExtender;
using BadgerPluginExtender.dto;
using IWshRuntimeLibrary;
using File = System.IO.File;

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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            _logger.Info("/***********************");
            _logger.Info("*  Application lancée (v. {0})", Assembly.GetExecutingAssembly().GetName().Version);
            _logger.Info("***********************/");
            if (ProcessUtils.CountAppInstanceOf(Assembly.GetExecutingAssembly().Location) > 1)
            {
                _logger.Error("Une autre instance de Badger2018 est déjà chargée...");
                Environment.Exit(EnumExitCodes.M_ALREADY_RUNNING_INSTANCE.ExitCodeInt);
            }


            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            if (StringUtils.CsvStringContains(Environment.UserName.ToUpper(),
                ((string)Settings.Default["licencedUser"]).ToUpper()) && !System.IO.File.Exists(Environment.UserName.ToUpper() + ".auth"))
            {
                _logger.Error("Utilisation non autorisée");
                Environment.Exit(EnumExitCodes.M_NOT_LICENCED_USER.ExitCodeInt);
            }


            AppOptions prgOptions = null;
            UpdaterManager updaterManager = null;
            PluginManager pManager = null;

            // BetaSpec(prgOptions);





            /*
             * Bloc chargement de la configuration et divers traitement pré-interface
             */
            try
            {
                pManager = new PluginManager();
                pManager.LoadPluginsFromDir("dll");


                Cst.ApplicationDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();

                AppArgsDto argsDto = new AppArgsDto();

                _logger.Debug("Chargement des options du programme");
                prgOptions = OptionManager.LoadOptions();
                if (prgOptions == null)
                {
                    ExceptionHandlingUtils.LogAndNewException("Erreur lors du chargement des options du programme. Impossible de démarrer le programme");

                }
                
                if (StringUtils.IsNullOrWhiteSpace(prgOptions.SqliteAppUserSalt) || prgOptions.SqliteAppUserSalt.Equals("NULL"))
                {
                    prgOptions.SqliteAppUserSalt = StringUtils.RandomString(32, @"AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789-+%.;,:_*/&é()[]");

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


               // DbbAccessManager.DbbPasswd = CommonCst.SqliteAppSalt + prgOptions.SqliteAppUserSalt + Environment.UserName.ToUpper();
                RemoveLegacyBadgeage(prgOptions);
                UpdateBdd();
                ConvertsXmlPointageToDEbb();
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

                MainWindow mainWindow = new MainWindow(prgOptions, updaterManager, pManager);
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

        private void UpdateBdd()
        {
            try
            {
                String fileUpdBdd = String.Format("sqlUpd-{0}.sql", Assembly.GetExecutingAssembly().GetName().Version);
                if (File.Exists(fileUpdBdd))
                {
                    _logger.Info("Fichier de mise à jour BDD detecté ({0}). Exécution", fileUpdBdd);

                    SqlLiteUtils.ExecuteContentFile(DbbAccessManager.Instance.Connection, fileUpdBdd);
                    System.IO.File.Delete(fileUpdBdd);
                }
                else
                {
                    _logger.Debug("Fichier de mise à jour BDD non detecté ({0}).", fileUpdBdd);
                }
            }
            catch (Exception ex)
            {
                ExceptionMsgBoxView.ShowException(ex);
                ExceptionHandlingUtils.LogAndEndsProgram(
                    ex,
                   EnumExitCodes.M_ERROR_UPDATE_BDD.ExitCodeInt,
                    "Erreur lors du programme");
            }
        }

        private void ConvertsXmlPointageToDEbb()
        {

            XmlToBddPointageConverter converter = new XmlToBddPointageConverter(Cst.PointagesDir);
            converter.Convert();

            ServicesMgr.Instance.BadgeagesServices.RemoveDuplicatesBadgeages();
        }

        private void RemoveLegacyBadgeage(AppOptions prgOptions)
        {
            if (!prgOptions.IsRemoveLegacyShorcutFirefox)
            {
                return;
            }

            try
            {

                List<IWshShortcut> listShortcut =
                       ShortcutUtils.GetShortcutsInDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Startup));

                foreach (IWshShortcut shortcut in listShortcut)
                {
                    if (shortcut.TargetPath.Contains("firefox") && shortcut.Arguments.Contains(prgOptions.Uri))
                    {
                        _logger.Info("Raccourci \"{0}\" : suppression de l'URI vers la page de badgeage", shortcut.FullName);
                        shortcut.Arguments = shortcut.Arguments.Replace(prgOptions.Uri, "");
                        shortcut.Arguments = shortcut.Arguments.Replace("\"\"", "");

                        shortcut.Save();
                        _logger.Debug(" > Terminé");

                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "Erreur lors de la suppression du raccourci au démarrage", true);
            }
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

            if (System.IO.File.Exists(CommonCst.OneShotUpdateConfFilename))
            {
                _logger.Info("Importation de la configuration à partir de {0}", CommonCst.OneShotUpdateConfFilename);
                _logger.Debug(" Lecture du fichier");
                AppOptions optionImported = OptionManager.LoadFromXml(CommonCst.OneShotUpdateConfFilename);

                _logger.Debug(" Prise en compte des nouvelles options");
                OptionManager.ChangeOptions(optionImported, prgOptions, CommonCst.OneShotUpdateConfFilename);

                System.IO.File.Delete(CommonCst.OneShotUpdateConfFilename);
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


        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;
            ExceptionMsgBoxView.ShowException(ex, "Erreur non prévue");
            ExceptionHandlingUtils.LogAndEndsProgram(
                ex,
               EnumExitCodes.M_ERROR_UNKNOW_IN_APP.ExitCodeInt,
                "Erreur non catchée lors du programme");

        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            AggregateException ex = args.Exception;

            bool isExceptionTrapped = false;

            if (ex.InnerExceptions.Count == 1)
            {
                StackTrace stackTrace = new StackTrace(ex.InnerExceptions[0]);
                foreach (var a in stackTrace.GetFrames())
                {
                    _logger.Debug("a.GetMethod().DeclaringType.FullName : ##{0}##", a.GetMethod().DeclaringType.FullName);
                    _logger.Debug("a.GetMethod().DeclaringType.Name     : ##{0}##", a.GetMethod().DeclaringType.Name);
                    _logger.Debug("a.GetMethod().DeclaringType.Namespace: ##{0}##", a.GetMethod().DeclaringType.Namespace);

                    if ("AudioSwitcher.AudioApi.Observables".Equals(a.GetMethod().DeclaringType.Namespace))
                    {
                        isExceptionTrapped = true;

                    }
                }
                //if (stackTrace.GetFrames().Where(r=>r.GetMethod().DeclaringType.Name)
            }

            if (isExceptionTrapped)
            {
                _logger.Warn("On prend l'exception, et on doucement on la glisse sous le tapis...");
                _logger.Warn("Chut ! [Le programme peut continuer]");
                return;
            }

            StringBuilder strException = new StringBuilder();
            foreach (Exception exInner in ex.InnerExceptions)
            {

                ExceptionHandlingUtils.LogAndHideException(exInner, "InnerException");
                strException.AppendLine(String.Format("Exception : {0}", exInner.GetType().Name));
                strException.AppendLine(String.Format("Message   : {0}", exInner.Message));
                strException.AppendLine(String.Format("StackTrace: {0}", exInner.StackTrace));
                strException.AppendLine("---");
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() =>
                {
                    ExceptionMsgBoxView.ShowException(ex.Flatten(), "Erreur non prévue" + Environment.NewLine + "---" + Environment.NewLine + strException);
                    Environment.Exit(EnumExitCodes.U_ERROR_UNKNOW_IN_APP.ExitCodeInt);
                })
                );

        }
    }
}
