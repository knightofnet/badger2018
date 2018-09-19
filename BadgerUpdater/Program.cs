using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AryxDevLibrary.constants;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.cliParser;
using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.dto;
using BadgerCommonLibrary.utils;
using BadgerUpdater.business;
using BadgerUpdater.constantes;
using BadgerUpdater.dto;
using BadgerUpdater.Properties;
using BadgerUpdater.utils;
using Ionic.Zip;

namespace BadgerUpdater
{
    internal class Program
    {
        private const string TmpReleaseBackupOptFilename = "tmpFile";

        private static AltLogger _logger = new AltLogger(CommonCst.UpdaterLogFile, CommonCst.ConsoleLogLvl,
            CommonCst.FileLogLvl, "1 Mo");

        private static bool _isUpdateUpdaterAvailable = false;

        private static readonly WorkDoneStepDto WorkDone = new WorkDoneStepDto();

        private static readonly ParamsTrtDto PrgParams = new ParamsTrtDto();
        private static FileInfo _fiXmlBackupOption;
        private static FileInfo _backupBinaries;

        private static void Main(string[] args)
        {
            try
            {
                _logger.ShowDtAndLogLevel = false;

                Console.WriteLine();
                _logger.Info("<*white*>Application {0} lancée (v. {1})<*/*>",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

                Console.WriteLine();
                InitsUpdate(args);


                Console.WriteLine();
                GetVersionAppExe();

                String runName = "";
                if (PrgParams.InArgs.IsReprise())
                {
                    runName = PrgParams.InArgs.NumRunReprise;
                }
                else
                {
                    runName = runName = String.Format(Cst.RunNameTpl, DateTime.Now.ToString("yyyyMMdd-hhmmss"));
                }


                CheckIfAppRunning();
                if (!PrgParams.InArgs.IsReprise())
                {
                    CleanGenericOldBackups(Cst.RunNameTpl, PrgParams.BadgerExeFileInfo.Directory);
                }
                InitUpdFilesInfo(runName);




                Console.WriteLine();
                List<UpdateInfoDto> lstUpd = null;
                if (PrgParams.InArgs.IsSideloadUpdate)
                {
                    UpdateInfoDto updateInfo = new UpdateInfoDto();
                    updateInfo.Description = String.Format("Mise à jour chargée à partir du fichier {0}", PrgParams.InArgs.UpdateExeFile);
                    updateInfo.FileUpdate = new FileInfo(PrgParams.InArgs.UpdateExeFile).FullName;
                    updateInfo.IsActive = true;
                    updateInfo.Title = "Sideload update";
                    updateInfo.Version = new Version("9.9.999.9999");

                    lstUpd = new List<UpdateInfoDto>();
                    lstUpd.Add(updateInfo);
                }
                else
                {
                    lstUpd = SearchAndControlUpdates(runName);
                }


                if (!PrgParams.InArgs.IsSideloadUpdate)
                {
                    DoUpdaterUpdateSeconde(runName, PrgParams.BadgerExeFileInfo.Directory);
                }



                Console.WriteLine();
                DoUpdates(runName, lstUpd);

                Console.WriteLine();
                _logger.Info("<*white*>Rechargement des options<*/*>");
                Console.WriteLine();
                BackupUtils.LoadBackupOptions(PrgParams.BadgerExeFileInfo, _fiXmlBackupOption);


                Console.WriteLine();
                _logger.Info("<*white*>Mise à jour terminée ! <*/*>");
                Console.WriteLine();


                // Console.ReadLine();
                Exit(EnumExitCodes.OK.ExitCodeInt);

            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);
                Exit(EnumExitCodes.U_ERROR_UNKNOW_IN_APP.ExitCodeInt);
            }
        }

        private static void CleanGenericOldBackups(string runNameTpl, DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                if (file.Name.Matches(runNameTpl.Replace("{0}", ".*")))
                {
                    _logger.Debug("CleanGenericOldBackups : suppression de {0}", file.FullName);
                    file.Delete();
                }
            }
        }

