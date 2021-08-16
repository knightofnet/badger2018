using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using Badger2018.business.dbb;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.Properties;
using Badger2018.services;
using Badger2018.utils;
using Badger2018.views;
using Badger2018.views.usercontrols;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;
using BadgerPluginExtender;
using IWshRuntimeLibrary;
using ExceptionHandlingUtils = BadgerCommonLibrary.utils.ExceptionHandlingUtils;
using File = System.IO.File;

namespace Badger2018
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger _logger = new Logger(CommonCst.AppLogFile, CommonCst.ConsoleLogLvl, CommonCst.FileLogLvl, "1 Mo", CommonCst.MainLogName);


        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            _logger.Debug("/***********************");
            _logger.Info("*  Application lancée (v. {0})", Assembly.GetExecutingAssembly().GetName().Version);
            _logger.Debug("***********************/");
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

            LicenceInfo licenceInfo = VerifyLicence();

            /*
            if (StringUtils.CsvStringContains(Environment.UserName.ToUpper(),
                ((string)Settings.Default["licencedUser"]).ToUpper()) && !File.Exists(Environment.UserName.ToUpper() + ".auth"))
            {
                _logger.Error("Utilisation non autorisée");
                Environment.Exit(EnumExitCodes.M_NOT_LICENCED_USER.ExitCodeInt);
            }
            */


            AppOptions prgOptions = null;
            UpdaterManager updaterManager = null;
            PluginManager pManager = null;

            // BetaSpec(prgOptions);


            AppArgsDto argsDto = null;


            /*
             * Bloc chargement de la configuration et divers traitement pré-interface
             */
            try
            {
                pManager = new PluginManager();
                pManager.LoadPluginsFromDir("dll");


                Cst.ApplicationDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();

                argsDto = new AppArgsDto();

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
                    updaterManager.IsUpdateEnabled = prgOptions.IsUpdateSvcEnable;
                    updaterManager.XmlUpdFilePath = prgOptions.UpdateXmlUri;
                    updaterManager.CheckForUpdates("launch", Assembly.GetEntryAssembly().GetName().Version.ToString());

                }
                catch (Exception ex)
                {
                    ExceptionHandlingUtils.LogAndHideException(ex, "Erreur lors de la recherche de mise à jour", true);

                }


                // DbbAccessManager.DbbPasswd = CommonCst.SqliteAppSalt + prgOptions.SqliteAppUserSalt + Environment.UserName.ToUpper();
                RemoveLegacyBadgeage(prgOptions);
                UpdateBdd(prgOptions);
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

                MainWindow mainWindow = new MainWindow(prgOptions, updaterManager, pManager, licenceInfo, argsDto);
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

        private static LicenceInfo VerifyLicence()
        {
#if DEBUG
            LicenceInfo l = new LicenceInfo();
            l.DateExpiration = new DateTime(2050, 1, 1, 1, 1, 1);
            l.NiceName = "Developper";
            l.TypeUser = 0;
            l.Username = "Developper";
            l.ReArmMail = "wolfaryx@gmail.com";
            return l;
#endif
            string licenceFile = Environment.UserName.ToUpper() + ".auth";
            if (File.Exists(licenceFile))
            {

                String fileContent = File.ReadAllText(licenceFile);
                LicenceInfo li = LicenceInfo.GetFromString(CryptUtils.Decrypt(fileContent));
                if (li != null && li.TypeUser == 0)
                {
                    _logger.Info("Licence ambassadeur");
                }
                else if (li == null || !Environment.UserName.Equals(li.Username, StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.Error("Utilisation non autorisée");

                    MessageBox.Show("La licence du programme n'est pas valide.", "Erreur de licence utilisateur", MessageBoxButton.OK, MessageBoxImage.Error);

                    Environment.Exit(EnumExitCodes.M_NOT_LICENCED_USER.ExitCodeInt);

                }
                else if (li != null && DateTime.Now > li.DateExpiration)
                {
                    _logger.Error("Utilisation non autorisée - date expirée");

                    MessageBox.Show(String.Format("La licence du programme a expiré. Contacter '{0}' pour renouveler votre licence.", li.ReArmMail),
                        "Erreur de licence utilisateur", MessageBoxButton.OK, MessageBoxImage.Error);

                    Environment.Exit(EnumExitCodes.M_NOT_LICENCED_USER.ExitCodeInt);
                }

                return li;
            }
            else
            {
                _logger.Error("Utilisation non autorisée");
                MessageBox.Show(String.Format("Le fichier de licence '{0}' n'a pas été trouvé.", licenceFile),
                      "Erreur de licence utilisateur", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(EnumExitCodes.M_NOT_LICENCED_USER.ExitCodeInt);

            }

            return null;
        }

        private void UpdateBdd(AppOptions prgOptions)
        {
            try
            {

                DbbUpdateManager dbbUpd = new DbbUpdateManager(DbbAccessManager.Instance.Connection, prgOptions.LastSqlUpdateVersion);
                if (dbbUpd.CheckUpdateRequired())
                {
                    dbbUpd.BackupDbb();
                    dbbUpd.UpdateDbb();
                }
                else
                {
                    _logger.Debug("Fichier de mise à jour BDD non detecté.");
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

            try
            {
                XmlToBddPointageConverter converter = new XmlToBddPointageConverter(Cst.PointagesDir);
                converter.Convert();
            }
            catch (DirectoryNotFoundException ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);
            }

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

            CheckAppHelper();

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

            if (prgOptions.CptCtrlStateShowned > (int) CompteurControl.CompteurState.TempsCdRealTime)
            {
                prgOptions.CptCtrlStateShowned = (int) CompteurControl.CompteurState.TempsTravailDuJour;
                OptionManager.SaveOptions(prgOptions);
                _logger.Debug("RaZ de l'état du CptCtrl");
            }

            if (argsDto.NoAutoBadgeage)
            {
                _logger.Info("Auto-badgeage non actif durant cette session");
            }

        }

        private static void CheckAppHelper()
        {
            if (!File.Exists(Cst.HelperWaveCompagnonPlayerRelPath))
            {
                MessageBox.Show(String.Format("L'application auxiliaire {0} n'est pas disponible. Impossible de continuer.",
                    Cst.HelperWaveCompagnonPlayerRelPath), "Badger2018", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception(String.Format("{0} n'est pas présent dans le dossier", Cst.HelperWaveCompagnonPlayerRelPath));
            }

            if (!File.Exists(Cst.HelperWatchDogRestartRelPath))
            {
                MessageBox.Show(String.Format("L'application auxiliaire {0} n'est pas disponible. Impossible de continuer.",
                    Cst.HelperWatchDogRestartRelPath), "Badger2018", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception(String.Format("{0} n'est pas présent dans le dossier", Cst.HelperWatchDogRestartRelPath));
            }

            if (!File.Exists(Cst.HelperGeckoDriverRelPath))
            {
                MessageBox.Show(String.Format("L'application auxiliaire {0} n'est pas disponible. Impossible de continuer.",
                    Cst.HelperGeckoDriverRelPath), "Badger2018", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception(String.Format("{0} n'est pas présent dans le dossier", Cst.HelperGeckoDriverRelPath));
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
