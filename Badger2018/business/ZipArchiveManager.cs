using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using BadgerCommonLibrary.utils;
using Ionic.Zip;

namespace Badger2018.business
{
    public class ZipArchiveManager
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public DirectoryInfo Directory { get; set; }


        public void Compress()
        {

            List<FileInfo> listFiles = Directory.GetFiles().ToList();

            DateTime firstDay = AppDateUtils.DtNow().WithFirstDayOfMonth();

            if (!listFiles.Any(r => r.LastWriteTime.IsBefore(firstDay))) return;


            if (!System.IO.Directory.Exists(Cst.ArchivesDirName))
            {
                System.IO.Directory.CreateDirectory(Cst.ArchivesDirName);
            }

            List<FileInfo> toRemoveFileList = new List<FileInfo>(listFiles.Count);
            foreach (FileInfo f in listFiles.Where(r => r.LastWriteTime.IsBefore(firstDay)))
            {

                String zipArchiveMonth = Cst.ArchivesDirName + "/" + f.LastWriteTime.ToString("yyyy-MM") + ".zip";

                try
                {


                    using (ZipFile zipFile = new ZipFile(zipArchiveMonth))
                    {
                        _logger.Debug("Ajout du fichier {0} dans l'archive {1}", f.Name, zipArchiveMonth);
                        zipFile.AddFile(f.FullName, Directory.Name);
                        zipFile.Save();
                    }

                }
                catch (Exception e)
                {
                    _logger.Error(" Echec de l'ajout du fichier {0} dans l'archive {1}", f.Name, zipArchiveMonth);
                    _logger.Debug("{0} ::: {1}", e.Message, e.StackTrace);

                }


                toRemoveFileList.Add(f);
            }

            foreach (FileInfo fi in toRemoveFileList)
            {
                try
                {

                    fi.Delete();
                }
                catch (Exception e)
                {
                    _logger.Error(" Echec de lors de la suppression du fichier {0}. {1} -- {2}", fi.Name, e.Message, e.StackTrace);

                }
            }
        }

    }
}
