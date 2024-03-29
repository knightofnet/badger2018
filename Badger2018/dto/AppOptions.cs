﻿using System;
using System.Reflection;
using Badger2018.constants;
using BadgerCommonLibrary.constants;

namespace Badger2018.dto
{
    public class AppOptions
    {
        private bool _isGlobalShowNotifications;
        private bool _isAutoBadgeAtStart;
        private bool _isAutoBadgeMeridienne;

        public bool IsFirstRun { get; set; }

        public bool IsConsentUse { get; set; }

        /// <summary>
        /// Evenement en cas de changement de IsAutoBadgeAtStart
        /// </summary>
        public Action<bool> OnAutoBadgeAtStartChange;
        public bool IsAutoBadgeAtStart
        {
            get { return _isAutoBadgeAtStart; }
            set
            {
                _isAutoBadgeAtStart = value;
                if (OnAutoBadgeAtStartChange != null)
                {
                    OnAutoBadgeAtStartChange(value);
                }
            }
        }


        public String Uri { get; set; }

        public String UriParam { get; set; }

        public String UriVerif { get; set; }

        public TimeSpan HeureMinJournee { get; set; }
        public TimeSpan HeureMaxJournee { get; set; }
        public TimeSpan TempsMaxJournee { get; set; }
        public TimeSpan TempsMaxDemieJournee { get; set; }
        public TimeSpan TempsDemieJournee { get; set; }
        public TimeSpan TempsMinPause { get; set; }

        public TimeSpan PlageFixeMatinStart { get; set; }
        public TimeSpan PlageFixeMatinFin { get; set; }
        public TimeSpan PlageFixeApremStart { get; set; }
        public TimeSpan PlageFixeApremFin { get; set; }

        public bool IsUseGeckoDebug { get; set; }

        /// <summary>
        /// Mode de badge : 0 par validation formulaire, 1 par click sur elementHtml;
        /// </summary>
        public EnumModePointage ModeBadgement { get; set; }

        public bool TemptBlockShutdown { get; set; }

        public EnumActionButtonClose ActionButtonClose { get; set; }

        public bool IsBtnManuelBadgeIsWithHotKeys { get; set; }

        public String FfExePath { get; set; }

        public EnumBrowser BrowserIndex { get; set; }

        /// <summary>
        /// Evenement en cas de changement de valeur IsGlobalShowNotifications
        /// </summary>
        public Action<bool> OnGlobalShowNotificationsChange;
        public bool IsGlobalShowNotifications
        {
            get { return _isGlobalShowNotifications; }
            set
            {
                _isGlobalShowNotifications = value;
                if (OnGlobalShowNotificationsChange != null)
                {
                    OnGlobalShowNotificationsChange(value);
                }
            }
        }



        public bool ShowNotifEndPfMatin { get; set; }
        public bool ShowNotifEndPfAprem { get; set; }
        public bool ShowNotifEndPause { get; set; }
        public bool ShowNotifEndTheo { get; set; }
        public bool ShowNotifWhenSessUnlockAfterMidi { get; set; }
        public bool ShowNotifEndMoyMatin { get; set; }
        public bool ShowNotifEndMoyAprem { get; set; }




        public bool IsUseAlternateNotification { get; set; }

        /// <summary>
        /// Evenement en cas de changement de IsAutoBadgeAtStart
        /// </summary>
        public Action<bool> OnAutoBadgeMeridienneChange;
        public bool IsAutoBadgeMeridienne
        {
            get { return _isAutoBadgeMeridienne; }
            set
            {
                _isAutoBadgeMeridienne = value;
                if (OnAutoBadgeMeridienneChange != null)
                {
                    OnAutoBadgeMeridienneChange(value);
                }
            }
        }

        public int DeltaAutoBadgeageMinute { get; set; }

        public bool IsLastBadgeIsAutoShutdown { get; set; }

        public bool IsDailyDisableAutoBadgeMerid { get; set; }

        public int LastBadgeDelay { get; set; }

        public bool IsStopCptAtMax { get; set; }

      

        public bool IsStopCptAtMaxDemieJournee { get; set; }

        public bool IsAdd5minCpt { get; set; }

        public String UrlMesPointages { get; set; }
        public String UrlSirhius { get; set; }
        public String UrlCptTpsReel { get; set; }

        public bool IsPlaySoundAtLockMidi { get; set; }

        public EnumSonWindows SoundPlayedAtLockMidi { get; set; }
        public String SoundDeviceFullName { get; set; }
        public String UpdateXmlUri { get; set; }
        public int TipsLastInt { get; set; }
        public bool ShowTipsAtStart { get; set; }
        public int SoundPlayedAtLockMidiVolume { get; set; }
        public EnumActionBtnBadgeM TypeBadgeageBtnM { get; set; }

        public bool IsRemoveLegacyShorcutFirefox { get; set; }
        public CustomNotificationDto Notif1Obj { get; set; }
        public CustomNotificationDto Notif2Obj { get; set; }

        public TimeSpan LastCdSeen { get; set; }
        public TimeSpan LastCdMondaySeen { get; set; }

        public TimeSpan CompteurCDMaxAbs { get; set; }

        public string SqliteAppUserSalt { get; set; }
        public string LastSqlUpdateVersion { get; set; }

        public bool ShowOnScreenProgressBar { get; set; }
        public bool IsAutoBadgeAtStartDelayed { get; internal set; }

        public bool IsPreloadFF { get; internal set; }
        public int CptCtrlStateShowned { get; set; }
        public int WaitBeforeClickBadger { get; set; }
        public bool IsUpdateSvcEnable { get; set; }
        public int NoConnexionTimeout { get; set; }
        public EnumBadgeageZeroAction BadgeageZeroAction { get; set; }
        public int BadgeageDefaultTimeout { get; set; }
        public int BadgeageTimeoutWaitAfterPost { get; set; }
        public int BadgeageNbTentativeVerif { get; set; }

        public bool IsCheckCDLastDay { get; set; }

        public bool IsCanAskForTT { get; set; }
        public int LastWeekNbrTtChecked { get; set; }
        public void ResetSpecOption()
        {
            IsAutoBadgeMeridienne = false;
            IsDailyDisableAutoBadgeMerid = true;
            LastBadgeDelay = 0;
            DeltaAutoBadgeageMinute = 0;
        }

        internal AppOptions Clone()
        {
            AppOptions newOptions = new AppOptions();
            foreach (PropertyInfo pInfo in this.GetType().GetProperties())
            {

                EnumAppOptions eApp = EnumAppOptions.GetEnumFromOptName(pInfo.Name);
                if (eApp != null)
                {
                    pInfo.SetValue(newOptions, pInfo.GetValue(this, new object[] { }), null);
                }
                else
                {
                    // _logger.Warn("La propriétée {0} n'existe pas dans EnumAppOptions.", pInfo.Name);
                }

            }

            return newOptions;
        }





    }
}
