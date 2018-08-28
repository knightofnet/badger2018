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

        private static string[] filesIgnored =
        {
            "Resources/AryxDevLibrary.dll",
            "Resources/BadgerCommonLibrary.dll",
            "Resources/BadgerUpdater.exe",
            "Resources/Interop.IWshRuntimeLibrary.dll",
            "Resources/Ionic.Zip.Reduced.dll",
            "Resources/updateBadger.cmd",
            "Resources/logUpd.log",

        };

        private static readonly ParamsTrtDto prgParams = new ParamsTrtDto();

        private static void Main(string[] args)
        {
            try
            {
                _logger.ShowDtAndLogLevel = false;


                Console.WriteLine("");
                _logger.Info("<*white*>Application Updater lancée (v. {0})<*/*>",
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

                Console.WriteLine("");
                InitsUpdate(args);

                Console.WriteLine("");
                GetVersionAppExe();

                String runName = "";
                if (prgParams.InArgs.IsReprise())
                {
                    runName = prgParams.InArgs.NumRunReprise;
                }
                else
                {
                    runName = runName = String.Format(Cst.RunNameTpl, DateTime.Now.ToString("yyyyMMdd-hhmmss"));
                }


                if (!prgParams.InArgs.IsReprise())
                {
                    CleanGenericOldBackups(Cst.RunNameTpl, prgParams.BadgerExeFileInfo.Directory);
                }

                Console.WriteLine("");
                List<UpdateInfoDto> lstUpd = SearchAndControlUpdates(runName);

                CheckIfAppRunning();
                DoUpdaterUpdate(runName);



                Console.WriteLine("");
                DoUpdates(runName, lstUpd);

                Console.WriteLine("");
                _logger.Info("<*white*>Mise à jour terminée ! <*/*>");
                Console.WriteLine("");


                if (prgParams.InArgs.LaunchAppIfSucess)
                {
                    _logger.Info("<*green*>Démarrage de Badger2018<*/*>");
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(prgParams.BadgerExeFileInfo.FullName);
                    processStartInfo.WorkingDirectory = prgParams.BadgerExeFileInfo.DirectoryName;
                    Process.Start(processStartInfo);

                }

                Console.ReadLine();
                Exit(EnumExitCodes.OK.ExitCodeInt);

            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);
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
            string cmdStr = String.Format("\"{0}\" -f \"{1}\" -v {2} -a \"{3}\" {4} -r {5}",
            new[]{
                Assembly.GetExecutingAssembly().Location,
                prgParams.InArgs.XmlUpdateFile,
                prgParams.InArgs.VergionTarget.ToString(),
                prgParams.InArgs.BadgerAppExe,
                prgParams.InArgs.LaunchAppIfSucess ? "-l" : "",
                runName
            }
                );

            using (StreamWriter fileStream = FileUtils.NewStreamWriterCstEncoding(cmdsUpdate.Name, FileMode.CreateNew, EnumOtherEncoding.IBM850))
            {
                fileStream.WriteLine("@echo off");

                fileStream.WriteLine(cmdWdStr);
                fileStream.WriteLine(@"xcopy ..\BKP\* .. /E /Y");
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

            Exit();
        }

        private static void DoUpdaterUpdate(string runName)
        {
            FileInfo cmdsUpdate = new FileInfo("updateBatch.cmd");
            if (cmdsUpdate.Exists)
            {
                cmdsUpdate.Delete();
            }


            FileInfo fiUpdate = new FileInfo("updaterPckg.exe");
            if (!fiUpdate.Exists)
            {
                return;
            }


            _logger.Info("Une mise à jour de l'outil de mise à jour existe.");



            string cmdWdStr = String.Format("cd \"{0}\"",
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                );
            string cmdStr = String.Format("\"{0}\" -f \"{1}\" -v {2} -a \"{3}\" {4} -r {5}",
            new[]{
                Assembly.GetExecutingAssembly().Location,
                prgParams.InArgs.XmlUpdateFile,
                prgParams.InArgs.VergionTarget.ToString(),
                prgParams.InArgs.BadgerAppExe,
                prgParams.InArgs.LaunchAppIfSucess ? "-l" : "",
                runName
            }
                );

            using (StreamWriter fileStream = FileUtils.NewStreamWriterCstEncoding(cmdsUpdate.Name, FileMode.CreateNew, EnumOtherEncoding.IBM850))
            {
                fileStream.WriteLine("@echo off");
                fileStream.WriteLine("IF [%1]==[] goto relBootstraper");
                fileStream.WriteLine("IF %1 EQU 0 goto startUpdateExe");
                fileStream.WriteLine("IF %1 EQU 1 goto relaunchUpdater");
                fileStream.WriteLine("GOTO:eof");
                fileStream.WriteLine("");

                fileStream.WriteLine(":relBootstraper");
                fileStream.WriteLine("start \"\" \"{0}\" 1", cmdsUpdate.FullName);
                fileStream.WriteLine("GOTO:eof");
                fileStream.WriteLine("");

                fileStream.WriteLine(":startUpdateExe");
                fileStream.WriteLine("TIMEOUT /T 5 /NOBREAK");
                fileStream.WriteLine("start \"\" /W \"{0}\" -o", fiUpdate.FullName);
#if DEBUG
                fileStream.WriteLine("pause");
#endif
                fileStream.WriteLine("GOTO:eof");

                fileStream.WriteLine("");
                fileStream.WriteLine(":relaunchUpdater");
                fileStream.WriteLine(cmdWdStr);
                fileStream.WriteLine("del \"{0}\"", fiUpdate.FullName);
                fileStream.WriteLine("TIMEOUT /T 2 /NOBREAK");
                fileStream.WriteLine(cmdStr);
#if DEBUG
                fileStream.WriteLine("pause");
#endif
                fileStream.WriteLine("GOTO:eof");

                fileStream.WriteLine("exit");
            }





            _logger.Info("Démarrage de la mise à jour");

            ProcessStartInfo processStartInfo = new ProcessStartInfo(cmdsUpdate.FullName, "0");
            processStartInfo.WorkingDirectory = cmdsUpdate.DirectoryName;
            Process.Start(processStartInfo);

            Exit();
        }

        private static void CheckIfAppRunning()
        {
            Process processExeBadger =
                Process.GetProcessesByName(prgParams.BadgerExeFileInfo.Name.Replace(prgParams.BadgerExeFileInfo.Extension, ""))
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
                    Exit(EnumExitCodes.ERROR_WAIT_PROGRAM_CLOSE.ExitCodeInt);
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
                prgParams.BadgerExeFileInfo = new FileInfo(prgParams.InArgs.BadgerAppExe);
                prgParams.BadgerExeVersion =
                    Version.Parse(FileVersionInfo.GetVersionInfo(prgParams.BadgerExeFileInfo.FullName).ProductVersion);
                _logger.Info("Version de l'executable de Badger2018: {0}", prgParams.BadgerExeVersion.ToString());

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
            try
            {
                _logger.Info("Initiation de la mise à jour");
                _logger.Info("============================");


                Console.WriteLine("");
                _logger.Info("<*white*>Lecture des paramètres en entrée :<*/*>");
                Console.WriteLine("");

                prgParams.InArgs = LoadArgs(args);

                _logger.Info("Paramètres en entrée :");
                _logger.Info(" Fichier xml des mises à jour : {0}", prgParams.InArgs.XmlUpdateFile);
                _logger.Info(" Version cible: {0}", prgParams.InArgs.VergionTarget);
                _logger.Info(" Exécutable Badger2018: {0}", prgParams.InArgs.BadgerAppExe);
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndEndsProgram(ex, EnumExitCodes.ERROR_IN_PARAMS.ExitCodeInt,
                    "Une erreur est survenue lors de la lecture des paramètres en entrée. Le programme va se terminer."
                    );
                // And the program ends here...
            }
        }

        private static List<UpdateInfoDto> SearchAndControlUpdates(string runName)
        {

            _logger.Info("<*white*>Recherche des mises à jour :<*/*>");
            Console.WriteLine("");


            _logger.Debug("RunName: {0}", runName);

            UpdaterManager updMgr = new UpdaterManager();
            updMgr.XmlUpdFilePath = prgParams.InArgs.XmlUpdateFile;

            _logger.Info("Vérification des mises à jour");
            updMgr.CheckForUpdates(runName, prgParams.BadgerExeVersion.ToString(), prgParams.InArgs.VergionTarget);

            if (!updMgr.IsUpdaterFileLoaded)
            {
                _logger.Info("Impossible de trouver le fichier contenant les mises à jours");

                Exit(EnumExitCodes.ERROR_IN_PARAMS_UPDXML_FILM.ExitCodeInt);
            }

            if (!updMgr.IsNewUpdateAvalaible)
            {
                _logger.Info("Badger2018 est déjà à jour (0).");

                Exit(EnumExitCodes.OK_NO_UPDATE_NEEDED.ExitCodeInt);
            }

            List<UpdateInfoDto> lstUpd =
                updMgr.ListReleases.Where(r => r.Version.CompareTo(prgParams.BadgerExeVersion) > 0).ToList();
            bool updAval = lstUpd.Any();

            if (!updAval)
            {
                _logger.Info("Badger2018 est déjà à jour (1).");

                Exit(EnumExitCodes.OK_NO_UPDATE_NEEDED.ExitCodeInt);
            }
            return lstUpd;
        }

        private static void DoUpdates(string runName, List<UpdateInfoDto> lstUpd)
        {
            FileInfo backupOptions = null;
            FileInfo backupBinaries = null;

            _logger.Info("Démarrage de la mise à jour. Run {0}", runName);
            Console.WriteLine("================================================");

            if (!prgParams.InArgs.IsReprise())
            {
                Console.WriteLine("");
                _logger.Info("<*white*>Sauvegarde de l'application :<*/*>");
                Console.WriteLine("");
                backupOptions = CreateBackupOptions(prgParams.BadgerExeFileInfo, runName);
                backupBinaries = CreateBackupFiles(prgParams.BadgerExeFileInfo.Directory, runName);
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("");
                _logger.Info("<*white*>Reprise de la mise à jour :<*/*>");
                Console.WriteLine("");
                backupOptions = new FileInfo(Path.Combine(prgParams.BadgerExeFileInfo.DirectoryName, String.Format("{0}.xml", runName)));
                backupBinaries = new FileInfo(Path.Combine(prgParams.BadgerExeFileInfo.DirectoryName, String.Format("{0}.zip", runName)));
                Console.WriteLine("");
            }

            try
            {
                Console.WriteLine("");
                _logger.Info("<*white*>Mise à jour :<*/*>");
                Console.WriteLine("");

                foreach (UpdateInfoDto release in lstUpd.OrderBy(r => r.Version))
                {

                    TrtOneRelease(release);
                    DoUpdaterUpdateSeconde(runName, prgParams.BadgerExeFileInfo.Directory);
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);

                Console.WriteLine("");
                _logger.Info("<*white*>Retour en arrière :<*/*>");
                Console.WriteLine("");

                try
                {
                    LoadBackupBinaries(backupBinaries, prgParams.BadgerExeFileInfo.Directory);
                    LoadBackupOptions(prgParams.BadgerExeFileInfo, backupOptions);
                }
                finally
                {
                    Exit(50);
                }
            }


            if (backupBinaries.Exists)
            {
                _logger.Debug("Suppression du backup de l'application...");
                backupBinaries.Delete();
                _logger.Debug(" Réalisée: {0}", backupBinaries.Exists);
            }

            LoadBackupOptions(prgParams.BadgerExeFileInfo, backupOptions);

            if (backupOptions.Exists)
            {
                _logger.Debug("Suppression du backup des options...");
                backupOptions.Delete();
                _logger.Debug(" Réalisée: {0}", backupBinaries.Exists);
            }


            FileInfo tmpRlsOptFileInfo = new FileInfo(
                Path.Combine(prgParams.BadgerExeFileInfo.DirectoryName,
                String.Format("{0}.xml", TmpReleaseBackupOptFilename))
            );
            if (tmpRlsOptFileInfo.Exists)
            {
                _logger.Debug("Suppression du backup intermédiaire des options...");
                tmpRlsOptFileInfo.Delete();
                _logger.Debug(" Réalisée: {0}", tmpRlsOptFileInfo.Exists);
            }





        }




        private static void TrtOneRelease(UpdateInfoDto release)
        {

            _logger.Info("Traitement de la mise à jour");
            Console.WriteLine("----------------------------");
            _logger.Info("Badger2018  : version {0} - {1}", release.Version, release.Title);
            _logger.Info("Description : {0}", release.Description);
            Console.WriteLine("");

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
                optionXmlSaved = CreateBackupOptions(prgParams.BadgerExeFileInfo, TmpReleaseBackupOptFilename);
            }

            // Décompression
            LoadBackupBinaries(updateFileInfo, prgParams.BadgerExeFileInfo.Directory, "AlBadger2018");

            if (release.NeedIntermediateLaunch)
            {
                LoadBackupOptions(prgParams.BadgerExeFileInfo, optionXmlSaved);
            }

        }




        private static FileInfo CreateBackupFiles(DirectoryInfo directory, string backupName)
        {
            try
            {

                _logger.Info("Sauvegarde des fichiers de l'application...");

                List<FileSystemInfo> listFiles = new List<FileSystemInfo>(5);
                List<FileInfo> rawListFile = FileUtils.GetFilesRecursively(directory);

                listFiles.AddRange(rawListFile);


                String zipArchiveMonth = Path.Combine(directory.FullName, String.Format("{0}.zip", backupName));

                foreach (FileSystemInfo f in listFiles)
                {
                    try
                    {
                        using (ZipFile zipFile = new ZipFile(zipArchiveMonth))
                        {

                            String dir = FileUtils.GetRelativePath(((FileInfo)f).DirectoryName + @"\",
                                directory.FullName);
                            //_logger.Debug("Fichier: {0}, relPAth : {1}", f.FullName, dir);
                            zipFile.AddFile(f.FullName, dir);

                            zipFile.Save();
                        }

                    }
                    catch (Exception e)
                    {
                        _logger.Error(" Echec de l'ajout du fichier {0} dans l'archive {1}", f.Name, zipArchiveMonth);
                        _logger.Debug("{0} ::: {1}", e.Message, e.StackTrace);

                    }
                }

                _logger.Info(" <*white*>========>><*/*> [<*green*>OK<*/*>]");
                return new FileInfo(zipArchiveMonth);
            }
            catch (Exception ex)
            {
                _logger.Info(" <*white*>========>><*/*> [<*red*>ECHEC<*/*>]");
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }

            return null;


        }

        private static FileInfo CreateBackupOptions(FileInfo badgerExeFi, String backupName)
        {
            try
            {
                _logger.Info("Sauvegarde des paramètres...");

                FileInfo fiXmlBackupOption = new FileInfo(Path.Combine(badgerExeFi.DirectoryName, String.Format("{0}.xml", backupName)));
                ProcessStartInfo psInfo = new ProcessStartInfo(badgerExeFi.FullName,
                    String.Format("-e {0}", fiXmlBackupOption.Name));
                psInfo.WorkingDirectory = badgerExeFi.DirectoryName;

                Process p = new Process();
                p.StartInfo = psInfo;


                p.Start();
                p.WaitForExit();
                while (!p.HasExited)
                {
                    Thread.Sleep(100);
                }

                if (!fiXmlBackupOption.Exists)
                {
                    throw new Exception(String.Format("Erreur : le fichier {0} n'existe pas.", fiXmlBackupOption.FullName));
                }

                _logger.Info(" <*white*>========>><*/*> [<*green*>OK<*/*>]");
                return fiXmlBackupOption;



            }
            catch (Exception ex)
            {
                _logger.Info(" <*white*>========>><*/*> [<*red*>ECHEC<*/*>]");
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }

            return null;
        }

        private static void LoadBackupOptions(FileInfo badgerExeFi, FileInfo fiXmlBackupOption)
        {
            try
            {
                _logger.Info("Chargement des options...");

                if (!fiXmlBackupOption.Exists)
                {
                    throw new Exception(String.Format("Erreur : le fichier {0} n'existe pas.", fiXmlBackupOption.FullName));
                }

                ProcessStartInfo psInfo = new ProcessStartInfo(badgerExeFi.FullName,
                    String.Format("-i {0}", fiXmlBackupOption.Name));
                psInfo.WorkingDirectory = badgerExeFi.DirectoryName;

                Process p = new Process();
                p.StartInfo = psInfo;


                p.Start();
                p.WaitForExit();

                _logger.Info(" <*white*>========>><*/*> [<*green*>OK<*/*>]");
            }
            catch (Exception ex)
            {
                _logger.Info(" <*white*>========>><*/*> [<*red*>ECHEC<*/*>]");
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }

        }

        private static void LoadBackupBinaries(FileInfo archiveBackup, DirectoryInfo directoryToExtract, string password = null)
        {
            try
            {
                _logger.Info("Extraction des fichiers");

                _logger.Debug("Pré-nettoyage");
                foreach (FileInfo file in directoryToExtract.GetFiles("*.tmp"))
                {
                    file.Delete();
                }
                foreach (FileInfo file in directoryToExtract.GetFiles("*.PendingOverwrite"))
                {
                    file.Delete();
                }

                _logger.Debug("Extraction dans {0}", directoryToExtract.FullName);
                using (ZipFile zipRelease = new ZipFile(archiveBackup.FullName))
                {
                    if (password != null)
                    {
                        zipRelease.Password = password;
                    }

                    foreach (ZipEntry entryInArchive in zipRelease.Entries)
                    {
                        //_logger.Info(entryInArchive.FileName);
                        if (filesIgnored.Contains(entryInArchive.FileName))
                        {
                            _logger.Warn("Fichiers ignorés car faisant partie du programme de mise à jour : {0}",
                                entryInArchive.FileName);

                            _isUpdateUpdaterAvailable = true;
                            entryInArchive.Extract(Path.Combine(directoryToExtract.FullName, "BKP"), ExtractExistingFileAction.OverwriteSilently);

                            continue;
                        }
                        entryInArchive.Extract(directoryToExtract.FullName, ExtractExistingFileAction.OverwriteSilently);
                    }
                    //zipRelease.ExtractAll(directoryToExtract.FullName, ExtractExistingFileAction.OverwriteSilently);
                }

                _logger.Info(" <*white*>========>><*/*> [<*green*>OK<*/*>]");
            }
            catch (Exception ex)
            {
                _logger.Info(" <*white*>========>><*/*> [<*red*>ECHEC<*/*>]");
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }
            finally
            {
                Console.ReadLine();
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

        private static AppArgsDto LoadArgs(string[] args)
        {
            try
            {

                _logger.Debug("Lecteurs des options et paramètres");
                AppArgsParser argsParser = new AppArgsParser();
                AppArgsDto argsDto = argsParser.ParseDirect(args);

                return argsDto;

            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndRethrows(ex, "<*red*>Erreur lors du chargement des paramètres en entrée du programme.<*/*>");

            }

            return null;
        }



        private static void Exit(int exitCode = 0)
        {
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
