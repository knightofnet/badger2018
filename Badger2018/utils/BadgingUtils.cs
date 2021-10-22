using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.dto;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;

namespace Badger2018.utils
{
    public static class BadgingUtils
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public static void SaveScreenshot(DateTime refDtTime, string str, RemoteWebDriver driver)
        {
            driver.GetScreenshot().SaveAsFile(Cst.ScreenshotDir + "tmpScreenshot.png");
            //driver.GetScreenshot().SaveAsFile(Cst.ScreenshotDir + MiscAppUtils.GetFileNameScreenshot(refDtTime, str + ""));
        }

        public static IWebElement FindEltById(string idVerif, IWebDriver driver, int nbTentative = 4)
        {
            int i = nbTentative;
            while (true)
            {
                try
                {
                    IWebElement c = driver.FindElement(By.Id(idVerif));
                    return c;

                }
                catch (NoSuchElementException e)
                {
                    if (i > 0)
                    {
                        _logger.Debug(" Elément {0} non trouvé, on retente dans 1s ", idVerif);
                        Thread.Sleep(1000);
                        i--;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }


        public static RemoteWebDriver GetWebDriver(AppOptions prgOptions)
        {
            if (prgOptions.BrowserIndex == EnumBrowser.FF)
            {
                _logger.Info(" Firefox webdriver");

                String geckoDriverFile = "geckodriver.exe";
                if (prgOptions.IsUseGeckoDebug)
                {
                    geckoDriverFile = "geckoWithLog.cmd";
                }


                if (!File.Exists(geckoDriverFile))
                {
                    string message = String.Format("Le fichier {0}, nécessaire à la liaison avec Firefox, n'existe pas dans le dossier de l'application. Impossible de badger avec Firefox", geckoDriverFile);
                    _logger.Error(message);
                    throw new FileNotFoundException(message, geckoDriverFile);
                }


                FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(@".\", geckoDriverFile);

                service.HideCommandPromptWindow = !prgOptions.IsUseGeckoDebug;


                FirefoxOptions fOpt = new FirefoxOptions();
                fOpt.SetPreference("network.proxy.type", 4);
                fOpt.SetPreference("capability.policy.PolitiqueNationale.sites", "an.cnav");
                fOpt.SetPreference("network.automatic-ntlm-auth.trusted-uris", "an.cnav,cnav.fr,cnavts.fr");
                fOpt.SetPreference("network.negotiate-auth.trusted-uris", "an.cnav,cnav.fr,cnavts.fr");
                fOpt.SetPreference("network.auth.use-sspi", true);
                fOpt.SetPreference("network.automatic-ntlm-auth.allow-proxies", true);
                fOpt.SetPreference("network.automatic-ntlm-auth.trusted-uris", "an.cnav,cnav.fr,cnavts.fr,50.101.93.187");

                fOpt.SetPreference("app.update.auto", false);
                fOpt.SetPreference("app.update.enabled", false);


                fOpt.BrowserExecutableLocation = prgOptions.FfExePath;
                //fOpt.BrowserExecutableLocation = @"C:\Program Files(x86)\Mozilla Firefox\firefox.exe";

               if (prgOptions.IsPreloadFF) {
                    fOpt.AddArguments("-headless");
                }
               


                return new FirefoxDriver(service, fOpt);

            }

            if (prgOptions.BrowserIndex == EnumBrowser.IE)
            {
                _logger.Info(" Ie webdriver");

                if (!File.Exists("IEDriverServer.exe"))
                {
                    string message = "Le fichier IEDriverServer.exe, nécessaire à la liaison avec Internet Explorer, n'existe pas dans le dossier de l'application. Impossible de badger avec Internet Explorer";
                    _logger.Error(message);
                    throw new FileNotFoundException(message, "IEDriverServer.exe");
                }

                return new InternetExplorerDriver();
            }


            return null;
        }

        internal static bool IsNetworkAdapterUp(string networkAdapterToCheck)
        {
            if (StringUtils.IsNullOrWhiteSpace(networkAdapterToCheck)) return true;

            NetworkInterface networkAdapterFound = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(r => r.Name.Equals(networkAdapterToCheck));
            
            return networkAdapterFound != null && networkAdapterFound.OperationalStatus == OperationalStatus.Up;

        }

        public static bool IsValidWebResponse(string url, int expectedWr = 200)
        {
            int statusNumber = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.AllowAutoRedirect = false;
            webRequest.UseDefaultCredentials = true;

            HttpWebResponse response = null;

            try
            {
                webRequest.Timeout = 10000;
                response = (HttpWebResponse)webRequest.GetResponse();
                // This will have statii from 200 to 30x
                statusNumber = (int)response.StatusCode;
            }
            catch (WebException we)
            {
                if (response == null)
                {
                    statusNumber = 0;
                }
                else
                {
                    // Statii 400 to 50x will be here
                    statusNumber = (int) ((HttpWebResponse) we.Response).StatusCode;
                }
            }

            _logger.Debug("IsValidWebResponse(url: {0}, expectedWr: {1}) => statusRep: {2}", url, expectedWr, statusNumber);

            return statusNumber == expectedWr;
        }
    }
}
