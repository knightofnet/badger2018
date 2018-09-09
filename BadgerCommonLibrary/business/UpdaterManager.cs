using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevLibrary.utils.xml;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.dto;

namespace BadgerCommonLibrary.business
{
    public class UpdaterManager
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;
        private string _xmlUpdFilePath;

        public string XmlUpdFilePath
        {
            get { return _xmlUpdFilePath; }
            set
            {
                _xmlUpdFilePath = value;

            }
        }


        public bool IsNewUpdateAvalaible { get; private set; }
        public bool IsUpdaterFileLoaded { get; private set; }
        public string UpdateCheckTag { get; set; }

        public List<UpdateInfoDto> ListReleases { get; private set; }

        private XmlFile xmlFile { get; set; }


        public UpdaterManager()
        {
            UpdateCheckTag = "";
        }

        private void LoadsXmlFile(string filepath)
        {
            FileInfo fileInfo = new FileInfo(filepath);
            IsUpdaterFileLoaded = false;
            if (!fileInfo.Exists)
            {
                _logger.Error("Le fichier contenant les mise à jour n'existe pas ou n'est pas lisible. Fichier {}", filepath ?? "null");
                throw new FileNotFoundException(filepath);
            }

            xmlFile = XmlFile.InitXmlFile(filepath);
            IsUpdaterFileLoaded = true;
        }

        public void CheckForUpdates(string tagCheckUpd, string versionIn, string versionTarget = "*")
        {
            try
            {

                if (versionTarget == null)
                {
                    versionTarget = "*";
                }

                List<UpdateInfoDto> listReleasesLoc = new List<UpdateInfoDto>();
                LoadsXmlFile(XmlUpdFilePath);
                if (!IsUpdaterFileLoaded)
                {
                    return;
                }
                XmlNodeList releasesXml = XmlUtils.GetNodesXpath(xmlFile.Root, "//release");
                for (int i = 0; i < releasesXml.Count; i++)
                {
                    XmlNode releaseXml = releasesXml[i];
                    UpdateInfoDto releaseDto = ParseReleaseXml(releaseXml);
                    if (releaseDto.IsActive)
                    {
                        listReleasesLoc.Add(releaseDto);
                    }
                }

                Version vsIn;
                if (!Version.TryParse(versionIn, out vsIn))
                {
                    throw new Exception("La version en entrée n'est pas correcte (" + versionIn + ")");
                }

                if (!versionTarget.Equals("*"))
                {
                    Version vs;
                    if (Version.TryParse(versionTarget, out vs))
                    {
                        listReleasesLoc = listReleasesLoc.Where(r => r.Version.CompareTo(vsIn) > 0 && r.Version.CompareTo(vs) <= 0).ToList();
                    }
                    else
                    {
                        _logger.Warn(
                            "Impossible de vérifier les mise à jour. Version cible invalide (version cible : {0})",
                            versionTarget);
                        throw new Exception("Impossible de vérifier les mise à jour. Version cible invalide");
                    }
                }
                else
                {
                    listReleasesLoc = listReleasesLoc.Where(r => r.Version.CompareTo(vsIn) > 0).ToList();
                }

                IsNewUpdateAvalaible = listReleasesLoc.Any();
                UpdateCheckTag = tagCheckUpd;

                ListReleases = listReleasesLoc.OrderByDescending(r => r.Version).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de la vérification de la mise à jour", ex);
                IsNewUpdateAvalaible = false;
                return;
            }
        }

        private UpdateInfoDto ParseReleaseXml(XmlNode releaseXml)
        {
            _logger.Debug("Extraction des informations de mise à jour");

            UpdateInfoDto updRet = new UpdateInfoDto();



            string title = XmlUtils.GetValueXpath(releaseXml, ".//title");
            updRet.Title = title;
            _logger.Debug(" Titre {0}", title);

            string newVersion = XmlUtils.GetValueXpath(releaseXml, ".//version");
            updRet.Version = Version.Parse(newVersion);
            _logger.Debug(" Version {0}", newVersion);

            string isActiveStr = XmlUtils.GetValueXpath(releaseXml, ".//active");
            updRet.IsActive = StringUtils.IsNullOrWhiteSpace(isActiveStr) || Boolean.Parse(isActiveStr);
            _logger.Debug(" IsActive {0}", updRet.NeedIntermediateLaunch);


            string description = XmlUtils.GetValueXpath(releaseXml, ".//description");
            updRet.Description = description;
            _logger.Debug(" Description {0}", description);

            if (!updRet.IsActive)
            {
                _logger.Debug(" Mise à jour désactivée");
                return updRet;
            }

            string auteurXml = XmlUtils.GetValueXpath(releaseXml, ".//authors");
            updRet.Authors = auteurXml;
            _logger.Debug(" Authors {0}", auteurXml);

            string fileUpdate = XmlUtils.GetValueXpath(releaseXml, ".//fileUpdate");
            updRet.FileUpdate = fileUpdate;
            _logger.Debug(" FileUpdate {0}", fileUpdate);

            string levelUpdateStr = XmlUtils.GetValueXpath(releaseXml, ".//levelUpdate");
            updRet.LevelUpdate = StringUtils.IsNullOrWhiteSpace(levelUpdateStr) ? 0 : Int16.Parse(levelUpdateStr);
            _logger.Debug(" LevelUpdate {0}", updRet.LevelUpdate);

            string needIntermediateLaunchStr = XmlUtils.GetValueXpath(releaseXml, ".//needIntermediateLaunch");
            updRet.NeedIntermediateLaunch = !StringUtils.IsNullOrWhiteSpace(levelUpdateStr) && Boolean.Parse(needIntermediateLaunchStr);
            _logger.Debug(" NeedIntermediateLaunch {0}", updRet.NeedIntermediateLaunch);


            return updRet;
        }

        public bool UpdateProgramTo(UpdateInfoDto upd)
        {
            try
            {
                string filenameUpdter = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Resources/BadgerUpdater.exe");
                string badgerExe = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Badger2018.exe");

                string args = String.Format("-l -v {0} -f \"{1}\" -a \"{2}\"", upd.Version.ToString(), XmlUpdFilePath,
                    badgerExe);

                _logger.Debug("Appel de {0} avec {1}", filenameUpdter, args);

                ProcessStartInfo processStartInfo = new ProcessStartInfo(filenameUpdter, args);
                processStartInfo.UseShellExecute = true;
                processStartInfo.WorkingDirectory =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Resources");

                var processUpd = Process.Start(processStartInfo);
                processUpd.WaitForExit(2000);
                if (processUpd.HasExited)
                {

                    if (processUpd.ExitCode > EnumExitCodes.U_OK_NO_UPDATE_NEEDED.ExitCodeInt)
                    {

                        string msg = "Erreur lors du lancement du programme de mise à jour :  celui-ci s'est arrêté précocement";

                        throw new Exception(msg);
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                string msg = "Erreur lors du lancement du programme de mise à jour";
                _logger.Error(msg);
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);


                throw ex;
            }
        }
    }
}
