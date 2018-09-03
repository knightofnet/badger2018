using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Badger2018.business
{
    class BadgingBckder
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public MainWindow Pwin { get; set; }

        public String IdElt { get; set; }
        public String IdVerif { get; set; }
        public String Url { get; set; }

        public bool HasSucceedTrt { get; private set; }

        public Action<int> OnEtapeChange;
        private int _etapeTrt;

        public int EtapeTrt
        {
            get { return _etapeTrt; }
            private set
            {
                _etapeTrt = value;
                if (OnEtapeChange != null)
                {
                    OnEtapeChange(value);
                }
            }
        }

        public void BadgeageBackgrounderOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            BackgroundWorker bkg = sender as BackgroundWorker;


            RemoteWebDriver driver = null;
            _logger.Info("Badgeage");

            // Pwin.RestoreWindow();

            EtapeTrt = 0;
            bkg.ReportProgress(EtapeTrt);

            try
            {

                driver = BadgingUtils.GetWebDriver(Pwin.PrgOptions);
                if (driver == null)
                {
                    HasSucceedTrt = false;
                    return;
                }
                _logger.Debug(" GoToUrl: {0}", Url);

                EtapeTrt = 1;
                bkg.ReportProgress(EtapeTrt);
                driver.Navigate().GoToUrl(Url);

                EtapeTrt = 2;
                bkg.ReportProgress(EtapeTrt);
                _logger.Debug(" Recherche de l'élément: {0}", IdElt);
                IWebElement b = BadgingUtils.FindEltById(IdElt, driver);
                if (Pwin.PrgOptions.ModeBadgement == EnumModePointage.FORM)
                {
                    EtapeTrt = 3;
                    bkg.ReportProgress(EtapeTrt);
                    _logger.Debug(" Soumission du formulaire");
                    b.Submit();
                }
                else if (Pwin.PrgOptions.ModeBadgement == EnumModePointage.ELEMENT)
                {
                    EtapeTrt = 3;
                    bkg.ReportProgress(EtapeTrt);
                    _logger.Debug(" Clic sur l'élément");
                    b.Click();
                }

                EtapeTrt = 4;
                bkg.ReportProgress(EtapeTrt);
                Thread.Sleep(2000);


                BadgingUtils.SaveScreenshot(Pwin.Times.TimeRef, Pwin.EtatBadger + "", driver);

                if (!StringUtils.IsNullOrWhiteSpace(IdVerif))
                {
                    EtapeTrt = 5;
                    bkg.ReportProgress(EtapeTrt);
                    _logger.Debug(" Recherche de l'élément [pour vérif]: {0}", IdVerif);
                    BadgingUtils.FindEltById(IdVerif, driver);
                    _logger.Debug(" Elément trouvé");

                }

                HasSucceedTrt = true;
                return;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                if (driver != null)
                    driver.Quit();
            }
        }
    }
}
