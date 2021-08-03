using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevViewLibrary.controls.Simplifier;
using AryxDevViewLibrary.utils;
using Badger2018.business;
using Badger2018.business.saver;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services;
using Badger2018.utils;
using Badger2018.views;
using Badger2018.views.usercontrols;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.dto;
using BadgerCommonLibrary.utils;
using BadgerPluginExtender;
using BadgerPluginExtender.dto;
using BadgerPluginExtender.interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static Badger2018.business.NoticationsManager;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace Badger2018
{



    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotificationManagerPresenter, IPresentableObject
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private MainTimerManager _mainTimerManager = new MainTimerManager();


        DispatcherTimer finPauseMidiTimer = null;
        DispatcherTimer finHeureReglTimer = null;
        DispatcherTimer pauseHorsPeriodeTimer = null;
        DispatcherTimer apresUnlockMidiWhileBadgeDebutApremTimer = null;

        public AppOptions PrgOptions { get; set; }

        public UpdaterManager UpdaterMgr;
        public PluginManager PluginMgr;

        public BadgerWorker BadgerWorker { get; private set; }
        public NoticationsManager NotifManager { get; private set; }
        public TimesBadgerDto Times { get; private set; }

        public CoreAudioCtrlerFactory CoreAudioFactory = new CoreAudioCtrlerFactory();
        private DidYouKnowView DidYouKnowWindow { get; set; }

        internal ISaverTimes PointageSaverObj { get; set; }



        private EnumTypesJournees _typeJournee;
        public EnumTypesJournees TypeJournee
        {
            get { return _typeJournee; }
            set
            {
                _typeJournee = value;
                if (OnTypeJourneeChange != null)
                {
                    OnTypeJourneeChange(value);
                }
            }
        }

        private int _etatBadger;
        public int EtatBadger
        {
            get { return _etatBadger; }
            set
            {
                _etatBadger = value;
                if (OnEtatBadgerChange != null)
                {
                    OnEtatBadgerChange(value);
                }
            }
        }

        public Action<int> OnEtatBadgerChange;
        public Action<EnumTypesJournees> OnTypeJourneeChange;

        internal int OldEtatBadger { get; set; }

        private ScreenProgressBarView _spv;

        private readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private ContextMenuStrip _notifyIconTrayMenu;
        private ToolStripLabel _notifyIconAppNameLblItem;
        private ToolStripLabel _notifyIconMatineeLblItem;
        private ToolStripLabel _notifyIconApremLblItem;

        private ToolStripMenuItem _nIconShowNotificationItem;
        private ToolStripMenuItem _nIconOpenOptionItem;
        private ToolStripMenuItem _nIconOpenTipsItem;
        private ToolStripMenuItem _nIconBadgerManItem;
        private ToolStripMenuItem _nIconRestoreItem;
        private ToolStripMenuItem _nIconQuit;

        private ImagesBooleanWrapper _imgBoolAutoBadgeAtStart;
        private ImagesBooleanWrapper _imgBoolShowGlobalNotification;
        private ImagesBooleanWrapper _imgBoolAutoBadgeMeridienne;
        private ImagesBooleanWrapper _imgBoolPauseReport;

        public int SpecDelayMeridAutoBadgage { get; set; }

        public AppSwitchs PrgSwitch { get; set; }

        public RealTimesObj RealTimes { get; private set; }
        public LicenceInfo LicenceApp { get; private set; }
        public bool IsFullyLoaded { get; private set; }

        public MainWindow(AppOptions prgOptions, UpdaterManager updaterManager, PluginManager pluginManager, LicenceInfo licenceInfo, AppArgsDto appArgs)
        {
            _logger.Debug("Chargement de l'écran principal");
#if DEBUG
            AppDateUtils.ForceDtNow(new DateTime(2021, 08, 3, 13, 00, 40));

            //SetDarkTheme();
#endif

            PrgSwitch = new AppSwitchs();

            InitializeComponent();
            Times = new TimesBadgerDto();
            Times.TimeRef = AppDateUtils.DtNow();
            Times.PausesHorsDelai = new List<IntervalTemps>();
            //imgOptions.Source = MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "iconSetting.png");
            lblVersion.Content = String.Format(lblVersion.Content.ToString(), Assembly.GetExecutingAssembly().GetName().Version, Properties.Resources.versionName);
            pbarTime.IsIndeterminate = true;

            pbarBtnBadger.IsIndeterminate = true;
            pbarBtnBadger.Color = Cst.SCBGold;
            pbarBtnBadger.BackgroundColor = null;

            if (prgOptions.ShowOnScreenProgressBar)
            {
                _spv = new ScreenProgressBarView();
                _spv.PairWithProgressBar(pbarTime);
            }

            RealTimes = new RealTimesObj();

            LicenceApp = licenceInfo;
            PrgOptions = prgOptions;
            UpdaterMgr = updaterManager;
            PluginMgr = pluginManager;
            Times.EndMoyPfMatin = ServicesMgr.Instance.BadgeagesServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_MATIN_END, 30) ?? PrgOptions.PlageFixeMatinFin;
            Times.EndMoyPfAprem = ServicesMgr.Instance.BadgeagesServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_APREM_END, 30) ?? PrgOptions.PlageFixeApremFin;


            //PointageSaverObj = new XmlPointageWriterReader(this);
            PointageSaverObj = new BddPointageWriterReader(this);
            PrgSwitch.UseBddSupport = true;


            BadgerWorker = new BadgerWorker(this);

            #region  Initialisation du gestionnation de notification
            /*
             * Initialisation du gestionnation de notification
             */
            NotifManager = new NoticationsManager(_notifyIcon, this);
            NotifManager.PluginMgrRef = PluginMgr;
            NotifManager.AfterShowNotif += delegate (NotificationDto n)
            {
                PushNewInfo(n.Message);
            };
            // evt lors du changement d'état du badgeage
            OnEtatBadgerChange += delegate (int a)
            {
                NotifManager.EtatBadger = a;

            };
            // evt lors du changement de type de journée
            OnTypeJourneeChange += delegate (EnumTypesJournees a)
            {
                NotifManager.TypeJournee = a;
            };

            #endregion


            RealTimes.RealTimeDtNow = AppDateUtils.DtNow();
            RealTimes.RealTimeTsNow = RealTimes.RealTimeDtNow.TimeOfDay;
            RealTimes.RealTimeTempsTravaille = TimeSpan.Zero;



            AssignBetaRole();
            PostInitializeComponent();


            #region Gestion du premier lancement du programme
            /**
             * Gestion du premier lancement du programme
             */
            if (PrgOptions.IsFirstRun || !PrgOptions.IsConsentUse)
            {
                DisclaimerView dView = new DisclaimerView(PrgOptions.IsFirstRun);
                dView.ShowDialog();

                PrgOptions.IsConsentUse = dView.IsConsent;
                if (!dView.IsConsent)
                {
                    MessageBox.Show("L'application va se fermer.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    Environment.Exit(1);


                }
                else if (PrgOptions.IsFirstRun)
                {
                    MessageBox.Show(
                        "Il s'agit de votre premier démarrage. Pensez à paramètrer l'application en vous rendant dans les options. " +
                        "Pour cela, cliquez sur la petite roue dentée en bas à gauche dans la fenêtre principale."
                        , "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                }

                OptionManager.SaveOptions(PrgOptions);
            }
            #endregion


            MiscAppUtils.CreatePaths();
            ArchiveDatas();

            // tOdo bug reload notifs
            AdaptUiFromOptions(PrgOptions);

            Title = Assembly.GetExecutingAssembly().GetName().Name;


            PrgSwitch.PbarMainTimerActif = true;
            PrgSwitch.IsCompteHeureTravActif = true;



            InitNotifyIcon();
            Closing += OnClosing;


            #region Evt KeyDown
            KeyDown += (sender, args) =>
            {
                if (args.Key == Key.LeftCtrl)
                {
                    imgBtnFirefoxLoaded.Visibility = Visibility.Visible;

                }
            };


            #endregion

            #region Evt KeyUp
            KeyUp += (sender, args) =>
            {
                if (args.Key == Key.LeftCtrl)
                {
                    if (!FfDriverSingleton.Instance.IsLoaded())
                        imgBtnFirefoxLoaded.Visibility = Visibility.Collapsed;

                }

                // Affiche la fenêtre à propos si appuie sur F1
                if (args.Key == Key.F1)
                {
                    AboutView aboutView = new AboutView();
                    aboutView.LicenceRef = LicenceApp;
                    aboutView.ShowDialog();
                }
                // Affiche les options si appuie sur F2
                else if (args.Key == Key.F2)
                {
                    btnOptions_Click(null, null);
                }
                // Teste si maj existante si appuie sur F3
                else if (args.Key == Key.F3)
                {
                    SignalUpdate(true);
                }
                // Affiche la fenetre "Le saviez vous" si appue sur F4
                else if (args.Key == Key.F4)
                {
                    LoadsDidYouKnow();
                }
                else if (args.Key == Key.F5)
                {
                    btnModTimes_Click(null, null);
                }
                else if (args.SystemKey == Key.F8 || args.Key == Key.F8)
                {
                    AfficheLicenceInfo(true);
                }
                else if (args.SystemKey == Key.F9 || args.Key == Key.F9)
                {
                    String title;
                    String message;

                    title = "Dernier C/D relevé";
                    message = "C/D : " + PrgOptions.LastCdSeen.ToStrSignedhhmm();
                    MiscAppUtils.ShowNotificationBaloon(
                        _notifyIcon,
                        title, message,
                        null, 5000, null, PrgOptions.IsUseAlternateNotification);
                    PluginMgr.PlayHook("OnNotifSend", new object[] { AppDateUtils.DtNow().TimeOfDay, title, message });
                }
                else if (args.SystemKey == Key.F10)
                {
                    String title = "Test notification";
                    String message = "Notification envoyée à " + AppDateUtils.DtNow().ToString("s");
                    MiscAppUtils.ShowNotificationBaloon(
                        _notifyIcon,
                        title, message,
                        null, 15000, null, PrgOptions.IsUseAlternateNotification);
                    PluginMgr.PlayHook("OnNotifSend", new object[] { AppDateUtils.DtNow().TimeOfDay, title, message });

                    SemaineView semV = new SemaineView(PrgOptions, Times);
                    semV.ShowDialog();

                }
                else if (args.Key == Key.F11)
                {
                    PlayAdvertise();
                }

                else if (
#if DEBUG
args.Key == Key.F12 ||
#endif
 KonamiCodeListener.IsCompletedBy(args.Key))
                {
                    DebugCommandView d = new DebugCommandView(this);
                    d.Show();
                }


            };


            #endregion


            EtatBadger = -1;
            TypeJournee = EnumTypesJournees.Complete;

            if (PointageSaverObj.MustReloadIncomplete())
            {
                _logger.Info("Rechargement des informations");
                ReloadUiFromInterface();
                _logger.Info("FIN : Rechargement des informations");
            }
            else
            {
                _logger.Info("Nouvelle session");

                Times.PlageTravMatin.Start = AppDateUtils.DtNow().AtSec(Cst.SecondeOffset);
                SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

                lblStartTime.Foreground = Cst.SCBGrey;

                ctrlCompteur.SetVisibility(CompteurControl.CompteurVisibility.Hidden);

                lblStartTime.ContentShortTime(Times.PlageTravMatin.Start);
                lblEndTime.ContentShortTime(Times.EndTheoDateTime);

                lblHmidiS.Visibility = Visibility.Hidden;
                lblHmidiE.Visibility = Visibility.Hidden;

                ctrlTyJournee.ChangeTypeJourneeWithoutAction(EnumTypesJournees.Complete);
                ctrlTyJournee.IsEnabledChange = false;

                TimeSpan tsfinMat = TimesUtils.GetTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions,
                    EnumTypesJournees.Matin);

                if (PrgOptions.IsAutoBadgeAtStart && EtatBadger == -1 && !appArgs.NoAutoBadgeage)
                {

                    if (RealTimes.RealTimeTsNow >= new TimeSpan(6, 45, 0))
                    {

                        _logger.Info("Badgeage automatique");
                        btnBadger.IsEnabled = false;
                        BadgerWorker.BadgeFullAction();
                        btnBadger.IsEnabled = true;

                        PushNewInfo("Badgeage automatique réalisé. Fin de la matinée à " +
                                    tsfinMat.ToString(Cst.TimeSpanFormat));
                        PrgOptions.IsAutoBadgeAtStartDelayed = false;
                    }
                    else
                    {
                        PrgOptions.IsAutoBadgeAtStartDelayed = true;
                        _logger.Info("Badgeage automatique du matin - en attente");
                    }
                }
                else if (EtatBadger == 0)
                {
                    PushNewInfo("Fin de la matinée à " + tsfinMat.ToString(Cst.TimeSpanFormat));
                }

                if (PrgOptions.IsFirstRun)
                {

                    MessageBoxResult question = MessageBox.Show(
                        "Si vous avez déjà effectué des badgeages pour cette journée, vous pouvez les reporter dans l'application. Désirez-vous reporter vos badgeages du jour ?"
                        , "Question",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (question == MessageBoxResult.Yes)
                    {
                        btnModTimes_Click(null, null);
                    }
                }



            }

            RegisterNotifications();

            _mainTimerManager.OnPauseToggled += MainTimerManagerOnOnPauseToggled;
            _mainTimerManager.Interval = new TimeSpan(0, 0, 10);
            _mainTimerManager.OnTick += ClockUpdTimerOnOnTick;




            IsFullyLoaded = true;
            ClockUpdTimerOnOnTick(null, null);
            PrgSwitch.CanCheckUpdate = true;


            Loaded += (sender, args) =>
            {

                Dispatcher.BeginInvoke(DispatcherPriority.Send,
                    new Action(() =>
                    {
                        if (PrgSwitch.IsInBadgeWork && BadgerWorker.Progress != null)
                        {
                            BadgeageProgressView b = new BadgeageProgressView(PrgOptions);
                            b.BackgrounderRef = BadgerWorker.Progress.BackgrounderRef;
                            int bStep = BadgerWorker.Progress.Step;

                            BadgerWorker.Progress.Hide();
                            BadgerWorker.Progress.Close();
                            BadgerWorker.Progress = b;
                            b.EnterStep(bStep);
                            b.Show();
                        }

                    })
                );





                _mainTimerManager.Start();

                AfterLoadPostInitComponent();
                CoreAppBridge.Instance.RegisterMethodsForPlugin(this);

                PrgSwitch.IsCloseWithSave = true;

                ReloadPauseState();

                if (EtatBadger < 2)
                {
                    CoreAudioFactory.InitProcess();
                }

                if (_spv != null)
                {
                    _spv.Show();
                }

                FixLastDay();


            };

        }



        private void SetDarkTheme()
        {
            Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#121212"));

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(this); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(this, i);

            }
        }

        private void AssignBetaRole()
        {
            PrgSwitch.IsBetaUser = (bool)PluginMgr.PlayHookAndReturn("IsBetaUser", null, typeof(bool)).ReturnFirstOrDefaultResultObject();

            if (!PrgSwitch.IsBetaUser)
            {

                PrgOptions.ResetSpecOption();
            }
        }

        internal void ArchiveDatas()
        {
            _logger.Info("ArchiveDatas");

            ZipArchiveManager z = new ZipArchiveManager();
            z.Directory = new DirectoryInfo(Cst.PointagesDirName);
            z.Compress();
            z.Directory = new DirectoryInfo(Cst.ScreenshotDirName);
            z.Compress();
            z.Directory = new DirectoryInfo(Cst.LogArchiveDirName);
            z.Compress();

        }

        private void PostInitializeComponent()
        {

#if !DEBUG
            rectDebug.Visibility = Visibility.Hidden;
            //wrapPanelTop.Visibility = Visibility.Hidden;
#endif

            imgBtnUpdate.Visibility = Visibility.Collapsed;
            imgBtnWarnLicence.Visibility = Visibility.Collapsed;
            imgBtnAutoBadgeMidi.Visibility = Visibility.Collapsed;
            imgBtnFirefoxLoaded.Visibility = Visibility.Collapsed;

            lblInfos.Visibility = Visibility.Collapsed;
            lblInfoLbl.Visibility = Visibility.Collapsed;

            lblTpsTravMatin.Visibility = Visibility.Collapsed;
            lblTpsTravAprem.Visibility = Visibility.Collapsed;
            lblPauseTime.Visibility = Visibility.Collapsed;

            ModifyAccentColor();

            /*
            chkMatin.Click += (sender, args) =>
            {
                ChangeTypeJournee();
            };
            chkAmidi.Click += (sender, args) =>
            {
                ChangeTypeJournee();
            };
            */


            lienCptTpsReel.Click += (sender, args) => MiscAppUtils.GoTo(PrgOptions.UrlCptTpsReel);
            lienMesBadgeages.Click += (sender, args) => MiscAppUtils.GoTo(PrgOptions.UrlMesPointages);
            lienSirhius.Click += (sender, args) => MiscAppUtils.GoTo(PrgOptions.UrlSirhius);

            PrgSwitch.IsSoundOver = true;

            MenuItem badgerTitleItem = new MenuItem();
            badgerTitleItem.Header = "Action du bouton :";
            badgerTitleItem.IsCheckable = false;
            badgerTitleItem.IsEnabled = false;

            MenuItem badgerOnlyItem = new MenuItem();
            badgerOnlyItem.Header = "Badger uniquement";

            MenuItem badgerPauseItem = new MenuItem();
            badgerPauseItem.Header = "Badger un interval";

            MenuItem badgerShowPausesItem = new MenuItem();
            badgerShowPausesItem.Header = "Consulter les pauses";
            badgerShowPausesItem.Click += (sender, args) => LoadDetailView();

            ContextMenu btnMContextMenu = new ContextMenu();
            btnMContextMenu.Items.Add(badgerTitleItem);
            btnMContextMenu.Items.Add(badgerOnlyItem);
            btnMContextMenu.Items.Add(badgerPauseItem);
            btnMContextMenu.Items.Add(new Separator());
            btnMContextMenu.Items.Add(badgerShowPausesItem);



            btnBadgerM.ContextMenu = btnMContextMenu;





            MenuItem mainCtxQuitItem = new MenuItem();
            mainCtxQuitItem.Header = "Quitter";

        }
        private void AfterLoadPostInitComponent()
        {

            ctrlTyJournee.OnTypeJourneeChange += (e) =>
            {
                ChangeTypeJournee();
            };


            ctrlCompteur.PwinRef = this;

            ctrlCompteur.IsEnabledCtrl = true;
            ctrlCompteur.UpdateInfos();

            pbarColSecond.Width = new GridLength(0, GridUnitType.Star);
        }


        private void ModifyAccentColor()
        {
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2)
            {
                try
                {
                    int argbColor =
                        (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM",
                            "ColorizationColor", null);
                    Color color = Color.FromArgb(argbColor);
                    System.Windows.Media.Color colorA = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);

                    rectMatin.Fill =
                        new SolidColorBrush(colorA);
                    rectAprem.Fill =
                        new SolidColorBrush(colorA);
                    rectPause.Fill = new SolidColorBrush(colorA);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Erreur lors de l'application d'une couleur personnalisée");
                    _logger.Warn(ex.Message);
                    _logger.Warn(ex.StackTrace);
                }
            }
        }

        #region EventHandler

        private void Window_Deactivated(object sender, EventArgs e)
        {
            //_notifyIcon.Visible = true;
            ShowInTaskbar = false;

        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //_notifyIcon.Visible = false;
            ShowInTaskbar = true;

            if (_spv != null && _spv.Visibility == Visibility.Visible)
            {
                _spv.BringToFront();
            }


        }

        private void MainTimerManagerOnOnPauseToggled(MainTimerManager mainTimerManager)
        {
            // PrgSwitch.PbarMainTimerActif = mainTimerManager.IsPaused;
            if (!mainTimerManager.IsPaused)
            {
                ctrlCompteur.SetFontColor(Cst.SCBBlack);
                ctrlCompteur.CurrentState = CompteurControl.CompteurState.TempsRestantJour;

                pauseHorsPeriodeTimer.Start();
                pauseHorsPeriodeTimer.Stop();


                PrgSwitch.PbarMainTimerActif = true;
                lblEndTime.Foreground = Cst.SCBBlack;
                lblEndTime.ToolTip = null;

                if (EtatBadger < 2)
                {
                    SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee, Times.GetTpsPause());

                }
                else
                {
                    TimeSpan tmpEndMore = Times.GetTpsPause();

                    TimeSpan tmpsPause = Times.PlageTravAprem.Start - Times.PlageTravMatin.EndOrDft;
                    var diffPause = tmpsPause - PrgOptions.TempsMinPause;

                    if (diffPause.TotalSeconds > 0)
                    {
                        tmpEndMore += diffPause;
                    }

                    if (PrgOptions.IsStopCptAtMaxDemieJournee && Times.GetTpsTravMatin().CompareTo(PrgOptions.TempsMaxDemieJournee) > 0)
                    {
                        tmpEndMore += (Times.GetTpsTravMatin() - PrgOptions.TempsMaxDemieJournee);
                    }

                    SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee, tmpEndMore);

                }

                lblEndTime.ContentShortTime(Times.EndTheoDateTime);
                lblEndTime.Content += Times.IsTherePauseAprem() || Times.IsTherePauseMatin() ? "*" : "";

                ClockUpdTimerOnOnTick(null, null);
            }
            else
            {
                // lblTpsTravReel.Foreground = Cst.SCBGrey;
                if (pauseHorsPeriodeTimer != null && pauseHorsPeriodeTimer.IsEnabled)
                {
                    pauseHorsPeriodeTimer.Stop();
                }
                pauseHorsPeriodeTimer = new DispatcherTimer();
                pauseHorsPeriodeTimer.Tick += (sender, args) =>
                {
                    IntervalTemps ivlTemps = Times.PausesHorsDelai.FirstOrDefault(r => !r.IsIntervalComplet());
                    if (ivlTemps != null)
                    {
                        TimeSpan tpsPause = AppDateUtils.DtNow().TimeOfDay - ivlTemps.Start.TimeOfDay;

                        ctrlCompteur.SetText(tpsPause.ToString("h':'mm':'ss"));
                        ctrlCompteur.SetTitle("Temps de la pause en cours :");
                        ctrlCompteur.SetVisibility(CompteurControl.CompteurVisibility.OnlyMain);
                        //lblTpsTravReel.Content = tpsPause.ToString("h':'mm':'ss");
                        //lblTpsTravReelLbl.Content = "Temps de la pause en cours :";

                        DateTime newEndTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start,
                                    PrgOptions, EnumTypesJournees.Complete)
                                                        + Times.GetTpsPause() + tpsPause;
                        lblEndTime.ContentShortTime(newEndTime);
                        //lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;

                    }
                };
                pauseHorsPeriodeTimer.Interval = new TimeSpan(0, 0, 1);

                ctrlCompteur.CurrentState = CompteurControl.CompteurState.CustumExt;

                pauseHorsPeriodeTimer.Start();

                ctrlCompteur.SetToolTip(null);

                PrgSwitch.PbarMainTimerActif = false;

                lblEndTime.Foreground = Cst.SCBGrey;
                lblEndTime.ToolTip = "Le temps de fin de journée augmente avec la durée de la pause en cours";


            }

        }

        private void CurrentOnSessionEnding(object sender, SessionEndingCancelEventArgs eventArgs)
        {
            //if (eventArgs.ReasonSessionEnding == ReasonSessionEnding.Logoff)
            //    MessageBox.Show("User Logged off the computer");

            if (eventArgs.ReasonSessionEnding != ReasonSessionEnding.Logoff &&
                eventArgs.ReasonSessionEnding != ReasonSessionEnding.Shutdown) return;

            if (EtatBadger < 3)
            {
                _logger.Info("Suspension de l'arrêt demandé");

                ShowInTaskbar = true;
                var res = MessageBox.Show("Avez-vous pensé à badger avant d'éteindre l'ordinateur ?", "Avez-vous pensé à badger avant d'éteindre l'ordinateur ?",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (res == MessageBoxResult.No)
                {
                    WindowState = WindowState.Normal;
                    eventArgs.Cancel = true;

                }

            }
        }

        private void MsgBoxAfterSessionUnlock(object sender, SessionSwitchEventArgs e)
        {
            if (EtatBadger == 1 && e.Reason == SessionSwitchReason.SessionUnlock)
            {
                if (apresUnlockMidiWhileBadgeDebutApremTimer != null)
                {
                    apresUnlockMidiWhileBadgeDebutApremTimer.Stop();
                }

                apresUnlockMidiWhileBadgeDebutApremTimer = new DispatcherTimer
                {
                    IsEnabled = true,
                    Interval = new TimeSpan(0, 5, 0)
                };
                apresUnlockMidiWhileBadgeDebutApremTimer.Tick += (o, args) =>
                {
                    if (EtatBadger > 1)
                    {
                        apresUnlockMidiWhileBadgeDebutApremTimer.Stop();
                        return;
                    }

                    ShowInTaskbar = true;
                    MessageBoxResult res = MessageBoxUtils.TopMostMessageBox(
                        String.Format(
                            "Bien déjeuné ?{0}La session a été déverrouillée pendant la pause du midi : pensez à Badger !",
                            Environment.NewLine),
                        "Question",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question
                    );

                    RestoreWindow();
                    ShowInTaskbar = false;


                    if (res != MessageBoxResult.OK)
                    {
                        apresUnlockMidiWhileBadgeDebutApremTimer.Stop();
                    }
                };
                apresUnlockMidiWhileBadgeDebutApremTimer.Start();

            }

        }

        private void OnAfterSessionLock(object sender, SessionSwitchEventArgs e)
        {
            if (EtatBadger == 0 && e.Reason == SessionSwitchReason.SessionLock && RealTimes.RealTimeTsNow >= PrgOptions.PlageFixeMatinFin)
            {
                _logger.Debug("SessionLock->Play");
                PlayAdvertise();
            }
        }

        private void btnBadger_Click(object sender, RoutedEventArgs e)
        {
            btnBadger.IsEnabled = false;

            if (EtatBadger == -1)
            {
                bool isSpecBadgeage = BadgeageM1Manuell();

                if (isSpecBadgeage)
                {
                    btnBadger.IsEnabled = true;
                    return;
                }
            }

            int origDelay = PrgOptions.LastBadgeDelay;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                PrgOptions.LastBadgeDelay = 0;
            }


            BadgerWorker.BadgeFullAction();

            PrgOptions.LastBadgeDelay = origDelay;

            ClockUpdTimerOnOnTick(null, null);

        }

        private bool BadgeageM1Manuell()
        {
            bool isSpecBadgeage;
            MessageBoxResult v = MessageBox.Show("Voulez-vous effectuer le premier badgeage de la journée ? Cliquez sur Oui pour badger ou Non pour entrer l'heure manuellement (dans le cas où le pointage a déjà été effectué via la page)",
                "Question",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
                );

            if (v == MessageBoxResult.Cancel)
            {
                isSpecBadgeage = true;
            }
            else if (v == MessageBoxResult.No)
            {
                _logger.Info("Premier badgeage du matin réalisé manuellement");
                DateTime? dtStart = SaisieManuelleTsView.ShowAskForDateTime();
                if (!dtStart.HasValue)
                {
                    _logger.Debug("btnBadger_Click : -1 : Aucune valeur saisie.");

                    MessageBox.Show("Aucune heure n'a été entrée. Premier badgeage annulé.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );

                    isSpecBadgeage = true;
                    return true;
                }

                Times.PlageTravMatin.Start = dtStart.Value;
                if (Times.PlageTravMatin.Start.TimeOfDay.CompareTo(PrgOptions.HeureMinJournee) < 0)
                {
                    Times.PlageTravMatin.Start = Times.PlageTravMatin.Start.ChangeTime(PrgOptions.HeureMinJournee);
                }




                AdaptUiFromState(EtatBadger + 1, null);
                EtatBadger = 0;

                ClockUpdTimerOnOnTick(null, null);

                isSpecBadgeage = true;

                _logger.Info("Sauvegarde de la session (EtatBadger: {0}", EtatBadger);
                PointageSaverObj.SaveCurrentDayTimes();
            }
            else
            {
                isSpecBadgeage = false;
            }

            return isSpecBadgeage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!btnBadger.IsEnabled)
            {
                StopTimers();
                btnBadger.IsEnabled = true;
                return;
            }


            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {



                PrgSwitch.IsRealClose = true;

                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    RestartApp();
                    return;
                }

            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                PointageSaverObj.SaveCurrentDayTimes();
                MessageBox.Show("Temps du jour enregistrés.", "Information");
            }

            Close();

        }

        private void btnBadgerM_Click(object sender, RoutedEventArgs e)
        {
            if ((PrgOptions.IsBtnManuelBadgeIsWithHotKeys
                    && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                || !PrgOptions.IsBtnManuelBadgeIsWithHotKeys)
            {

                _logger.Info("Badger Manuellement");

                btnBadger.IsEnabled = false;
                btnBadgerM.IsEnabled = false;

                int origDelay = PrgOptions.LastBadgeDelay;
                PrgOptions.LastBadgeDelay = 0;

                try
                {
                    PrgSwitch.IsInBadgeWork = true;
                    BadgerWorker.BadgeAction(PrgOptions.Uri, PrgOptions.UriParam, PrgOptions.UriVerif, time =>
                    {

                        btnBadgerM.IsEnabled = true;

                        if (!time.HasValue)
                        {
                            return;
                        }

                        if (Times.PausesHorsDelai.All(r => r.IsIntervalComplet()))
                        {
                            _logger.Info(" > Début de pause ({0})", time.Value.AtSec(Cst.SecondeOffset));

                            IntervalTemps ivlTemps = new IntervalTemps()
                            {
                                Start = time.Value.AtSec(Cst.SecondeOffset)
                            };
                            Times.PausesHorsDelai.Add(ivlTemps);

                            btnBadgerM.Content = "Badger et reprendre";
                            PrgSwitch.PauseCurrentState = EnumStatePause.IN_PROGRESS;

                            _mainTimerManager.Pause();

                            if (PrgOptions.IsPreloadFF)
                            {
                                if (!File.Exists(PrgOptions.FfExePath))
                                {
                                    NotifManager.NotifyNow("Firefox n'existe pas à cette adrese : " + PrgOptions.FfExePath,
                                        "Erreur");
                                    PrgSwitch.IsFfNormalyLoaded = false;
                                }
                                else
                                {
                                    FfDriverSingleton.Instance.Load(PrgOptions);
                                    PrgSwitch.IsFfNormalyLoaded = true;
                                }
                            }


                        }
                        else
                        {
                            _logger.Info(" > Fin de pause ({0})", time.Value.AtSec(Cst.SecondeOffset));

                            IntervalTemps ivlTemps = Times.PausesHorsDelai.First(r => !r.IsIntervalComplet());
                            ivlTemps.End = time.Value.AtSec(Cst.SecondeOffset);

                            btnBadgerM.Content = "Badger et suspendre";

                            btnBadger.IsEnabled = true;


                            PrgSwitch.PauseCurrentState = EnumStatePause.HAVE_PAUSES;

                            _mainTimerManager.Resume();
                        }

                        PrgOptions.LastBadgeDelay = origDelay;

                    }, -2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur s'est produite lors du badgeage. Celui n'a surement pas été pris en compte dans l'outils GTA." + Environment.NewLine + Environment.NewLine
                   + "Consulter le ficher log pour plus d'information sur l'erreur : "
                   + ex.Message
                   , "Erreur");
                    _logger.Error(ex.Message);
                    _logger.Error(ex.StackTrace);

                }

            }
        }

        private void ReloadUiFromInterface()
        {
            PointageElt p = PointageSaverObj.LoadIncomplete();
            _logger.Debug(p.ToString());

            try
            {
                EtatBadger = p.EtatBadger;
                ctrlTyJournee.IsEnabledChange = true;
                if (EtatBadger >= 0)
                {
                    Times.PlageTravMatin.Start = DateTime.Parse(p.B0).AtSec(Cst.SecondeOffset);

                    SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee, null);

                }
                if (EtatBadger >= 1)
                {
                    Times.PlageTravMatin.EndOrDft = DateTime.Parse(p.B1).AtSec(Cst.SecondeOffset);
                }
                if (EtatBadger >= 2)
                {
                    Times.PlageTravAprem.Start = DateTime.Parse(p.B2).AtSec(Cst.SecondeOffset);
                    ctrlTyJournee.IsEnabledChange = false;
                }
                if (EtatBadger >= 3)
                {
                    Times.PlageTravAprem.End = DateTime.Parse(p.B3).AtSec(Cst.SecondeOffset);
                    Times.EndTheoDateTime = Times.PlageTravAprem.End.Value;
                    Times.MaxTimeForOneDay = TimesUtils.GetMaxDateTimeForOneDay(Times.EndTheoDateTime, PrgOptions, TypeJournee);

                }

                if (p.Pauses != null && p.Pauses.Any())
                {
                    Times.PausesHorsDelai = p.Pauses;
                }

                NotifManager.SetNotifShow(Cst.NotifCust1Name, p.IsNotif1Showed);
                NotifManager.SetNotifShow(Cst.NotifCust2Name, p.IsNotif2Showed);

                TypeJournee = EnumTypesJournees.GetFromIndex(p.TypeJournee);
                ctrlTyJournee.ChangeTypeJourneeWithoutAction(TypeJournee);

                OldEtatBadger = p.OldEtatBadger;

                UpdRealTimes();

                AdaptUiLowerThanState();

                ClockUpdTimerOnOnTick(null, null);


                if (EnumTypesJournees.IsDemiJournee(TypeJournee))
                {
                    ChangeTypeJournee();
                }

            }
            catch (Exception e)
            {
                string msg = "Erreur lors de la lecture de la journée mémorisée. Supprimez le fichier xml du jour s'il existe et relancez le programme";
                _logger.Error(msg);
                _logger.Error(e.Message);
                _logger.Error(e.StackTrace);
                MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void ReloadPauseState()
        {
            if (Times.PausesHorsDelai != null && Times.PausesHorsDelai.Any())
            {

                _logger.Debug("Des pauses sont à charger.");
                if (Times.PausesHorsDelai.All(r => r.IsIntervalComplet()))
                {
                    _logger.Debug("> Aucune pause en cours");
                    //  btnBadger.IsEnabled = true;
                    btnBadgerM.Content = "Badger et suspendre";
                    PrgSwitch.PauseCurrentState = EnumStatePause.HAVE_PAUSES;

                    /*
                    Times.EndTheoDateTime = Times.EndTheoDateTime
                                                   + Times.GetTpsPause();
                    Times.MaxTimeForOneDay = TimesUtils.GetMaxDateTimeForOneDay(Times.EndTheoDateTime, PrgOptions, TypeJournee);

                    lblEndTime.ContentShortTime(Times.EndTheoDateTime);
                    lblEndTime.Content += "*";
                    */
                }
                else
                {
                    _logger.Debug("> Une ou plusieurs (!) pauses en cours");
                    btnBadgerM.Content = "Badger et reprendre";
                    //  btnBadger.IsEnabled = false;
                    PrgSwitch.PauseCurrentState = EnumStatePause.IN_PROGRESS;

                    _mainTimerManager.Pause();
                }
            }
        }


        private void ClockUpdTimerOnOnTick(object sender, EventArgs eventArgs)
        {
            if (!IsFullyLoaded)
            {
                return;
            }

            rectTemoinActivite.Visibility = rectTemoinActivite.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            //this.ToolTip = RealTimes.RealTimeDtNow.ToString("dd/MM/yy HH:mm:ss");

            bool isMaxDepass = UpdRealTimes();
            PluginMgr.PlayHook("OnClockUpdTimerOnOnTick", null);


            TimeSpan diffTotal = Times.EndTheoDateTime - Times.PlageTravMatin.Start;
            TimeSpan diff = RealTimes.RealTimeDtNow - Times.PlageTravMatin.Start;

            if (PrgSwitch.PbarMainTimerActif && EtatBadger < 3)
            {
                TimeSpan tpsTravRestant = Times.EndTheoDateTime - RealTimes.RealTimeDtNow;

                if (tpsTravRestant.TotalSeconds >= 0)
                {
                    pbarColSecond.Width = new GridLength(0);
                    pbarTime.IsIndeterminate = false;
                    pbarBtnBadger.IsIndeterminate = false;
                    pbarTime.Value = 100 * diff.TotalSeconds / diffTotal.TotalSeconds;

                    double tR = 0;
                    double tM = 0;
                    double pbV = 0;
                    if (EtatBadger == 0)
                    {
                        tR = (PrgOptions.PlageFixeMatinFin - RealTimes.RealTimeTsNow).TotalSeconds;
                        tM = (PrgOptions.PlageFixeMatinFin - PrgOptions.PlageFixeMatinStart).TotalSeconds;
                        pbV = 100 * (1 - tR / tM);
                    }
                    else if (EtatBadger == 2)
                    {
                        tR = (PrgOptions.PlageFixeApremFin - RealTimes.RealTimeTsNow).TotalSeconds;
                        tM = (PrgOptions.PlageFixeApremFin - PrgOptions.PlageFixeApremStart).TotalSeconds;
                        pbV = 100 * (1 - tR / tM);
                    }


                    pbarBtnBadger.Value = pbV;
                    pbarBtnBadger.Color = null;

                }
                else if (EtatBadger >= 0)
                {
                    pbarTime.Value = 100;
                    pbarTimeExtension.IsIndeterminate = false;
                    pbarColSecond.Width = new GridLength(1, GridUnitType.Star);

                    TimeSpan maxTpsAutorise = (PrgOptions.TempsMaxJournee - (PrgOptions.TempsDemieJournee + PrgOptions.TempsDemieJournee));
                    TimeSpan tsFinJourneeReglementaire = Times.EndTheoDateTime.TimeOfDay + maxTpsAutorise;
                    TimeSpan tpsMaxTravRestant = tsFinJourneeReglementaire - RealTimes.RealTimeTsNow;
                    if (tpsMaxTravRestant.TotalSeconds >= 0)
                    {
                        double percent = 100 - (100 * tpsMaxTravRestant.TotalSeconds / maxTpsAutorise.TotalSeconds);
                        pbarTimeExtension.Value = percent;
                        pbarTimeExtension.Foreground = percent >= 50
                            ? (percent >= 10 ? Cst.SCBDarkRed : Cst.SCBOrange)
                            : Cst.SCBGold;
                        pbarTimeExtension.ToolTip =
                            String.Format(
                                "Travail au-dela du temps hebdomadaire : fin de l'accumulation du temps supplémentaire à {0}.",
                                tsFinJourneeReglementaire.ToStrSignedhhmm());
                    }
                    else
                    {
                        pbarTimeExtension.Value = 100;
                        pbarTimeExtension.IsIndeterminate = true;
                        pbarTimeExtension.ToolTip = String.Format("Fin du temps depuis {0}. Avez-vous pensé à badger ?", tsFinJourneeReglementaire.ToStrSignedhhmm());

                    }


                }
            }

            if (RealTimes.RealTimeDtNow.DayOfYear != Times.TimeRef.DayOfYear)
            {
                // Le jour a changé. Il faut redémarrer.
                // PointageSaverObj.SaveCurrentDayTimes();
                PrgSwitch.IsCloseWithSave = false;
                RestartApp();
            }

            //DoNotification();
            NotifManager.DoNotification(RealTimes.RealTimeTsNow, EnumTypesTemps.RealTime, PrgOptions.IsGlobalShowNotifications);
            NotifManager.DoNotification(RealTimes.RealTimeTempsTravaille, EnumTypesTemps.TpsTrav, PrgOptions.IsGlobalShowNotifications);
            NotifManager.DoNotification(RealTimes.RealTimeTempsTravailleMatin, EnumTypesTemps.TpsTravMatin, PrgOptions.IsGlobalShowNotifications);
            NotifManager.DoNotification((RealTimes.RealTimeTempsTravaille - RealTimes.RealTimeTempsTravailleMatin), EnumTypesTemps.TpsTravAprem, PrgOptions.IsGlobalShowNotifications);

            if (PrgOptions.IsStopCptAtMax
               && isMaxDepass
               && !PrgSwitch.IsTimerStoppedByMaxTime)
            {
                PrgSwitch.PbarMainTimerActif = false;
                pbarTimeExtension.IsIndeterminate = true;

                PrgSwitch.IsTimerStoppedByMaxTime = true;
            }


            if (PrgOptions.IsAutoBadgeAtStartDelayed && PrgOptions.IsAutoBadgeAtStart && RealTimes.RealTimeTsNow >= new TimeSpan(6, 45, 0) && EtatBadger == -1)
            {
                PrgOptions.IsAutoBadgeAtStartDelayed = false;

                _logger.Info("Badgeage automatique du matin - délayé");
                btnBadger.IsEnabled = false;
                BadgerWorker.BadgeFullAction();
                btnBadger.IsEnabled = true;
            }

            // ***************
            // On précharge FF si les conditions de temps, ou d'instance sont réunis.
            if (PrgOptions.IsPreloadFF)
            {
                bool isOkToStartPreloadedFf = PrgSwitch.IsFfForcedLoaded;



                if (EtatBadger == EnumBadgeageType.PLAGE_TRAV_MATIN_START.Index
                    && TypeJournee == EnumTypesJournees.Complete
                    && RealTimes.RealTimeTsNow >= PrgOptions.PlageFixeMatinFin)
                {
                    isOkToStartPreloadedFf = true;
                }

                if (EtatBadger == EnumBadgeageType.PLAGE_TRAV_MATIN_END.Index
                    && TypeJournee == EnumTypesJournees.Complete
                    && RealTimes.RealTimeTsNow >= (PrgOptions.PlageFixeMatinFin + PrgOptions.TempsMinPause))
                {
                    isOkToStartPreloadedFf = true;
                }

                if (EtatBadger == EnumBadgeageType.PLAGE_TRAV_APREM_START.Index
                    && (TypeJournee == EnumTypesJournees.Complete || EnumTypesJournees.IsDemiJournee(TypeJournee))
                    && (
                        (TypeJournee == EnumTypesJournees.Complete && RealTimes.RealTimeTsNow >= PrgOptions.PlageFixeApremFin)
                        || (TypeJournee == EnumTypesJournees.Matin && RealTimes.RealTimeTsNow >= PrgOptions.PlageFixeMatinFin)
                        || (TypeJournee == EnumTypesJournees.ApresMidi && RealTimes.RealTimeTsNow >= PrgOptions.PlageFixeApremFin)
                    )
                    )
                {
                    isOkToStartPreloadedFf = true;
                }

                if (isOkToStartPreloadedFf)
                {
                    if (!File.Exists(PrgOptions.FfExePath))
                    {
                        NotifManager.NotifyNow("Firefox n'existe pas à cette adrese : " + PrgOptions.FfExePath,
                            "Erreur");
                        PrgSwitch.IsFfNormalyLoaded = false;
                    }
                    else
                    {
                        FfDriverSingleton.Instance.Load(PrgOptions);
                        PrgSwitch.IsFfNormalyLoaded = true;
                    }
                }
                else
                {
                    if (!PrgSwitch.IsInBadgeWork)
                    {
                        FfDriverSingleton.Instance.Quit();
                    }
                    PrgSwitch.IsFfNormalyLoaded = false;
                }

            }

            // On adapte l'interface (l'icone d'instance de Ff préchargée)
            if (PrgSwitch.IsFfNormalyLoaded
                && FfDriverSingleton.Instance.IsLoaded())
            {
                imgBtnFirefoxLoaded.Visibility = Visibility.Visible;
            }
            else
            {
                imgBtnFirefoxLoaded.Visibility = Visibility.Collapsed;
                PrgSwitch.IsFfNormalyLoaded = false;
            }

            //
            // (TimesBadgerDto Times, AppOptions PrgOptions, AppSwitchs PrgSwitch, int EtatBadger, TimeSpan RealTimeTsNow)

            if (IsLoaded)
            {
                PluginMgr.PlayHook("FullOnClockUpdTimerOnOnTick", new object[]
                {
                 Times, PrgOptions, PrgSwitch, EtatBadger, RealTimes, TypeJournee
                });
            }


            //UpdateInfos();
            ctrlCompteur.UpdateInfos();

            if (PrgSwitch.CanCheckUpdate)
            {
                SignalUpdate();
            }

            if (PrgSwitch.IsCheckLicence)
            {
                PrgSwitch.IsCheckLicence = false;
                AfficheLicenceInfo();

            }

            if (PrgOptions.ShowTipsAtStart && EtatBadger >= 0 && !PrgSwitch.IsShowOnStartupDone && IsLoaded)
            {
                PrgSwitch.IsShowOnStartupDone = true;
                LoadsDidYouKnow();

            }
            if (EtatBadger >= 0 && !PrgSwitch.IsShowResumeLastDayNotif && IsLoaded)
            {

                DateTime? lastDayDt = ServicesMgr.Instance.JoursServices.GetPreviousDayOf(AppDateUtils.DtNow());
                if (lastDayDt == null)
                {
                    PrgSwitch.IsShowResumeLastDayNotif = true;
                    return;
                }

                JourEntryDto lastDay = ServicesMgr.Instance.JoursServices.GetJourData(lastDayDt.Value);
                if (!lastDay.IsHydrated)
                {
                    PrgSwitch.IsShowResumeLastDayNotif = true;
                    return;
                }


                if (lastDay.EtatBadger < 3)
                {

                }

                String title = "Résumé de votre dernière journée travaillée";
                String precTpsTrav = "non renseigné";
                if (lastDay.TpsTravaille.HasValue)
                {

                    if (lastDay.TpsTravaille.Value.CompareTo(PrgOptions.TempsMaxJournee) >= 0)
                    {
                        precTpsTrav = String.Format("{0} (réellement travaillé : {1})",
                            PrgOptions.TempsMaxJournee.ToString(Cst.TimeSpanFormatWithH),
                            lastDay.TpsTravaille.Value.ToString(Cst.TimeSpanFormatWithH)
                            );
                    }
                    else
                    {
                        precTpsTrav = lastDay.TpsTravaille.Value.ToString(Cst.TimeSpanFormatWithH);
                    }
                }

                String message = String.Format("Le {1}{0}Type de journée : {2}{0}Temps travaillé{3} : {4}",
                        Environment.NewLine,
                        lastDayDt.Value.ToString("D"),
                        lastDay.TypeJour.Libelle,
                        PrgOptions.IsAdd5minCpt ? " (avec 5min)" : "",
                        precTpsTrav
                     );
                MiscAppUtils.ShowNotificationBaloon(
                    _notifyIcon,
                    title, message,
                    null, 15000, null, PrgOptions.IsUseAlternateNotification);
                PluginMgr.PlayHook("OnNotifSend", new object[] { AppDateUtils.DtNow().TimeOfDay, title, message });


                PrgSwitch.IsShowResumeLastDayNotif = true;
            }



        }


        private bool UpdRealTimes()
        {
            // Maj la date Time courante
            RealTimes.RealTimeDtNow = AppDateUtils.DtNow();

            // Maj l'heure courante
            RealTimes.RealTimeTsNow = RealTimes.RealTimeDtNow.TimeOfDay;


            bool isMaxDepass = false;

            // Maj le temps travaille
            RealTimes.RealTimeTempsTravaille = TimesUtils.GetTempsTravaille(RealTimes.RealTimeDtNow, EtatBadger, Times, PrgOptions, TypeJournee, true, ref isMaxDepass);
            if (TypeJournee == EnumTypesJournees.Complete)
            {
                if (EtatBadger <= 1)
                {
                    RealTimes.RealTimeTempsTravailleMatin = RealTimes.RealTimeTempsTravaille;
                }
                else
                {
                    RealTimes.RealTimeTempsTravailleMatin = RealTimes.RealTimeTempsTravaille - (Times.PlageTravAprem.Start - Times.PlageTravMatin.EndOrDft);
                }
            }
            else
            {
                RealTimes.RealTimeTempsTravailleMatin = RealTimes.RealTimeTempsTravaille;
            }

            /**
             *             DateTime timeEndTheoRaw = TimesUtils.GetDateTimeEndTravTheoriqueBis(start, prgOptions, typeJournee);
            timeEndTheoRaw += endDelta.GetValueOrDefault();
            times.EndRawDateTime = timeEndTheoRaw;
            times.MaxTimeForOneDay = TimesUtils.GetMaxDateTimeForOneDay(timeEndTheoRaw, prgOptions, typeJournee);
            times.EndTheoDateTime = TimesUtils.ClassicTransform(timeEndTheoRaw, prgOptions);
             * */

            RealTimes.RealTimeMinTpsTravRestant = Times.EndTheoDateTime.TimeOfDay - RealTimes.RealTimeTsNow;
            RealTimes.RealTimeMaxTpsTravRestant = Times.MaxTimeForOneDay.TimeOfDay - RealTimes.RealTimeTsNow;


            return isMaxDepass;
        }

        private void UpdateInfos()
        {
            /*
            
            TimeSpan tpsRestant = Times.EndTheoDateTime - RealTimes.RealTimeDtNow;


            // Tps trav pour une journée ou une demie-journée.
            TimeSpan tTravTheo = TimesUtils.GetTpsTravTheoriqueOneDay(PrgOptions, TypeJournee);

            // if (RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) <= 0)
            //  {
            String lblTpsTrav = "Compteur temps travaillé du jour :";
            String msgTooltip = String.Format("{0}Double-cliquer pour afficher le temps de travail restant",
                PrgOptions.IsAdd5minCpt ? "Le temps travaillé prend en compte les 5 min supplémentaires." + Environment.NewLine : "");


            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(RealTimes.RealTimeTempsTravaille);

            if (PrgSwitch.IsTimeRemainingNotTimeWork)
            {

                msgTooltip = "Double-cliquer pour afficher le compteur temps travaillé du jour";
                lblTpsTrav = RealTimes.RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0
                    ? "Temps supplémentaire pour la journée :"
                    : "Temps restant pour la journée :";
                strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(tpsRestant);
            }
            else
            {
                if (Times.IsTherePauseAprem() || Times.IsTherePauseMatin())
                {
                    strTpsTrav += "*";
                    msgTooltip += Environment.NewLine + "Prend en compte les pauses effectuées durant la journée";
                }
            }

            lblTpsTravReelLbl.Content = lblTpsTrav;
            lblTpsTravReel.Content = strTpsTrav;
            lblTpsTravReel.ToolTip = msgTooltip;

            //      }

            if (RealTimes.RealTimeTsNow.CompareTo(PrgOptions.PlageFixeApremFin) >= 0)
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Visible;



                string tplTpsReelSuppl = "({0})";
                if (RealTimes.RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0)
                {
                    tplTpsReelSuppl = "(+{0})";

                    if (!PrgSwitch.IsMoreThanTpsTheo && RealTimes.RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) < 0)
                    {
                        PrgSwitch.IsMoreThanTpsTheo = true;
                        lblTpsTravReel.Foreground = Cst.SCBDarkGreen;

                    }
                    else if (RealTimes.RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) >= 0)
                    {
                        PrgSwitch.IsMoreThanTpsTheo = false;
                        lblTpsTravReel.Foreground = Cst.SCBDarkRed;
                    }
                }
                lblTpsTravReelSuppl.Content = String.Format(tplTpsReelSuppl, MiscAppUtils.TimeSpanShortStrFormat((RealTimes.RealTimeTempsTravaille - tTravTheo)));


            }

            /*
            if (RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0)
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Visible;
                lblTpsTravReelSuppl.Content = String.Format("(+{0})",
                    (RealTimeTempsTravaille - tTravTheo).ToString(Cst.TimeSpanAltFormat));

                if (!PrgSwitch.IsMoreThanTpsTheo && RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) < 0)
                {
                    PrgSwitch.IsMoreThanTpsTheo = true;
                    lblTpsTravReel.Foreground = Cst.SCBDarkGreen;

                }

            }
            else
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;
                PrgSwitch.IsMoreThanTpsTheo = false;
                lblTpsTravReel.Foreground = Cst.SCBBlack;
            }
            */
        }

        public void AfficheLicenceInfo(bool isForceShowMsgBox = false)
        {
            if (LicenceApp == null) return;

            bool showMsgBox = false;

            String message = String.Format("Badger2018 est activé.{0}{0}Type de licence : {1}.{0}Attribuée à : {2}{0}{0}Date d'expiration : {3}",
                    Environment.NewLine,
                    LicenceApp.TypeUser == 0 ? "ambassadeur" : "utilisateur",
                    LicenceApp.NiceName.Trim(),
                    LicenceApp.TypeUser == 0 ? "validitée perpétuelle" : LicenceApp.DateExpiration.ToShortDateString()

                );
            MessageBoxImage icon = MessageBoxImage.Information;

            if (RealTimes.RealTimeDtNow.AddDays(31) >= LicenceApp.DateExpiration && LicenceApp.TypeUser > 0)
            {
                icon = MessageBoxImage.Warning;
                message += Environment.NewLine + "Licence bientôt expirée : pensez à demander son renouvellement.";

                imgBtnWarnLicence.Visibility = Visibility.Visible;
                showMsgBox = true;
            }

            if (isForceShowMsgBox || showMsgBox)
            {
                MessageBox.Show(message, "Information sur la licence", MessageBoxButton.OK, icon);
            }
        }

        private void SignalUpdate(bool isForceCheck = false)
        {
            //UpdateChecker chk = UpdateChecker.Instance;
            if (!UpdaterMgr.IsUpdateEnabled) return;


            bool isOkToSignalUpd = false;
            if (!isForceCheck && EtatBadger == 0 && UpdaterMgr.IsNewUpdateAvalaible &&
                UpdaterMgr.UpdateCheckTag.Equals("launch"))
            {
                UpdaterMgr.UpdateCheckTag = "-1";

                isOkToSignalUpd = true;
            }

            if (isForceCheck || (EtatBadger == 2 && UpdaterMgr.UpdateCheckTag.Equals("-1")))
            {
                UpdaterMgr.CheckForUpdates("2", Assembly.GetEntryAssembly().GetName().Version.ToString());
                if (UpdaterMgr.IsNewUpdateAvalaible)
                {
                    isOkToSignalUpd = true;
                }

            }

            if (!isOkToSignalUpd)
            {
                imgBtnUpdate.Visibility = UpdaterMgr.IsNewUpdateAvalaible ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            // Invite différente selon le nombre de mises à jour à effectuer.
            bool isGoToUpd = false;
            UpdateInfoDto upd = UpdaterMgr.ListReleases[0];
            if (UpdaterMgr.ListReleases.Count == 1)
            {
                var result =
                    MessageBoxUtils.TopMostMessageBox(
                        "Une nouvelle version du programme est disponible :" + Environment.NewLine +
                        " Titre : " + upd.Title + Environment.NewLine +
                        " Version : " + upd.Version + Environment.NewLine +
                        " Description : " + upd.Description + Environment.NewLine + Environment.NewLine +
                        "Voulez-vous effectuer la mise à jour ?", "Information", MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                isGoToUpd = MessageBoxResult.Yes.Equals(result);
            }
            else
            {
                var result =
                   MessageBoxUtils.TopMostMessageBox(
                       "Des nouvelles mises à jour sont disponibles." + Environment.NewLine + Environment.NewLine +
                       "Voulez-vous passer en revue les mises à jour, puis démarrer la procédure de mise à jour ?", "Information", MessageBoxButton.YesNo,
                       MessageBoxImage.Information);
                if (MessageBoxResult.Yes.Equals(result))
                {
                    UpdatesReviewerView upView = new UpdatesReviewerView(UpdaterMgr.ListReleases);
                    upView.ShowDialog();
                    isGoToUpd = upView.IsDoUpdate;
                }
            }

            // Tout est OK, on lance la mise à jour
            if (isGoToUpd)
            {
                try
                {
                    if (!UpdaterMgr.UpdateProgramTo(upd)) return;
                    PrgSwitch.IsRealClose = true;
                    Close();
                }
                catch (Exception ex)
                {
                    ExceptionMsgBoxView.ShowException(ex, "Processus de mise à jour",
                        "Une erreur inconnue est survenue lors du processus de mise à jour. Celui-ci n'a pas aboutit. " +
                        "Consultez les fichiers de journalisation ou transférez les par email pour résoudre cette anomalie."
                        );
                }
            }
            else
            {
                PrgSwitch.CanCheckUpdate = false;
                imgBtnUpdate.Visibility = Visibility.Visible;
            }
        }

        private void LoadsDidYouKnow()
        {
            if (DidYouKnowWindow == null || !DidYouKnowWindow.IsLoaded)
            {
                DidYouKnowWindow = new DidYouKnowView();
                DidYouKnowWindow.LastIntTips = PrgOptions.TipsLastInt;
                DidYouKnowWindow.ShowTipsAtStart = PrgOptions.ShowTipsAtStart;
                DidYouKnowWindow.Closing += (sender, args) =>
                {
                    PrgOptions.TipsLastInt = DidYouKnowWindow.LastIntTips;
                    PrgOptions.ShowTipsAtStart = DidYouKnowWindow.ShowTipsAtStart;
                    OptionManager.SaveOptions(PrgOptions);
                    DidYouKnowWindow = null;
                };

            }

            DidYouKnowWindow.NextTips();

            if (!DidYouKnowWindow.IsLoaded)
            {
                DidYouKnowWindow.Show();
            }
        }



        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            bool specShowSpecOpt = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            bool isOrigOptAdd5Min = PrgOptions.IsAdd5minCpt;

            ShowInTaskbar = true;
            _notifyIcon.Visible = false;
            RestoreWindow();

            OptionsView opt = new OptionsView(PrgOptions, PrgSwitch.IsBetaUser, _notifyIcon, specShowSpecOpt, this);
            _logger.Debug("Chargement fenêtre Options");


            opt.ShowDialog();

            if (opt.HasChangeOption)
            {
                _logger.Info("Les options ont changées => Mise à jour");

                PrgOptions = opt.NewOptions;
                OptionManager.SaveOptions(PrgOptions);

                RegisterNotifications();

                AdaptUiFromOptions(PrgOptions);
                UpdaterMgr.XmlUpdFilePath = PrgOptions.UpdateXmlUri;
                if (EtatBadger < 3)
                {
                    if (isOrigOptAdd5Min && !PrgOptions.IsAdd5minCpt)
                    {
                        Times.EndTheoDateTime += new TimeSpan(0, 5, 0);
                    }
                    else if (!isOrigOptAdd5Min && PrgOptions.IsAdd5minCpt)
                    {
                        Times.EndTheoDateTime -= new TimeSpan(0, 5, 0);
                    }

                    lblEndTime.ContentShortTime(Times.EndTheoDateTime);
                    lblEndTime.Content += Times.IsTherePauseAprem() || Times.IsTherePauseMatin() ? "*" : "";
                }


                if (PrgOptions.ShowOnScreenProgressBar)
                {
                    if (_spv == null)
                    {
                        _spv = new ScreenProgressBarView();
                        _spv.PairWithProgressBar(pbarTime);
                        _spv.Show();
                    }
                    else
                    {
                        _spv.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (_spv != null)
                    {
                        _spv.Visibility = Visibility.Collapsed;
                    }
                }

                if (!PrgOptions.IsPreloadFF)
                {
                    FfDriverSingleton.Instance.Quit();
                }

            }

            if (opt.IsRazNotifs)
            {
                NotifManager.ResetNotificationShow();
            }

            ShowInTaskbar = false;
            _notifyIcon.Visible = true;

            ClockUpdTimerOnOnTick(null, null);
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {

            if (PrgSwitch.IsRealClose || PrgOptions.ActionButtonClose == EnumActionButtonClose.CLOSE)
            {
                _mainTimerManager.OnTick -= ClockUpdTimerOnOnTick;
                _mainTimerManager.Stop();

                if (PrgOptions.IsFirstRun)
                {
                    PrgOptions.IsFirstRun = false;
                }

                if (EtatBadger != -1 && PrgSwitch.IsCloseWithSave)
                {
                    _logger.Info("Sauvegarde de la session (EtatBadger: {0})", EtatBadger);
                    PointageSaverObj.SaveCurrentDayTimes();
                }

                _logger.Debug("Sauvegarde des options");
                OptionManager.SaveOptions(PrgOptions);

                _notifyIcon.Visible = false;

                CoreAudioFactory.CloseProcess();
                FfDriverSingleton.Instance.Quit();

                // PluginMgr.PlayHook("OnClosingApp", new object[] { sender, cancelEventArgs });

            }
            else if (PrgOptions.ActionButtonClose == EnumActionButtonClose.MINIMIZE)
            {
                cancelEventArgs.Cancel = true;
                WindowState = WindowState.Minimized;

            }

        }



        #endregion EventHandler



        private void InitNotifyIcon()
        {
            _notifyIcon.Icon = PresentationImageUtils.DoGetIconSourceFromResource(GetType().Assembly.GetName().Name, "Paomedia-Small-N-Flat-Clock.ico");
            _notifyIcon.Visible = true;

            _notifyIconTrayMenu = new ContextMenuStrip();

            _notifyIconAppNameLblItem = new ToolStripLabel(Assembly.GetExecutingAssembly().GetName().Name);


            _notifyIconMatineeLblItem = new ToolStripLabel();
            _notifyIconMatineeLblItem.Visible = false;

            _notifyIconApremLblItem = new ToolStripLabel();
            _notifyIconApremLblItem.Visible = false;

            _nIconOpenOptionItem = new ToolStripMenuItem("O&ptions...");
            _nIconOpenOptionItem.ToolTipText = "Raccourci : touche F2";
            _nIconOpenOptionItem.Click += delegate (object sender, EventArgs args)
            {
                btnOptions_Click(null, null);
            };

            _nIconOpenTipsItem = new ToolStripMenuItem("Afficher &les astuces...");
            _nIconOpenTipsItem.ToolTipText = "Raccourci : touche F4";
            _nIconOpenTipsItem.Click += delegate (object sender, EventArgs args)
            {
                LoadsDidYouKnow();
            };

            _nIconShowNotificationItem = new ToolStripMenuItem("&Afficher les notifications");
            _nIconShowNotificationItem.Checked = PrgOptions.IsGlobalShowNotifications;
            _nIconShowNotificationItem.Click += delegate (object sender, EventArgs args)
            {
                PrgOptions.IsGlobalShowNotifications = !PrgOptions.IsGlobalShowNotifications;
                _nIconShowNotificationItem.Checked = PrgOptions.IsGlobalShowNotifications;
            };

            _nIconBadgerManItem = new ToolStripMenuItem("&Badger");
            _nIconBadgerManItem.Click += (sender, args) => btnBadger_Click(sender, null);

            _nIconRestoreItem = new ToolStripMenuItem("&Ouvrir");
            _nIconRestoreItem.Font = new Font(_nIconRestoreItem.Font, _nIconRestoreItem.Font.Style | System.Drawing.FontStyle.Bold);
            _nIconRestoreItem.Click += delegate (object sender, EventArgs args)
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
            };


            _nIconQuit = new ToolStripMenuItem("&Fermer");
            _nIconQuit.Click += (sender, args) =>
            {
                PrgSwitch.IsRealClose = true;
                Close();
            };

            //_notifyIconTrayMenu.Items.Add(_notifyIconAppNameLblItem);
            _notifyIconTrayMenu.Items.Add(_notifyIconMatineeLblItem);
            _notifyIconTrayMenu.Items.Add(_notifyIconApremLblItem);
            _notifyIconTrayMenu.Items.Add(new ToolStripSeparator());
            _notifyIconTrayMenu.Items.Add(_nIconBadgerManItem);
            _notifyIconTrayMenu.Items.Add(new ToolStripSeparator());
            _notifyIconTrayMenu.Items.Add(_nIconShowNotificationItem);
            _notifyIconTrayMenu.Items.Add(_nIconOpenOptionItem);
            _notifyIconTrayMenu.Items.Add(_nIconOpenTipsItem);
            _notifyIconTrayMenu.Items.Add(new ToolStripSeparator());
            _notifyIconTrayMenu.Items.Add(_nIconRestoreItem);
            _notifyIconTrayMenu.Items.Add(_nIconQuit);

            _notifyIcon.ContextMenuStrip = _notifyIconTrayMenu;

            _notifyIcon.Click += delegate (object sender, EventArgs args)
            {
                if (_spv != null && _spv.IsVisible)
                {
                    _spv.BringToFront();

                }
            };

            _notifyIcon.DoubleClick += delegate (object sender, EventArgs args)
            {
                RestoreWindow();
                this.Activate();
            };
            _notifyIcon.Text = Assembly.GetExecutingAssembly().GetName().Name;
        }


        internal void AdaptUiFromState(int state, TimeSpan? tmpsPause, bool managerChkBox = true)
        {
            _logger.Debug(String.Format(
                "AdaptUiFromState(state: {0}, tmpsPause: {1}, managerChkBox: {2})",
                state,
                tmpsPause != null ? tmpsPause.Value.ToString() : "null",
                managerChkBox)
             );

            UpdRealTimes();

            if (state == -1)
            {
                AdaptUiForStateM1();
            }
            else if (state == 0)
            {
                AdaptUiForState0(managerChkBox);

            }
            else if (state == 1)
            {
                AdaptUiForState1();
            }
            else if (state == 2 && tmpsPause != null)
            {
                AdaptUiForState2(tmpsPause);
            }
            else if (state == 3)
            {
                AdaptUiForState3();
            }
            btnBadger.IsEnabled = Times.PausesHorsDelai.All(r => r.IsIntervalComplet());

            _logger.Debug("FIN - AdaptUiFromState(...)");
        }

        private void AdaptUiForStateM1()
        {
            gridBtnBadger.Margin = Cst.BtnBadgerPositionAtLeft;
            btnBadger.ToolTip = "Cliquer pour badger le début de la matinée";
            btnBadger.Visibility = Visibility.Visible;

            lblStartTime.ContentShortTime(Times.PlageTravMatin.Start);
            lblEndTime.ContentShortTime(Times.EndTheoDateTime);

            ctrlTyJournee.IsEnabledChange = true;

            SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

            lblHmidiS.Visibility = Visibility.Hidden;
            lblHmidiE.Visibility = Visibility.Hidden;
            lblTpsTravMatin.Visibility = Visibility.Hidden;
            lblTpsTravAprem.Visibility = Visibility.Hidden;

            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", PrgOptions.PlageFixeMatinFin.ToString(Cst.TimeSpanFormatWithH));

            _notifyIconApremLblItem.Visible = false;
            _notifyIconMatineeLblItem.Visible = false;
            _nIconBadgerManItem.Text = "Badger le début de la matinée";
            _nIconBadgerManItem.ToolTipText = btnBadger.ToolTip.ToString();

            PushNewInfo(null);

            lblPauseTime.Visibility = Visibility.Collapsed;
        }


        private void AdaptUiForState0(bool managerChkBox)
        {
            // Etat de l'UI lorsque l'heure d'arrivé est placée.

            gridBtnBadger.Margin = TypeJournee == EnumTypesJournees.Complete ? Cst.BtnBadgerPositionAtLeft : Cst.BtnBadgerPositionAtCenter;
            btnBadger.ToolTip = "Cliquer pour badger la fin de la matinée";
            btnBadger.Visibility = Visibility.Visible;

            btnBadgerM.IsEnabled = true;

            SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

            lblStartTime.ContentShortTime(Times.PlageTravMatin.Start);
            lblEndTime.ContentShortTime(Times.EndTheoDateTime);
            lblEndTime.Content += Times.IsTherePauseAprem() || Times.IsTherePauseMatin() ? "*" : "";

            lblHmidiS.Visibility = Visibility.Hidden;
            lblHmidiE.Visibility = Visibility.Hidden;
            lblTpsTravMatin.Visibility = Visibility.Hidden;
            lblTpsTravAprem.Visibility = Visibility.Hidden;
            lblPauseTime.Visibility = Visibility.Hidden;

            lblStartTime.Foreground = Cst.SCBBlack;

            ctrlCompteur.SetVisibility(CompteurControl.CompteurVisibility.OnlyMain);

            /*
            lblTpsTravReel.ContentShortTime(TimeSpan.Zero);
            lblTpsTravReel.Visibility = Visibility.Visible;
            */

            ctrlTyJournee.IsEnabledChange = true;

            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", PrgOptions.PlageFixeMatinFin.ToString(Cst.TimeSpanFormatWithH));

            _notifyIconMatineeLblItem.Visible = true;
            _notifyIconMatineeLblItem.Text = String.Format(Cst.MatineeStartStr, Times.PlageTravMatin.Start.ToShortTimeString());
            _nIconBadgerManItem.Text = "Badger la fin de la matinée";
            _nIconBadgerManItem.ToolTipText = btnBadger.ToolTip.ToString();

            pbarTime.Foreground = Cst.SCBGreenPbar;
            if (finPauseMidiTimer != null)
            {
                finPauseMidiTimer.Stop();
                PrgSwitch.PbarMainTimerActif = true;
                pbarTime.ToolTip = null;
                pbarTime.Foreground = Cst.SCBGreenPbar;
            }

        }

        private static void SetTimesEndTheoAndMaxForOneDay(TimesBadgerDto times, DateTime start, AppOptions prgOptions, EnumTypesJournees typeJournee, TimeSpan? endDelta = null)
        {
            DateTime timeEndTheoRaw = TimesUtils.GetDateTimeEndTravTheoriqueBis(start, prgOptions, typeJournee);
            timeEndTheoRaw += endDelta.GetValueOrDefault();
            if (times.IsTherePauseMatin() || times.IsTherePauseAprem())
            {
                timeEndTheoRaw += times.GetTpsPause();
            }
            times.EndRawDateTime = timeEndTheoRaw;
            times.MaxTimeForOneDay = TimesUtils.GetMaxDateTimeForOneDay(timeEndTheoRaw, prgOptions, typeJournee);
            times.EndTheoDateTime = TimesUtils.ClassicTransform(timeEndTheoRaw, prgOptions, typeJournee);
        }

        private void AdaptUiForState1()
        {

            CoreAudioFactory.CloseProcess();

            // Etat de l'UI après avoir badgé la fin de la matinée.
            PrgSwitch.PbarMainTimerActif = true;
            pbarTime.IsIndeterminate = false;
            PrgSwitch.IsTimerStoppedByMaxTime = false;

            SetBtnBadgerEnabled(true);

            lblTpsTravMatin.Visibility = Visibility.Visible;
            lblTpsTravMatin.ContentShortTime(PrgOptions.IsStopCptAtMaxDemieJournee ?
                Times.GetTpsTravMatinOrMax(PrgOptions.TempsMaxDemieJournee)
                : Times.GetTpsTravMatin()
            );
            if (Times.IsTherePauseMatin())
            {
                lblTpsTravMatin.Content += "*";
                lblTpsTravMatin.ToolTip += Environment.NewLine + "Prend en compte les pauses effectuées le matin";
            }


            gridBtnBadger.Margin = Cst.BtnBadgerPositionAtCenter;
            btnBadger.ToolTip = "Cliquer pour badger le début de l'après-midi";
            btnBadger.Visibility = Visibility.Visible;

            lblHmidiS.ContentShortTime(Times.PlageTravMatin.EndOrDft);
            lblHmidiS.Visibility = Visibility.Visible;
            lblHmidiE.Visibility = Visibility.Hidden;



            TimeSpan tsFinPause = Times.PlageTravMatin.EndOrDft.TimeOfDay + PrgOptions.TempsMinPause;
            PushNewInfo("Prochain badgeage à partir de " + tsFinPause.ToString(Cst.TimeSpanFormatWithH));
            ctrlCompteur.SetFontColor(Cst.SCBGrey);
            /*lblTpsTravReel.Foreground = Cst.SCBGrey;*/
            ctrlCompteur.SetToolTip("C'est la pause du midi ! Le temps de travail réel n'est pas compté.");

            if (EtatBadger == 1)
            {
                PrgSwitch.PbarMainTimerActif = false;
                pbarTime.ToolTip = "Décompte jusqu'à " + tsFinPause.ToString(Cst.TimeSpanFormatWithH);

                if (finPauseMidiTimer != null)
                {
                    finPauseMidiTimer.Stop();
                    PrgSwitch.PbarMainTimerActif = true;
                    pbarTime.ToolTip = null;
                    pbarTime.Foreground = Cst.SCBGreenPbar;
                    btnBadgerM.IsEnabled = true;
                }

                finPauseMidiTimer = new DispatcherTimer();
                finPauseMidiTimer.Interval = new TimeSpan(0, 0, 5);
                finPauseMidiTimer.Tick += (sender, args) =>
                {
                    TimeSpan remainingTimer = tsFinPause - RealTimes.RealTimeTsNow;
                    if (remainingTimer < TimeSpan.Zero && EtatBadger == 2)
                    {
                        finPauseMidiTimer.Stop();
                        PrgSwitch.PbarMainTimerActif = true;
                        pbarTime.ToolTip = null;
                        pbarTime.Foreground = Cst.SCBGreenPbar;

                    }
                    else
                    {
                        pbarTime.IsIndeterminate = false;
                        ChangePBarValue(100 * remainingTimer.TotalSeconds / PrgOptions.TempsMinPause.TotalSeconds);
                        pbarTime.Foreground = Cst.SCBGold;
                        PrgSwitch.PbarMainTimerActif = false;
                        btnBadgerM.IsEnabled = false;
                    }
                };
                finPauseMidiTimer.Start();
            }

            if (PrgSwitch.IsBetaUser && PrgOptions.IsAutoBadgeMeridienne && EtatBadger == 1)
            {
                Random rnd = new Random();
                SpecDelayMeridAutoBadgage = rnd.Next(0, PrgOptions.DeltaAutoBadgeageMinute);

                Title += " - " +
                         (Times.PlageTravMatin.EndOrDft.TimeOfDay + PrgOptions.TempsMinPause +
                          new TimeSpan(0, 0, SpecDelayMeridAutoBadgage)).ToString(Cst.TimeSpanFormat);
            }


            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", tsFinPause.ToString(Cst.TimeSpanFormat));

            _notifyIconMatineeLblItem.Text = String.Format(Cst.MatineeEndStr, Times.PlageTravMatin.Start.ToShortTimeString(), Times.PlageTravMatin.EndOrDft.ToShortTimeString());
            _nIconBadgerManItem.Text = "Badger la fin de la pause du midi";
            _nIconBadgerManItem.ToolTipText = btnBadger.ToolTip.ToString();
        }

        private void AdaptUiForState2(TimeSpan? tmpsPause)
        {
            // Etat de l'UI après avoir badgé le début de l'après-midi.
            if (finPauseMidiTimer != null)
            {
                finPauseMidiTimer.Stop();
                PrgSwitch.PbarMainTimerActif = true;
                pbarTime.ToolTip = null;
                pbarTime.Foreground = Cst.SCBGreenPbar;
                btnBadgerM.IsEnabled = true;
            }

            PrgSwitch.PbarMainTimerActif = true;
            PrgSwitch.IsTimerStoppedByMaxTime = false;
            NotifManager.SetNotifShow(Cst.NotifTpsMaxJournee, false);

            SetBtnBadgerEnabled(true);

            TimeSpan timeEndMore = TimeSpan.Zero;
            if (TypeJournee == EnumTypesJournees.Complete)
            {
                var diffPause = tmpsPause.Value - PrgOptions.TempsMinPause;

                if (diffPause.TotalSeconds > 0)
                {
                    timeEndMore = diffPause;
                }

                if (PrgOptions.IsStopCptAtMaxDemieJournee && Times.GetTpsTravMatin().CompareTo(PrgOptions.TempsMaxDemieJournee) > 0)
                {
                    timeEndMore += (Times.GetTpsTravMatin() - PrgOptions.TempsMaxDemieJournee);
                }

                btnBadger.ToolTip = "Cliquer pour badger la fin de l'après-midi";

            }
            else
            {
                btnBadger.ToolTip = "Cliquer pour badger la fin de la demie-journée";
            }

            // On MaJ l'heure finale theorique de fin, en prenant en compte le temps de pause (pour une journée de complete).
            SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee, timeEndMore);

            if (PrgSwitch.IsBetaUser)
            {
                if (PrgOptions.LastBadgeDelay > 0)
                {
                    btnBadger.ToolTip += " avec un delai de " + (PrgOptions.LastBadgeDelay / 60) + " minutes. " + Environment.NewLine +
                        "Maintenez CTRL pour badger immédiatement. Fermer l'application pour annuler le timer";
                }

            }
            if (PrgOptions.IsLastBadgeIsAutoShutdown)
            {
                btnBadger.ToolTip += ". " + Environment.NewLine + "L'ordinateur s'éteindra ensuite.";
            }

            lblEndTime.ContentShortTime(Times.EndTheoDateTime);
            lblEndTime.Content += Times.IsTherePauseAprem() || Times.IsTherePauseMatin() ? "*" : "";

            gridBtnBadger.Margin = Cst.BtnBadgerPositionAtRight;
            btnBadger.Visibility = Visibility.Visible;

            btnBadgerM.IsEnabled = true;

            lblHmidiE.ContentShortTime(Times.PlageTravAprem.Start);
            lblHmidiE.Visibility = Visibility.Visible;

            lblPauseTime.ContentShortTime(tmpsPause.Value);
            lblPauseTime.Visibility = Visibility.Visible;

            /*
             * lblTpsTravReel.Foreground = Cst.SCBBlack;
             * lblTpsTravReel.ToolTip = null;
             */
            ctrlCompteur.SetFontColor(Cst.SCBBlack);
            ctrlCompteur.SetToolTip(null, null);

            ctrlTyJournee.IsEnabledChange = false;

            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", Times.EndTheoDateTime.TimeOfDay.ToString(Cst.TimeSpanFormat));
            _notifyIconApremLblItem.Visible = true;
            _notifyIconApremLblItem.Text = String.Format(Cst.ApremStartStr, Times.PlageTravAprem.Start.ToShortTimeString());
            _nIconBadgerManItem.Text = "Badger la fin de la journée";
            _nIconBadgerManItem.ToolTipText = btnBadger.ToolTip.ToString();

            PushNewInfo(null);

            Title = Assembly.GetExecutingAssembly().GetName().Name;
        }

        private void AdaptUiForState3()
        {
            // Etat de l'UI après avoir badgé la fin de la journée.

            btnBadger.Visibility = Visibility.Hidden;
            pbarTime.IsIndeterminate = false;

            if (TypeJournee == EnumTypesJournees.Complete)
            {
                lblTpsTravMatin.Visibility = Visibility.Visible;

                lblTpsTravMatin.ContentShortTime(PrgOptions.IsStopCptAtMaxDemieJournee ?
                    Times.GetTpsTravMatinOrMax(PrgOptions.TempsMaxDemieJournee)
                    : Times.GetTpsTravMatin()
                    );
                if (Times.IsTherePauseMatin())
                {
                    lblTpsTravMatin.Content += "*";
                    lblTpsTravMatin.ToolTip += Environment.NewLine + "Prend en compte les pauses effectuées le matin";
                }

                lblTpsTravAprem.Visibility = Visibility.Visible;
                lblTpsTravAprem.ContentShortTime(PrgOptions.IsStopCptAtMaxDemieJournee ?
                    Times.GetTpsTravApremOrMax(PrgOptions.TempsMaxDemieJournee)
                    : Times.GetTpsTravAprem()
                    );
                if (Times.IsTherePauseAprem())
                {
                    lblTpsTravAprem.Content += "*";
                    lblTpsTravAprem.ToolTip += Environment.NewLine + "Prend en compte les pauses effectuées durant l'après-midi";
                }
            }
            else
            {
                lblTpsTravMatin.Visibility = Visibility.Collapsed;
                lblTpsTravAprem.Visibility = Visibility.Collapsed;

            }

            lblEndTime.ContentShortTime(Times.PlageTravAprem.End.Value);
            lblEndTime.Content += Times.IsTherePauseAprem() || Times.IsTherePauseMatin() ? "*" : "";
            lblFinStr.Content = "Fin à";

            TimeSpan tempSuppl = RealTimes.RealTimeTempsTravaille - TimesUtils.GetTpsTravTheoriqueOneDay(PrgOptions, TypeJournee);
            PushNewInfo(String.Format("Temps travaillé aujourd'hui : {0} ({2}{1}) ",
                RealTimes.RealTimeTempsTravaille.ToString(Cst.TimeSpanFormat),
                tempSuppl.ToString(Cst.TimeSpanFormat),
                tempSuppl.TotalSeconds < 0 ? "-" : "+"));

            ctrlTyJournee.IsEnabled = false;
            btnBadgerM.IsEnabled = false;

            _nIconBadgerManItem.ToolTipText = null;
            _notifyIconApremLblItem.Visible = true;
            _notifyIconApremLblItem.Text = String.Format(Cst.ApremEndStr, Times.PlageTravAprem.Start.ToShortTimeString(), Times.PlageTravAprem.End.Value.ToShortTimeString());

            _nIconBadgerManItem.Enabled = false;
            _nIconBadgerManItem.Text = "Badger";
            _nIconBadgerManItem.ToolTipText = null;



        }



        private void AdaptUiFromOptions(AppOptions opt)
        {

            _logger.Debug("Chargement de l'interface à partir des options");

            try
            {

                NotifManager.UseAlternateNotification = opt.IsUseAlternateNotification;

                _imgBoolAutoBadgeAtStart = new ImagesBooleanWrapper(
                    PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoStartOn.png"),
                    PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoStartOff.png"),
                    imgBtnStartAutoBadge
                    );
                _imgBoolAutoBadgeAtStart.SetTooltipTextValues(
                    "Badgeage automatique au démarrage activé." + Environment.NewLine +
                    "Cliquez pour le désactiver.",
                    "Badgeage automatique au démarrage désactivé." + Environment.NewLine +
                    "Cliquez pour l'activer.");
                _imgBoolAutoBadgeAtStart.ChangeValue(PrgOptions.IsAutoBadgeAtStart);
                PrgOptions.OnAutoBadgeAtStartChange += delegate (bool b)
                {
                    _imgBoolAutoBadgeAtStart.ChangeValue(PrgOptions.IsAutoBadgeAtStart);
                    if (IsFullyLoaded)
                        OptionManager.SaveOptions(PrgOptions);
                };

                _imgBoolShowGlobalNotification = new ImagesBooleanWrapper(
                    PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "notifOn.png"),
                    PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "notifOff.png"),
                    imgBtnShowNotif
                    );
                _imgBoolShowGlobalNotification.SetTooltipTextValues(
                    "Notifications activées." + Environment.NewLine +
                    "Cliquez pour les désactiver.",
                    "Notifications désactivées." + Environment.NewLine +
                    "Cliquez pour les activer.");
                _imgBoolShowGlobalNotification.ChangeValue(PrgOptions.IsGlobalShowNotifications);
                PrgOptions.OnGlobalShowNotificationsChange += delegate (bool b)
                {
                    _imgBoolShowGlobalNotification.ChangeValue(PrgOptions.IsGlobalShowNotifications);
                    if (IsFullyLoaded)
                        OptionManager.SaveOptions(PrgOptions);
                };


                _imgBoolPauseReport = new ImagesBooleanWrapper(
                PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "pauseExists.png"),
                PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "pauseBlinkOn.png"),
                imgBtnPauseReport
                );
                _imgBoolPauseReport.SetTooltipTextValues(
                    "Le temps de travail prend en compte les pauses effectuées." + Environment.NewLine +
                    "Cliquez pour voir les détails.",
                    "Une pause est active (le nombre de badgeages, hors ceux normaux, est impair)." + Environment.NewLine +
                    "Cliquez sur le bouton \"Badger et reprendre\" pour badger et reprendre le compteur de temps travaillé.");
                _imgBoolPauseReport.ChangeValue(PrgSwitch.PauseCurrentState.BoolState);
                imgBtnPauseReport.Visibility = PrgSwitch.PauseCurrentState == EnumStatePause.NONE
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                PrgSwitch.OnPauseCurrentStateChange += delegate (EnumStatePause b)
                {
                    _imgBoolPauseReport.ChangeValue(b.BoolState);
                    imgBtnPauseReport.Visibility = b == EnumStatePause.NONE
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                };


                // Surveillance de l'extinction de l'ordinateur (si option true)
                Application.Current.SessionEnding -= CurrentOnSessionEnding;
                if (opt.TemptBlockShutdown)
                {
                    Application.Current.SessionEnding += CurrentOnSessionEnding;
                }
                else
                {
                    Application.Current.SessionEnding -= CurrentOnSessionEnding;
                }

                // 
                SystemEvents.SessionSwitch -= MsgBoxAfterSessionUnlock;
                if (opt.ShowNotifWhenSessUnlockAfterMidi)
                {
                    SystemEvents.SessionSwitch += MsgBoxAfterSessionUnlock;
                }
                else
                {
                    SystemEvents.SessionSwitch -= MsgBoxAfterSessionUnlock;
                }

                // 
                SystemEvents.SessionSwitch -= OnAfterSessionLock;
                if (opt.IsPlaySoundAtLockMidi)
                {
                    SystemEvents.SessionSwitch += OnAfterSessionLock;
                }
                else
                {
                    SystemEvents.SessionSwitch -= OnAfterSessionLock;
                }


                if (opt.ActionButtonClose == EnumActionButtonClose.CLOSE)
                {
                    btnClose.Content = "Fermer";
                    btnClose.ToolTip = "Enregistre la session courante et ferme le programme";
                }
                else if (opt.ActionButtonClose == EnumActionButtonClose.MINIMIZE)
                {
                    btnClose.Content = "Minimiser";
                    btnClose.ToolTip = "Minimise le programme" + Environment.NewLine + "Maintenir CTRL pour quitter le programme";
                }


                NotifManager.SetNotifShow(Cst.NotifCust1Name, false);
                NotifManager.SetNotifShow(Cst.NotifCust2Name, false);
                NotifManager.SetNotifShow(Cst.NotifTpsMaxJournee, false);

                btnBadgerM.ToolTip = opt.IsBtnManuelBadgeIsWithHotKeys ? "Maintenir CTRL pour badger" : "Badger";

                if (EtatBadger <= 1)
                {
                    lblPauseTime.Content = PrgOptions.TempsMinPause.ToString(Cst.TimeSpanFormat);
                }

                if (PrgSwitch.IsBetaUser)
                {
                    imgBtnAutoBadgeMidi.Visibility = Visibility.Collapsed;

                    _imgBoolAutoBadgeMeridienne = new ImagesBooleanWrapper(
                        PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoBadgeMidiOn.png"),
                        PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoBadgeMidiOff.png"),
                        imgBtnAutoBadgeMidi
                        );
                    _imgBoolAutoBadgeMeridienne.SetTooltipTextValues(
                        "Badgeage automatique de le pause du midi activé." + Environment.NewLine +
                        "Cliquez pour le désactiver.",
                        "Badgeage automatique de le pause du midi désactivé." + Environment.NewLine +
                        "Cliquez pour l'activer.");
                    _imgBoolAutoBadgeMeridienne.ChangeValue(PrgOptions.IsAutoBadgeMeridienne);
                    PrgOptions.OnAutoBadgeMeridienneChange += delegate (bool b)
                    {
                        _imgBoolAutoBadgeMeridienne.ChangeValue(PrgOptions.IsAutoBadgeMeridienne);
                        if (IsFullyLoaded)
                            OptionManager.SaveOptions(PrgOptions);
                    };

                    KeyDown += (sender, args) =>
                    {
                        if (args.Key == Key.LeftCtrl)
                        {
                            imgBtnAutoBadgeMidi.Visibility = Visibility.Visible;

                        }
                    };
                    KeyUp += (sender, args) =>
                    {
                        if (args.Key == Key.LeftCtrl)
                        {
                            imgBtnAutoBadgeMidi.Visibility = Visibility.Collapsed;

                        }
                    };

                }

                if (_nIconShowNotificationItem != null)
                {
                    _nIconShowNotificationItem.Checked = PrgOptions.IsGlobalShowNotifications;
                }


            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors du chargement de l'interface à partir des options");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
            }

        }

        private void RegisterNotifications()
        {

            // Notification : plage fixe du matin terminée
            String notifName = Cst.NotifEndPfMatinName;
            if (PrgOptions.ShowNotifEndPfMatin)
            {
                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":00",
                    "Il est l'heure de badger !",
                    "La plage fixe du matin est terminée. Vous pouvez badger.",
                    EnumTypesJournees.Complete,
                    0,
                    PrgOptions.PlageFixeMatinFin
                    );


                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":12",
                    "Il est l'heure de badger !",
                    "La plage fixe du matin est terminée. Vous pouvez badger.",
                    EnumTypesJournees.Matin,
                    2,
                    PrgOptions.PlageFixeMatinFin
                    );
            }
            else
            {
                NotifManager.RemoveNotificationsSeries(notifName);
            }

            // Notification : heure habituelle de la fin plage fixe du matin
            notifName = Cst.NotifEndMoyMatin;
            if (PrgOptions.ShowNotifEndMoyMatin)
            {
                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":02",
                    "Heure habituelle de badgeage",
                    "D'habitude vous badgez la fin de la plage fixe du matin à cette heure-ci.",
                    EnumTypesJournees.Complete,
                    0,
                    Times.EndMoyPfMatin
                    );


                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":12",
                    "Heure habituelle de badgeage",
                    "D'habitude vous badgez la fin de la plage fixe du matin à cette heure-ci.",
                    EnumTypesJournees.Matin,
                    0, Times.EndMoyPfMatin
                    );

            }
            else
            {
                NotifManager.RemoveNotificationsSeries(notifName);
            }

            // Notification : plage fixe de l'après-midi terminée
            notifName = Cst.NotifEndPfApremName;
            if (PrgOptions.ShowNotifEndPfAprem)
            {
                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":02",
                    "Il est l'heure de badger !",
                    "La plage fixe de l'après-midi est terminée. Vous pouvez badger.",
                    EnumTypesJournees.Complete,
                    2,
                    PrgOptions.PlageFixeApremFin
                    );

                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":22",
                    "Il est l'heure de badger !",
                    "La plage fixe de l'après-midi est terminée. Vous pouvez badger.",
                    EnumTypesJournees.ApresMidi,
                    2,
                    PrgOptions.PlageFixeApremFin
                    );
            }
            else
            {
                NotifManager.RemoveNotificationsSeries(notifName);
            }

            // Notification : heure habituelle de la fin plage fixe de l'après-midi 
            notifName = Cst.NotifEndMoyAprem;
            if (PrgOptions.ShowNotifEndMoyAprem)
            {
                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":02",
                    "Heure habituelle de badgeage",
                    "D'habitude vous badgez la fin de la plage fixe de l'après-midi à cette heure-ci.",
                    EnumTypesJournees.Complete,
                    2,
                    Times.EndMoyPfAprem
                    );

                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName + ":22",
                    "Heure habituelle de badgeage",
                    "D'habitude vous badgez la fin de la plage fixe de l'après-midi à cette heure-ci.",
                    EnumTypesJournees.ApresMidi,
                    2,
                    Times.EndMoyPfAprem
                    );
            }
            else
            {
                NotifManager.RemoveNotificationsSeries(notifName);
            }

            // Notification : pause méridienne minimum terminée
            notifName = Cst.NotifEndPauseName;
            if (PrgOptions.ShowNotifEndPause)
            {
                NotifManager.RegisterNotification(
                    notifName,
                     "Pause méridienne minimum terminée",
                    "Le temps minimum de la pause méridienne est écoulé. Vous pouvez badger.",
                    null,
                    1,
                    () => Times.PlageTravMatin.EndOrDft.TimeOfDay + PrgOptions.TempsMinPause,
                    EnumTypesTemps.RealTime
                );

            }
            else
            {
                NotifManager.RemoveNotification(notifName);
            }


            notifName = Cst.NotifEndTheoName;
            if (PrgOptions.ShowNotifEndTheo)
            {
                NotifManager.RegisterNotification(
                    notifName + ":02",
                    "Temps de travail pour la journée écoulé",
                    "Le temps minimum de travail pour une journée est écoulé. Vous pouvez badger.",
                    EnumTypesJournees.Complete,
                    2,
                    () => Times.EndTheoDateTime.TimeOfDay,
                    EnumTypesTemps.RealTime
                    );


                NotifManager.RegisterNotification(
                    notifName + ":12",
                    "Temps de travail pour la matinée écoulé",
                    "Le temps minimum de travail pour la matinée est écoulé. Vous pouvez badger.",
                    EnumTypesJournees.Matin,
                    2,
                    () => Times.EndTheoDateTime.TimeOfDay,
                    EnumTypesTemps.RealTime
                    );

                NotifManager.RegisterNotification(
                    notifName + ":22",
                    "Temps de travail pour l'après-midi écoulé",
                    "Le temps minimum de travail pour l'après-midi est écoulé. Vous pouvez badger.",
                    EnumTypesJournees.ApresMidi,
                    2,
                    () => Times.EndTheoDateTime.TimeOfDay,
                    EnumTypesTemps.RealTime
                    );
            }
            else
            {
                NotifManager.RemoveNotificationsSeries(notifName);
            }



            notifName = Cst.NotifCust1Name;
            if (PrgOptions.Notif1Obj != null && PrgOptions.Notif1Obj.IsActive)
            {
                NotifManager.RegisterNotification(
                   notifName,
                   null,
                   PrgOptions.Notif1Obj.Message,
                   null,
                   null,
                   () => PrgOptions.Notif1Obj.GetRealTimeSpan(PrgOptions, Times),
                   EnumTypesTemps.RealTime
               );
            }
            else
            {
                NotifManager.RemoveNotification(notifName);
            }

            notifName = Cst.NotifCust2Name;
            if (PrgOptions.Notif2Obj != null && PrgOptions.Notif2Obj.IsActive)
            {
                NotifManager.RegisterNotification(
                   notifName,
                   null,
                   PrgOptions.Notif2Obj.Message,
                   null,
                   null,
                   () => PrgOptions.Notif2Obj.GetRealTimeSpan(PrgOptions, Times),
                   EnumTypesTemps.RealTime
               );
            }
            else
            {
                NotifManager.RemoveNotification(notifName);
            }


            notifName = Cst.NotifTpsMaxJournee;
            Action<NotificationDto> afterShowAction = delegate (NotificationDto dto)
            {
                ctrlCompteur.SetFontColor(Cst.SCBDarkRed);
                ctrlCompteur.SetToolTip(dto.Message);
                /*
                lblTpsTravReel.Foreground = Cst.SCBDarkRed;
                lblTpsTravReel.ToolTip = dto.Message;
                 */
            };

            NotificationDto n = null;
            n = NotifManager.RegisterNotification(
                notifName,
                "Information",
                "Le temps maximum de travail pour une journée est dépassé. Le temps supplémentaire pourrait ne pas être pris en compte.",
                null,
                null,
                PrgOptions.TempsMaxJournee,
                EnumTypesTemps.TpsTrav
                );
            n.DtoAfterShowNotif += afterShowAction;

            notifName = Cst.NotifTpsMaxDemieJournee;
            n = NotifManager.RegisterNotification(
                notifName + ":0",
                "Information",
                "Le temps maximum de travail pour une demie-journée est dépassé. Le temps supplémentaire pourrait ne pas être pris en compte.",
                null,
                0,
                PrgOptions.TempsMaxDemieJournee,
                EnumTypesTemps.TpsTravMatin
                );
            n.DtoAfterShowNotif += afterShowAction;

            n = NotifManager.RegisterNotification(
                notifName + ":2",
                "Information",
                "Le temps maximum de travail pour une demie-journée est dépassé. Le temps supplémentaire pourrait ne pas être en compte.",
                null,
                2,
                PrgOptions.TempsMaxDemieJournee,
                EnumTypesTemps.TpsTravAprem
                );
            n.DtoAfterShowNotif += afterShowAction;


            ///
            notifName = "Temps maximum travail pour la matinée";
            n = NotifManager.RegisterNotification(
                notifName + ":0",
                "Information",
                "Le temps maximum de travail pour une demie-journée est dépassé. Le temps supplémentaire pourrait ne pas être pris en compte.",
                null,
                0,
                PrgOptions.PlageFixeApremStart - PrgOptions.TempsMinPause,
                EnumTypesTemps.RealTime
                );
            n.DtoAfterShowNotif += afterShowAction;


            notifName = "Heure max travail pour la journée";
            n = NotifManager.RegisterNotification(
                notifName + ":0",
                "Information",
                String.Format("Il est {0} : le temps de travail n'est plus compté.", PrgOptions.HeureMaxJournee.ToStrSignedhhmm()),
                null,
                null,
                PrgOptions.HeureMaxJournee,
                EnumTypesTemps.RealTime
            );
            n.DtoAfterShowNotif += afterShowAction;

        }





        private void ChangeTypeJournee()
        {
            bool isMatin = ctrlTyJournee.TypeJournee == EnumTypesJournees.Matin;
            bool isAprem = ctrlTyJournee.TypeJournee == EnumTypesJournees.ApresMidi;

            _logger.Info("Changement type de journée :");
            if (EnumTypesJournees.IsDemiJournee(ctrlTyJournee.TypeJournee))
            {

                lblPauseTime.Visibility = Visibility.Hidden;
                gridHoraireMerid.Visibility = Visibility.Hidden;
                rectAprem.Visibility = Visibility.Hidden;

                rectMatin.Width = Width;
                OldEtatBadger = EtatBadger;

                if (EtatBadger == 0)
                {
                    EtatBadger = 2;
                }
                else if (EtatBadger == 1)
                {
                    EtatBadger = 3;
                    Times.PlageTravAprem.End = Times.PlageTravMatin.End;
                }

                if (!isMatin)
                {
                    _logger.Info("   Travail que l'après-midi");
                    TypeJournee = EnumTypesJournees.ApresMidi;
                    //chkAmidi.IsEnabled = false;
                }
                else
                {
                    _logger.Info("   Travail que le matin");
                    TypeJournee = EnumTypesJournees.Matin;
                    //chkMatin.IsEnabled = false;

                }

                SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

                if (EtatBadger < 3)
                {
                    AdaptUiFromState(0, null, false);
                }
                else
                {

                    AdaptUiFromState(3, null, false);
                }
            }
            else
            {
                _logger.Info("   Toute la journée");

                gridHoraireMerid.Visibility = Visibility.Visible;
                rectAprem.Visibility = Visibility.Visible;

                rectMatin.Width = rectAprem.Width;
                TypeJournee = EnumTypesJournees.Complete;


                if (OldEtatBadger != 3)
                {
                    //chkMatin.IsEnabled = true;
                    //chkAmidi.IsEnabled = true;
                    ctrlTyJournee.IsEnabledChange = true;
                }
                else
                {
                    //chkMatin.IsEnabled = false;
                    //chkAmidi.IsEnabled = false;
                    ctrlTyJournee.IsEnabledChange = false;
                }



                EtatBadger = OldEtatBadger;



                SetTimesEndTheoAndMaxForOneDay(Times, Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

                AdaptUiLowerThanState();
            }
        }


        public void RestoreWindow()
        {
            WindowState = WindowState.Normal;
            Topmost = true;
            UpdateLayout();
            Topmost = false;
        }




        private void AdaptUiLowerThanState()
        {
            _logger.Debug("AdaptUiLowerThanState (EtatBadger=" + EtatBadger + ")");

            for (int i = -1; i <= EtatBadger; i++)
            {
                if (i == 2)
                {
                    AdaptUiFromState(i, Times.PlageTravAprem.Start - Times.PlageTravMatin.EndOrDft);
                }
                else
                {
                    AdaptUiFromState(i, null);
                }
            }
        }


        public void ChangePBarValue(double d)
        {
            pbarTime.Value = d;
        }

        public void ChangeBtnBadgerValue(double d)
        {
            pbarBtnBadger.Value = d;
        }

        public void PushNewInfo(String str)
        {
            if (StringUtils.IsNullOrWhiteSpace(str))
            {
                lblInfoLbl.Visibility = Visibility.Hidden;
                lblInfos.Visibility = Visibility.Hidden;
            }
            else
            {
                lblInfoLbl.Visibility = Visibility.Visible;
                lblInfos.Visibility = Visibility.Visible;
                lblInfos.Text = str;
            }


        }

        private void lblTpsTravReel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_mainTimerManager.IsPaused)
            {
                PrgSwitch.IsTimeRemainingNotTimeWork = !PrgSwitch.IsTimeRemainingNotTimeWork;
                //UpdateInfos();
                ctrlCompteur.UpdateInfos();
            }

        }

        private void btnModTimes_Click(object sender, RoutedEventArgs e)
        {
            LoadDetailView();
        }

        private void LoadDetailView()
        {
            bool isReloadView = true;

            DateTime dt = AppDateUtils.DtNow();

            while (isReloadView)
            {
                var mDetailsView = new MoreDetailsView(PrgOptions, dt);
                mDetailsView.ShowDialog();

                isReloadView = mDetailsView.IsMustLoadsModTimeView;

                if (isReloadView)
                {
                    LoadModTimeView(mDetailsView.CurrentShowDay);
                    dt = mDetailsView.CurrentShowDay;
                }
            }
        }

        private void LoadModTimeView(DateTime dayToMod)
        {
            bool isCurrentDay = dayToMod.Date == AppDateUtils.DtNow().Date;

            ModTimeView m = new ModTimeView(dayToMod, PrgOptions.UrlMesPointages, PrgOptions);


            m.SetCurrentDay(dayToMod.Date, Times, TypeJournee, EtatBadger, isCurrentDay);

            m.ShowDialog();


            if (m.HasChanged)
            {
                if (isCurrentDay)
                {
                    TypeJournee = m.TypeJournee;
                    EtatBadger = m.EtatBadger;
                    OldEtatBadger = m.EtatBadger;
                    PrgOptions.LastCdSeen = m.LastCdSeen;

                    AdaptUiLowerThanState();

                    ctrlTyJournee.TypeJournee = TypeJournee;

                    ChangeTypeJournee();

                    ClockUpdTimerOnOnTick(null, null);

                    PointageSaverObj.SaveCurrentDayTimes();

                    PrgSwitch.IsMoreThanTpsTheo = false;
                    PrgSwitch.IsTimerStoppedByMaxTime = false;
                    PrgSwitch.PbarMainTimerActif = true;

                    StopTimers();
                }
                else
                {
                    PointageSaverObj.SaveAnotherDayTime(dayToMod, m.Times, m.TypeJournee, m.EtatBadger, m.LastCdSeen);
                }
            }
        }

        private void pbarTime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClockUpdTimerOnOnTick(null, null);
        }



        private void PlayAdvertise()
        {
            if (!PrgSwitch.IsSoundOver)
            {
                _logger.Debug("Son en cours...");
                return;
            }
            try
            {
                PrgSwitch.IsSoundOver = false;
                Action onSucessAction = () =>
                {
                    PrgSwitch.IsSoundOver = true;
                };
                Action<Exception> onFailAction = (ex) =>
                {
                    PrgSwitch.IsSoundOver = true;
                    ExceptionMsgBoxView.ShowException(ex, null, "Une erreur s'est produite lors de la lecture du son.");
                };

                CoreAudioFactory.AsyncPlaySound(PrgOptions.SoundPlayedAtLockMidi,
                    PrgOptions.SoundDeviceFullName,
                    PrgOptions.SoundPlayedAtLockMidiVolume,
                    onSucessAction, onFailAction);



            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur s'est produite lors de la lecture du son." + Environment.NewLine + Environment.NewLine
               + "Consulter le ficher log pour plus d'information sur l'erreur : "
               + ex.Message
               , "Erreur");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
            }

        }

        private void imgBtnStartAutoBadge_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PrgOptions.IsAutoBadgeAtStart = !PrgOptions.IsAutoBadgeAtStart;
        }

        private void imgBtnAutoBadgeMidi_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PrgSwitch.IsBetaUser)
                PrgOptions.IsAutoBadgeMeridienne = !PrgOptions.IsAutoBadgeMeridienne;
        }


        private void imgBtnShowNotif_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PrgOptions.IsGlobalShowNotifications = !PrgOptions.IsGlobalShowNotifications;
        }


        private void imgBtnUpdate_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SignalUpdate(true);
        }

        private void imgBtnWarnLicence_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AfficheLicenceInfo(true);
        }

        private void imgBtnPauseReport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LoadDetailView();
        }

        private void imgBtnFirefoxLoaded_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FfDriverSingleton.Instance.IsLoaded())
            {
                MessageBoxResult result = MessageBox.Show("Voulez-vous terminez l'instance Firefox ?", "Question", MessageBoxButton.YesNo);
                if (result.Equals(MessageBoxResult.Yes))
                {
                    FfDriverSingleton.Instance.Quit();
                    PrgSwitch.IsFfForcedLoaded = false;
                    imgBtnFirefoxLoaded.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (!File.Exists(PrgOptions.FfExePath))
                {
                    NotifManager.NotifyNow("Firefox n'existe pas à cette adrese : " + PrgOptions.FfExePath,
                        "Erreur");

                    return;
                }

                FfDriverSingleton.Instance.Load(PrgOptions);
                PrgSwitch.IsFfForcedLoaded = true;
                imgBtnFirefoxLoaded.Visibility = Visibility.Visible;
            }

        }


        private void lblLastInfoShown_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {



        }

        private void FixLastDay()
        {

        }

        internal void StopTimers()
        {
            if (finHeureReglTimer != null)
            {
                finHeureReglTimer.Stop();
            }

            if (finPauseMidiTimer != null)
            {
                finPauseMidiTimer.Stop();
            }

            if (BadgerWorker != null && BadgerWorker.LastBadgeTimer != null)
            {
                BadgerWorker.LastBadgeTimer.Stop();
            }

            PrgSwitch.IsInBadgeWork = false;
        }

        public void Tick()
        {
            ClockUpdTimerOnOnTick(null, null);
        }


        private void RestartApp()
        {
            FileInfo watchDogRestarter = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Resources/WatchDogRestart.exe"));
            if (!watchDogRestarter.Exists)
            {
                _logger.Error("Impossible de redémarrer l'application : l'exécutable du watchdog ({0}) n'existe pas.",
                    watchDogRestarter.FullName);
                return;
            }
            string args = String.Format("{0}", Assembly.GetEntryAssembly().GetName().Name);
            ProcessStartInfo processStartInfo = new ProcessStartInfo(watchDogRestarter.FullName, args);
            processStartInfo.UseShellExecute = true;
            processStartInfo.WorkingDirectory = Assembly.GetEntryAssembly().Location;

            var processWatchDogRestarter = Process.Start(processStartInfo);
            if (processWatchDogRestarter == null)
            {
                _logger.Warn("L'application n'a pas pu redémarrer");
                return;
            }
            processWatchDogRestarter.WaitForExit(1000);
            if (!processWatchDogRestarter.HasExited)
            {

                _logger.Info("Redémarrage de l'application");
                PrgSwitch.IsRealClose = true;
                Close();
            }
            else
            {
                _logger.Warn("L'application n'a pas pu redémarrer");
            }
        }

        public void SetBtnBadgerEnabled(bool state)
        {
            btnBadger.IsEnabled = state;
        }

        public void ExtShowNotif(string title, string content, int timeoutInMs)
        {
            MiscAppUtils.ShowNotificationBaloon(_notifyIcon, title, content, null, timeoutInMs, null, PrgOptions.IsUseAlternateNotification);
        }

        public Dictionary<String, String> ExtGetLicenceInfo()
        {
            return LicenceApp.ToDictionnary();
        }




        public MethodRecordWithInstance[] GetMethodToPresents()
        {
            List<MethodRecordWithInstance> l = new List<MethodRecordWithInstance>();


            MethodRecordWithInstance m = new MethodRecordWithInstance();
            m.Instance = BadgerWorker;
            m.TargetHookName = "BadgeFullAction";
            m.MethodResponder = "BadgeFullAction";
            m.Dispatcher = Dispatcher.CurrentDispatcher;
            l.Add(m);

            m = new MethodRecordWithInstance();
            m.Instance = this;
            m.TargetHookName = "ExtSetBtnBadger";
            m.MethodResponder = "SetBtnBadgerEnabled";
            m.Dispatcher = Dispatcher.CurrentDispatcher;
            l.Add(m);

            m = new MethodRecordWithInstance();
            m.StaticType = typeof(OptionManager);
            m.TargetHookName = "ExtSaveOptions";
            //  OptionManager.SaveOptions(PrgOptions);
            m.MethodResponder = "SaveOptions";
            // m.Dispatcher = Dispatcher.CurrentDispatcher;
            l.Add(m);

            m = new MethodRecordWithInstance();
            m.Instance = this;
            // m.StaticType = typeof(XXXX);
            m.TargetHookName = "ExtShowNotificationBaloon";
            //  OptionManager.SaveOptions(PrgOptions);
            m.MethodResponder = "ExtShowNotif";
            m.Dispatcher = Dispatcher.CurrentDispatcher;
            l.Add(m);

            m = new MethodRecordWithInstance();
            m.Instance = this;
            m.TargetHookName = "ExtGetLicenceInfo";
            m.MethodResponder = "ExtGetLicenceInfo";
            l.Add(m);


            return l.ToArray();
        }













    }
}
