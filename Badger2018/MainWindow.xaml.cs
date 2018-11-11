﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevViewLibrary.controls.Simplifier;
using AryxDevViewLibrary.utils;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using Badger2018.business;
using Badger2018.business.saver;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.Properties;
using Badger2018.services;
using Badger2018.utils;
using Badger2018.views;
using BadgerCommonLibrary.business;
using BadgerCommonLibrary.dto;
using BadgerCommonLibrary.utils;
using BadgerPluginExtender;
using BadgerPluginExtender.dto;
using BadgerPluginExtender.interfaces;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace Badger2018
{



    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPresentableObject
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private MainTimerManager _mainTimerManager = new MainTimerManager();


        DispatcherTimer finPauseMidiTimer = null;
        DispatcherTimer pauseHorsPeriodeTimer = null;

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


        public TimeSpan RealTimeTempsTravaille { get; private set; }
        public TimeSpan RealTimeTempsTravailleMatin { get; private set; }
        public DateTime RealTimeDtNow { get; private set; }
        public TimeSpan RealTimeTsNow { get; private set; }
        public bool IsFullyLoaded { get; private set; }

        public MainWindow(AppOptions prgOptions, UpdaterManager updaterManager, PluginManager pluginManager)
        {
            _logger.Debug("Chargement de l'écran principal");

            PrgSwitch = new AppSwitchs();

            InitializeComponent();
            Times = new TimesBadgerDto();
            Times.TimeRef = AppDateUtils.DtNow();
            Times.PausesHorsDelai = new List<IntervalTemps>();
            //imgOptions.Source = MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "iconSetting.png");
            lblVersion.Content = String.Format(lblVersion.Content.ToString(), Assembly.GetExecutingAssembly().GetName().Version, Properties.Resources.versionName);
            pbarTime.IsIndeterminate = true;

            PrgOptions = prgOptions;
            UpdaterMgr = updaterManager;
            PluginMgr = pluginManager;
            Times.EndMoyPfMatin = ServicesMgr.Instance.BadgeagesServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_MATIN_END, 30) ?? PrgOptions.PlageFixeMatinFin;
            Times.EndMoyPfAprem = ServicesMgr.Instance.BadgeagesServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_APREM_END, 30) ?? PrgOptions.PlageFixeApremFin;


            //PointageSaverObj = new XmlPointageWriterReader(this);
            PointageSaverObj = new BddPointageWriterReader(this);
            PrgSwitch.UseBddSupport = true;


            BadgerWorker = new BadgerWorker(this);



            NotifManager = new NoticationsManager(_notifyIcon);
            NotifManager.AfterShowNotif += delegate(NotificationDto n)
            {
                PushNewInfo(n.Message);
            };
            OnEtatBadgerChange += delegate(int a)
            {
                NotifManager.EtatBadger = a;

            };
            OnTypeJourneeChange += delegate(EnumTypesJournees a)
            {
                NotifManager.TypeJournee = a;
            };

            RealTimeDtNow = AppDateUtils.DtNow();
            RealTimeTsNow = RealTimeDtNow.TimeOfDay;
            RealTimeTempsTravaille = TimeSpan.Zero;



            AssignBetaRole();
            PostInitializeComponent();




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


            MiscAppUtils.CreatePaths();
            ArchiveDatas();

            // tOdo bug reload notifs
            AdaptUiFromOptions(PrgOptions);

            Title = Assembly.GetExecutingAssembly().GetName().Name;
            PrgSwitch.PbarMainTimerActif = true;
            InitNotifyIcon();
            Closing += OnClosing;

            KeyUp += (sender, args) =>
            {
                if (args.Key == Key.F1)
                {
                    AboutView aboutView = new AboutView();
                    aboutView.ShowDialog();
                }
                else if (args.Key == Key.F2)
                {
                    btnOptions_Click(null, null);
                }
                else if (args.Key == Key.F3)
                {
                    SignalUpdate(true);
                }
                else if (args.Key == Key.F4)
                {
                    LoadsDidYouKnow();
                }
                else if (args.Key == Key.F5)
                {
                    btnModTimes_Click(null, null);
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
                    DebugCommandView d = new DebugCommandView();
                    d.Show();
                }


            };

            //   btnModTimes.Visibility = prgOptions.IsFirstRun ? Visibility.Visible : Visibility.Hidden;

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
                Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

                lblStartTime.Foreground = Cst.SCBGrey;

                lblTpsTravReel.Visibility = Visibility.Hidden;

                lblStartTime.ContentShortTime(Times.PlageTravMatin.Start);
                lblEndTime.ContentShortTime(Times.EndTheoDateTime);

                lblHmidiS.Visibility = Visibility.Hidden;
                lblHmidiE.Visibility = Visibility.Hidden;

                ctrlTyJournee.ChangeTypeJourneeWithoutAction(EnumTypesJournees.Complete);
                ctrlTyJournee.IsEnabledChange = false;

                TimeSpan tsfinMat = TimesUtils.GetTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions,
                    EnumTypesJournees.Matin);

                if (PrgOptions.IsAutoBadgeAtStart)
                {
                    _logger.Info("Badgeage automatique");
                    btnBadger.IsEnabled = false;
                    BadgerWorker.BadgeFullAction();
                    btnBadger.IsEnabled = true;

                    PushNewInfo("Badgeage automatique réalisé. Fin de la matinée à " +
                                tsfinMat.ToString(Cst.TimeSpanFormat));
                }
                else
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
            _mainTimerManager.Start();



            IsFullyLoaded = true;
            ClockUpdTimerOnOnTick(null, null);
            PrgSwitch.CanCheckUpdate = true;


            Loaded += (sender, args) =>
            {
                AfterLoadPostInitComponent();
                CoreAppBridge.Instance.RegisterMethodsForPlugin(this);

                ReloadPauseState();
            };

        }




        private void AssignBetaRole()
        {
            PrgSwitch.IsBetaUser = (bool)PluginMgr.PlayHookAndReturn("IsBetaUser", null, typeof(bool)).ReturnFirstOrDefaultResultObject();

            if (!PrgSwitch.IsBetaUser)
            {

                PrgOptions.ResetSpecOption();
            }
        }

        private void ArchiveDatas()
        {
            _logger.Info("ArchiveDatas");

            ZipArchiveManager z = new ZipArchiveManager();
            z.Directory = new DirectoryInfo(Cst.PointagesDirName);
            z.Compress();
            z.Directory = new DirectoryInfo(Cst.ScreenshotDirName);
            z.Compress();

        }

        private void PostInitializeComponent()
        {

#if !DEBUG
            rectDebug.Visibility = Visibility.Hidden;
            //wrapPanelTop.Visibility = Visibility.Hidden;
#endif

            imgBtnUpdate.Visibility = Visibility.Collapsed;
            imgBtnAutoBadgeMidi.Visibility = Visibility.Collapsed;

            lblInfos.Visibility = Visibility.Collapsed;
            lblInfoLbl.Visibility = Visibility.Collapsed;

            lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;

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

            System.Windows.Controls.ContextMenu btnMContextMenu = new System.Windows.Controls.ContextMenu();
            btnMContextMenu.Items.Add(badgerTitleItem);
            btnMContextMenu.Items.Add(badgerOnlyItem);
            btnMContextMenu.Items.Add(badgerPauseItem);
            btnMContextMenu.Items.Add(new Separator());
            btnMContextMenu.Items.Add(badgerShowPausesItem);



            btnBadgerM.ContextMenu = btnMContextMenu;

        }
        private void AfterLoadPostInitComponent()
        {

            ctrlTyJournee.OnTypeJourneeChange += (e) =>
            {
                ChangeTypeJournee();
            };
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


        }

        private void MainTimerManagerOnOnPauseToggled(MainTimerManager mainTimerManager)
        {
            // PrgSwitch.PbarMainTimerActif = mainTimerManager.IsPaused;
            if (!mainTimerManager.IsPaused)
            {
                lblTpsTravReel.Foreground = Cst.SCBBlack;

                pauseHorsPeriodeTimer.Start();
                pauseHorsPeriodeTimer.Stop();


                PrgSwitch.PbarMainTimerActif = true;
                lblEndTime.Foreground = Cst.SCBBlack;
                lblEndTime.ToolTip = null;

                if (EtatBadger < 2)
                {

                    Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start,
                        PrgOptions, EnumTypesJournees.Complete)
                                            + Times.GetTpsPause();
                }
                else
                {
                    Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start,
                        PrgOptions, EnumTypesJournees.Complete)
                                            + Times.GetTpsPause();
                    TimeSpan tmpsPause = Times.PlageTravAprem.Start - Times.PlageTravMatin.EndOrDft;
                    var diffPause = tmpsPause - PrgOptions.TempsMinPause;

                    if (diffPause.TotalSeconds > 0)
                    {
                        Times.EndTheoDateTime += diffPause;
                    }

                    if (PrgOptions.IsStopCptAtMaxDemieJournee && Times.GetTpsTravMatin().CompareTo(PrgOptions.TempsMaxDemieJournee) > 0)
                    {
                        Times.EndTheoDateTime += (Times.GetTpsTravMatin() - PrgOptions.TempsMaxDemieJournee);
                    }

                }

                lblEndTime.ContentShortTime(Times.EndTheoDateTime);
                lblEndTime.Content += "*";

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
                    IntervalTemps ivlTemps = Times.PausesHorsDelai.First(r => !r.IsIntervalComplet());
                    TimeSpan tpsPause = AppDateUtils.DtNow().TimeOfDay - ivlTemps.Start.TimeOfDay;
                    lblTpsTravReel.Content = tpsPause.ToString("h':'mm':'ss");
                    lblTpsTravReelLbl.Content = "Temps de la pause en cours :";

                    DateTime newEndTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start,
                                PrgOptions, EnumTypesJournees.Complete)
                                                    + Times.GetTpsPause() + tpsPause;
                    lblEndTime.ContentShortTime(newEndTime);
                    lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;


                };
                pauseHorsPeriodeTimer.Interval = new TimeSpan(0, 0, 1);

                pauseHorsPeriodeTimer.Start();

                lblTpsTravReel.ToolTip = null;

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
                ShowInTaskbar = true;
                MessageBoxUtils.TopMostMessageBox(
                    String.Format("Bien déjeuné ?{0}La session a été dévérouillée pendant la pause du midi : penser à Badger !", Environment.NewLine),
                    "Question",
                     MessageBoxButton.OK,
                    MessageBoxImage.Question
                    );

                RestoreWindow();
                ShowInTaskbar = false;
            }

        }

        private void OnAfterSessionLock(object sender, SessionSwitchEventArgs e)
        {
            if (EtatBadger == 0 && e.Reason == SessionSwitchReason.SessionLock && RealTimeTsNow >= PrgOptions.PlageFixeMatinFin)
            {
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
            MessageBoxResult v = MessageBox.Show("Voulez-vous effectuer le premier badgeage de la journée ? Cliquez sur Oui pour badger ou Non pour entrer l'heure manuellement (dans le cas où le pointage a déjà été effectuer via la page)",
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
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                PrgSwitch.IsRealClose = true;

                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    RestartApp();
                    return;
                }
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

                try
                {
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

                    Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start,
                        PrgOptions, EnumTypesJournees.Complete)
                                            + Times.GetTpsPause();



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

                    Times.EndTheoDateTime = Times.EndTheoDateTime
                                                   + Times.GetTpsPause();


                    lblEndTime.ContentShortTime(Times.EndTheoDateTime);
                    lblEndTime.Content += "*";
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

            bool isMaxDepass = UpdRealTimes();

            TimeSpan diffTotal = Times.EndTheoDateTime - Times.PlageTravMatin.Start;
            TimeSpan diff = RealTimeDtNow - Times.PlageTravMatin.Start;

            if (PrgSwitch.PbarMainTimerActif)
            {
                pbarTime.IsIndeterminate = false;
                pbarTime.Value = 100 * diff.TotalSeconds / diffTotal.TotalSeconds;
            }

            if (RealTimeDtNow.DayOfYear != Times.TimeRef.DayOfYear)
            {
                // Le jour a changé. Il faut redémarrer.
                PointageSaverObj.SaveCurrentDayTimes();
                RestartApp();
            }

            //DoNotification();
            NotifManager.DoNotification(RealTimeTsNow, EnumTypesTemps.RealTime, PrgOptions.IsGlobalShowNotifications);
            NotifManager.DoNotification(RealTimeTempsTravaille, EnumTypesTemps.TpsTrav, PrgOptions.IsGlobalShowNotifications);
            NotifManager.DoNotification(RealTimeTempsTravailleMatin, EnumTypesTemps.TpsTravMatin, PrgOptions.IsGlobalShowNotifications);
            NotifManager.DoNotification((RealTimeTempsTravaille - RealTimeTempsTravailleMatin), EnumTypesTemps.TpsTravAprem, PrgOptions.IsGlobalShowNotifications);

            if (PrgOptions.IsStopCptAtMax
               && isMaxDepass
               && !PrgSwitch.IsTimerStoppedByMaxTime)
            {
                PrgSwitch.PbarMainTimerActif = false;
                pbarTime.IsIndeterminate = true;

                PrgSwitch.IsTimerStoppedByMaxTime = true;
            }


            //
            // (TimesBadgerDto Times, AppOptions PrgOptions, AppSwitchs PrgSwitch, int EtatBadger, TimeSpan RealTimeTsNow)

            PluginMgr.PlayHook("BadgeBetaTest", new object[]
            {
                 Times, PrgOptions, PrgSwitch, EtatBadger, RealTimeTsNow
            });

            /*
            if (PrgSwitch.IsBetaUser
                && PrgOptions.IsAutoBadgeMeridienne
                && EtatBadger == 1
                && !PrgSwitch.IsAutoBadgeage
                && RealTimeTsNow.CompareTo(Times.PlageTravMatin.EndOrDft.TimeOfDay + PrgOptions.TempsMinPause + new TimeSpan(0, 0, SpecDelayMeridAutoBadgage)) >= 0)
            {
                PrgSwitch.IsAutoBadgeage = true;

                _logger.Debug("Spec: Autobadgeage");
                btnBadger.IsEnabled = false;
                BadgerWorker.BadgeFullAction(true);
                btnBadger.IsEnabled = true;

                if (PrgOptions.IsDailyDisableAutoBadgeMerid)
                {
                    PrgOptions.IsAutoBadgeMeridienne = false;
                    OptionManager.SaveOptions(PrgOptions);
                    _logger.Debug("Spec: Autobadgeage désactivé par IsDailyDisableAutoBadgeMerid");
                }
            }
             * */

            UpdateInfos();

            if (PrgSwitch.CanCheckUpdate)
            {
                SignalUpdate();
            }

            if (PrgOptions.ShowTipsAtStart && EtatBadger >= 0 && !PrgSwitch.IsShowOnStartupDone && IsLoaded)
            {
                PrgSwitch.IsShowOnStartupDone = true;
                LoadsDidYouKnow();

            }



        }


        private bool UpdRealTimes()
        {
            RealTimeDtNow = AppDateUtils.DtNow();
            RealTimeTsNow = RealTimeDtNow.TimeOfDay;
            bool isMaxDepass = false;
            RealTimeTempsTravaille = TimesUtils.GetTempsTravaille(RealTimeDtNow, EtatBadger, Times, PrgOptions, TypeJournee, true, ref isMaxDepass);
            if (TypeJournee == EnumTypesJournees.Complete)
            {
                if (EtatBadger <= 1)
                {
                    RealTimeTempsTravailleMatin = RealTimeTempsTravaille;
                }
                else
                {
                    RealTimeTempsTravailleMatin = RealTimeTempsTravaille - (Times.PlageTravAprem.Start - Times.PlageTravMatin.EndOrDft);
                }
            }
            else
            {
                RealTimeTempsTravailleMatin = RealTimeTempsTravaille;
            }
            return isMaxDepass;
        }

        private void UpdateInfos()
        {
            TimeSpan tpsRestant = Times.EndTheoDateTime - RealTimeDtNow;


            // Tps trav pour une journée ou une demie-journée.
            TimeSpan tTravTheo = TimesUtils.GetTpsTravTheoriqueOneDay(PrgOptions, TypeJournee);

            // if (RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) <= 0)
            //  {
            String lblTpsTrav = "Compteur temps travaillé du jour :";
            String msgTooltip = String.Format("{0}Double-cliquer pour afficher le temps de travail restant",
                PrgOptions.IsAdd5minCpt ? "Le temps travaillé prend en compte les 5 min supplémentaires." + Environment.NewLine : "");


            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(RealTimeTempsTravaille);

            if (PrgSwitch.IsTimeRemainingNotTimeWork)
            {

                msgTooltip = "Double-cliquer pour afficher le compteur temps travaillé du jour";
                lblTpsTrav = RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0
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

            if (RealTimeTsNow.CompareTo(PrgOptions.PlageFixeApremFin) >= 0)
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Visible;

                string tplTpsReelSuppl = "({0})";
                if (RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0)
                {
                    tplTpsReelSuppl = "(+{0})";

                    if (!PrgSwitch.IsMoreThanTpsTheo && RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) < 0)
                    {
                        PrgSwitch.IsMoreThanTpsTheo = true;
                        lblTpsTravReel.Foreground = Cst.SCBDarkGreen;

                    }
                    else if (RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) >= 0)
                    {
                        PrgSwitch.IsMoreThanTpsTheo = false;
                        lblTpsTravReel.Foreground = Cst.SCBDarkRed;
                    }
                }
                lblTpsTravReelSuppl.Content = String.Format(tplTpsReelSuppl, MiscAppUtils.TimeSpanShortStrFormat((RealTimeTempsTravaille - tTravTheo)));


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

        private void SignalUpdate(bool isForceCheck = false)
        {
            //UpdateChecker chk = UpdateChecker.Instance;

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

            bool isOrigOptAdd5min = PrgOptions.IsAdd5minCpt;

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
                AdaptUiFromOptions(PrgOptions);
                UpdaterMgr.XmlUpdFilePath = PrgOptions.UpdateXmlUri;
                if (EtatBadger < 3)
                {
                    if (isOrigOptAdd5min && !PrgOptions.IsAdd5minCpt)
                    {
                        Times.EndTheoDateTime += new TimeSpan(0, 5, 0);
                    }
                    else if (!isOrigOptAdd5min && PrgOptions.IsAdd5minCpt)
                    {
                        Times.EndTheoDateTime -= new TimeSpan(0, 5, 0);
                    }

                    lblEndTime.ContentShortTime(Times.EndTheoDateTime);
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

                if (EtatBadger != -1)
                {
                    _logger.Info("Sauvegarde de la session (EtatBadger: {0})", EtatBadger);
                    PointageSaverObj.SaveCurrentDayTimes();
                }

                _logger.Debug("Sauvegarde des options");
                OptionManager.SaveOptions(PrgOptions);

                _notifyIcon.Visible = false;



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
            _nIconOpenOptionItem.Click += delegate(object sender, EventArgs args)
            {
                btnOptions_Click(null, null);
            };

            _nIconOpenTipsItem = new ToolStripMenuItem("Afficher &les astuces...");
            _nIconOpenTipsItem.ToolTipText = "Raccourci : touche F4";
            _nIconOpenTipsItem.Click += delegate(object sender, EventArgs args)
            {
                LoadsDidYouKnow();
            };

            _nIconShowNotificationItem = new ToolStripMenuItem("&Afficher les notifications");
            _nIconShowNotificationItem.Checked = PrgOptions.IsGlobalShowNotifications;
            _nIconShowNotificationItem.Click += delegate(object sender, EventArgs args)
            {
                PrgOptions.IsGlobalShowNotifications = !PrgOptions.IsGlobalShowNotifications;
                _nIconShowNotificationItem.Checked = PrgOptions.IsGlobalShowNotifications;
            };

            _nIconBadgerManItem = new ToolStripMenuItem("&Badger");
            _nIconBadgerManItem.Click += (sender, args) => btnBadger_Click(sender, null);

            _nIconRestoreItem = new ToolStripMenuItem("&Ouvrir");
            _nIconRestoreItem.Font = new Font(_nIconRestoreItem.Font, _nIconRestoreItem.Font.Style | System.Drawing.FontStyle.Bold);
            _nIconRestoreItem.Click += delegate(object sender, EventArgs args)
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

            _notifyIcon.DoubleClick += delegate(object sender, EventArgs args)
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
            btnBadger.Margin = Cst.BtnBadgerPositionAtLeft;
            btnBadger.ToolTip = "Cliquer pour badger le début de la matinée";
            btnBadger.Visibility = Visibility.Visible;

            lblStartTime.ContentShortTime(Times.PlageTravMatin.Start);
            lblEndTime.ContentShortTime(Times.EndTheoDateTime);

            ctrlTyJournee.IsEnabledChange = true;

            Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

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

            if (TypeJournee == EnumTypesJournees.Complete)
            {
                btnBadger.Margin = Cst.BtnBadgerPositionAtLeft;
            }
            else
            {
                btnBadger.Margin = Cst.BtnBadgerPositionAtCenter;
            }
            btnBadger.ToolTip = "Cliquer pour badger la fin de la matinée";
            btnBadger.Visibility = Visibility.Visible;


            Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

            lblStartTime.ContentShortTime(Times.PlageTravMatin.Start);
            lblEndTime.ContentShortTime(Times.EndTheoDateTime);
            lblHmidiS.Visibility = Visibility.Hidden;
            lblHmidiE.Visibility = Visibility.Hidden;
            lblTpsTravMatin.Visibility = Visibility.Hidden;
            lblTpsTravAprem.Visibility = Visibility.Hidden;
            lblPauseTime.Visibility = Visibility.Hidden;

            lblStartTime.Foreground = Cst.SCBBlack;

            lblTpsTravReel.ContentShortTime(TimeSpan.Zero);
            lblTpsTravReel.Visibility = Visibility.Visible;

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

        private void AdaptUiForState1()
        {
            // Etat de l'UI après avoir badgé la fin de la matinée.
            PrgSwitch.PbarMainTimerActif = true;
            pbarTime.IsIndeterminate = false;
            PrgSwitch.IsTimerStoppedByMaxTime = false;

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


            btnBadger.Margin = Cst.BtnBadgerPositionAtCenter;
            btnBadger.ToolTip = "Cliquer pour badger le début de l'après-midi";
            btnBadger.Visibility = Visibility.Visible;

            lblHmidiS.ContentShortTime(Times.PlageTravMatin.EndOrDft);
            lblHmidiS.Visibility = Visibility.Visible;
            lblHmidiE.Visibility = Visibility.Hidden;



            TimeSpan tsFinPause = Times.PlageTravMatin.EndOrDft.TimeOfDay + PrgOptions.TempsMinPause;
            PushNewInfo("Prochain badgeage à partir de " + tsFinPause.ToString(Cst.TimeSpanFormatWithH));
            lblTpsTravReel.Foreground = Cst.SCBGrey;
            lblTpsTravReel.ToolTip = "C'est la pause du midi ! Le temps de travail réel n'est pas compté.";

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
                    TimeSpan remainingTimer = tsFinPause - RealTimeTsNow;
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

            // On MaJ l'heure finale theorique de fin, en prenant en compte le temps de pause (pour une journée de complete).
            Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions, TypeJournee);
            if (TypeJournee == EnumTypesJournees.Complete)
            {

                var diffPause = tmpsPause.Value - PrgOptions.TempsMinPause;

                if (diffPause.TotalSeconds > 0)
                {
                    Times.EndTheoDateTime += diffPause;
                }

                if (PrgOptions.IsStopCptAtMaxDemieJournee && Times.GetTpsTravMatin().CompareTo(PrgOptions.TempsMaxDemieJournee) > 0)
                {
                    Times.EndTheoDateTime += (Times.GetTpsTravMatin() - PrgOptions.TempsMaxDemieJournee);
                }


                btnBadger.ToolTip = "Cliquer pour badger la fin de l'après-midi";

            }
            else
            {
                btnBadger.ToolTip = "Cliquer pour badger la fin de la demie-journée";
            }

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

            btnBadger.Margin = Cst.BtnBadgerPositionAtRight;
            btnBadger.Visibility = Visibility.Visible;

            btnBadgerM.IsEnabled = true;

            lblHmidiE.ContentShortTime(Times.PlageTravAprem.Start);
            lblHmidiE.Visibility = Visibility.Visible;

            lblPauseTime.ContentShortTime(tmpsPause.Value);
            lblPauseTime.Visibility = Visibility.Visible;
            lblTpsTravReel.Foreground = Cst.SCBBlack;
            lblTpsTravReel.ToolTip = null;

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
            lblFinStr.Content = "Fin à";

            TimeSpan tempSuppl = RealTimeTempsTravaille - TimesUtils.GetTpsTravTheoriqueOneDay(PrgOptions, TypeJournee);
            PushNewInfo(String.Format("Temps travaillé aujourd'hui : {0} ({2}{1}) ",
                RealTimeTempsTravaille.ToString(Cst.TimeSpanFormat),
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
                PrgOptions.OnAutoBadgeAtStartChange += delegate(bool b)
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
                PrgOptions.OnGlobalShowNotificationsChange += delegate(bool b)
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
                PrgSwitch.OnPauseCurrentStateChange += delegate(EnumStatePause b)
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
                    PrgOptions.OnAutoBadgeMeridienneChange += delegate(bool b)
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
            if (PrgOptions.Notif1Obj.IsActive)
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
            if (PrgOptions.Notif2Obj.IsActive)
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
            Action<NotificationDto> afterShowAction = delegate(NotificationDto dto)
            {
                lblTpsTravReel.Foreground = Cst.SCBDarkRed;
                lblTpsTravReel.ToolTip = dto.Message;
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

                Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

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

                Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.PlageTravMatin.Start, PrgOptions, TypeJournee);

                AdaptUiLowerThanState();
            }
        }


        internal void RestoreWindow()
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
                UpdateInfos();
            }

        }

        private void btnModTimes_Click(object sender, RoutedEventArgs e)
        {
            LoadDetailView();
        }

        private void LoadDetailView()
        {
            bool isReloadView = true;

            while (isReloadView)
            {
                var mDetailsView = new MoreDetailsView(Times, EtatBadger, TypeJournee, PrgOptions);
                mDetailsView.ShowDialog();

                isReloadView = mDetailsView.IsMustLoadsModTimeView;

                if (isReloadView)
                {
                    LoadModTimeView();
                }
            }
        }

        private void LoadModTimeView()
        {
            ModTimeView m = new ModTimeView(Times, TypeJournee, EtatBadger, PrgOptions.UrlMesPointages);
            m.ShowDialog();

            if (m.HasChanged)
            {
                TypeJournee = m.TypeJournee;
                EtatBadger = m.EtatBadger;
                OldEtatBadger = m.EtatBadger;

                AdaptUiLowerThanState();

                ctrlTyJournee.TypeJournee = TypeJournee;

                ChangeTypeJournee();

                ClockUpdTimerOnOnTick(null, null);

                PointageSaverObj.SaveCurrentDayTimes();


                PrgSwitch.IsMoreThanTpsTheo = false;
                PrgSwitch.IsTimerStoppedByMaxTime = false;
                PrgSwitch.PbarMainTimerActif = true;
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

        private void imgBtnPauseReport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LoadDetailView();
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
                _logger.Warn("L'application n'a pas pu redémarrée");
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



            return l.ToArray();
        }



    }
}
