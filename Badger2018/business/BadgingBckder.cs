using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.exceptions;
using Badger2018.utils;
using BadgerCommonLibrary.utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using ExceptionHandlingUtils = BadgerCommonLibrary.utils.ExceptionHandlingUtils;

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

        //public TimeSpan? TsCd { get; private set; }
        public ElementsFromPageBadgeage ElementsFromPage { get; private set; }

        public void BadgeageBackgrounderOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "BadgerBckder";

            BackgroundWorker bkg = sender as BackgroundWorker;
            
            ElementsFromPage = new ElementsFromPageBadgeage();

            RemoteWebDriver driver = null;
            _logger.Info("Badgeage");

            // Pwin.RestoreWindow();

            EtapeTrt = 0;
            bkg.ReportProgress(EtapeTrt);

            try
            {
                ElementsFromPage.TsCd = null;
                ElementsFromPage.HeureBadgee = null;


                try
                {
                    if (FfDriverSingleton.Instance.IsLoaded())
                    {
                        driver = FfDriverSingleton.Instance.GetWebDriver();
                    }
                    else
                    {
                        FfDriverSingleton.Instance.Load(Pwin.PrgOptions);
                        driver = FfDriverSingleton.Instance.FfDriver;
                    }
                }
                catch (Exception ex)
                {
                    if (bkg.CancellationPending)
                    {
                        doWorkEventArgs.Cancel = true;
                        throw new UserCancelBadgeageException();
                    }

                    throw ex;
                }
                if (driver == null)
                {
                    HasSucceedTrt = false;
                    return;
                }
                _logger.Debug(" GoToUrl: {0}", Url);

                EtapeTrt = 1;
                if (bkg.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    throw new UserCancelBadgeageException();
                }
                bkg.ReportProgress(EtapeTrt);

                driver.Navigate().GoToUrl(Url);

                EtapeTrt = 2;
                if (bkg.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    throw new UserCancelBadgeageException();
                }
                bkg.ReportProgress(EtapeTrt);
                _logger.Debug(" Recherche de l'élément: {0}", IdElt);
                IWebElement b = BadgingUtils.FindEltById(IdElt, driver);
                if (Pwin.PrgOptions.ModeBadgement == EnumModePointage.FORM)
                {
                    EtapeTrt = 3;
                    if (bkg.CancellationPending)
                    {
                        doWorkEventArgs.Cancel = true;
                        throw new UserCancelBadgeageException();
                    }
                    bkg.ReportProgress(EtapeTrt);
                    _logger.Debug(" Soumission du formulaire");
                    b.Submit();
                }
                else if (Pwin.PrgOptions.ModeBadgement == EnumModePointage.ELEMENT)
                {
                    EtapeTrt = 3;
                    if (bkg.CancellationPending)
                    {
                        doWorkEventArgs.Cancel = true;
                        throw new UserCancelBadgeageException();
                    }
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

                string cssSelector = "";
                try
                {
                    EtapeTrt = 6;
                    bkg.ReportProgress(EtapeTrt);
                    cssSelector = "#gv_detailBadgeage tr:last-child td:last-child";
                    ElementsFromPage.HeureBadgee = GetTimeSpanEltByCssSelector(driver, cssSelector);

                }
                catch (Exception e)
                {
                    ExceptionHandlingUtils.LogAndHideException(e, "Erreur lor de la lecture de " + cssSelector);
                }
                finally
                {
                    _logger.Warn("Enregistré");

                }


                try
                {           
                    EtapeTrt = 7;
                    bkg.ReportProgress(EtapeTrt);
                    //_logger.Debug(driver.PageSource);
                    cssSelector = "#gv_cpt tr:last-child td:last-child";
                    ElementsFromPage.TsCd = GetTimeSpanEltByCssSelector(driver, cssSelector);

                }
                catch (Exception e)
                {
                    ExceptionHandlingUtils.LogAndHideException(e, "Erreur lor de la lecture de " + cssSelector);
                    ElementsFromPage.TsCd = TimeSpan.Zero;
                }
                finally
                {
                    _logger.Warn("Enregistré");

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
                    FfDriverSingleton.Instance.Quit();

            }
        }

        private static TimeSpan? GetTimeSpanEltByCssSelector(RemoteWebDriver driver, string cssSelectorCreditDebit)
        {
            TimeSpan? tsCd = null;
            string txt = driver.FindElementByCssSelector(cssSelectorCreditDebit).Text;
            if (!StringUtils.IsNullOrWhiteSpace(txt))
            {
                _logger.Debug("Raw({0}) : '{1}'", cssSelectorCreditDebit, txt);

                // On extrait l'heure
                string charPivot = txt.Contains("h") ? "h" : ":";

                String hPartStr = txt.Substring(txt.IndexOf(charPivot) - 2, 2);
                String mPartStr = txt.Substring(txt.IndexOf(charPivot) + 1, 2);

                short hPart;
                short mPart;
                if (Int16.TryParse(hPartStr, out hPart) && Int16.TryParse(mPartStr, out mPart))
                {
                    tsCd = new TimeSpan(hPart, mPart, 0);
                    if (txt.Contains("-"))
                    {
                        tsCd = tsCd.Value.Negate();
                    }
                }


            }

            return tsCd;
        }
    }
}