        private static void DoUpdaterUpdateSeconde(string runName, DirectoryInfo directory)
        {
            if (!_isUpdateUpdaterAvailable)
            {
                return;
            }

            FileInfo cmdsUpdate = new FileInfo("updateBatch2.cmd");
            if (cmdsUpdate.Exists)
            {
                cmdsUpdate.Delete();
            }


            _logger.Info("Une mise à jour de l'outil de mise à jour existe.");



            string cmdWdStr = String.Format("cd \"{0}\"",
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                );
            string cmdStr = String.Format("\"{0}\" -f \"{1}\" -v {2} -a \"{3}\" {4} {6} -r {5}",
            new[]{
                Assembly.GetExecutingAssembly().Location,
                PrgParams.InArgs.XmlUpdateFile,
                PrgParams.InArgs.VergionTarget.ToString(),
                PrgParams.InArgs.BadgerAppExe,
                PrgParams.InArgs.LaunchAppIfSucess ? "-l" : "",
                runName,
                PrgParams.InArgs.IsForceDebug ? "-d" : "",
            }
                );

            using (StreamWriter fileStream = FileUtils.NewStreamWriterCstEncoding(cmdsUpdate.Name, FileMode.CreateNew, EnumOtherEncoding.IBM850))
            {
                fileStream.WriteLine("@echo off");

                fileStream.WriteLine(cmdWdStr);
                fileStream.WriteLine(@"xcopy ..\TMP\* .. /E /Y");
                fileStream.WriteLine("TIMEOUT /T 2 /NOBREAK");
                fileStream.WriteLine(cmdStr);
#if DEBUG
                fileStream.WriteLine("pause");
#endif
                fileStream.WriteLine("exit");
            }


            _logger.Info("Démarrage de la mise à jour");

            ProcessStartInfo processStartInfo = new ProcessStartInfo(cmdsUpdate.FullName);
            processStartInfo.WorkingDirectory = cmdsUpdate.DirectoryName;
            Process.Start(processStartInfo);

            Exit(EnumExitCodes.U_OK_UPD_UPDATE_RELAUNCH.ExitCodeInt);
        }



        private static void CheckIfAppRunning()
        {
            Process processExeBadger =
                Process.GetProcessesByName(PrgParams.BadgerExeFileInfo.Name.Replace(PrgParams.BadgerExeFileInfo.Extension, ""))
                    .FirstOrDefault();


            if (processExeBadger != null)
            {
                string[] colorsWait = { "gray", "white", "yellow", "magenta", "red", "DarkRed" };
                int i = 0;
                while (!processExeBadger.HasExited && i < colorsWait.Length)
                {
                    _logger.Info("<*{0}*>Attente de la fermeture du programme...<*/*>", colorsWait[i++]);
                    Thread.Sleep(1000);
                }
                if (!processExeBadger.HasExited)
                {
                    _logger.Error(
                        "L'application Badger2018 est toujours active. Impossible de procéder à la mise à jour.");
                    Exit(EnumExitCodes.U_ERROR_WAIT_PROGRAM_CLOSE.ExitCodeInt);
                }
                else
                {
                    _logger.Debug("L'application est terminée.");
                }
            }
            else
            {
                _logger.Debug("L'application est terminée et n'est plus en mémoire.");
            }
        }

        private static void GetVersionAppExe()
        {
            try
            {
                PrgParams.BadgerExeFileInfo = new FileInfo(PrgParams.InArgs.BadgerAppExe);
                PrgParams.BadgerExeVersion =
                    Version.Parse(FileVersionInfo.GetVersionInfo(PrgParams.BadgerExeFileInfo.FullName).ProductVersion);
                _logger.Info("Version de l'executable de Badger2018: {0}", PrgParams.BadgerExeVersion.ToString());

                /*
                string sevenZexeStr = (string) Settings.Default["SevenZipFilePath"];
                FileInfo sevenZexe;
                
                if (StringUtils.IsNullOrWhiteSpace(sevenZexeStr))
                {
                    sevenZexe = Finds7ZExe();
                    Settings.Default["SevenZipFilePath"] = sevenZexe.FullName;
                    Settings.Default.Save();
                }
                else
                {
                    sevenZexe = new FileInfo(sevenZexeStr);
                }

                if (sevenZexe == null)
                {
                    throw new FileNotFoundException("");
                }
                 * */
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndEndsProgram(ex, 20,
                    "Une erreur est survenue lors de la préparation de la mise à jour. Le programme va se terminer."
                    );

                // And the program ends here...
            }
        }

