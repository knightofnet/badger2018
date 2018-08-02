using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;
using Badger2018.views;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;

using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Threading.Timer;

namespace Badger2018
{



    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        readonly DispatcherTimer _clockUpdTimer = new DispatcherTimer();

        public AppOptions PrgOptions { get; set; }

        public BadgerWorker BadgerWorker { get; private set; }

        public NoticationsManager NotifManager { get; private set; }

        public TimesBadgerDto Times { get; private set; }

        public CoreAudioController CoreAudioCtrler = new CoreAudioController();
        private DidYouKnowView DidYouKnowWindow { get; set; }

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

        private int OldEtatBadger { get; set; }

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

        public int SpecDelayMeridAutoBadgage { get; set; }

        public AppSwitchs PrgSwitch { get; set; }


        public TimeSpan RealTimeTempsTravaille { get; private set; }
        public TimeSpan RealTimeTempsTravailleMatin { get; private set; }
        public DateTime RealTimeDtNow { get; private set; }
        public TimeSpan RealTimeTsNow { get; private set; }
        public bool IsFullyLoaded { get; private set; }

        public MainWindow(AppOptions prgOptions)
        {
            _logger.Debug("Chargement de l'écran principal");

            PrgSwitch = new AppSwitchs();

            InitializeComponent();
            Times = new TimesBadgerDto();
            //imgOptions.Source = MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "iconSetting.png");
            lblVersion.Content = String.Format(lblVersion.Content.ToString(), System.Reflection.Assembly.GetExecutingAssembly().GetName().Version, Properties.Resources.versionName);
            pbarTime.IsIndeterminate = true;

            PrgOptions = prgOptions;

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

            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            PrgSwitch.PbarMainTimerActif = true;
            InitNotifyIcon();
            Closing += OnClosing;

            KeyUp += (sender, args) =>
            {
                if (args.Key == Key.F2)
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

            btnModTimes.Visibility = prgOptions.IsFirstRun ? Visibility.Visible : Visibility.Hidden;

            EtatBadger = -1;
            TypeJournee = EnumTypesJournees.Complete;

            if (MustReloadIncomplete())
            {
                _logger.Info("Rechargement des informations");
                ReloadUiFromInterface();
                _logger.Info("FIN : Rechargement des informations");
            }
            else
            {
                _logger.Info("Nouvelle session");

                Times.StartDateTime = AppDateUtils.DtNow().AtSec(Cst.SecondeOffset);
                Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.StartDateTime, PrgOptions, TypeJournee);

                lblStartTime.Foreground = Cst.SCBGrey;

                lblTpsTravReel.Visibility = Visibility.Hidden;

                lblStartTime.Content = Times.StartDateTime.ToShortTimeString();
                lblEndTime.Content = Times.EndTheoDateTime.ToShortTimeString();

                lblHmidiS.Visibility = Visibility.Hidden;
                lblHmidiE.Visibility = Visibility.Hidden;

                ctrlTyJournee.TypeJournee = EnumTypesJournees.Complete;

                /*
                chkAmidi.IsEnabled = false;
                chkMatin.IsEnabled = false;
                */

                TimeSpan tsfinMat = TimesUtils.GetTimeEndTravTheorique(Times.StartDateTime, PrgOptions,
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

            // Initialise le timer principal
            _clockUpdTimer.Tick += ClockUpdTimerOnTick;
            _clockUpdTimer.Interval = new TimeSpan(0, 0, 10);
            _clockUpdTimer.Start();

            IsFullyLoaded = true;
            ClockUpdTimerOnTick(null, null);
            PrgSwitch.CanCheckUpdate = true;


            Loaded += (sender, args) =>
            {
                if (PrgOptions.ShowTipsAtStart)
                {
                    LoadsDidYouKnow();
                }
            };

        }

        private void AssignBetaRole()
        {
            if (MiscAppUtils.CsvStringContains(Environment.UserName.ToUpper(), ((string)Properties.Settings.Default["specUser"]).ToUpper()))
            {
                PrgSwitch.IsBetaUser = true;
            }
            else
            {
                PrgSwitch.IsBetaUser = false;
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

            ctrlTyJournee.OnTypeJourneeChange += (e) =>
            {
                ChangeTypeJournee();
            };

            lienCptTpsReel.Click += (sender, args) => MiscAppUtils.GoTo(PrgOptions.UrlCptTpsReel);
            lienMesBadgeages.Click += (sender, args) => MiscAppUtils.GoTo(PrgOptions.UrlMesPointages);
            lienSirhius.Click += (sender, args) => MiscAppUtils.GoTo(PrgOptions.UrlSirhius);

            PrgSwitch.IsSoundOver = true;
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
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(argbColor);
                    Color colorA = Color.FromRgb(color.R, color.G, color.B);

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
                MiscAppUtils.TopMostMessageBox(
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

            btnBadger.IsEnabled = false;
            BadgerWorker.BadgeFullAction();

            PrgOptions.LastBadgeDelay = origDelay;
            btnBadger.IsEnabled = true;

            ClockUpdTimerOnTick(null, null);

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
                }

                Times.StartDateTime = dtStart.Value;
                if (Times.StartDateTime.TimeOfDay.CompareTo(PrgOptions.HeureMinJournee) < 0)
                {
                    Times.StartDateTime = Times.StartDateTime.ChangeTime(PrgOptions.HeureMinJournee);
                }




                AdaptUiFromState(EtatBadger + 1, null);
                EtatBadger = 0;

                ClockUpdTimerOnTick(null, null);

                isSpecBadgeage = true;

                _logger.Info("Sauvegarde de la session (EtatBadger: {0}", EtatBadger);
                SaveCurrentDayTimes();
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
                        btnBadger.IsEnabled = true;
                        btnBadgerM.IsEnabled = true;
                    });
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
            PointageElt p = LoadIncomplete();
            _logger.Debug(p.ToString());

            try
            {
                EtatBadger = p.EtatBadger;

                if (EtatBadger >= 0)
                {
                    Times.StartDateTime = DateTime.Parse(p.B0);

                    Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.StartDateTime, PrgOptions, EnumTypesJournees.Complete);


                }
                if (EtatBadger >= 1)
                {
                    Times.PauseStartDateTime = DateTime.Parse(p.B1);
                }
                if (EtatBadger >= 2)
                {
                    Times.PauseEndDateTime = DateTime.Parse(p.B2);

                }
                if (EtatBadger >= 3)
                {
                    Times.EndDateTime = DateTime.Parse(p.B3);
                    Times.EndTheoDateTime = Times.EndDateTime;


                }

                NotifManager.SetNotifShow(Cst.NotifCust1Name, p.IsNotif1Showed);
                NotifManager.SetNotifShow(Cst.NotifCust2Name, p.IsNotif2Showed);

                TypeJournee = EnumTypesJournees.GetFromIndex(p.TypeJournee);
                ctrlTyJournee.TypeJournee = TypeJournee;
                OldEtatBadger = p.OldEtatBadger;

                UpdRealTimes();

                AdaptUiLowerThanState();

                ClockUpdTimerOnTick(null, null);


                if (EnumTypesJournees.IsDemiJournee(TypeJournee))
                {
                    ChangeTypeJournee();
                }

            }
            catch (Exception e)
            {
                string msg = "Erreur lors de la lecture de la journée mémorisée. Supprimer le fichier xml du jour s'il existe et relancez le programme";
                _logger.Error(msg);
                _logger.Error(e.Message);
                _logger.Error(e.StackTrace);
                MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }



        private void ClockUpdTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (!IsFullyLoaded)
            {
                return;
            }

            bool isMaxDepass = UpdRealTimes();

            TimeSpan diffTotal = Times.EndTheoDateTime - Times.StartDateTime;
            TimeSpan diff = RealTimeDtNow - Times.StartDateTime;

            if (PrgSwitch.PbarMainTimerActif)
            {
                pbarTime.IsIndeterminate = false;
                pbarTime.Value = 100 * diff.TotalSeconds / diffTotal.TotalSeconds;
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


            if (PrgSwitch.IsBetaUser
                && PrgOptions.IsAutoBadgeMeridienne
                && EtatBadger == 1
                && !PrgSwitch.IsAutoBadgeage
                && RealTimeTsNow.CompareTo(Times.PauseStartDateTime.TimeOfDay + PrgOptions.TempsMinPause + new TimeSpan(0, 0, SpecDelayMeridAutoBadgage)) >= 0)
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

            UpdateInfos();

            if (PrgSwitch.CanCheckUpdate)
            {
                SignalUpdate();
            }

        }



        private bool UpdRealTimes()
        {
            RealTimeDtNow = AppDateUtils.DtNow();
            RealTimeTsNow = RealTimeDtNow.TimeOfDay;
            bool isMaxDepass = false;
            RealTimeTempsTravaille = GetTempsTravaille(RealTimeDtNow, ref isMaxDepass);
            if (TypeJournee == EnumTypesJournees.Complete)
            {
                if (EtatBadger <= 1)
                {
                    RealTimeTempsTravailleMatin = RealTimeTempsTravaille;
                }
                else
                {
                    RealTimeTempsTravailleMatin = RealTimeTempsTravaille - (Times.PauseEndDateTime - Times.PauseStartDateTime);
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
            if (tpsRestant.TotalSeconds > 0)
            {
                lblEndTime.ToolTip = "Reste " + tpsRestant.ToString(Cst.TimeSpanFormat);
            }
            else
            {
                lblEndTime.ToolTip = "Temps supplémentaire " + tpsRestant.ToString(Cst.TimeSpanFormat);
            }

            // Tps trav pour une journée ou une demie-journée.
            TimeSpan tTravTheo = TimesUtils.GetTpsTravTheoriqueOneDay(PrgOptions, TypeJournee);

            if (RealTimeTempsTravaille.CompareTo(PrgOptions.TempsMaxJournee) <= 0)
            {
                String lblTpsTrav = "Compteur temps travaillé du jour :";
                String msgTooltip = String.Format("{0}Double-cliquer pour afficher le temps de travail restant",
                    PrgOptions.IsAdd5minCpt ? "Le temps travaillé prend en compte les 5 min supplémentaires." + Environment.NewLine : "");

                String strTpsTrav = RealTimeTempsTravaille.ToString(Cst.TimeSpanFormat);

                if (PrgSwitch.IsTimeRemainingNotTimeWork)
                {

                    msgTooltip = "Double-cliquer pour afficher le compteur temps travaillé du jour";
                    lblTpsTrav = RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0 ? "Temps supplémentaire pour la journée :" : "Temps restant pour la journée :";
                    strTpsTrav = tpsRestant.ToString(Cst.TimeSpanFormat);
                }

                lblTpsTravReelLbl.Content = lblTpsTrav;
                lblTpsTravReel.Content = strTpsTrav;
                lblTpsTravReel.ToolTip = msgTooltip;
            }

            if (RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0)
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Visible;
                lblTpsTravReelSuppl.Content = String.Format("(+{0})",
                    (RealTimeTempsTravaille - tTravTheo).ToString(Cst.TimeSpanAltFormat));

                if (!NotifManager.IsNotifShow(Cst.NotifTpsMaxJournee) && !PrgSwitch.IsMoreThanTpsTheo)
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

        }

        private void SignalUpdate(bool isForceCheck = false)
        {
            UpdateChecker chk = UpdateChecker.Instance;

            bool isOkToSignalUpd = false;
            if (!isForceCheck && EtatBadger <= 0 && chk.IsNewUpdateAvalaible &&
                chk.UpdateCheckTag.Equals("launch"))
            {
                chk.UpdateCheckTag = "-1";

                isOkToSignalUpd = true;
            }

            if (isForceCheck || (EtatBadger == 2 && chk.UpdateCheckTag.Equals("-1")))
            {
                chk.CheckForNewUpdate("2");
                if (chk.IsUpdateCheckSuccess && chk.IsNewUpdateAvalaible)
                {
                    isOkToSignalUpd = true;
                }

            }

            if (!isOkToSignalUpd)
            {
                imgBtnUpdate.Visibility = chk.IsNewUpdateAvalaible ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            var result = MiscAppUtils.TopMostMessageBox("Une nouvelle version du programme est disponible :" + Environment.NewLine +
                                                        " Titre : " + chk.UpdateInfo.Title + Environment.NewLine +
                                                        " Version : " + chk.UpdateInfo.Version + Environment.NewLine +
                                                        " Description : " + chk.UpdateInfo.Description + Environment.NewLine + Environment.NewLine +
                                                        "Voulez-vous récupérer le fichier de mise à jour ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                if (UpdateChecker.UpdateProgram(chk))
                {
                    PrgSwitch.IsRealClose = true;
                    Close();
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

            }
            if (opt.IsRazNotifs)
            {
                NotifManager.ResetNotificationShow();
            }

            ShowInTaskbar = false;
            _notifyIcon.Visible = true;

            ClockUpdTimerOnTick(null, null);
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {

            if (PrgSwitch.IsRealClose || PrgOptions.ActionButtonClose == EnumActionButtonClose.CLOSE)
            {
                _clockUpdTimer.Tick -= ClockUpdTimerOnTick;
                _clockUpdTimer.Stop();

                if (PrgOptions.IsFirstRun)
                {
                    PrgOptions.IsFirstRun = false;
                }

                if (EtatBadger != -1)
                {
                    _logger.Info("Sauvegarde de la session (EtatBadger: {0})", EtatBadger);
                    SaveCurrentDayTimes();
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
            _notifyIcon.Icon = MiscAppUtils.DoGetIconSourceFromResource(GetType().Assembly.GetName().Name, "Paomedia-Small-N-Flat-Clock.ico");
            _notifyIcon.Visible = true;

            _notifyIconTrayMenu = new ContextMenuStrip();

            _notifyIconAppNameLblItem = new ToolStripLabel(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);


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
            _notifyIcon.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
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



            _logger.Debug("FIN - AdaptUiFromState(...)");
        }

        private void AdaptUiForStateM1()
        {
            btnBadger.Margin = Cst.BtnBadgerPositionAtLeft;
            btnBadger.ToolTip = "Cliquer pour badger le début de la matinée";
            btnBadger.Visibility = Visibility.Visible;

            lblStartTime.ContentShortTime(Times.StartDateTime);
            lblEndTime.ContentShortTime(Times.EndTheoDateTime);


            Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.StartDateTime, PrgOptions, TypeJournee);

            lblHmidiS.Visibility = Visibility.Hidden;
            lblHmidiE.Visibility = Visibility.Hidden;
            lblTpsTravMatin.Visibility = Visibility.Hidden;
            lblTpsTravAprem.Visibility = Visibility.Hidden;

            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", PrgOptions.PlageFixeMatinFin.ToString(Cst.TimeSpanFormat));

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


            Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.StartDateTime, PrgOptions, TypeJournee);

            lblStartTime.ContentShortTime(Times.StartDateTime);
            lblEndTime.ContentShortTime(Times.EndTheoDateTime);
            lblHmidiS.Visibility = Visibility.Hidden;
            lblHmidiE.Visibility = Visibility.Hidden;
            lblTpsTravMatin.Visibility = Visibility.Hidden;
            lblTpsTravAprem.Visibility = Visibility.Hidden;
            lblPauseTime.Visibility = Visibility.Hidden;

            lblStartTime.Foreground = Cst.SCBBlack;

            lblTpsTravReel.ContentShortTime(TimeSpan.Zero);
            lblTpsTravReel.Visibility = Visibility.Visible;

            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", PrgOptions.PlageFixeMatinFin.ToString(Cst.TimeSpanFormat));

            _notifyIconMatineeLblItem.Visible = true;
            _notifyIconMatineeLblItem.Text = String.Format(Cst.MatineeStartStr, Times.StartDateTime.ToShortTimeString());
            _nIconBadgerManItem.Text = "Badger la fin de la matinée";
            _nIconBadgerManItem.ToolTipText = btnBadger.ToolTip.ToString();

        }

        private void AdaptUiForState1()
        {
            // Etat de l'UI après avoir badgé la fin de la matinée.
            PrgSwitch.PbarMainTimerActif = true;
            pbarTime.IsIndeterminate = false;
            PrgSwitch.IsTimerStoppedByMaxTime = false;

            lblTpsTravMatin.Visibility = Visibility.Visible;
            lblTpsTravMatin.Content = Times.GetTpsTravMatinOrMax(PrgOptions.TempsMaxDemieJournee).ToString(Cst.TimeSpanFormat);

            btnBadger.Margin = Cst.BtnBadgerPositionAtCenter;
            btnBadger.ToolTip = "Cliquer pour badger le début de l'après-midi";
            btnBadger.Visibility = Visibility.Visible;

            lblHmidiS.ContentShortTime(Times.PauseStartDateTime);
            lblHmidiS.Visibility = Visibility.Visible;
            lblHmidiE.Visibility = Visibility.Hidden;

            Times.TpsTravMatin = Times.PauseStartDateTime.TimeOfDay - Times.StartDateTime.TimeOfDay;

            TimeSpan tsFinPause = Times.PauseStartDateTime.TimeOfDay + PrgOptions.TempsMinPause;
            PushNewInfo("Prochain badgeage à partir de " + tsFinPause.ToString(Cst.TimeSpanFormat));
            lblTpsTravReel.Foreground = Cst.SCBGrey;
            lblTpsTravReel.ToolTip = "C'est la pause du midi ! Le temps de travail réel n'est pas compté.";

            if (EtatBadger == 1)
            {
                PrgSwitch.PbarMainTimerActif = false;
                pbarTime.ToolTip = "Décompte jusqu'à " + tsFinPause.ToString(Cst.TimeSpanFormat);

                DispatcherTimer finPauseMidiTimer = new DispatcherTimer();
                finPauseMidiTimer.Interval = new TimeSpan(0, 0, 5);
                finPauseMidiTimer.Tick += (sender, args) =>
                {
                    TimeSpan remainingTimer = tsFinPause - RealTimeTsNow;
                    if (remainingTimer < TimeSpan.Zero)
                    {
                        finPauseMidiTimer.Stop();
                        PrgSwitch.PbarMainTimerActif = true;
                        pbarTime.ToolTip = null;
                    }
                    else
                    {
                        pbarTime.IsIndeterminate = false;
                        ChangePBarValue(100 * remainingTimer.TotalSeconds / PrgOptions.TempsMinPause.TotalSeconds);
                    }
                };
                finPauseMidiTimer.Start();
            }

            if (PrgSwitch.IsBetaUser && PrgOptions.IsAutoBadgeMeridienne && EtatBadger == 1)
            {
                Random rnd = new Random();
                SpecDelayMeridAutoBadgage = rnd.Next(0, PrgOptions.DeltaAutoBadgeageMinute);

                Title += " - " +
                         (Times.PauseStartDateTime.TimeOfDay + PrgOptions.TempsMinPause +
                          new TimeSpan(0, 0, SpecDelayMeridAutoBadgage)).ToString(Cst.TimeSpanFormat);
            }


            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", tsFinPause.ToString(Cst.TimeSpanFormat));

            _notifyIconMatineeLblItem.Text = String.Format(Cst.MatineeEndStr, Times.StartDateTime.ToShortTimeString(), Times.PauseStartDateTime.ToShortTimeString());
            _nIconBadgerManItem.Text = "Badger la fin de la pause du midi";
            _nIconBadgerManItem.ToolTipText = btnBadger.ToolTip.ToString();
        }

        private void AdaptUiForState2(TimeSpan? tmpsPause)
        {
            // Etat de l'UI après avoir badgé le début de l'après-midi.



            PrgSwitch.PbarMainTimerActif = true;
            PrgSwitch.IsTimerStoppedByMaxTime = false;
            NotifManager.SetNotifShow(Cst.NotifTpsMaxJournee, false);

            // On MaJ l'heure finale theorique de fin, en prenant en compte le temps de pause (pour une journée de complete).
            Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.StartDateTime, PrgOptions, TypeJournee);
            if (TypeJournee == EnumTypesJournees.Complete)
            {

                var diffPause = tmpsPause.Value - PrgOptions.TempsMinPause;
                if (diffPause.TotalSeconds > 0)
                {
                    Times.EndTheoDateTime = Times.EndTheoDateTime + diffPause;
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
                if (PrgOptions.IsLastBadgeIsAutoShutdown)
                {
                    btnBadger.ToolTip += ". " + Environment.NewLine + "L'ordinateur s'éteindra ensuite.";
                }
            }

            lblEndTime.ContentShortTime(Times.EndTheoDateTime);

            btnBadger.Margin = Cst.BtnBadgerPositionAtRight;
            btnBadger.Visibility = Visibility.Visible;


            lblHmidiE.ContentShortTime(Times.PauseEndDateTime);
            lblHmidiE.Visibility = Visibility.Visible;

            lblPauseTime.ContentShortTime(tmpsPause.Value);
            lblPauseTime.Visibility = Visibility.Visible;
            lblTpsTravReel.Foreground = Cst.SCBBlack;
            lblTpsTravReel.ToolTip = null;
            /*
            chkAmidi.IsEnabled = false;
            chkMatin.IsEnabled = false;
            */

            _nIconBadgerManItem.ToolTipText = String.Format("Badgeage possible à partir de {0}", Times.EndTheoDateTime.TimeOfDay.ToString(Cst.TimeSpanFormat));
            _notifyIconApremLblItem.Visible = true;
            _notifyIconApremLblItem.Text = String.Format(Cst.ApremStartStr, Times.PauseEndDateTime.ToShortTimeString());
            _nIconBadgerManItem.Text = "Badger la fin de la journée";
            _nIconBadgerManItem.ToolTipText = btnBadger.ToolTip.ToString();

            PushNewInfo(null);

            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }

        private void AdaptUiForState3()
        {
            // Etat de l'UI après avoir badgé la fin de la journée.

            btnBadger.Visibility = Visibility.Hidden;
            pbarTime.IsIndeterminate = false;

            if (TypeJournee == EnumTypesJournees.Complete)
            {
                lblTpsTravMatin.Visibility = Visibility.Visible;
                lblTpsTravMatin.ContentShortTime(Times.GetTpsTravMatinOrMax(PrgOptions.TempsMaxDemieJournee));

                lblTpsTravAprem.Visibility = Visibility.Visible;
                lblTpsTravAprem.ContentShortTime(Times.GetTpsTravApremOrMax(PrgOptions.TempsMaxDemieJournee));
            }
            else
            {
                lblTpsTravMatin.Visibility = Visibility.Collapsed;
                lblTpsTravAprem.Visibility = Visibility.Collapsed;

            }

            lblEndTime.ContentShortTime(Times.EndDateTime);
            lblFinStr.Content = "Fin à";

            TimeSpan tempSuppl = RealTimeTempsTravaille - TimesUtils.GetTpsTravTheoriqueOneDay(PrgOptions, TypeJournee);
            PushNewInfo(String.Format("Temps travaillé aujourd'hui : {0} ({2}{1}) ",
                RealTimeTempsTravaille.ToString(Cst.TimeSpanFormat),
                tempSuppl.ToString(Cst.TimeSpanFormat),
                tempSuppl.TotalSeconds < 0 ? "-" : "+"));

            ctrlTyJournee.IsEnabled = false;
            /*
            chkAmidi.IsEnabled = false;
            chkMatin.IsEnabled = false;
            */

            _nIconBadgerManItem.ToolTipText = null;
            _notifyIconApremLblItem.Visible = true;
            _notifyIconApremLblItem.Text = String.Format(Cst.ApremEndStr, Times.PauseEndDateTime.ToShortTimeString(), Times.EndDateTime.ToShortTimeString());

            _nIconBadgerManItem.Enabled = false;
            _nIconBadgerManItem.Text = "Badger";
            _nIconBadgerManItem.ToolTipText = null;



        }

        private TimeSpan GetTempsTravaille(DateTime now, ref bool isMaxDepass)
        {

            TimeSpan retTsTpsTrav = TimeSpan.Zero;

            if (EtatBadger == 0)
            {
                retTsTpsTrav = now.TimeOfDay - Times.StartDateTime.TimeOfDay;
                if (PrgOptions.IsStopCptAtMax && retTsTpsTrav.CompareTo(PrgOptions.TempsMaxDemieJournee) >= 0)
                {
                    retTsTpsTrav = PrgOptions.TempsMaxDemieJournee;
                    isMaxDepass = true;
                }
            }
            else if (EtatBadger == 1)
            {
                retTsTpsTrav = Times.PauseStartDateTime.TimeOfDay - Times.StartDateTime.TimeOfDay;
                if (PrgOptions.IsStopCptAtMax && retTsTpsTrav.CompareTo(PrgOptions.TempsMaxDemieJournee) >= 0)
                {
                    retTsTpsTrav = PrgOptions.TempsMaxDemieJournee;
                    isMaxDepass = true;
                }
            }
            else if (EtatBadger == 2)
            {
                TimeSpan matin = TimeSpan.Zero;
                TimeSpan current = TimeSpan.Zero;

                if (TypeJournee == EnumTypesJournees.Complete)
                {
                    current = now.TimeOfDay;

                    if (PrgOptions.TempsMinPause.CompareTo(Times.PauseEndDateTime.TimeOfDay - Times.PauseStartDateTime.TimeOfDay) > 0)
                    {
                        current -= (Times.PauseStartDateTime.TimeOfDay + PrgOptions.TempsMinPause);
                    }
                    else
                    {
                        current -= Times.PauseEndDateTime.TimeOfDay;
                    }

                }

                if (TypeJournee == EnumTypesJournees.Complete)
                {
                    matin = Times.PauseStartDateTime.TimeOfDay - Times.StartDateTime.TimeOfDay;
                }
                else if (EnumTypesJournees.IsDemiJournee(TypeJournee))
                {
                    current = now.TimeOfDay - Times.StartDateTime.TimeOfDay;
                }

                if (PrgOptions.IsStopCptAtMax && matin.CompareTo(PrgOptions.TempsMaxDemieJournee) >= 0)
                {
                    matin = PrgOptions.TempsMaxDemieJournee;
                }
                if (PrgOptions.IsStopCptAtMax && current.CompareTo(PrgOptions.TempsMaxDemieJournee) >= 0)
                {
                    current = PrgOptions.TempsMaxDemieJournee;
                    isMaxDepass = true;
                }

                retTsTpsTrav = current + matin;
                if (PrgOptions.IsStopCptAtMax && retTsTpsTrav.CompareTo(PrgOptions.TempsMaxJournee) >= 0)
                {
                    retTsTpsTrav = PrgOptions.TempsMaxJournee;
                    isMaxDepass = true;
                }
            }
            else if (EtatBadger == 3)
            {

                if (TypeJournee == EnumTypesJournees.Complete)
                {
                    TimeSpan matin = Times.PauseStartDateTime.TimeOfDay - Times.StartDateTime.TimeOfDay;
                    TimeSpan aprem = Times.EndDateTime.TimeOfDay;

                    if (PrgOptions.TempsMinPause.CompareTo(Times.PauseEndDateTime.TimeOfDay - Times.PauseStartDateTime.TimeOfDay) > 0)
                    {
                        aprem -= (Times.PauseStartDateTime.TimeOfDay + PrgOptions.TempsMinPause);
                    }
                    else
                    {
                        aprem -= Times.PauseEndDateTime.TimeOfDay;
                    }




                    if (PrgOptions.IsStopCptAtMax && matin.CompareTo(PrgOptions.TempsMaxDemieJournee) >= 0)
                    {
                        matin = PrgOptions.TempsMaxDemieJournee;
                    }
                    if (PrgOptions.IsStopCptAtMax && aprem.CompareTo(PrgOptions.TempsMaxDemieJournee) >= 0)
                    {
                        aprem = PrgOptions.TempsMaxDemieJournee;
                    }

                    retTsTpsTrav = aprem + matin;
                    if (PrgOptions.IsStopCptAtMax && retTsTpsTrav.CompareTo(PrgOptions.TempsMaxJournee) >= 0)
                    {
                        retTsTpsTrav = PrgOptions.TempsMaxJournee;
                        isMaxDepass = true;
                    }

                }
                else
                {
                    retTsTpsTrav = Times.EndDateTime.TimeOfDay - Times.StartDateTime.TimeOfDay;
                }
            }

            if (PrgOptions.IsAdd5minCpt && !isMaxDepass)
            {
                retTsTpsTrav = retTsTpsTrav + new TimeSpan(0, 5, 0);
            }

            return retTsTpsTrav;
        }

        private void AdaptUiFromOptions(AppOptions opt)
        {

            _logger.Debug("Chargement de l'interface à partir des options");

            try
            {

                NotifManager.UseAlternateNotification = opt.IsUseAlternateNotification;

                _imgBoolAutoBadgeAtStart = new ImagesBooleanWrapper(
                    MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoStartOn.png"),
                    MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoStartOff.png"),
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
                    MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "notifOn.png"),
                    MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "notifOff.png"),
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

                btnBadgerM.ToolTip = opt.IsBtnManuelBadgeIsWithHotKeys ? "Maintenir CTRL pour badger manuellement" : "Badger manuellement";

                if (EtatBadger <= 1)
                {
                    lblPauseTime.Content = PrgOptions.TempsMinPause.ToString(Cst.TimeSpanFormat);
                }

                if (PrgSwitch.IsBetaUser)
                {
                    imgBtnAutoBadgeMidi.Visibility = Visibility.Collapsed;

                    _imgBoolAutoBadgeMeridienne = new ImagesBooleanWrapper(
                        MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoBadgeMidiOn.png"),
                        MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "autoBadgeMidiOff.png"),
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


            notifName = Cst.NotifEndPauseName;
            if (PrgOptions.ShowNotifEndPause)
            {
                NotifManager.RegisterNotification(
                    notifName,
                     "Pause méridienne minimum terminée",
                    "Le temps minimum de la pause méridienne est écoulé. Vous pouvez badger.",
                    null,
                    1,
                    () => Times.PauseStartDateTime.TimeOfDay + PrgOptions.TempsMinPause,
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
            if (PrgOptions.IsNotif1Enabled)
            {
                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName,
                    null,
                    PrgOptions.Notif1Text,
                    null,
                    null,
                    PrgOptions.Notif1Time
                    );
            }
            else
            {
                NotifManager.RemoveNotification(notifName);
            }

            notifName = Cst.NotifCust2Name;
            if (PrgOptions.IsNotif2Enabled)
            {
                NotifManager.RegisterNotificationOnRealTimeNow(
                    notifName,
                    null,
                    PrgOptions.Notif2Text,
                    null,
                    null,
                    PrgOptions.Notif2Time
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
                "Le temps maximum de travail pour une journée est dépassé. Le temps supplémentaire ne sera pas pris en compte.",
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
                "Le temps maximum de travail pour une demie-journée est dépassé. Le temps supplémentaire ne sera pas pris en compte.",
                null,
                0,
                PrgOptions.TempsMaxDemieJournee,
                EnumTypesTemps.TpsTravMatin
                );
            n.DtoAfterShowNotif += afterShowAction;

            n = NotifManager.RegisterNotification(
                notifName + ":2",
                "Information",
                "Le temps maximum de travail pour une demie-journée est dépassé. Le temps supplémentaire ne sera pas pris en compte.",
                null,
                2,
                PrgOptions.TempsMaxDemieJournee,
                EnumTypesTemps.TpsTravAprem
                );
            n.DtoAfterShowNotif += afterShowAction;
        }

        internal void SaveCurrentDayTimes()
        {
            DbbAccessManager.Instance.StartTransaction();

            try
            {
                String sFile = Cst.PointagesDirName + "/" + MiscAppUtils.GetFileNamePointageCurrentDay();

                PointageElt pElt = new PointageElt
                {
                    DateDay = RealTimeDtNow.ToString(),
                    EtatBadger = EtatBadger,
                    OldEtatBadger = OldEtatBadger,
                    TypeJournee = TypeJournee.Index,
                    IsNotif1Showed = NotifManager.IsNotifShow(Cst.NotifCust1Name),
                    IsNotif2Showed = NotifManager.IsNotifShow(Cst.NotifCust2Name),

                };

                DbbAccessManager.Instance.RemoveBadgeagesOfToday();
                if (EtatBadger >= 0)
                {
                    pElt.B0 = Times.StartDateTime.ToString();
                    DbbAccessManager.Instance.AddBadgeageForToday(0, Times.StartDateTime.TimeOfDay);
                }
                if (EtatBadger >= 1)
                {
                    pElt.B1 = Times.PauseStartDateTime.ToString();
                    DbbAccessManager.Instance.AddBadgeageForToday(1, Times.PauseStartDateTime.TimeOfDay);
                }
                if (EtatBadger >= 2)
                {
                    pElt.B2 = Times.PauseEndDateTime.ToString();
                    DbbAccessManager.Instance.AddBadgeageForToday(2, Times.PauseEndDateTime.TimeOfDay);
                }
                if (EtatBadger >= 3)
                {
                    pElt.B3 = Times.EndDateTime.ToString();
                    DbbAccessManager.Instance.AddBadgeageForToday(3, Times.EndDateTime.TimeOfDay);
                }

                _logger.Debug("Sauvegarde : {0}", pElt.ToString());

                String a = SerializeUtils.Serialize(pElt);
                using (StreamWriter sw = new StreamWriter(sFile))
                {
                    sw.Write(XmlUtils.FormatXmlString(a));
                }

                DbbAccessManager.Instance.StopAndCommitTransaction();
            }
            catch (Exception ex)
            {
                DbbAccessManager.Instance.StopAndRollbackTransaction();

                _logger.Error("Erreur lors de la sauvegarde des horaires");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                throw ex;
            }

        }


        private bool MustReloadIncomplete()
        {
            string sFile = Cst.PointagesDirName + "/" + MiscAppUtils.GetFileNamePointageCurrentDay();
            _logger.Debug("Test de l'existence du fichier " + sFile);
            return File.Exists(sFile);
        }
        private PointageElt LoadIncomplete()
        {
            string sFile = Cst.PointagesDirName + "/" + MiscAppUtils.GetFileNamePointageCurrentDay();

            if (File.Exists(sFile))
            {
                PointageElt pointageElt = SerializeUtils.Deserialize<PointageElt>(sFile);
                return pointageElt;
            }
            return null;

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

                if (EtatBadger < 3)
                {
                    EtatBadger = 2;
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

                Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.StartDateTime, PrgOptions, TypeJournee);

                if (EtatBadger < 3)
                {
                    AdaptUiFromState(0, null, false);
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

                Times.EndTheoDateTime = TimesUtils.GetDateTimeEndTravTheorique(Times.StartDateTime, PrgOptions, TypeJournee);

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
                    AdaptUiFromState(i, Times.PauseEndDateTime - Times.PauseStartDateTime);
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
            PrgSwitch.IsTimeRemainingNotTimeWork = !PrgSwitch.IsTimeRemainingNotTimeWork;
            UpdateInfos();

        }

        private void btnModTimes_Click(object sender, RoutedEventArgs e)
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

                ClockUpdTimerOnTick(null, null);

                SaveCurrentDayTimes();


                PrgSwitch.IsMoreThanTpsTheo = false;
                PrgSwitch.IsTimerStoppedByMaxTime = false;
                PrgSwitch.PbarMainTimerActif = true;
            }
        }

        private void pbarTime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClockUpdTimerOnTick(null, null);
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

                bool wasMuted = false;
                bool wasDftDeviceChanged = false;
                double originalVolume = 0;
                IDevice originalDftDevice = null;


                originalDftDevice =
                    CoreAudioCtrler.GetPlaybackDevices()
                        .FirstOrDefault(r => r.FullName.Equals(CoreAudioCtrler.DefaultPlaybackDevice.FullName));
                _logger.Debug("OrigDftDevice: {0}", originalDftDevice.FullName);

                IDevice device =
                    CoreAudioCtrler.GetPlaybackDevices()
                        .FirstOrDefault(r => r.FullName.Equals(PrgOptions.SoundDeviceFullName)) ??
                    originalDftDevice;

                _logger.Debug("DeviceChoosed: {0}", device.FullName);

                if (!device.Equals(originalDftDevice))
                {
                    CoreAudioCtrler.SetDefaultDevice(device);
                    wasDftDeviceChanged = true;
                    _logger.Debug("2-OrigDftDevice: {0}", originalDftDevice.FullName);
                }

                if (device.IsMuted)
                {
                    wasMuted = true;
                    device.Mute(false);
                }

                originalVolume = device.Volume;
                device.Volume = PrgOptions.SoundPlayedAtLockMidiVolume;


                Action<Task> stepAction = delegate
                {
                    _logger.Debug("Action Sound");
                    PrgOptions.SoundPlayedAtLockMidi.Play();

                };
                Action<Task> finalAction = delegate
                {
                    MiscAppUtils.Delay(1200).ContinueWith(delegate
                    {
                        device.Volume = originalVolume;
                        if (wasMuted)
                        {
                            device.Mute(true);
                        }

                        if (wasDftDeviceChanged)
                        {

                            CoreAudioCtrler.SetDefaultDevice(originalDftDevice);
                            _logger.Debug("3-OrigDftDevice: {0}", originalDftDevice.FullName);

                            _logger.Debug("Real DftDevice: {0}", CoreAudioCtrler.DefaultPlaybackDevice.FullName);

                        }

                        PrgSwitch.IsSoundOver = true;
                    });
                };

                MiscAppUtils.RecDelayAction(stepAction, 1, 300, finalAction);

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
    }
}
