using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;
using BadgerCommonLibrary.constants;
using BadgerPluginExtender.dto;
using IWshRuntimeLibrary;
using CheckBox = System.Windows.Controls.CheckBox;
using File = System.IO.File;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour Options.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        private const string LblPasDeControle = "Pas de contrôle";
        private const string lblNoConnexionTimeoutInfini = "Infini";
        private static readonly Logger _logger = Logger.LastLoggerInstance;
        public MainWindow Pwin { get; set; }
        public bool HasChangeOption { get; private set; }
        public AppOptions NewOptions { get; private set; }

        public AppOptions OrigOptions { get; private set; }
        public bool IsRazNotifs { get; private set; }
        private bool IsSpecUse { get; set; }

        private NotifyIcon NotifyIcon { get; set; }


        private OptionsCtxtHelpView c = null;

        public OptionsView(AppOptions appOptions, bool isSpecUse, NotifyIcon notifyIcon, bool specShowSpecOpt, MainWindow parentWindow)
        {
            InitializeComponent();
            parentWindow.PluginMgr.PlayHook("OnOptionsViewInit", new object[] { tabCtrl });

            btnEditCustomNotifs.Visibility = Visibility.Collapsed;
            imgBtnHelp.Visibility = Visibility.Collapsed;

            OrigOptions = appOptions;

            KeyUp += (sender, args) =>
            {
                if (args.Key == Key.F1 && (c == null || !c.IsLoaded))
                {
                    c = new OptionsCtxtHelpView(Path.GetFullPath("./Resources/help.htm"));
                    c.Show();
                }
            };

            tabCtrl.SelectionChanged += delegate (object sender, SelectionChangedEventArgs args)
            {
                if (c != null && tabCtrl.SelectedItem != null)
                {
                    c.GoToAnchor(this.Name, ((TabItem)tabCtrl.SelectedItem).Header.ToString());
                }
            };

            Closing += (sender, args) =>
            {
                if (c != null)
                    c.Close();
            };

            Pwin = parentWindow;

            NotifyIcon = notifyIcon;

            IsSpecUse = isSpecUse;
            tabSpec.Visibility = isSpecUse && specShowSpecOpt ? Visibility.Visible : Visibility.Collapsed;

            foreach (EnumModePointage enumModeP in EnumModePointage.Values)
            {
                cboxMode.Items.Add(enumModeP.Libelle);
            }

            foreach (EnumBrowser enumBrowser in EnumBrowser.Values)
            {
                cboxListBrowser.Items.Add(enumBrowser.Libelle);
            }
            cboxListBrowser.SelectedItem = EnumBrowser.FF.Libelle;

            foreach (EnumBadgeageZeroAction enumB0Action in EnumBadgeageZeroAction.Values)
            {
                cboxB0AskUser.Items.Add(enumB0Action.Libelle);
            }
            cboxB0AskUser.SelectedItem = EnumBadgeageZeroAction.NO_CHOICE.Libelle;

            
            foreach (EnumActionButtonClose enumAbOc in EnumActionButtonClose.Values)
            {
                cboxActionButtonClose.Items.Add(enumAbOc.Libelle);
            }

            foreach (EnumSonWindows enumSound in EnumSonWindows.Values.OrderBy(r => r.Libelle))
            {

                if (enumSound.IsSystemSound || (enumSound.IsWaveFile && enumSound.WaveFileInfo.Exists))
                {
                    cboxSonChoosed.Items.Add(enumSound.Libelle);
                }
            }

            cboxNoConnexionTimeout.Items.Add(LblPasDeControle);
            for (int i =  1; i <= 60; i++)
            {
                cboxNoConnexionTimeout.Items.Add(i);
            }
            cboxNoConnexionTimeout.Items.Add(lblNoConnexionTimeoutInfini);


            AsyncLoadListOfSoundDevices();


            for (int i = 0; i < 11; i++)
            {
                cboxWaitBeforeClick.Items.Add(i);
            }

            for (int i = 0; i < 6; i++)
            {
                cboxLastBadgeDelay.Items.Add(i);
            }

            for (int i = 0; i < 6; i++)
            {
                cboxDeltaAutoBadgeage.Items.Add(i);
            }

            NewOptions = appOptions.Clone();


            imgBtnHelp.Visibility = IsSpecUse ? Visibility.Visible : Visibility.Collapsed;


            LoadOptionsFrom(OrigOptions);

        }

        private void AsyncLoadListOfSoundDevices()
        {
            cboxSonDevice.Items.Add("En chargement...");
            cboxSonDevice.SelectedIndex = 0;
            cboxSonDevice.IsEnabled = false;
            cboxSonChoosed.IsEnabled = false;
            chkPlaySoundBeforePauseMidi.IsEnabled = false;

            Action<IList<String>> actionSuccess = listDevices =>
            {
                cboxSonDevice.Items.Clear();
                foreach (string device in listDevices)
                {
                    cboxSonDevice.Items.Add(device);
                }

                cboxSonDevice.SelectedIndex = 0;
                cboxSonDevice.IsEnabled = true;
                cboxSonChoosed.IsEnabled = true;
                chkPlaySoundBeforePauseMidi.IsEnabled = true;


                cboxSonDevice.SelectedItem = OrigOptions.SoundDeviceFullName;
            };

            Action<Exception> actionFail = (ex) =>
            {
                cboxSonDevice.Items.Clear();
                ExceptionMsgBoxView.ShowException(ex, null, "Une erreur s'est produite lors de la récupération des périphériques sonores de lecture.");
            };

            if (Pwin.CoreAudioFactory.ListOfSoundDevices.Count > 0)
            {
                actionSuccess.Invoke(Pwin.CoreAudioFactory.ListOfSoundDevices);
            }
            else
            {
                Pwin.CoreAudioFactory.AsyncLoadListOfSoundDevice(actionSuccess, actionFail);
            }

        }


        private void LoadOptionsFrom(AppOptions opt)
        {
            tboxPfMS.Text = opt.PlageFixeMatinStart.ToString(Cst.TimeSpanFormatWithH);
            tboxPfME.Text = opt.PlageFixeMatinFin.ToString(Cst.TimeSpanFormat);

            tboxPfAS.Text = opt.PlageFixeApremStart.ToString(Cst.TimeSpanFormat);
            tboxPfAE.Text = opt.PlageFixeApremFin.ToString(Cst.TimeSpanFormat);

            tboxPtmpsPause.Text = opt.TempsMinPause.ToString(Cst.TimeSpanFormat);
            tboxDayMaxTpsTime.Text = opt.TempsMaxJournee.ToString(Cst.TimeSpanFormat);
            tboxMaxTravTimeDemi.Text = opt.TempsMaxDemieJournee.ToString(Cst.TimeSpanFormat);
            tboxMinHourTime.Text = opt.HeureMinJournee.ToString(Cst.TimeSpanFormat);
            tboxMaxHourTime.Text = opt.HeureMaxJournee.ToString(Cst.TimeSpanFormat);
            tboxTpsReglementaireDemieJournee.Text = opt.TempsDemieJournee.ToString(Cst.TimeSpanFormat);


            cboxMode.SelectedItem = opt.ModeBadgement.Libelle;
            cboxListBrowser.SelectedItem = opt.BrowserIndex.Libelle;
            tboxExecFf.Text = opt.FfExePath;

            tboxUrl.Text = opt.Uri;
            tboxIdForm.Text = opt.UriParam;
            tboxUriVerif.Text = opt.UriVerif;

            cboxBlockShutdown.IsChecked = opt.TemptBlockShutdown;
            cboxAutoBadgeAtStart.IsChecked = opt.IsAutoBadgeAtStart;

            cboxWaitBeforeClick.SelectedItem = opt.WaitBeforeClickBadger;
            cboxB0AskUser.SelectedItem = opt.BadgeageZeroAction.Libelle;

            cboxActionButtonClose.SelectedItem = opt.ActionButtonClose.Libelle;
            cboxBtnManuelBadgeIsWithHotKeys.IsChecked = opt.IsBtnManuelBadgeIsWithHotKeys;

            chkGlobalShowNotifications.IsChecked = opt.IsGlobalShowNotifications;

            chkShowNotifEndPfMatin.IsChecked = opt.ShowNotifEndPfMatin;
            chkShowNotifEndPfAprem.IsChecked = opt.ShowNotifEndPfAprem;
            chkShowNotifEndPause.IsChecked = opt.ShowNotifEndPause;
            chkShowNotifEndTheo.IsChecked = opt.ShowNotifEndTheo;
            chkShowNotifAfterUnlockMidi.IsChecked = opt.ShowNotifWhenSessUnlockAfterMidi;
            chkShowNotifEndMoyMatin.IsChecked = opt.ShowNotifEndMoyMatin;
            chkShowNotifEndMoyAprem.IsChecked = opt.ShowNotifEndMoyAprem;

            chkboxAlternateNotifs.IsChecked = opt.IsUseAlternateNotification;

            chkIsPreloadFF.IsChecked = opt.IsPreloadFF;

            chkStopAfterMaxTravTime.IsChecked = opt.IsStopCptAtMax;
            chkStopAfterMaxTravTimeJournee.IsChecked = opt.IsStopCptAtMaxDemieJournee;
            chkCount5minAdded.IsChecked = opt.IsAdd5minCpt;

            chkPlaySoundBeforePauseMidi.IsChecked = opt.IsPlaySoundAtLockMidi;
            cboxSonChoosed.SelectedItem = opt.SoundPlayedAtLockMidi.Libelle;
            //cboxSonDevice.SelectedItem = opt.SoundDeviceFullName;


            if (StringUtils.IsNullOrWhiteSpace(opt.SoundDeviceFullName))
            {
                cboxSonDevice.SelectedIndex = 0;
            }
            sliderVolume.Value = opt.SoundPlayedAtLockMidiVolume;

            chkAutoShutdown.IsChecked = opt.IsLastBadgeIsAutoShutdown;
            chkRemoveLegacyShorcutFirefox.IsChecked = opt.IsRemoveLegacyShorcutFirefox;


            chkShowScreenBar.IsChecked = opt.ShowOnScreenProgressBar;

            //Spec
            if (IsSpecUse)
            {
                chkAutoBadgeMerid.IsChecked = opt.IsAutoBadgeMeridienne;

                chkDailyDisableAutoBadgeMerid.IsChecked = opt.IsDailyDisableAutoBadgeMerid;

                cboxLastBadgeDelay.SelectedItem = opt.LastBadgeDelay / 60;

                cboxDeltaAutoBadgeage.SelectedItem = opt.DeltaAutoBadgeageMinute / 60;

            }
            chkStopAfterMaxTravTime.Visibility = IsSpecUse ? Visibility.Visible : Visibility.Hidden;

            if (opt.NoConnexionTimeout == 0)
            {
                cboxNoConnexionTimeout.SelectedItem = LblPasDeControle;
            } else if (opt.NoConnexionTimeout >= opt.TempsDemieJournee.TotalSeconds)
            {
                cboxNoConnexionTimeout.SelectedItem = lblNoConnexionTimeoutInfini;
            }
            else
            {
                cboxNoConnexionTimeout.SelectedItem = opt.NoConnexionTimeout;
            }

            LoadLblNotif();
        }

        private void LoadLblNotif()
        {

            lblDescNotifFEndPfMatin.Content = String.Format(ConvertTxtTplLbl(lblDescNotifFEndPfMatin), NewOptions.PlageFixeMatinFin.ToString(Cst.TimeSpanFormatWithH));
            lblDescNotifFEndMoyMatin.Text = String.Format(ConvertTxtTplLbl(lblDescNotifFEndMoyMatin), Pwin.Times.EndMoyPfMatin.ToString(Cst.TimeSpanFormatWithH));
            lblDescNotifEndPause.Text = String.Format(ConvertTxtTplLbl(lblDescNotifEndPause), MiscAppUtils.TimeSpanShortStrFormat(NewOptions.TempsMinPause));
            lblDescNotifEndPfAprem.Content = String.Format(ConvertTxtTplLbl(lblDescNotifEndPfAprem),
                NewOptions.PlageFixeApremFin.ToString(Cst.TimeSpanFormatWithH));
            lblDescNotifEndMoyAprem.Text = String.Format(ConvertTxtTplLbl(lblDescNotifEndMoyAprem), Pwin.Times.EndMoyPfAprem.ToString(Cst.TimeSpanFormatWithH));
            lblDescNotifEndTheo.Text = String.Format(ConvertTxtTplLbl(lblDescNotifEndTheo), (NewOptions.TempsDemieJournee + NewOptions.TempsDemieJournee).ToString(Cst.TimeSpanFormatWithH));
        }

        private string ConvertTxtTplLbl(object lblOrAccessText)
        {
            String text = "";
            if (lblOrAccessText == null) return text;

            if (lblOrAccessText is Label)
            {
                text = (string)((Label)lblOrAccessText).Content;
            }
            else if (lblOrAccessText is AccessText)
            {
                text = ((AccessText)lblOrAccessText).Text;
            }

            String ret = text.Replace("XXhXX", "{0}");
            ret = ret.Replace("XXX", "{0}");

            return ret;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            tabCtrl.SelectedItem = tabNotifs;
            if (VerifDatasOngletNotification()) return;

            tabCtrl.SelectedItem = tabBadgeage;
            if (VerifDatasOngletBadgeage()) return;

            tabCtrl.SelectedItem = tabRappels;
            if (VerifDatasOngletRappels()) return;

            tabCtrl.SelectedItem = tabDivers;
            if (VerifDatasOngletDivers()) return;

            tabCtrl.SelectedItem = tabHoraire;
            if (VerifDatasOngletHeure()) return;

            if (IsSpecUse)
            {
                tabCtrl.SelectedItem = tabSpec;
                if (VerifDatasOngletSpecial()) return;
            }

            HookReturns hookReturns = Pwin.PluginMgr.PlayHookAndReturn("OnOptionsValidOptions", new object[] { }, typeof(bool));
            if (hookReturns.ListReturns.Any(r => ((bool)r.ReturnedObject) == false))
            {
                return;
            }


            PostActions();


            Pwin.PluginMgr.PlayHook("OnSaveOptions", new object[] { });
            Close();

        }



        private bool VerifDatasOngletRappels()
        {
            // Option tentative blocage arrêt
            if (cboxBlockShutdown.IsChecked != null &&
                cboxBlockShutdown.IsChecked.Value != OrigOptions.TemptBlockShutdown)
            {
                HasChangeOption = true;
                NewOptions.TemptBlockShutdown = cboxBlockShutdown.IsChecked.Value;
            }

            // Option ShowNotifWhenSessUnlockAfterMidi
            if (chkShowNotifAfterUnlockMidi.IsChecked != null &&
                chkShowNotifAfterUnlockMidi.IsChecked.Value != OrigOptions.ShowNotifWhenSessUnlockAfterMidi)
            {
                HasChangeOption = true;
                NewOptions.ShowNotifWhenSessUnlockAfterMidi = chkShowNotifAfterUnlockMidi.IsChecked.Value;
            }

            // Option ShowNotifWhenSessUnlockAfterMidi
            if (chkPlaySoundBeforePauseMidi.IsChecked != null &&
                chkPlaySoundBeforePauseMidi.IsChecked.Value != OrigOptions.IsPlaySoundAtLockMidi)
            {
                HasChangeOption = true;
                NewOptions.IsPlaySoundAtLockMidi = chkPlaySoundBeforePauseMidi.IsChecked.Value;
            }

            // SoundPlayedAtLockMidi
            String soundAtMidi = cboxSonChoosed.SelectedItem as string;
            if (soundAtMidi != null && !soundAtMidi.Equals(OrigOptions.SoundPlayedAtLockMidi.Libelle))
            {
                HasChangeOption = true;
                EnumSonWindows son = EnumSonWindows.GetFromLibelle(soundAtMidi);
                NewOptions.SoundPlayedAtLockMidi = son;
            }

            // SoundDeviceFullName
            String soundDevice = cboxSonDevice.SelectedItem as string;
            if (soundDevice != null && !soundDevice.Equals(OrigOptions.SoundDeviceFullName))
            {
                HasChangeOption = true;
                NewOptions.SoundDeviceFullName = soundDevice;
            }

            // SoundPlayedAtLockMidiVolume
            int soundVol = (int)sliderVolume.Value;
            if (soundVol != OrigOptions.SoundPlayedAtLockMidiVolume)
            {
                HasChangeOption = true;
                NewOptions.SoundPlayedAtLockMidiVolume = soundVol;
            }

            return false;

        }

        private bool VerifDatasOngletSpecial()
        {
            // Option IsAutoBadgeMeridienne
            CheckBox chkBox = chkAutoBadgeMerid;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.IsAutoBadgeMeridienne)
            {
                HasChangeOption = true;
                NewOptions.IsAutoBadgeMeridienne = chkBox.IsChecked.Value;
            }


            // Option IsDailyDisableAutoBadgeMerid
            chkBox = chkDailyDisableAutoBadgeMerid;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.IsDailyDisableAutoBadgeMerid)
            {
                HasChangeOption = true;
                NewOptions.IsDailyDisableAutoBadgeMerid = chkBox.IsChecked.Value;
            }

            // Delta auto-badgeage
            int selValueD = (int)cboxDeltaAutoBadgeage.SelectedItem;
            if ((selValueD * 60) != OrigOptions.DeltaAutoBadgeageMinute)
            {
                HasChangeOption = true;
                NewOptions.DeltaAutoBadgeageMinute = selValueD * 60;
            }





            // Ts Lastdelay 
            int selValue = (int)cboxLastBadgeDelay.SelectedItem;

            if ((selValue * 60) != OrigOptions.LastBadgeDelay)
            {
                HasChangeOption = true;
                NewOptions.LastBadgeDelay = selValue * 60;
            }



            return false;
        }


        private bool VerifDatasOngletNotification()
        {
            // Option globalShowNotifs
            CheckBox chkBox = chkGlobalShowNotifications;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.IsGlobalShowNotifications)
            {
                HasChangeOption = true;
                NewOptions.IsGlobalShowNotifications = chkBox.IsChecked.Value;
            }


            // Option notif end pf matin
            chkBox = chkShowNotifEndPfMatin;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.ShowNotifEndPfMatin)
            {
                HasChangeOption = true;
                NewOptions.ShowNotifEndPfMatin = chkBox.IsChecked.Value;
            }

            // Option notif end pf aprem
            chkBox = chkShowNotifEndPfAprem;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.ShowNotifEndPfAprem)
            {
                HasChangeOption = true;
                NewOptions.ShowNotifEndPfAprem = chkBox.IsChecked.Value;
            }

            // Option notif end pause
            chkBox = chkShowNotifEndPause;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.ShowNotifEndPause)
            {
                HasChangeOption = true;
                NewOptions.ShowNotifEndPause = chkBox.IsChecked.Value;
            }

            // Option notif end theo
            chkBox = chkShowNotifEndTheo;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.ShowNotifEndTheo)
            {
                HasChangeOption = true;
                NewOptions.ShowNotifEndTheo = chkBox.IsChecked.Value;
            }

            // Option ShowNotifEndMoyMatin
            chkBox = chkShowNotifEndMoyMatin;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.ShowNotifEndMoyMatin)
            {
                HasChangeOption = true;
                NewOptions.ShowNotifEndMoyMatin = chkBox.IsChecked.Value;
            }

            // Option ShowNotifEndMoyAprem
            chkBox = chkShowNotifEndMoyAprem;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.ShowNotifEndMoyAprem)
            {
                HasChangeOption = true;
                NewOptions.ShowNotifEndMoyAprem = chkBox.IsChecked.Value;
            }


            return false;
        }

        private bool VerifDatasOngletBadgeage()
        {
            // Mode badgeage
            String modeBadgeSeleted = cboxMode.SelectedItem as string;
            if (modeBadgeSeleted != null && !modeBadgeSeleted.Equals(OrigOptions.ModeBadgement.Libelle))
            {
                HasChangeOption = true;
                EnumModePointage mode = EnumModePointage.GetFromLibelle(modeBadgeSeleted);
                NewOptions.ModeBadgement = mode;
            }

            // Mode navigateur
            String browserSeleted = cboxListBrowser.SelectedItem as string;
            if (browserSeleted != null && !browserSeleted.Equals(OrigOptions.BrowserIndex.Libelle))
            {
                HasChangeOption = true;
                EnumBrowser browser = EnumBrowser.GetFromLibelle(browserSeleted);
                NewOptions.BrowserIndex = browser;
            }

            // Network adapter
            int noConnexionOutNewVal = 0;
            if (cboxNoConnexionTimeout.SelectedItem is string)
            {
                string valStr = cboxNoConnexionTimeout.SelectedItem as string;
                if (LblPasDeControle.Equals(valStr))
                {
                    noConnexionOutNewVal = 0;
                } else if (lblNoConnexionTimeoutInfini.Equals(valStr))
                {
                    noConnexionOutNewVal = (int) NewOptions.TempsMaxJournee.TotalSeconds;
                }
            } else if (cboxNoConnexionTimeout.SelectedItem is int)
            {
                noConnexionOutNewVal = (int)cboxNoConnexionTimeout.SelectedItem;
            }
            if (noConnexionOutNewVal >= 0 && noConnexionOutNewVal != OrigOptions.NoConnexionTimeout)
            {
                HasChangeOption = true;
                NewOptions.NoConnexionTimeout = noConnexionOutNewVal;
            }

            // Exec FF
            if (NewOptions.BrowserIndex == EnumBrowser.FF)
            {
                String tempExecFf = tboxExecFf.Text;
                if (tempExecFf.EndsWith("\"") || tempExecFf.StartsWith("\""))
                {
                    tempExecFf = tempExecFf.Trim('\"');
                    tboxExecFf.Text = tempExecFf;
                }
                if (tempExecFf.IsEmpty() || !File.Exists(tempExecFf))
                {
                    MessageBox.Show("Le chemin vers l'exécutable FF n'est pas correct.");
                    tboxExecFf.Focus();
                    return true;
                }
                else
                {
                    HasChangeOption = true;
                    NewOptions.FfExePath = tempExecFf;
                }
            }


            // PreloadFF
            CheckBox chkBox = chkIsPreloadFF;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.IsPreloadFF)
            {
                HasChangeOption = true;
                NewOptions.IsPreloadFF = chkBox.IsChecked.Value;
            }

            // Uri
            String tboxUrlStr = tboxUrl.Text;
            if (tboxUrlStr == null || tboxUrlStr.IsEmpty())
            {
                MessageBox.Show("L'url vers la page de pointage ne doit pas être vide.");
                tboxUrl.Focus();
                return true;
            }
            else
            {
                HasChangeOption = true;
                NewOptions.Uri = tboxUrlStr;
            }

            // Uri param
            String tboxIdFormStr = tboxIdForm.Text;
            if (tboxIdFormStr == null || tboxIdFormStr.IsEmpty())
            {
                MessageBox.Show("L'id de l'élément sur la page de pointage ne doit pas être vide.");
                tboxIdForm.Focus();
                return true;
            }
            else
            {
                HasChangeOption = true;
                NewOptions.UriParam = tboxIdFormStr;
            }

            // Uri param verif
            tboxIdFormStr = tboxUriVerif.Text;
            if (tboxIdFormStr == null)
            {
                MessageBox.Show("L'id de l'élément HTML de vérification sur la page de pointage ne doit pas être vide.");
                tboxUriVerif.Focus();
                return true;
            }
            else
            {
                HasChangeOption = true;
                NewOptions.UriVerif = tboxIdFormStr;
            }

            // WaitBeforeClick
            int selValueD = (int)cboxWaitBeforeClick.SelectedItem;
            if (selValueD != OrigOptions.WaitBeforeClickBadger)
            {
                HasChangeOption = true;
                NewOptions.WaitBeforeClickBadger = selValueD;
            }

            // 
            string valChkB0AskUser = cboxB0AskUser.SelectedItem as string;
            if (valChkB0AskUser != null && !valChkB0AskUser.Equals(OrigOptions.BadgeageZeroAction.Libelle))
            {
                HasChangeOption = true;
                EnumBadgeageZeroAction b0AskUser = EnumBadgeageZeroAction.GetFromLibelle(valChkB0AskUser);
                NewOptions.BadgeageZeroAction = b0AskUser;
            }
          

            return false;
        }

        private bool VerifDatasOngletDivers()
        {
            // Action Button on close
            String actionButtonOnClose = cboxActionButtonClose.SelectedItem as string;
            if (actionButtonOnClose != null && !actionButtonOnClose.Equals(OrigOptions.ActionButtonClose.Libelle))
            {
                HasChangeOption = true;
                EnumActionButtonClose actionButton = EnumActionButtonClose.GetFromLibelle(actionButtonOnClose);
                NewOptions.ActionButtonClose = actionButton;
            }

            // Option auto badge at start
            if (cboxAutoBadgeAtStart.IsChecked != null &&
                cboxAutoBadgeAtStart.IsChecked.Value != OrigOptions.IsAutoBadgeAtStart)
            {
                HasChangeOption = true;
                NewOptions.IsAutoBadgeAtStart = cboxAutoBadgeAtStart.IsChecked.Value;
            }



            // Option IsBtnManuelBadgeIsWithHotKeys
            if (cboxBtnManuelBadgeIsWithHotKeys.IsChecked != null &&
                cboxBtnManuelBadgeIsWithHotKeys.IsChecked.Value != OrigOptions.IsBtnManuelBadgeIsWithHotKeys)
            {
                HasChangeOption = true;
                NewOptions.IsBtnManuelBadgeIsWithHotKeys = cboxBtnManuelBadgeIsWithHotKeys.IsChecked.Value;
            }

            // Option IsUseAlternateNotification
            if (chkboxAlternateNotifs.IsChecked != null &&
                chkboxAlternateNotifs.IsChecked.Value != OrigOptions.IsUseAlternateNotification)
            {
                HasChangeOption = true;
                NewOptions.IsUseAlternateNotification = chkboxAlternateNotifs.IsChecked.Value;
            }

            // Option IsLastBadgeIsAutoShutdown
            CheckBox chkBox = chkAutoShutdown;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.IsLastBadgeIsAutoShutdown)
            {
                HasChangeOption = true;
                NewOptions.IsLastBadgeIsAutoShutdown = chkBox.IsChecked.Value;
            }

            // Option IsRemoveLegacyShorcutFirefox
            chkBox = chkRemoveLegacyShorcutFirefox;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.IsRemoveLegacyShorcutFirefox)
            {
                HasChangeOption = true;
                NewOptions.IsRemoveLegacyShorcutFirefox = chkBox.IsChecked.Value;
            }

            // Option ShowOnScreenProgressBar
            chkBox = chkShowScreenBar;
            if (chkBox.IsChecked != null &&
                chkBox.IsChecked.Value != OrigOptions.ShowOnScreenProgressBar)
            {
                HasChangeOption = true;
                NewOptions.ShowOnScreenProgressBar = chkBox.IsChecked.Value;
            }



            return false;
        }

        private bool VerifDatasOngletHeure()
        {

            // Option IsStopCptAtMax
            if (chkStopAfterMaxTravTime.IsChecked != null &&
                chkStopAfterMaxTravTime.IsChecked.Value != OrigOptions.IsStopCptAtMax)
            {
                HasChangeOption = true;
                NewOptions.IsStopCptAtMax = chkStopAfterMaxTravTime.IsChecked.Value;
            }

            // Option IsStopCptAtMaxDemieJournee
            if (chkStopAfterMaxTravTimeJournee.IsChecked != null &&
                chkStopAfterMaxTravTimeJournee.IsChecked.Value != OrigOptions.IsStopCptAtMaxDemieJournee)
            {
                HasChangeOption = true;
                NewOptions.IsStopCptAtMaxDemieJournee = chkStopAfterMaxTravTimeJournee.IsChecked.Value;
            }

            // Option IsAdd5minCpt
            if (chkCount5minAdded.IsChecked != null &&
                chkCount5minAdded.IsChecked.Value != OrigOptions.IsAdd5minCpt)
            {
                HasChangeOption = true;
                NewOptions.IsAdd5minCpt = chkCount5minAdded.IsChecked.Value;
            }


            /*
            * Vérification format HH:mm
            */

            // Plage fixe matin start
            TimeSpan newTboxPfMS = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxPfMS.Text, out newTboxPfMS))
            {
                if (!newTboxPfMS.Equals(OrigOptions.PlageFixeMatinStart))
                {
                    HasChangeOption = true;
                    NewOptions.PlageFixeMatinStart = newTboxPfMS;
                }
            }
            else
            {
                MessageBox.Show("L'heure de démarrage de la plage fixe du matin doit être au format HH:mm.");
                tboxPfMS.Focus();
                return true;
            }

            // Plage fixe matin fin
            TimeSpan newTboxPfME = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxPfME.Text, out newTboxPfME))
            {
                if (!newTboxPfME.Equals(OrigOptions.PlageFixeMatinFin))
                {
                    HasChangeOption = true;
                    NewOptions.PlageFixeMatinFin = newTboxPfME;
                }
            }
            else
            {
                MessageBox.Show("L'heure de fin de la plage fixe du matin doit être au format HH:mm.");
                tboxPfME.Focus();
                return true;
            }

            // Plage fixe aprem start
            TimeSpan newTboxPfAS = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxPfAS.Text, out newTboxPfAS))
            {
                if (!newTboxPfAS.Equals(OrigOptions.PlageFixeApremStart))
                {
                    HasChangeOption = true;
                    NewOptions.PlageFixeApremStart = newTboxPfAS;
                }
            }
            else
            {
                MessageBox.Show("L'heure de démarrage de la plage fixe de l'après-midi doit être au format HH:mm.");
                tboxPfAS.Focus();
                return true;
            }

            // Plage fixe aprem fin
            TimeSpan newTboxPfAE = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxPfAE.Text, out newTboxPfAE))
            {
                if (!newTboxPfAE.Equals(OrigOptions.PlageFixeApremFin))
                {
                    HasChangeOption = true;
                    NewOptions.PlageFixeApremFin = newTboxPfAE;
                }
            }
            else
            {
                MessageBox.Show("L'heure de fin de la plage fixe de l'après-midi doit être au format HH:mm.");
                tboxPfAE.Focus();
                return true;
            }

            // tps pause
            TimeSpan newTboxTpause = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxPtmpsPause.Text, out newTboxTpause))
            {
                if (!newTboxTpause.Equals(OrigOptions.TempsMinPause))
                {
                    HasChangeOption = true;
                    NewOptions.TempsMinPause = newTboxTpause;
                }
            }
            else
            {
                MessageBox.Show("Le temps de pause doit être au format HH:mm.");
                tboxPtmpsPause.Focus();
                return true;
            }

            // tps max trav
            TimeSpan newTboxTmax = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxDayMaxTpsTime.Text, out newTboxTmax))
            {
                if (!newTboxTmax.Equals(OrigOptions.TempsMaxJournee))
                {
                    HasChangeOption = true;
                    NewOptions.TempsMaxJournee = newTboxTmax;
                }
            }
            else
            {
                MessageBox.Show("Le temps maximum de travail doit être au format HH:mm.");
                tboxDayMaxTpsTime.Focus();
                return true;
            }

            // tps max trav demi J
            TimeSpan newTboxTmaxD = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxMaxTravTimeDemi.Text, out newTboxTmaxD))
            {
                if (!newTboxTmaxD.Equals(OrigOptions.TempsMaxDemieJournee))
                {
                    HasChangeOption = true;
                    NewOptions.TempsMaxDemieJournee = newTboxTmaxD;
                }
            }
            else
            {
                MessageBox.Show("Le temps maximum de travail sur une demie journée doit être au format HH:mm.");
                tboxMaxTravTimeDemi.Focus();
                return true;
            }

            // heure min trav
            TimeSpan newTboxTmin = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxMinHourTime.Text, out newTboxTmin))
            {
                if (newTboxTmin.CompareTo(NewOptions.PlageFixeMatinStart) >= 0)
                {
                    MessageBox.Show("L'horaire minimum pour commencer à travailler doit être inférieur au début de la plage fixe du matin.");
                    tboxMinHourTime.Focus();
                    return true;
                }
                else if (!newTboxTmin.Equals(OrigOptions.HeureMinJournee))
                {
                    HasChangeOption = true;
                    NewOptions.HeureMinJournee = newTboxTmin;
                }
            }
            else
            {
                MessageBox.Show("L'horaire minimum pour commencer à travailler doit être au format HH:mm.");
                tboxMinHourTime.Focus();
                return true;
            }

            // heure max trav sur une journée
            TimeSpan newTboxMaxHourTime = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxMaxHourTime.Text, out newTboxMaxHourTime))
            {
                if (newTboxMaxHourTime.CompareTo(NewOptions.PlageFixeApremFin) <= 0)
                {
                    MessageBox.Show("L'horaire maximum pour finir de travailler doit être supérieur à la fin de la plage fixe de l'après-midi.");
                    tboxMaxHourTime.Focus();
                    return true;
                }
                else if (!newTboxMaxHourTime.Equals(OrigOptions.HeureMaxJournee))
                {
                    HasChangeOption = true;
                    NewOptions.HeureMaxJournee = newTboxMaxHourTime;
                }
            }
            else
            {
                MessageBox.Show("L'horaire maximum pour finir de travailler doit être au format HH:mm.");
                tboxMaxHourTime.Focus();
                return true;
            }

            // tboxTpsReglementaireDemieJournee
            TimeSpan newTboxTpsRegl = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxTpsReglementaireDemieJournee.Text, out newTboxTpsRegl))
            {
                if (newTboxTpsRegl.CompareTo(NewOptions.TempsMaxDemieJournee) >= 0)
                {
                    MessageBox.Show("Le temps de travail réglementaire pour une demie journée doit être inférieur au temps maximum de tarvail pour une demie journée.");
                    tboxTpsReglementaireDemieJournee.Focus();
                    return true;
                }
                else if (!newTboxTpsRegl.Equals(OrigOptions.TempsDemieJournee))
                {
                    HasChangeOption = true;
                    NewOptions.TempsDemieJournee = newTboxTpsRegl;
                }
            }
            else
            {
                MessageBox.Show("Le temps de travail réglementaire pour une demie journée doit être au format HH:mm.");
                tboxTpsReglementaireDemieJournee.Focus();
                return true;
            }

            /*
             * Vérification cohérences
             */
            if (NewOptions.PlageFixeApremFin.CompareTo(NewOptions.PlageFixeApremStart) <= 0)
            {
                HasChangeOption = false;
                MessageBox.Show("L'heure de fin de la plage fixe de l'après-midi doit être supérieure à l'heure de début.");
                tboxPfAE.Focus();
                return true;
            }

            if (NewOptions.PlageFixeMatinFin.CompareTo(NewOptions.PlageFixeMatinStart) <= 0)
            {
                HasChangeOption = false;
                MessageBox.Show("L'heure de fin de la plage fixe du matin doit être supérieure à l'heure de début.");
                tboxPfME.Focus();
                return true;
            }
            return false;
        }

        private void PostActions()
        {
            AddOrRemoveStartupShortcut();
        }

        private void AddOrRemoveStartupShortcut()
        {
            if (NewOptions.IsAutoBadgeAtStart != OrigOptions.IsAutoBadgeAtStart)
            {
                List<IWshShortcut> listShortcut =
                    ShortcutUtils.GetShortcutsInDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Startup));

                if (NewOptions.IsAutoBadgeAtStart)
                {
                    if (listShortcut.Any(r => r.TargetPath.Contains(Assembly.GetExecutingAssembly().Location)))
                    {
                        return;
                    }

                    MessageBoxResult reponseMsg =
                        MessageBox.Show(
                            "L'option \"Badger au premier lancement du programme de la journée\" a été activée. " +
                            "Voulez-vous créer un raccourci vers Badger2018 dans le dossier des applications lancées au démarrage ?",
                            "Question",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                            );

                    if (reponseMsg == MessageBoxResult.Yes)
                    {
                        var newShortcut = ShortcutUtils.CreateShortcut("Badger2018",
                            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                            Assembly.GetExecutingAssembly().Location,
                            (new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName)
                            );

                        newShortcut.Description =
                            "Démarre l'application Badger2018. Si l'option \"Badger au premier lancement du programme de la journée\" a été activée, alors l'application badgera le premier badgeage du jour.";

                        newShortcut.Save();
                    }
                }
                else
                {
                    /*
                    if (!listShortcut.Any(r => r.TargetPath.Contains(Assembly.GetExecutingAssembly().Location)))
                    {
                        return;
                    }

                    MessageBoxResult reponseMsg =
                        MessageBox.Show(
                            "L'option \"Badger au premier lancement du programme de la journée\" a été désactivée. " +
                            "Voulez-vous supprimer le.s raccourci.s vers Badger2018 présent.s dans le dossier des applications lancées au démarrage ?",
                            "Question",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                            );

                    if (reponseMsg == MessageBoxResult.Yes)
                    {
                        foreach (
                            IWshShortcut shtcut in
                                listShortcut.Where(r => r.TargetPath.Contains(Assembly.GetExecutingAssembly().Location)))
                        {
                            File.Delete(shtcut.FullName);
                        }
                    }
                    */
                }
            }
        }

        private void btnRestoreDft_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult questionWarn = MessageBox.Show("Ceci va restaurer les paramètres à leurs valeurs initiales. " +
                            "L'Url de badgeage est volontairement fausse lorsqu'elle a sa valeur par défaut, " +
                            "afin d'éviter de mauvaises manipulations au premier lancement. " +
                            "Si vous aviez correctement configuré le badgeage, assurez-vous de sauvegarder les valeurs paramètrées." +
                            Environment.NewLine + Environment.NewLine +
                            "Cliquez sur Oui par réinitialiser les paramètres, Non pour annuler.", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (questionWarn != MessageBoxResult.Yes) return;


            NewOptions = OptionManager.GetDefaultOptionObj();
            HasChangeOption = NewOptions != null;
            Close();
        }

        private void btnResetNotifs_Click(object sender, RoutedEventArgs e)
        {
            IsRazNotifs = true;
        }

        private void btnTestNotifs_Click(object sender, RoutedEventArgs e)
        {

            Random rnd = new Random();
            int r = rnd.Next(0, 4);

            ToolTipIcon? ico = ToolTipIcon.None;
            switch (r)
            {
                case 0:
                    ico = ToolTipIcon.Info;
                    break;
                case 1:
                    ico = ToolTipIcon.Warning;
                    break;
                case 2:
                    ico = ToolTipIcon.Error;
                    break;
                default:
                    ico = null;
                    break;
            }

            MiscAppUtils.ShowNotificationBaloon(NotifyIcon, "Test notification", "Il s'agit d'un test de notification", null, 2000, ico, chkboxAlternateNotifs.IsChecked != null && chkboxAlternateNotifs.IsChecked.Value);
        }

        private void tboxUrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!tboxUrl.Text.IsEmpty())
            {
                MiscAppUtils.GoTo(tboxUrl.Text);
            }
        }

        private void tboxExecFf_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                List<FileInfo> listFiles = FileUtils.GetFilesRecursively(new DirectoryInfo("."));
                if (listFiles == null) return;

                FileInfo fi = listFiles.FirstOrDefault(r => r.Name.Equals("firefox.exe"));
                if (fi != null)
                {
                    tboxExecFf.Text = fi.FullName;
                    _logger.Info("Fichier firefox.exe trouvé à l'adresse suivante : {0}", fi.FullName);
                }
            }
            else
            {

                String str = tboxExecFf.Text;
                if (!str.IsEmpty() && File.Exists(str))
                {
                    FileUtils.ShowFileInWindowsExplorer(str);
                }
            }
        }

        private void cboxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String modeBadgeSeleted = cboxMode.SelectedItem as string;
            if (modeBadgeSeleted != null)
            {
                EnumModePointage mode = EnumModePointage.GetFromLibelle(modeBadgeSeleted);
                lblIdForm.Content = mode.UiLibelle;
            }



        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Pwin.BadgerWorker.TestNavigation(tboxUrl.Text, tboxIdForm.Text, tboxUriVerif.Text, chkIsPreloadFF.IsChecked ?? false);
        }

        private void btnExportConfig_Click(object sender, RoutedEventArgs e)
        {

            ImportExportOptionsView ieView = new ImportExportOptionsView(NewOptions);
            _logger.Debug("Ouverture de la fenêtre d'import/export");
            ieView.ShowDialog();

            if (!ieView.HasDoneAction)
            {
                return;
            }


            if (ieView.IsImportJob && ieView.OptionImported != null && ieView.HasDoneAction)
            {
                _logger.Debug(" Des options sont à importer");
                String filename = ieView.FileName;
                AppOptions opt = ieView.OptionImported;
                OptionManager.ChangeOptions(opt, NewOptions, filename);

                HasChangeOption = true;
            }

            Close();

        }

        private void btnPlaySoundAfterPgFixeMidi_Click(object sender, RoutedEventArgs e)
        {


            String soundChoosed = cboxSonChoosed.SelectedItem as String;
            String soundDevide = cboxSonDevice.SelectedItem as String;
            if (soundChoosed == null) return;

            EnumSonWindows sonChoosed = EnumSonWindows.GetFromLibelle(soundChoosed);
            PlayAdvertise(sonChoosed, soundDevide);
        }

        private void imgBtnHelp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (c == null || !c.IsLoaded)
            {
                c = new OptionsCtxtHelpView(Path.GetFullPath("./Resources/help.htm"));
                c.Show();
            }
        }



        private void PlayAdvertise(EnumSonWindows sonChoosed, String deviceFullName)
        {
            if (!Pwin.PrgSwitch.IsSoundOver)
            {
                _logger.Debug("Son en cours...");
                return;
            }

            Pwin.PrgSwitch.IsSoundOver = false;

            Action onSucessAction = () =>
            {
                Pwin.PrgSwitch.IsSoundOver = true;
            };
            Action<Exception> onFailAction = (ex) =>
            {
                Pwin.PrgSwitch.IsSoundOver = true;
                ExceptionMsgBoxView.ShowException(ex, null, "Une erreur s'est produite lors de la lecture du son.");
            };

            Pwin.CoreAudioFactory.AsyncPlaySound(sonChoosed, deviceFullName, (int)sliderVolume.Value, onSucessAction, onFailAction);




        }

        private void btnEditCustomNotifs_Click(object sender, RoutedEventArgs e)
        {
            CustomNotificationView v = new CustomNotificationView(NewOptions.Notif1Obj, NewOptions.Notif2Obj, NewOptions, Pwin.Times);
            v.ShowDialog();
            if (v.IsOkClose)
            {
                HasChangeOption = true;
            }
        }

        private void btnShowShortcutMgr_Click(object sender, RoutedEventArgs e)
        {
            CreateShortcutsView cs = new CreateShortcutsView();
            cs.ShowDialog();
        }
    }
}
