using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;
using BadgerUpdater.constantes;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BadgerUpdater.utils
{
    internal class BackupUtils
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        internal static FileInfo CreateBackupOptions(FileInfo badgerExeFi, String backupName)
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

                if (p.ExitCode != EnumExitCodes.M_OK_IMPORT_EXPORT_OK.ExitCodeInt )
                {
                    _logger.Warn("Erreur lors de la sauvegarde des paramètres : l'application Badger2018 ne s'est pas terminée comme attendu.");
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

        internal static FileInfo CreateBackupFiles(DirectoryInfo directory, string backupName)
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


        internal static void LoadBackupOptions(FileInfo badgerExeFi, FileInfo fiXmlBackupOption)
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
                while (!p.HasExited)
                {
                    Thread.Sleep(100);
                }

                if (p.ExitCode != EnumExitCodes.M_OK_IMPORT_EXPORT_OK.ExitCodeInt)
                {
                    _logger.Warn("Erreur lors du chargement des options : l'application Badger2018 ne s'est pas terminée comme attendu");
                }

                _logger.Info(" <*white*>========>><*/*> [<*green*>OK<*/*>]");
            }
            catch (Exception ex)
            {
                _logger.Info(" <*white*>========>><*/*> [<*red*>ECHEC<*/*>]");
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }

        }

        internal static void LoadBackupBinaries(FileInfo archiveBackup, DirectoryInfo directoryToExtract, out bool IsNewUpdateForUpdaterAvlb, string password = null)
        {
            IsNewUpdateForUpdaterAvlb = false;
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
                        if (Cst.FilesIgnored.Contains(entryInArchive.FileName))
                        {
                            _logger.Warn("Fichiers ignorés car faisant partie du programme de mise à jour : {0}",
                                entryInArchive.FileName);

                            IsNewUpdateForUpdaterAvlb = true;
                            entryInArchive.Extract(Path.Combine(directoryToExtract.FullName, "TMP"), ExtractExistingFileAction.OverwriteSilently);


                        }
                        else
                        {
                            entryInArchive.Extract(directoryToExtract.FullName, ExtractExistingFileAction.OverwriteSilently);
                        }
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
              //  Console.ReadLine();
            }
        }
    }
}
