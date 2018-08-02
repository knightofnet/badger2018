using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevLibrary.utils.xml;
using BadgerCommonLibrary.dto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace BadgerCommonLibrary.business
{
    
        public sealed class UpdateChecker
        {
            private static readonly Logger _logger = Logger.LastLoggerInstance;

            private static UpdateChecker _instance;

            public string UpdateUri { get; set; }

            public bool IsNewUpdateAvalaible { get; private set; }
            public bool IsUpdateCheckSuccess { get; private set; }

            public string UpdateCheckTag { get; set; }

            public UpdateInfoDto UpdateInfo { get; set; }

            public static UpdateChecker Instance
            {
                get { return _instance ?? (_instance = new UpdateChecker()); }
                private set { _instance = value; }
            }

            private UpdateChecker()
            {
                UpdateCheckTag = "";
            }

            public void CheckForNewUpdate(string updateCheckName, string uriUpdate = null)
            {

                try
                {
                    if (UpdateUri == null)
                    {

                        if (uriUpdate == null)
                        {
                            IsUpdateCheckSuccess = false;
                            IsNewUpdateAvalaible = false;
                            return;
                        }

                        UpdateUri = uriUpdate;
                    }

                    _logger.Debug("Vérification de la mise à jour à partir de {0}", UpdateUri);
                    string filePath = UpdateUri;
                    if (UpdateUri.ToLower().StartsWith("http"))
                    {
                        filePath = GetUpdateFileOverNet(UpdateUri);
                    }

                    if (filePath == null || !File.Exists(filePath))
                    {
                        _logger.Warn("Le fichier {0} n'existe pas ou n'est pas accessible.", filePath);
                        IsUpdateCheckSuccess = false;
                        IsNewUpdateAvalaible = false;
                        return;

                    }

                    XmlFile xmlFile = XmlFile.InitXmlFile(filePath);
                    UpdateInfo = ExtractUpdateInfo(xmlFile);
                    if (UpdateUri.ToLower().StartsWith("http"))
                    {
                        File.Delete(filePath);
                    }


                    IsNewUpdateAvalaible = UpdateInfo.Version > Assembly.GetExecutingAssembly().GetName().Version;
                    IsUpdateCheckSuccess = true;
                    UpdateCheckTag = updateCheckName;
                }
                catch (Exception e)
                {
                    _logger.Error("Erreur lors de la vérification de la mise à jour", e);
                    IsUpdateCheckSuccess = false;
                    IsNewUpdateAvalaible = false;
                    return;
                }

            }




            private UpdateInfoDto ExtractUpdateInfo(XmlFile xmlFile)
            {
                _logger.Debug("Extraction des informations de mise à jour");

                UpdateInfoDto updRet = new UpdateInfoDto();

                string newVersion = XmlUtils.GetValueXpath(xmlFile.Root, "//version");
                updRet.Version = Version.Parse(newVersion);
                _logger.Debug(" Version {0}", newVersion);

                string title = XmlUtils.GetValueXpath(xmlFile.Root, "//title");
                updRet.Title = title;
                _logger.Debug(" Titre {0}", title);

                string description = XmlUtils.GetValueXpath(xmlFile.Root, "//description");
                updRet.Description = description;
                _logger.Debug(" Description {0}", description);

                string fileUpdate = XmlUtils.GetValueXpath(xmlFile.Root, "//fileUpdate");
                updRet.FileUpdate = fileUpdate;
                _logger.Debug(" FileUpdate {0}", fileUpdate);

                return updRet;

            }

            private static string GetUpdateFileOverNet(string url)
            {
                string fileName = null;

                if (url.ToLower().StartsWith("https"))
                {
                    _logger.Error("HTTPS n'est pas supporté avec le .net framework 4.0");
                }

                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                    fileName = Path.GetTempFileName();
                    _logger.Debug("Essai de téléchargement de {0} vers {1}", url, fileName);

                    IWebProxy defaultProxy = WebRequest.DefaultWebProxy;
                    defaultProxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = defaultProxy;
                    client.DownloadFile(url, fileName);

                }

                return fileName;
            }

            public static bool UpdateProgram(UpdateChecker chk)
            {
                try
                {
                    string updateExeFilePath = chk.UpdateInfo.FileUpdate;

                    if (chk.UpdateInfo.FileUpdate.ToLower().StartsWith("http"))
                    {
                        updateExeFilePath = GetUpdateFileOverNet(updateExeFilePath);
                        File.Move(updateExeFilePath, updateExeFilePath + ".exe");
                    }

                    if (!File.Exists(updateExeFilePath))
                    {
                        _logger.Error("Le fichier de la mise à jour {0} n'existe pas ou n'est pas accessible.", updateExeFilePath);
                        return false;
                    }

                    ProcessStartInfo piInfo = new ProcessStartInfo(Path.Combine(Cst.ApplicationDirectory, "Resources/updateBadger.cmd"), "\"" + updateExeFilePath + "\"");
                    piInfo.WorkingDirectory = Path.Combine(Cst.ApplicationDirectory, "Resources");
                    Process.Start(piInfo);
                    return true;


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur s'est produite de l'affichage du fichier de mise à jour."
                                    + "Consulter le ficher log pour plus d'information sur l'erreur : "
                                    + ex.Message
                        , "Erreur");
                    _logger.Error(ex.Message);
                    _logger.Error(ex.StackTrace);
                    return false;
                }


            }
        }
    }

