using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AryxDevLibrary.utils.logger;

namespace Badger2018.constants
{
    public static class Cst
    {


        public const string TimeSpanFormat = "hh':'mm";
        public const string TimeSpanFormatWithH = "hh'h'mm";

        public const string TimeSpanAltFormat = "h'h'mm";

        public const string MatineeStartStr = "Matiné: {0}";

        public const string MatineeEndStr = "Matiné: {0} à {1}";

        public const string ApremStartStr = "Après-midi: {0}";

        public const string ApremEndStr = "Après-midi: {0} à {1}";

        public const string ArchivesDirName = "archives";

        public const string PointagesDirName = "pointages";

        public const string PointagesDir = "./" + PointagesDirName + "/";

        public const string ScreenshotDirName = "screenshots";

        public const string ScreenshotDir = "./" + ScreenshotDirName + "/";

        public static readonly Thickness BtnBadgerPositionAtLeft = new Thickness(39.5, 10, 0, 10);

        public static readonly Thickness BtnBadgerPositionAtCenter = new Thickness(218.5, 10, 0, 10);

        public static readonly Thickness BtnBadgerPositionAtRight = new Thickness(397.5, 10, 0, 10);

        public static readonly SolidColorBrush SCBGrey = new SolidColorBrush(Colors.Gray);
        public static readonly SolidColorBrush SCBBlack = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush SCBDarkRed = new SolidColorBrush(Colors.DarkRed);
        public static readonly SolidColorBrush SCBDarkGreen = new SolidColorBrush(Colors.DarkGreen);
        public static readonly SolidColorBrush SCBGreenPbar = new SolidColorBrush(Color.FromArgb(255, 6, 176, 37));
        public static readonly SolidColorBrush SCBGold = new SolidColorBrush(Colors.Gold);
        internal static readonly int SecondeOffset = 0;
        public const string NotifEndPfMatinName = "ShowNotifEndPfMatin";
        public const string NotifEndPfApremName = "ShowNotifEndPfAprem";
        public const string NotifEndPauseName = "ShowNotifEndPause";
        public const string NotifEndTheoName = "ShowNotifEndTheo";
        public const string NotifCust1Name = "Notif1Time";
        public const string NotifCust2Name = "Notif2Time";
        public const string NotifTpsMaxJournee = "TempsMaxJournee";
        public const string NotifTpsMaxDemieJournee = "TempsMaxDemieJournee";

        public const string XmlRootName = "ConfigFile";
        public static string ApplicationDirectory { get; set; }
    }
}
