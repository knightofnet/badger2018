using AryxDevLibrary.utils.logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;

namespace Badger2018.constants
{
    public class EnumSonWindows : IEnumSerializableWithIndex<EnumSonWindows>
    {

        public static readonly EnumSonWindows Asterisk = new EnumSonWindows(0, "Asterisque", SystemSounds.Asterisk, null);
        public static readonly EnumSonWindows Beep = new EnumSonWindows(1, "Beep", SystemSounds.Beep, null);
        public static readonly EnumSonWindows Exclamation = new EnumSonWindows(2, "Exclamation", SystemSounds.Exclamation, null);
        public static readonly EnumSonWindows Hand = new EnumSonWindows(3, "Hand", SystemSounds.Hand, null);
        public static readonly EnumSonWindows Question = new EnumSonWindows(4, "Question", SystemSounds.Question, null);
        public static readonly EnumSonWindows Tada = new EnumSonWindows(5, "Tada", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\tada.wav"));

        public static readonly EnumSonWindows Alarm01 = new EnumSonWindows(6, "Alarme 01", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm01.wav"));
        public static readonly EnumSonWindows Alarm02 = new EnumSonWindows(7, "Alarme 02", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm02.wav"));
        public static readonly EnumSonWindows Alarm03 = new EnumSonWindows(8, "Alarme 03", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm03.wav"));
        public static readonly EnumSonWindows Alarm04 = new EnumSonWindows(9, "Alarme 04", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm04.wav"));
        public static readonly EnumSonWindows Alarm05 = new EnumSonWindows(10, "Alarme 05", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm05.wav"));
        public static readonly EnumSonWindows Alarm06 = new EnumSonWindows(11, "Alarme 06", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm06.wav"));
        public static readonly EnumSonWindows Alarm07 = new EnumSonWindows(12, "Alarme 07", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm07.wav"));
        public static readonly EnumSonWindows Alarm08 = new EnumSonWindows(13, "Alarme 08", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm08.wav"));
        public static readonly EnumSonWindows Alarm09 = new EnumSonWindows(14, "Alarme 09", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm09.wav"));
        public static readonly EnumSonWindows Alarm10 = new EnumSonWindows(15, "Alarme 10", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Alarm10.wav"));

        public static readonly EnumSonWindows Chimes = new EnumSonWindows(16, "chimes", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\chimes.wav"));
        public static readonly EnumSonWindows Chord = new EnumSonWindows(17, "chord", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\chord.wav"));
        public static readonly EnumSonWindows Ding = new EnumSonWindows(18, "ding", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\ding.wav"));
        public static readonly EnumSonWindows Ir_begin = new EnumSonWindows(19, "ir_begin", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\ir_begin.wav"));
        public static readonly EnumSonWindows Ir_end = new EnumSonWindows(20, "ir_end", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\ir_end.wav"));
        public static readonly EnumSonWindows Ir_inter = new EnumSonWindows(21, "ir_inter", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\ir_inter.wav"));
        public static readonly EnumSonWindows Notify = new EnumSonWindows(22, "notify", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\notify.wav"));
        public static readonly EnumSonWindows Recycle = new EnumSonWindows(23, "recycle", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\recycle.wav"));
        public static readonly EnumSonWindows Ring01 = new EnumSonWindows(24, "Ring01", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring01.wav"));
        public static readonly EnumSonWindows Ring02 = new EnumSonWindows(25, "Ring02", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring02.wav"));
        public static readonly EnumSonWindows Ring03 = new EnumSonWindows(26, "Ring03", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring03.wav"));
        public static readonly EnumSonWindows Ring04 = new EnumSonWindows(27, "Ring04", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring04.wav"));
        public static readonly EnumSonWindows Ring05 = new EnumSonWindows(29, "Ring05", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring05.wav"));
        public static readonly EnumSonWindows Ring06 = new EnumSonWindows(30, "Ring06", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring06.wav"));
        public static readonly EnumSonWindows Ring07 = new EnumSonWindows(31, "Ring07", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring07.wav"));
        public static readonly EnumSonWindows Ring08 = new EnumSonWindows(32, "Ring08", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring08.wav"));
        public static readonly EnumSonWindows Ring09 = new EnumSonWindows(33, "Ring09", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring09.wav"));
        public static readonly EnumSonWindows Ring10 = new EnumSonWindows(34, "Ring10", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Ring10.wav"));
        public static readonly EnumSonWindows Ringout = new EnumSonWindows(35, "ringout", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\ringout.wav"));
        public static readonly EnumSonWindows SpeechDisambiguation = new EnumSonWindows(36, "Speech Disambiguation", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Speech Disambiguation.wav"));
        public static readonly EnumSonWindows SpeechMisrecognition = new EnumSonWindows(37, "Speech Misrecognition", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Speech Misrecognition.wav"));
        public static readonly EnumSonWindows SpeechOff = new EnumSonWindows(38, "Speech Off", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Speech Off.wav"));
        public static readonly EnumSonWindows SpeechOn = new EnumSonWindows(39, "Speech On", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Speech On.wav"));
        public static readonly EnumSonWindows SpeechSleep = new EnumSonWindows(40, "Speech Sleep", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Speech Sleep.wav"));
        public static readonly EnumSonWindows WindowsBackground = new EnumSonWindows(41, "Windows Background", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Background.wav"));
        public static readonly EnumSonWindows WindowsBalloon = new EnumSonWindows(42, "Windows Balloon", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Balloon.wav"));
        public static readonly EnumSonWindows WindowsBatteryCritical = new EnumSonWindows(43, "Windows Battery Critical", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Battery Critical.wav"));
        public static readonly EnumSonWindows WindowsBatteryLow = new EnumSonWindows(44, "Windows Battery Low", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Battery Low.wav"));
        public static readonly EnumSonWindows WindowsCriticalStop = new EnumSonWindows(45, "Windows Critical Stop", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Critical Stop.wav"));
        public static readonly EnumSonWindows WindowsDefault = new EnumSonWindows(46, "Windows Default", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Default.wav"));
        public static readonly EnumSonWindows WindowsDing = new EnumSonWindows(47, "Windows Ding", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Ding.wav"));
        public static readonly EnumSonWindows WindowsError = new EnumSonWindows(48, "Windows Error", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Error.wav"));
        public static readonly EnumSonWindows WindowsExclamation = new EnumSonWindows(49, "Windows Exclamation", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Exclamation.wav"));
        public static readonly EnumSonWindows WindowsFeedDiscovered = new EnumSonWindows(50, "Windows Feed Discovered", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Feed Discovered.wav"));
        public static readonly EnumSonWindows WindowsForeground = new EnumSonWindows(51, "Windows Foreground", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Foreground.wav"));
        public static readonly EnumSonWindows WindowsHardwareFail = new EnumSonWindows(52, "Windows Hardware Fail", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Hardware Fail.wav"));
        public static readonly EnumSonWindows WindowsHardwareInsert = new EnumSonWindows(53, "Windows Hardware Insert", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Hardware Insert.wav"));
        public static readonly EnumSonWindows WindowsHardwareRemove = new EnumSonWindows(54, "Windows Hardware Remove", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Hardware Remove.wav"));
        public static readonly EnumSonWindows WindowsInformationBar = new EnumSonWindows(55, "Windows Information Bar", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Information Bar.wav"));
        public static readonly EnumSonWindows WindowsLogoffSound = new EnumSonWindows(56, "Windows Logoff Sound", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Logoff Sound.wav"));
        public static readonly EnumSonWindows WindowsLogonSound = new EnumSonWindows(57, "Windows Logon Sound", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Logon Sound.wav"));
        public static readonly EnumSonWindows WindowsLogon = new EnumSonWindows(58, "Windows Logon", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Logon.wav"));
        public static readonly EnumSonWindows WindowsMenuCommand = new EnumSonWindows(59, "Windows Menu Command", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Menu Command.wav"));
        public static readonly EnumSonWindows WindowsMessageNudge = new EnumSonWindows(60, "Windows Message Nudge", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Message Nudge.wav"));
        public static readonly EnumSonWindows WindowsMinimize = new EnumSonWindows(61, "Windows Minimize", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Minimize.wav"));
        public static readonly EnumSonWindows WindowsNavigationStart = new EnumSonWindows(62, "Windows Navigation Start", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Navigation Start.wav"));
        public static readonly EnumSonWindows WindowsNotifyCalendar = new EnumSonWindows(63, "Windows Notify Calendar", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Notify Calendar.wav"));
        public static readonly EnumSonWindows WindowsNotifyEmail = new EnumSonWindows(64, "Windows Notify Email", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Notify Email.wav"));

        public static readonly EnumSonWindows WindowsNotifyMessaging = new EnumSonWindows(65, "Windows Notify Messaging",
            null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Notify Messaging.wav"));

        public static readonly EnumSonWindows WindowsNotifySystemGeneric = new EnumSonWindows(66,
            "Windows Notify System Generic", null,
            new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Notify System Generic.wav"));
        public static readonly EnumSonWindows WindowsNotify = new EnumSonWindows(67, "Windows Notify", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Notify.wav"));

        public static readonly EnumSonWindows WindowsPopupBlocked = new EnumSonWindows(68, "Windows Pop-up Blocked", null,
            new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Pop-up Blocked.wav"));
        public static readonly EnumSonWindows WindowsPrintcomplete = new EnumSonWindows(69, "Windows Print complete", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Print complete.wav"));
        public static readonly EnumSonWindows WindowsProximityConnection = new EnumSonWindows(70, "Windows Proximity Connection", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Proximity Connection.wav"));
        public static readonly EnumSonWindows WindowsProximityNotification = new EnumSonWindows(71, "Windows Proximity Notification", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Proximity Notification.wav"));
        public static readonly EnumSonWindows WindowsRecycle = new EnumSonWindows(72, "Windows Recycle", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Recycle.wav"));
        public static readonly EnumSonWindows WindowsRestore = new EnumSonWindows(73, "Windows Restore", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Restore.wav"));
        public static readonly EnumSonWindows WindowsRingin = new EnumSonWindows(74, "Windows Ringin", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Ringin.wav"));
        public static readonly EnumSonWindows WindowsRingout = new EnumSonWindows(75, "Windows Ringout", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Ringout.wav"));
        public static readonly EnumSonWindows WindowsShutdown = new EnumSonWindows(76, "Windows Shutdown", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Shutdown.wav"));
        public static readonly EnumSonWindows WindowsStartup = new EnumSonWindows(77, "Windows Startup", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Startup.wav"));
        public static readonly EnumSonWindows WindowsUnlock = new EnumSonWindows(78, "Windows Unlock", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows Unlock.wav"));
        public static readonly EnumSonWindows WindowsUserAccountControl = new EnumSonWindows(79, "Windows User Account Control", null, new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\media\Windows User Account Control.wav"));

        public static readonly EnumSonWindows CustomSound = new EnumSonWindows(80, "Son personnalisé", null, new FileInfo(Directory.GetCurrentDirectory() + @"\CustomSound.wav"));


        public static IEnumerable<EnumSonWindows> Values
        {
            get
            {
                yield return CustomSound;

                yield return Asterisk;
                yield return Beep;
                yield return Exclamation;
                yield return Hand;
                yield return Question;
                yield return Tada;
                yield return Alarm01;
                yield return Alarm02;
                yield return Alarm03;
                yield return Alarm04;
                yield return Alarm05;
                yield return Alarm06;
                yield return Alarm07;
                yield return Alarm08;
                yield return Alarm09;
                yield return Alarm10;

                yield return Chimes;
                yield return Chord;
                yield return Ding;
                yield return Ir_begin;
                yield return Ir_end;
                yield return Ir_inter;
                yield return Notify;
                yield return Recycle;
                yield return Ring01;
                yield return Ring02;
                yield return Ring03;
                yield return Ring04;
                yield return Ring05;
                yield return Ring06;
                yield return Ring07;
                yield return Ring08;
                yield return Ring09;
                yield return Ring10;
                yield return Ringout;
                yield return SpeechDisambiguation;
                yield return SpeechMisrecognition;
                yield return SpeechOff;
                yield return SpeechOn;
                yield return SpeechSleep;
                yield return WindowsBackground;
                yield return WindowsBalloon;
                yield return WindowsBatteryCritical;
                yield return WindowsBatteryLow;
                yield return WindowsCriticalStop;
                yield return WindowsDefault;
                yield return WindowsDing;
                yield return WindowsError;
                yield return WindowsExclamation;
                yield return WindowsFeedDiscovered;
                yield return WindowsForeground;
                yield return WindowsHardwareFail;
                yield return WindowsHardwareInsert;
                yield return WindowsHardwareRemove;
                yield return WindowsInformationBar;
                yield return WindowsLogoffSound;
                yield return WindowsLogonSound;
                yield return WindowsLogon;
                yield return WindowsMenuCommand;
                yield return WindowsMessageNudge;
                yield return WindowsMinimize;
                yield return WindowsNavigationStart;
                yield return WindowsNotifyCalendar;
                yield return WindowsNotifyEmail;
                yield return WindowsNotifyMessaging;
                yield return WindowsNotifySystemGeneric;
                yield return WindowsNotify;
                yield return WindowsPopupBlocked;
                yield return WindowsPrintcomplete;
                yield return WindowsProximityConnection;
                yield return WindowsProximityNotification;
                yield return WindowsRecycle;
                yield return WindowsRestore;
                yield return WindowsRingin;
                yield return WindowsRingout;
                yield return WindowsShutdown;
                yield return WindowsStartup;
                yield return WindowsUnlock;
                yield return WindowsUserAccountControl;



            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }
        public SystemSound Sound { get; private set; }
        public FileInfo WaveFileInfo { get; private set; }

        public bool IsSystemSound { get; private set; }
        public bool IsWaveFile { get; private set; }

        private Action PlayAction { get; set; }


        private EnumSonWindows(int index, String libelle, SystemSound soundAssoc, FileInfo wavFile)
        {
            Index = index;
            Libelle = libelle;
            if (soundAssoc != null)
            {
                Sound = soundAssoc;
                IsSystemSound = true;

                PlayAction += () =>
                {
                    Sound.Play();
                };
            }
            else
            {
                WaveFileInfo = wavFile;
                IsWaveFile = true;

                PlayAction += () =>
                {

                        if (!WaveFileInfo.Exists) return;
                        SoundPlayer player = new SoundPlayer(WaveFileInfo.FullName);
                        player.PlaySync();
        

                };
            }

        }


        public static EnumSonWindows GetFromIndex(int index)
        {
            return index < 0 ? null : Values.FirstOrDefault(enumModeP => enumModeP.Index == index);
        }


        public static EnumSonWindows GetFromLibelle(string modeBadgeSeleted)
        {
            return modeBadgeSeleted == null ? null : Values.FirstOrDefault(enumModeP => enumModeP.Libelle == modeBadgeSeleted);
        }


        public void Play()
        {
                try
                {
                    PlayAction.Invoke();
                } catch (Exception ex)        {
                    Logger _logger = Logger.LastLoggerInstance;
                    _logger.Error("{0} : {1}", ex.GetType().Name, ex.Message);
                    _logger.Error(ex.StackTrace);
            }
        }

        EnumSonWindows IEnumSerializableWithIndex<EnumSonWindows>.GetFromIndex(int index)
        {
            return GetFromIndex(index);
        }

        public int GetIndex()
        {
            return Index;

        }
    }
}
