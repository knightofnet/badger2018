using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using AryxDevLibrary.utils.logger;
using AryxDevViewLibrary.utils;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.exceptions;
using Badger2018.utils;
using Badger2018.views;
using BadgerCommonLibrary.utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Badger2018.business
{
    public class BadgerWorker
    {

        private BackgroundWorker _badgeageBackgrounder = null;

        private static readonly Logger _logger = Logger.LastLoggerInstance;
        private readonly Action<DateTime?, int> _actionAfterBadgeage;
        public BadgeageProgressView Progress;
        public MainWindow Pwin { get; set; }

        //public bool isInBadgeWork = false;

        public DispatcherTimer LastBadgeTimer { get; private set; }

        public BadgerWorker(MainWindow pwin)
        {
            Pwin = pwin;

            _actionAfterBadgeage = (dtTime, etatBadger) =>
            {
                if (dtTime.HasValue && File.Exists(Cst.ScreenshotDir + "tmpScreenshot.png"))
                {
                    string destFileName = Cst.ScreenshotDir + MiscAppUtils.GetFileNameScreenshot(dtTime.Value.AtSec(Cst.SecondeOffset), etatBadger + "");
                    if (File.Exists(destFileName))
                    {
                        File.Delete(destFileName);
                    }
                    File.Move(Cst.ScreenshotDir + "tmpScreenshot.png",
                        destFileName);
                }
                if (Pwin.EtatBadger != -1)
                {
                    _logger.Info("Sauvegarde de la session (EtatBadger: {0})", Pwin.EtatBadger);
                    Pwin.PointageSaverObj.SaveCurrentDayTimes();
                }

                Pwin.PrgSwitch.IsInBadgeWork = false;
            };
        }

        public void BadgeFullAction(bool forceWhenMsg = false, bool isOkToShutdownIfOptionEnabled = true)
        {

            if (Pwin.PrgSwitch.IsInBadgeWork)
            {
                _logger.Debug("BadgeFullAction : déjà en cours");
                return;
            }

            Pwin.PrgSwitch.IsInBadgeWork = true;

            _logger.Info("BadgeFullAction (forceWhenMsg: {0}) EtatBadger: {1})", forceWhenMsg ? "true" : "false", Pwin.EtatBadger);

            if (!BadgingUtils.IsValidWebResponse(Pwin.PrgOptions.Uri))
            {
                NoConnexionBadgingView noConnexionBadgingView = new NoConnexionBadgingView(Pwin.PrgOptions.NoConnexionTimeout);
                noConnexionBadgingView.ShowDialog();
                if (noConnexionBadgingView.Result == MessageBoxResult.Cancel)
                {
                    Pwin.PrgSwitch.IsInBadgeWork = false;
                    Pwin.SetBtnBadgerEnabled(true);
                    return;
                }
            }

            switch (Pwin.EtatBadger)
            {
                case -1:
                    BadgeageEtapeM1();
                    break;
                case 0:
                    BadgeageEtapeP0(forceWhenMsg);
                    break;
                case 1:
                    BadgeageEtapeP1(forceWhenMsg);
                    break;
                case 2:
                    BadgeageEtapeP2(forceWhenMsg, isOkToShutdownIfOptionEnabled);
                    break;
            }
        }

        internal DateTime? BadgerActionBis(String url, String idElt, String idVerif, Action<DateTime?> afterWork)
        {

            while (true)
            {
                try
                {

                    BadgeAction(url, idElt, idVerif, afterWork, Pwin.EtatBadger);
                    return AppDateUtils.DtNow().AtSec(Cst.SecondeOffset);
                }
                catch (Exception ex)
                {
                    ExceptionHandlingUtils.ShowStackTrace = true;
                    ExceptionHandlingUtils.LogAndHideException(ex);
                }
            }

        }


        public bool TestNavigation(string url, string idElt, string idVerif, bool isPreloadFf)
        {

            RemoteWebDriver driver = null;
            _logger.Info("Badgeage");

            Pwin.RestoreWindow();

            try
            {
                try
                {
                    driver = FfDriverSingleton.Instance.GetWebDriver();
                    if (driver == null)
                    {
                        AppOptions moddedOpt = Pwin.PrgOptions.Clone();
                        moddedOpt.IsPreloadFF = isPreloadFf;
                        FfDriverSingleton.Instance.Load(moddedOpt);
                        driver = FfDriverSingleton.Instance.GetWebDriver();
                    }
                    driver.GetScreenshot().SaveAsFile("test.png");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Impossible de charger le navigateur. " + e.Message, "Erreur", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return false;
                }
                _logger.Debug(" GoToUrl: {0}", url);
                driver.Navigate().GoToUrl(url);


                _logger.Debug(" Recherche de l'élément: {0}", idElt);
                try
                {
                    IWebElement b = BadgingUtils.FindEltById(idElt, driver);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Impossible de charger le navigateur. " + e.Message, "Erreur", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return false;

                }

                MessageBox.Show("Test réalisé avec succés.", "Information", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return true;
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

        internal bool BadgeAction(String url, String idElt, String idVerif, Action<DateTime?> afterWork, int etatBadger)
        {

            if (!File.Exists(Pwin.PrgOptions.FfExePath))
            {
                Pwin.NotifManager.NotifyNow("Firefox n'existe pas à cette adrese : " + Pwin.PrgOptions.FfExePath,
                    "Erreur");
                _logger.Info("Erreur lors du badgeage : aucune action");
                _logger.Debug("Firefox n'existe pas à cette adrese : " + Pwin.PrgOptions.FfExePath);
                afterWork(null);
                Pwin.SetBtnBadgerEnabled(true);
                return false;
            }

            Progress = new BadgeageProgressView(Pwin.PrgOptions);
#if DEBUG
            Progress.Topmost = false;
#endif
            Progress.Show();


            BadgingBckder b = new BadgingBckder
            {
                Pwin = Pwin,
                IdElt = idElt,
                IdVerif = idVerif,
                Url = url
            };


            _badgeageBackgrounder = new BackgroundWorker();
            Progress.BackgrounderRef = _badgeageBackgrounder;
            _badgeageBackgrounder.DoWork += b.BadgeageBackgrounderOnDoWork;
            _badgeageBackgrounder.WorkerSupportsCancellation = true;
            _badgeageBackgrounder.WorkerReportsProgress = true;
            _badgeageBackgrounder.ProgressChanged += (sender, args) =>
            {
                int pInt = args.ProgressPercentage;
                if (Progress != null)
                {

                    Progress.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                        new Action(() =>
                        {
                            Progress.ValidStep(pInt - 1);
                            Progress.EnterStep(pInt);
                            if (pInt == 3)
                            {
                                Progress.ToogleBtnCancel();
                            }

                        })
                    );


                }

            };
            _badgeageBackgrounder.RunWorkerCompleted += (sender, args) =>
            {
                DateTime dtNow = AppDateUtils.DtNow();
                if (b.ElementsFromPage.HeureBadgee != null)
                {
                    _logger.Debug("Prise en compte de l'heure badgée");
                    dtNow = dtNow.ChangeTime(b.ElementsFromPage.HeureBadgee.Value);
                }

                if (args.Error == null)
                {
                    _logger.Debug("BadgeWorker:::BadgeAction() : do Afterwork()");
                    afterWork(dtNow);
                    _actionAfterBadgeage(dtNow, etatBadger);
                    if (Progress != null)
                    {
                        Progress.Hide();
                    }
                    Pwin.PrgOptions.LastCdSeen = b.ElementsFromPage.TsCd.Value;
                }
                else
                {
                    Exception ex = args.Error;
                    _logger.Warn("Erreur lors du badgeage");
                    _logger.Warn(ex.Message);
                    _logger.Warn(ex.StackTrace);

                    if (Progress != null)
                    {
                        Progress.ErrorStep(b.EtapeTrt);
                        Progress.Topmost = false;
                    }
                    Pwin.PrgSwitch.IsInBadgeWork = false;

                    // Annulation du badgeage par l'utilisateur
                    if (ex is UserCancelBadgeageException)
                    {

                        string msg = "L'action de badgeage a été annulée suite à la demande de l'utilisateur.";
                        MessageBoxButton msgButton = MessageBoxButton.OK;
                        if (b.EtapeTrt == 3)
                        {
                            msg += " L'annulation étant tardive vérifiez vos badgeage en vous rendant sur la page Mes Badgeages. Voulez-vous vous y rendre maintenant ?";
                            msgButton = MessageBoxButton.YesNo;
                        }
                        MessageBoxResult result = MessageBoxUtils.TopMostMessageBox("L'action de badgeage a été annulée suite à la demande de l'utilisateur.", "Information", msgButton, MessageBoxImage.Information);
                        _logger.Info("Erreur lors du badgeage : " + msg);
                        _logger.Debug("BadgeWorker:::BadgeAction() : do Afterwork(null)");

                        Action<MessageBoxResult> actionAfter = (resultat) =>
                        {
                            Pwin.RestoreWindow();
                            Pwin.SetBtnBadgerEnabled(true);

                            if (resultat == MessageBoxResult.Yes)
                            {
                                MiscAppUtils.GoTo(Pwin.PrgOptions.UrlMesPointages);
                            }
                        };

                        actionAfter(result);
                        afterWork(null);
                        if (Progress != null)
                        {
                            Progress.Hide();
                        }

                        Pwin.PrgSwitch.IsInBadgeWork = false;
                        return;
                    }

                    // Erreur lors du badgeage
                    IntPtr hwnd = new WindowInteropHelper(Pwin).Handle;
                    FlashWindowUtils.Flash(hwnd, 10);
                    if (b.EtapeTrt > 0)
                    {
                        FfDriverSingleton.Instance.Load(Pwin.PrgOptions);
                    }
                    EnumErrorCodeRetour response = MessageErrorBadgeageView.ShowMessageError(ex, dtNow, Progress, b.EtapeTrt);
                    if (response == EnumErrorCodeRetour.ANNULER)
                    {
                        _logger.Info("Erreur lors du badgeage : aucune action");
                        _logger.Debug("BadgeWorker:::BadgeAction() : do Afterwork(null)");
                        afterWork(null);
                        Pwin.SetBtnBadgerEnabled(true);
                        if (Progress != null)
                        {
                            Progress.Hide();
                        }

                        FfDriverSingleton.Instance.Quit();

                    }
                    if (response == EnumErrorCodeRetour.CONSULTER_POINTAGE)
                    {
                        _logger.Info("Erreur lors du badgeage : consultation pointage");
                        MiscAppUtils.GoTo(Pwin.PrgOptions.UrlMesPointages);
                        CommonAfterReprise(afterWork, Progress, etatBadger);
                    }
                    if (response == EnumErrorCodeRetour.OPEN_SIRHIUS)
                    {
                        _logger.Info("Erreur lors du badgeage : ouverture de Sirhius");
                        MiscAppUtils.GoTo(Pwin.PrgOptions.UrlSirhius);
                        CommonAfterReprise(afterWork, Progress, etatBadger);
                    }

                    if (response == EnumErrorCodeRetour.OPEN_BADGE_PAGE)
                    {
                        Pwin.PrgSwitch.IsInBadgeWork = false;
                        _logger.Info("Erreur lors du badgeage : ouverture page badgeage");
                        MiscAppUtils.GoTo(Pwin.PrgOptions.Uri);
                        CommonAfterReprise(afterWork, Progress, etatBadger);

                    }
                    if (response == EnumErrorCodeRetour.RETRY)
                    {
                        Pwin.PrgSwitch.IsInBadgeWork = true;
                        _logger.Info("Erreur lors du badgeage : réessai");
                        if (Progress != null)
                        {
                            Progress.Hide();
                        }

                        Progress = new BadgeageProgressView(Pwin.PrgOptions);
                        Progress.Show();
                        _badgeageBackgrounder.RunWorkerAsync();
                    }
                }



            };

            _badgeageBackgrounder.RunWorkerAsync();

            return b.HasSucceedTrt;

        }

        private void CommonAfterReprise(Action<DateTime?> afterWork, BadgeageProgressView progress, int etatBadger)
        {
            Thread.Sleep(2000);
            Pwin.RestoreWindow();
            DateTime? dtSaisieManuelle = SaisieManuelleTsView.ShowAskForDateTime();
            _logger.Info(" > Date saisie manuellement : {0}", dtSaisieManuelle != null ? dtSaisieManuelle.Value.ToShortTimeString() : "null");
            _logger.Debug("BadgeWorker:::BadgeAction() : do Afterwork()");
            afterWork(dtSaisieManuelle == null ? dtSaisieManuelle : dtSaisieManuelle.Value.AtSec(Cst.SecondeOffset));
            if (progress != null)
            {
                progress.Hide();
            }
            _actionAfterBadgeage(dtSaisieManuelle == null ? dtSaisieManuelle : dtSaisieManuelle.Value.AtSec(Cst.SecondeOffset), etatBadger);
        }

        private void BadgeageEtapeP2(bool forceWhenMsg, bool isOkToShutdownIfOptionEnabled)
        {

            if (!forceWhenMsg && !ShowMessageAvertissementFinPlageFixe())
            {
                Pwin.PrgSwitch.IsInBadgeWork = false;
                return;
            }

            DateTime endTimer = AppDateUtils.DtNow().AddSeconds(Pwin.PrgOptions.LastBadgeDelay);
            Pwin.StopTimers();
            LastBadgeTimer = new DispatcherTimer();
            LastBadgeTimer.Interval = new TimeSpan(0, 0, 1);
            LastBadgeTimer.Tick += (sender, args) =>
            {
                TimeSpan remainingTimer = endTimer - AppDateUtils.DtNow();
                if (remainingTimer < TimeSpan.Zero)
                {
                    LastBadgeCoreAction(LastBadgeTimer, isOkToShutdownIfOptionEnabled);
                    Pwin.PrgSwitch.PbarMainTimerActif = true;
                }
                else
                {
                    Pwin.ChangeBtnBadgerValue(100 * remainingTimer.TotalSeconds / Pwin.PrgOptions.LastBadgeDelay);
                }
            };

            if (Pwin.PrgOptions.LastBadgeDelay > 0)
            {
                Pwin.PrgSwitch.PbarMainTimerActif = false;
                LastBadgeTimer.Start();
            }
            else
            {
                LastBadgeCoreAction(LastBadgeTimer, isOkToShutdownIfOptionEnabled);
            }
        }

        private bool ShowMessageAvertissementFinPlageFixe()
        {
            if (Pwin.TypeJournee == EnumTypesJournees.Complete)
            {
                if (Pwin.EtatBadger == 0 && AppDateUtils.DtNow().TimeOfDay.CompareTo(Pwin.PrgOptions.PlageFixeMatinFin) < 0)
                {
                    MessageBoxResult doValid = MessageBox.Show(
                        String.Format(
                            "Il n'est pas encore {0}. Voulez-vous quand même badger la fin de la matinée ?",
                            Pwin.PrgOptions.PlageFixeMatinFin), "Question", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (doValid == MessageBoxResult.No)
                    {
                        Pwin.SetBtnBadgerEnabled(true);
                        return false;
                    }
                }

                if (Pwin.EtatBadger == 2 && AppDateUtils.DtNow().TimeOfDay.CompareTo(Pwin.PrgOptions.PlageFixeApremFin) < 0)
                {
                    MessageBoxResult doValid = MessageBox.Show(
                        String.Format(
                            "Il n'est pas encore {0}. Voulez-vous quand même badger la fin de journée de travail ?",
                            Pwin.PrgOptions.PlageFixeApremFin), "Question", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (doValid == MessageBoxResult.No)
                    {
                        Pwin.SetBtnBadgerEnabled(true);
                        return false;
                    }
                }
            }
            else if (Pwin.TypeJournee == EnumTypesJournees.ApresMidi)
            {
                if (AppDateUtils.DtNow().TimeOfDay.CompareTo(Pwin.PrgOptions.PlageFixeApremFin) < 0)
                {
                    MessageBoxResult doValid = MessageBox.Show(
                        String.Format(
                            "Il n'est pas encore {0}. Voulez-vous quand même badger la fin de journée de travail ?",
                            Pwin.PrgOptions.PlageFixeApremFin), "Question", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (doValid == MessageBoxResult.No)
                    {
                        Pwin.SetBtnBadgerEnabled(true);
                        return false;
                    }
                }
            }
            else
            {

                if (Pwin.RealTimes.RealTimeTsNow.CompareTo(Pwin.PrgOptions.PlageFixeMatinFin) < 0)
                {
                    MessageBoxResult doValid = MessageBox.Show(
                        String.Format(
                            "Il n'est pas encore {0}. Voulez-vous quand même badger la fin de la matinée de travail ?",
                            Pwin.PrgOptions.PlageFixeMatinFin), "Question", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (doValid == MessageBoxResult.No)
                    {
                        Pwin.SetBtnBadgerEnabled(true);
                        return false;
                    }
                }

            }


            return true;
        }

        private void BadgeageEtapeP1(bool forceWhenMsg)
        {
            DateTime tempPauseEndTime = AppDateUtils.DtNow().AtSec(Cst.SecondeOffset);
            TimeSpan tmpsPause = tempPauseEndTime - Pwin.Times.PlageTravMatin.Start;

            // Si la pause du midi fait moins de 45min, alors on mets 45 min d'office.
            if (!forceWhenMsg && tmpsPause.CompareTo(Pwin.PrgOptions.TempsMinPause) < 0)
            {
                MessageBoxResult doValid = MessageBox.Show(
                    String.Format(
                        "Les {0} de pause obligatoire ne sont pas encore passées, voulez-vous quand-même badger ? Si vous cliquez sur 'Oui', Le premier badgeage de l'après-midi sera compté, mais l'heure finale théorique sera adaptée pour prendre en compte {0} de pause.",
                        Pwin.PrgOptions.TempsMinPause), "Question", MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (doValid == MessageBoxResult.No)
                {
                    Pwin.SetBtnBadgerEnabled(true);
                    return;
                }

                // tempPauseEndTime = AppDateUtils.DtNow().AtSec(Cst.SecondeOffset);
                // pauseEndDateTime = pauseStartDateTime + AppOptions.TempsMinPause;
                // tmpsPause = AppOptions.TempsMinPause;

                Pwin.PushNewInfo("Fin de l'après-midi à ");
            }

            Action<DateTime?> actionAfter = (DateTime? b) =>
            {
                Pwin.RestoreWindow();

                if (b == null)
                {
                    _logger.Error("Badgeage non effectué, ou incomplet");
                    return;
                }
                Pwin.Times.PlageTravAprem.Start = b.Value.AtSec(Cst.SecondeOffset);

                tmpsPause = Pwin.Times.PlageTravAprem.Start - Pwin.Times.PlageTravMatin.EndOrDft;

                Pwin.EtatBadger = 2;

                Pwin.AdaptUiFromState(Pwin.EtatBadger, tmpsPause);


            };
            BadgerActionBis(Pwin.PrgOptions.Uri, Pwin.PrgOptions.UriParam, Pwin.PrgOptions.UriVerif, actionAfter);
        }

        private void BadgeageEtapeP0(bool forceWhenMsg)
        {
            if (!forceWhenMsg && !ShowMessageAvertissementFinPlageFixe()) // AppDateUtils.DtNow().TimeOfDay.CompareTo(Pwin.PrgOptions.PlageFixeMatinFin) < 0)
            {
                Pwin.PrgSwitch.IsInBadgeWork = false;
                return;
            }


            Action<DateTime?> actionAfter = (DateTime? b) =>
            {
                Pwin.RestoreWindow();

                if (b == null)
                {
                    _logger.Error("Badgeage non effectué, ou incomplet");
                    return;
                }


                Pwin.Times.PlageTravMatin.EndOrDft = b.Value.AtSec(Cst.SecondeOffset);
                if (Pwin.Times.PlageTravMatin.EndOrDft.TimeOfDay.CompareTo(Pwin.PrgOptions.HeureMaxJournee) > 0)
                {
                    Pwin.Times.PlageTravMatin.EndOrDft = Pwin.Times.PlageTravMatin.EndOrDft.ChangeTime(Pwin.PrgOptions.HeureMaxJournee);
                }

                Pwin.EtatBadger = 1;

                Pwin.AdaptUiFromState(Pwin.EtatBadger, null);



            };
            BadgerActionBis(Pwin.PrgOptions.Uri, Pwin.PrgOptions.UriParam, Pwin.PrgOptions.UriVerif, actionAfter);

        }

        private void BadgeageEtapeM1()
        {
            Action<DateTime?> actionAfter = (DateTime? b) =>
            {
                Pwin.RestoreWindow();

                if (b == null)
                {
                    _logger.Error("Badgeage non effectué, ou incomplet");
                    return;
                }
                Pwin.Times.PlageTravMatin.Start = b.Value.AtSec(Cst.SecondeOffset);
                if (Pwin.Times.PlageTravMatin.Start.TimeOfDay.CompareTo(Pwin.PrgOptions.HeureMinJournee) < 0)
                {
                    Pwin.Times.PlageTravMatin.Start = Pwin.Times.PlageTravMatin.Start.ChangeTime(Pwin.PrgOptions.HeureMinJournee);
                }
                Pwin.EtatBadger = 0;
                Pwin.AdaptUiFromState(Pwin.EtatBadger, null);


            };
            BadgerActionBis(Pwin.PrgOptions.Uri, Pwin.PrgOptions.UriParam, Pwin.PrgOptions.UriVerif, actionAfter);

        }

        private void LastBadgeCoreAction(DispatcherTimer lastBadgeTimer, bool isOkToShutdownIfOptionEnabled)
        {
            lastBadgeTimer.Stop();


            Action<DateTime?> actionAfter = (DateTime? b) =>
            {
                Pwin.RestoreWindow();

                if (b == null)
                {
                    _logger.Error("Badgeage non effectué, ou incomplet");
                    return;
                }
                Pwin.Times.PlageTravAprem.EndOrDft = b.Value.AtSec(Cst.SecondeOffset);
                if (Pwin.Times.PlageTravAprem.EndOrDft.TimeOfDay.CompareTo(Pwin.PrgOptions.HeureMaxJournee) > 0)
                {
                    Pwin.Times.PlageTravAprem.EndOrDft = Pwin.Times.PlageTravAprem.EndOrDft.ChangeTime(Pwin.PrgOptions.HeureMaxJournee);
                }

                Pwin.EtatBadger = 3;
                Pwin.AdaptUiFromState(Pwin.EtatBadger, null);

                if (isOkToShutdownIfOptionEnabled && !Pwin.PrgOptions.IsLastBadgeIsAutoShutdown)
                {
                    _logger.Info("Extinction de l'ordinateur refusée");
                }

                if (isOkToShutdownIfOptionEnabled && Pwin.PrgOptions.IsLastBadgeIsAutoShutdown)
                {
                    var psi = new ProcessStartInfo("shutdown", "/s /t 10");
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);

                    Pwin.PrgSwitch.IsRealClose = true;
                    Pwin.Close();
                    //Environment.Exit(0);
                }

            };
            BadgerActionBis(Pwin.PrgOptions.Uri, Pwin.PrgOptions.UriParam, Pwin.PrgOptions.UriVerif, actionAfter);

        }


    }
}
