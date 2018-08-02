using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Badger2018.constants
{
    public sealed class EnumAppOptions
    {


        public static readonly EnumAppOptions TempsDemieJournee = new EnumAppOptions("TempsDemieJournee", typeof(TimeSpan), "03:54:00", false);
        public static readonly EnumAppOptions TempsMinPause = new EnumAppOptions("TempsMinPause", typeof(TimeSpan), "00:45:00", false);
        public static readonly EnumAppOptions TempsMaxJournee = new EnumAppOptions("TempsMaxJournee", typeof(TimeSpan), "09:30:00", false);
        public static readonly EnumAppOptions HeureMinJournee = new EnumAppOptions("HeureMinJournee", typeof(TimeSpan), "07:00:00", false);
        public static readonly EnumAppOptions TempsMaxDemieJournee = new EnumAppOptions("TempsMaxDemieJournee", typeof(TimeSpan), "05:30:00", false);
        public static readonly EnumAppOptions PlageFixeMatinStart = new EnumAppOptions("PlageFixeMatinStart", typeof(TimeSpan), "10:00:00", false);
        public static readonly EnumAppOptions PlageFixeMatinFin = new EnumAppOptions("PlageFixeMatinFin", typeof(TimeSpan), "11:30:00", false);
        public static readonly EnumAppOptions PlageFixeApremStart = new EnumAppOptions("PlageFixeApremStart", typeof(TimeSpan), "14:30:00", false);
        public static readonly EnumAppOptions PlageFixeApremFin = new EnumAppOptions("PlageFixeApremFin", typeof(TimeSpan), "15:30:00", false);
        public static readonly EnumAppOptions ModeBadgement = new EnumAppOptions("ModeBadgement", typeof(IEnumSerializableWithIndex<EnumModePointage>), "" + EnumModePointage.ELEMENT.GetIndex(), false);
        public static readonly EnumAppOptions TemptBlockShutdown = new EnumAppOptions("TemptBlockShutdown", typeof(bool), "True", false);
        public static readonly EnumAppOptions Uri = new EnumAppOptions("Uri", typeof(string), "https://www.google.com", false);
        public static readonly EnumAppOptions UriParam = new EnumAppOptions("UriParam", typeof(string), "tsf", false);
        public static readonly EnumAppOptions UriVerif = new EnumAppOptions("UriVerif", typeof(string), "", false);
        public static readonly EnumAppOptions FfExePath = new EnumAppOptions("FfExePath", typeof(string), @"ff\App\Firefox\firefox.exe", false);
        public static readonly EnumAppOptions IsUseGeckoDebug = new EnumAppOptions("IsUseGeckoDebug", typeof(bool), "False", false);
        public static readonly EnumAppOptions BrowserIndex = new EnumAppOptions("BrowserIndex", typeof(IEnumSerializableWithIndex<EnumBrowser>), "" + EnumBrowser.FF.GetIndex(), false);
        public static readonly EnumAppOptions IsAutoBadgeAtStart = new EnumAppOptions("IsAutoBadgeAtStart", typeof(bool), "False", false);
        public static readonly EnumAppOptions ActionButtonClose = new EnumAppOptions("ActionButtonClose", typeof(IEnumSerializableWithIndex<EnumActionButtonClose>), "" + EnumActionButtonClose.MINIMIZE.GetIndex(), false);
        public static readonly EnumAppOptions IsBtnManuelBadgeIsWithHotKeys = new EnumAppOptions("IsBtnManuelBadgeIsWithHotKeys", typeof(bool), "False", false);
        public static readonly EnumAppOptions IsGlobalShowNotifications = new EnumAppOptions("IsGlobalShowNotifications", typeof(bool), "True", false);
        public static readonly EnumAppOptions ShowNotifEndPfMatin = new EnumAppOptions("ShowNotifEndPfMatin", typeof(bool), "True", false);
        public static readonly EnumAppOptions ShowNotifEndPfAprem = new EnumAppOptions("ShowNotifEndPfAprem", typeof(bool), "True", false);
        public static readonly EnumAppOptions ShowNotifEndPause = new EnumAppOptions("ShowNotifEndPause", typeof(bool), "True", false);
        public static readonly EnumAppOptions ShowNotifEndTheo = new EnumAppOptions("ShowNotifEndTheo", typeof(bool), "True", false);
        public static readonly EnumAppOptions ShowNotifWhenSessUnlockAfterMidi = new EnumAppOptions("ShowNotifWhenSessUnlockAfterMidi", typeof(bool), "True", false);
        public static readonly EnumAppOptions IsNotif1Enabled = new EnumAppOptions("IsNotif1Enabled", typeof(bool), "False", false);
        public static readonly EnumAppOptions Notif1Time = new EnumAppOptions("Notif1Time", typeof(TimeSpan), "00:00:00", false);
        public static readonly EnumAppOptions Notif1Text = new EnumAppOptions("Notif1Text", typeof(string), "C'est l'heure !", false);
        public static readonly EnumAppOptions IsNotif2Enabled = new EnumAppOptions("IsNotif2Enabled", typeof(bool), "False", false);
        public static readonly EnumAppOptions Notif2Time = new EnumAppOptions("Notif2Time", typeof(TimeSpan), "00:00:00", false);
        public static readonly EnumAppOptions Notif2Text = new EnumAppOptions("Notif2Text", typeof(string), "C'est toujours l'heure !", false);
        public static readonly EnumAppOptions IsUseAlternateNotification = new EnumAppOptions("IsUseAlternateNotification", typeof(bool), "False", false);
        public static readonly EnumAppOptions IsFirstRun = new EnumAppOptions("IsFirstRun", typeof(bool), "True", false);
        public static readonly EnumAppOptions IsConsentUse = new EnumAppOptions("IsConsentUse", typeof(bool), "False", false);
        public static readonly EnumAppOptions IsAutoBadgeMeridienne = new EnumAppOptions("IsAutoBadgeMeridienne", typeof(bool), "False", true);
        public static readonly EnumAppOptions IsLastBadgeIsAutoShutdown = new EnumAppOptions("IsLastBadgeIsAutoShutdown", typeof(bool), "False", true);
        public static readonly EnumAppOptions LastBadgeDelay = new EnumAppOptions("LastBadgeDelay", typeof(int), "0", true);
        public static readonly EnumAppOptions DeltaAutoBadgeageMinute = new EnumAppOptions("DeltaAutoBadgeageMinute", typeof(int), "0", true);
        public static readonly EnumAppOptions IsDailyDisableAutoBadgeMerid = new EnumAppOptions("IsDailyDisableAutoBadgeMerid", typeof(bool), "True", true);
        public static readonly EnumAppOptions IsStopCptAtMax = new EnumAppOptions("IsStopCptAtMax", typeof(bool), "True", false);
        public static readonly EnumAppOptions IsAdd5MinCpt = new EnumAppOptions("IsAdd5minCpt", typeof(bool), "True", false);

        public static readonly EnumAppOptions UrlMesPointages = new EnumAppOptions("UrlMesPointages", typeof(string), "http://zvpw068.r12.an.cnav:7796/Pages/Espace%20RH/Mon%20Suivi%20RH/GTA/GTA_MesBageages.aspx?Rubrique=ERHMSRHMB", false);
        public static readonly EnumAppOptions UrlSirhius = new EnumAppOptions("UrlSirhius", typeof(string), "http://sirhius-r12.n06.an.cnav/hra-space/portal", false);
        public static readonly EnumAppOptions UrlCptTpsReel = new EnumAppOptions("UrlCptTpsReel", typeof(string), "http://zvpw068.r12.an.cnav:7796/Pages/Espace%20RH/Mon%20Suivi%20RH/GTA/GTA_TpsReel.aspx?Rubrique=ERHMSRHCT", false);


        public static readonly EnumAppOptions IsPlaySoundAtLockMidi = new EnumAppOptions("IsPlaySoundAtLockMidi", typeof(bool), "False", false);
        public static readonly EnumAppOptions SoundPlayedAtLockMidi = new EnumAppOptions("SoundPlayedAtLockMidi", typeof(IEnumSerializableWithIndex<EnumSonWindows>), "" + EnumSonWindows.Beep.GetIndex(), false);
        public static readonly EnumAppOptions SoundDeviceFullName = new EnumAppOptions("SoundDeviceFullName", typeof(string), "", false);
        public static readonly EnumAppOptions SoundPlayedAtLockMidiVolume = new EnumAppOptions("SoundPlayedAtLockMidiVolume", typeof(int), "75", false);

        public static readonly EnumAppOptions UpdateXmlUri = new EnumAppOptions("UpdateXmlUri", typeof(string), @"E:\CSharp\Données accessoires\Badger\update.xml", false);

        public static readonly EnumAppOptions TipsLastInt = new EnumAppOptions("TipsLastInt", typeof(int), "1", false);
        public static readonly EnumAppOptions ShowTipsAtStart = new EnumAppOptions("ShowTipsAtStart", typeof(bool), "False", false);

        public static IEnumerable<EnumAppOptions> Values
        {
            get
            {
                yield return TempsDemieJournee;
                yield return TempsMinPause;
                yield return TempsMaxJournee;
                yield return HeureMinJournee;
                yield return TempsMaxDemieJournee;
                yield return PlageFixeMatinStart;
                yield return PlageFixeMatinFin;
                yield return PlageFixeApremStart;
                yield return PlageFixeApremFin;
                yield return ModeBadgement;
                yield return TemptBlockShutdown;
                yield return Uri;
                yield return UriParam;
                yield return UriVerif;
                yield return FfExePath;
                yield return IsUseGeckoDebug;
                yield return BrowserIndex;
                yield return IsAutoBadgeAtStart;
                yield return ActionButtonClose;
                yield return IsBtnManuelBadgeIsWithHotKeys;
                yield return IsGlobalShowNotifications;
                yield return ShowNotifEndPfMatin;
                yield return ShowNotifEndPfAprem;
                yield return ShowNotifEndPause;
                yield return ShowNotifEndTheo;
                yield return ShowNotifWhenSessUnlockAfterMidi;
                yield return IsNotif1Enabled;
                yield return Notif1Time;
                yield return Notif1Text;
                yield return IsNotif2Enabled;
                yield return Notif2Time;
                yield return Notif2Text;
                yield return IsUseAlternateNotification;
                yield return IsFirstRun;
                yield return IsConsentUse;
                yield return IsAutoBadgeMeridienne;
                yield return IsLastBadgeIsAutoShutdown;
                yield return LastBadgeDelay;
                yield return DeltaAutoBadgeageMinute;
                yield return IsDailyDisableAutoBadgeMerid;
                yield return IsStopCptAtMax;
                yield return IsAdd5MinCpt;
                yield return UrlMesPointages;
                yield return UrlSirhius;
                yield return UrlCptTpsReel;
                yield return IsPlaySoundAtLockMidi;
                yield return SoundPlayedAtLockMidi;
                yield return SoundDeviceFullName;
                yield return SoundPlayedAtLockMidiVolume;
                yield return UpdateXmlUri;
                yield return TipsLastInt;
                yield return ShowTipsAtStart;


            }
        }

        public string Name { get; private set; }
        public Type UType { get; private set; }
        public String DefaultStrValue { get; private set; }
        public bool IsSpec { get; private set; }



        private EnumAppOptions(string optionName, Type type, string defaultStrValue, bool isSpec)
        {
            Name = optionName;
            UType = type;
            DefaultStrValue = defaultStrValue;
            IsSpec = isSpec;

        }

        public bool IsString()
        {
            return UType == typeof(String);
        }

        public bool IsBoolean()
        {
            return UType == typeof(bool);
        }

        public bool IsInt()
        {
            return UType == typeof(int);
        }

        public object GetObjDefaultValue()
        {
            if (UType == typeof(TimeSpan))
            {
                return TimeSpan.Parse(DefaultStrValue);
            }

            if (UType == typeof(String))
            {
                return DefaultStrValue;
            }

            if (UType == typeof(bool))
            {
                return Boolean.Parse(DefaultStrValue);
            }

            if (UType == typeof(int))
            {
                return Int16.Parse(DefaultStrValue);
            }

            if (UType.IsGenericType && UType.Name.Equals(typeof(IEnumSerializableWithIndex<>).Name) && UType.GetGenericArguments().Any())
            {
                return GetEnumFromIndex(Int16.Parse(DefaultStrValue), UType.GetGenericArguments()[0]);
            }

            return null;

        }

        public static object GetEnumFromIndex(int intEnumIndex, Type type)
        {
            //Type finalE = interfaceType.MakeGenericType(UType.GetGenericArguments()[0]);
            object obj =
               type.GetMethod("GetFromIndex", BindingFlags.Static | BindingFlags.Public)
                    .Invoke(null, new object[] { intEnumIndex });


            return obj;
        }

        public static int GetIndexFromEnum(object en)
        {
            if (en.GetType().GetInterfaces().Any(r => r.Name.Equals(typeof(IEnumSerializableWithIndex<>).Name)))
            {
                //Type finalE = interfaceType.MakeGenericType(UType.GetGenericArguments()[0]);
                int obj =
                   (int)en.GetType().GetMethod("GetIndex")
                       .Invoke(en, new object[] { });


                return obj;
            }

            return -1;


        }

        public static EnumAppOptions GetEnumFromOptName(string optionName)
        {
            return optionName == null ? null : Values.FirstOrDefault(enumModeP => enumModeP.Name == optionName);
        }
    }
}