        private static void InitsUpdate(string[] args)
        {
            AppArgsParser argsParser = null;
            try
            {
                _logger.Info("Initiation de la mise à jour");
                _logger.Info("============================");


                Console.WriteLine();
                _logger.Info("<*white*>Lecture des paramètres en entrée :<*/*>");
                Console.WriteLine();

                _logger.Debug("Lecteurs des options et paramètres");
                argsParser = new AppArgsParser();
                PrgParams.InArgs = argsParser.ParseDirect(args);

                if (PrgParams.InArgs.IsForceDebug)
                {
                    _logger.Info("Passage de log en debug");
                    _logger = new AltLogger(_logger.FileLog.FullName, Logger.LogLvl.DEBUG, Logger.LogLvl.DEBUG);
                }

                _logger.Info("Paramètres en entrée :");
                _logger.Info(" Chargement manuelle de la mise à jour : {0}", PrgParams.InArgs.IsSideloadUpdate ? "oui" : "non");
                if (PrgParams.InArgs.IsSideloadUpdate)
                {
                    _logger.Info(" Fichier exe de la mise à jour : {0}", PrgParams.InArgs.UpdateExeFile);
                }
                else
                {
                    _logger.Info(" Fichier xml des mises à jour : {0}", PrgParams.InArgs.XmlUpdateFile);
                }
                _logger.Info(" Version cible: {0}", PrgParams.InArgs.VergionTarget);
                _logger.Info(" Exécutable Badger2018: {0}", PrgParams.InArgs.BadgerAppExe);
                _logger.Info(" Redémarrer Badger2018 si succés: {0}", PrgParams.InArgs.LaunchAppIfSucess ? "oui" : "non");
                _logger.Info(" Reprise de la mise à jour: {0}", PrgParams.InArgs.IsReprise() ? "oui (" + PrgParams.InArgs.NumRunReprise + ")" : "non");
            }
            catch (CliParsingException ex)
            {
                _logger.Error("Erreur lors de la lecture des paramètres en entrée :");
                _logger.Error(ex.Message);
                argsParser.ShowSyntax();

                Exit(EnumExitCodes.U_ERROR_IN_PARAMS.ExitCodeInt);
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "Une erreur est survenue lors de la lecture des paramètres en entrée. Le programme va se terminer."
                    );

                Exit(EnumExitCodes.U_ERROR_IN_PARAMS.ExitCodeInt);

            }
        }

        private static List<UpdateInfoDto> SearchAndControlUpdates(string runName)
        {

            _logger.Info("<*white*>Recherche des mises à jour :<*/*>");
            Console.WriteLine();


            _logger.Debug("RunName: {0}", runName);

            UpdaterManager updMgr = new UpdaterManager();
            updMgr.XmlUpdFilePath = PrgParams.InArgs.XmlUpdateFile;

            _logger.Info("Vérification des mises à jour");
            updMgr.CheckForUpdates(runName, PrgParams.BadgerExeVersion.ToString(), PrgParams.InArgs.VergionTarget);

            if (!updMgr.IsUpdaterFileLoaded)
            {
                _logger.Error(
                    "Impossible de trouver le fichier détaillant les mises à jours (fichier recherché : {0}). Vérifiez que ce dernier existe, et qu'il est accessible en lecteur.",
                    PrgParams.InArgs.XmlUpdateFile);

                Exit(EnumExitCodes.U_ERROR_IN_PARAMS_UPDXML_FILM.ExitCodeInt);
            }

            if (!updMgr.IsNewUpdateAvalaible)
            {
                _logger.Info("Badger2018 est déjà à jour (version de Badger2018 : {0}). Le programme de mise à jour va s'arrêter.", PrgParams.BadgerExeVersion);

                Exit(EnumExitCodes.U_OK_NO_UPDATE_NEEDED.ExitCodeInt);
            }

            List<UpdateInfoDto> lstUpd =
                updMgr.ListReleases.Where(r => r.Version.CompareTo(PrgParams.BadgerExeVersion) > 0).ToList();
            bool updAval = lstUpd.Any();

            if (!updAval)
            {
                _logger.Info("Badger2018 est déjà à jour (version de Badger2018 : {0}). Le programme de mise à jour va s'arrêter.", PrgParams.BadgerExeVersion);

                Exit(EnumExitCodes.U_OK_NO_UPDATE_NEEDED.ExitCodeInt);
            }
            return lstUpd;
        }

        private static void DoUpdates(string runName, List<UpdateInfoDto> lstUpd)
        {

            _logger.Info("Démarrage de la mise à jour. Run {0}", runName);
            Console.WriteLine("================================================");



            try
            {
                Console.WriteLine();
                _logger.Info("<*white*>Mise à jour :<*/*>");
                Console.WriteLine();

                List<UpdateInfoDto> listUpds = lstUpd.OrderBy(r => r.Version).ToList();
                for (int i = 0; i < listUpds.Count; i++)
                {
                    UpdateInfoDto release = listUpds[i];

                    TrtOneRelease(release);

                    if (!PrgParams.InArgs.IsSideloadUpdate && i < listUpds.Count - 1)
                    {
                        DoUpdaterUpdateSeconde(runName, PrgParams.BadgerExeFileInfo.Directory);
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);

                Console.WriteLine();
                _logger.Info("<*white*>Retour en arrière :<*/*>");
                Console.WriteLine();

                try
                {
                    bool outBool = false;
                    BackupUtils.LoadBackupBinaries(_backupBinaries, PrgParams.BadgerExeFileInfo.Directory, out outBool);
                    BackupUtils.LoadBackupOptions(PrgParams.BadgerExeFileInfo, _fiXmlBackupOption);

                    Exit(EnumExitCodes.U_ERROR_UPD_ROOLBACK_OK.ExitCodeInt);
                }
                catch (Exception exIn)
                {
                    ExceptionHandlingUtils.LogAndHideException(exIn, "Erreur lors du retour en arrière");
                    Exit(EnumExitCodes.U_ERROR_UPD_ROOLBACK_KO.ExitCodeInt);
                }

            }



        }

        private static void InitUpdFilesInfo(string runName)
        {
            if (!PrgParams.InArgs.IsReprise())
            {
                Console.WriteLine();
                _logger.Info("<*white*>Sauvegarde de l'application :<*/*>");
                Console.WriteLine();
                _fiXmlBackupOption = BackupUtils.CreateBackupOptions(PrgParams.BadgerExeFileInfo, runName);
                _backupBinaries = BackupUtils.CreateBackupFiles(PrgParams.BadgerExeFileInfo.Directory, runName);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                _logger.Info("<*white*>Reprise de la mise à jour :<*/*>");
                Console.WriteLine();
                _fiXmlBackupOption = new FileInfo(Path.Combine(PrgParams.BadgerExeFileInfo.DirectoryName, String.Format("{0}.xml", runName)));
                _backupBinaries = new FileInfo(Path.Combine(PrgParams.BadgerExeFileInfo.DirectoryName, String.Format("{0}.zip", runName)));
                Console.WriteLine();
            }
        }

        private static void CleanAfterInstall()
        {
            if (_backupBinaries.Exists)
            {
                _logger.Debug("Suppression du backup de l'application...");
                _backupBinaries.Delete();
                _logger.Debug(" Réalisée: {0}", _backupBinaries.Exists);
            }


            if (_fiXmlBackupOption.Exists)
            {
                _logger.Debug("Suppression du backup des options...");
                _fiXmlBackupOption.Delete();
                _logger.Debug(" Réalisée: {0}", _backupBinaries.Exists);
            }


            FileInfo tmpRlsOptFileInfo = new FileInfo(
                Path.Combine(PrgParams.BadgerExeFileInfo.DirectoryName,
                    String.Format("{0}.xml", TmpReleaseBackupOptFilename))
                );
            if (tmpRlsOptFileInfo.Exists)
            {
                _logger.Debug("Suppression du backup intermédiaire des options...");
                tmpRlsOptFileInfo.Delete();
                _logger.Debug(" Réalisée: {0}", tmpRlsOptFileInfo.Exists);
            }

            DirectoryInfo dirTmp = new DirectoryInfo(Path.Combine(PrgParams.BadgerExeFileInfo.Directory.FullName, "TMP"));
            if (dirTmp.Exists)
            {
                _logger.Debug("Suppression du dossier temporaire de mise à jour...");
                dirTmp.Delete(true);
                _logger.Debug(" Réalisée: {0}", tmpRlsOptFileInfo.Exists);
            }
        }


        private static void TrtOneRelease(UpdateInfoDto release)
        {

            _logger.Info("Traitement de la mise à jour");
            Console.WriteLine("----------------------------");
            _logger.Info("Badger2018  : version {0} - {1}", release.Version, release.Title);
            _logger.Info("Description : {0}", release.Description);
            Console.WriteLine();

            FileInfo updateFileInfo = new FileInfo(release.FileUpdate);

            if (!updateFileInfo.Exists)
            {
                ExceptionHandlingUtils.LogAndNewException("Le fichier contenant la mise à jour n'existe pas");
            }

            if (ContainsOneShotUpdate(updateFileInfo, "AlBadger2018"))
            {
                _logger.Debug("La mise à jour contient une mise à jour des options.");
                release.NeedIntermediateLaunch = true;
            }

            // Si NeedLaunch : Export options vers tmpFile.xml
            FileInfo optionXmlSaved = null;
            if (release.NeedIntermediateLaunch)
            {
                optionXmlSaved = BackupUtils.CreateBackupOptions(PrgParams.BadgerExeFileInfo, TmpReleaseBackupOptFilename);
            }

            // Décompression
            BackupUtils.LoadBackupBinaries(updateFileInfo, PrgParams.BadgerExeFileInfo.Directory, out _isUpdateUpdaterAvailable, "AlBadger2018");

            if (release.NeedIntermediateLaunch)
            {
                BackupUtils.LoadBackupOptions(PrgParams.BadgerExeFileInfo, optionXmlSaved);
            }

        }


        public static bool ContainsOneShotUpdate(FileInfo archiveBackup, string password)
        {
            try
            {
                _logger.Info("Recherche du fichier de mise à jour unique");

                using (ZipFile zipRelease = new ZipFile(archiveBackup.FullName))
                {
                    if (password != null)
                    {
                        zipRelease.Password = password;
                    }

                    if (
                        zipRelease.EntriesSorted.Any(
                            r => !(r.IsDirectory) && r.FileName.Equals(CommonCst.OneShotUpdateConfFilename)))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }

            return false;

        }

        private static void Exit(int exitCode = 0)
        {
            Console.WriteLine();

            if (exitCode > 0 && exitCode != EnumExitCodes.U_OK_UPD_UPDATE_RELAUNCH.ExitCodeInt)
            {
                Console.WriteLine("Appuyer sur la touche \"Entrer\" pour continuer (ou attendez 5sec)...");

                string line;
                ConsoleUtils.Reader.TryReadLine(out line, 5000);
            }

            if (exitCode == EnumExitCodes.OK.ExitCodeInt
                || (exitCode == EnumExitCodes.U_OK_NO_UPDATE_NEEDED.ExitCodeInt
                    && PrgParams.InArgs.IsReprise()))
            {
                CleanAfterInstall();
            }


            if (exitCode <= EnumExitCodes.U_OK_NO_UPDATE_NEEDED.ExitCodeInt
                && PrgParams.InArgs.LaunchAppIfSucess
                && !_isUpdateUpdaterAvailable)
            {
                _logger.Info("<*green*>Démarrage de Badger2018<*/*>");
                ProcessStartInfo processStartInfo = new ProcessStartInfo(PrgParams.BadgerExeFileInfo.FullName);
                processStartInfo.WorkingDirectory = PrgParams.BadgerExeFileInfo.DirectoryName;
                Process.Start(processStartInfo);

            }
            Console.WriteLine();
            Environment.Exit(exitCode);
        }

        private static FileInfo Finds7ZExe()
        {
            string target = "7z.exe";
            List<String> fileList = new List<string>();

            List<DirectoryInfo> dirsToSearchs = new List<DirectoryInfo>();
            dirsToSearchs.Add(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)));
            dirsToSearchs.Add(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)));

            dirsToSearchs.AddRange(DriveInfo.GetDrives().Where(r => r.DriveType == DriveType.Fixed).Select(r => r.RootDirectory).ToList());

            foreach (DirectoryInfo dirToSearch in dirsToSearchs)
            {
                _logger.Debug("Recherche {0} dans {1}", target, dirToSearch.FullName);

                SearchDirectory(dirToSearch, fileList, target, true);
                if (fileList.Any())
                {
                    return new FileInfo(fileList[0]);
                }
            }

            return null;
        }

        private static bool SearchDirectory(DirectoryInfo dirInfo, List<string> fileList, String targetFileName, bool breakAtFirst = false)
        {
            try
            {
                if (dirInfo.GetDirectories().Any(subdirInfo => SearchDirectory(subdirInfo, fileList, targetFileName, breakAtFirst)))
                {
                    return true;
                }
            }
            catch
            {
            }
            try
            {
                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                {
                    if (fileInfo.Name.Contains(targetFileName))
                    {
                        fileList.Add(fileInfo.FullName);
                        if (breakAtFirst)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }



    }

}
