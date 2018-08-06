using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevLibrary.utils.xml;
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
                LoadsXmlFile(value);
            }
        }



        public bool IsNewUpdateAvalaible { get; private set; }
        public string UpdateCheckTag { get; set; }

        public List<UpdateInfoDto> ListReleases { get; private set; }

        private XmlFile xmlFile { get; set; }


        public UpdaterManager(String xmlFilePath)
        {
            XmlUpdFilePath = xmlFilePath;
        }

        private void LoadsXmlFile(string filepath)
        {
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(filepath);
            }

            xmlFile = XmlFile.InitXmlFile(filepath);
        }

        public void CheckForUpdates(string tagCheckUpd, string versionTarget = "*")
        {
            try
            {
                List<UpdateInfoDto> listReleasesLoc = new List<UpdateInfoDto>();
                LoadsXmlFile(XmlUpdFilePath);
                XmlNodeList releasesXml = XmlUtils.GetNodesXpath(xmlFile.Root, "//release");
                for (int i = 0; i < releasesXml.Count; i++)
                {
                    XmlNode releaseXml = releasesXml[i];
                    UpdateInfoDto releaseDto = ParseReleaseXml(releaseXml);
                    listReleasesLoc.Add(releaseDto);
                }

                if (!versionTarget.Equals("*"))
                {
                    Version vs;
                    if (Version.TryParse(versionTarget, out vs))
                    {
                        listReleasesLoc = listReleasesLoc.Where(r => r.Version.CompareTo(vs) <= 0).ToList();
                    }
                    else
                    {
                        _logger.Warn(
                            "Impossible de vérifier les mise à jour. Version cible invalide (version cible : {0})",
                            versionTarget);
                        throw new Exception("Impossible de vérifier les mise à jour. Version cible invalide");
                    }
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

            string newVersion = XmlUtils.GetValueXpath(releaseXml, ".//version");
            updRet.Version = Version.Parse(newVersion);
            _logger.Debug(" Version {0}", newVersion);

            string title = XmlUtils.GetValueXpath(releaseXml, ".//title");
            updRet.Title = title;
            _logger.Debug(" Titre {0}", title);

            string description = XmlUtils.GetValueXpath(releaseXml, ".//description");
            updRet.Description = description;
            _logger.Debug(" Description {0}", description);

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
            throw new NotImplementedException();
        }
    }
}
