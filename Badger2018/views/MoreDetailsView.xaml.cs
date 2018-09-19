using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AryxDevLibrary.utils;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;
using Badger2018.views.usercontrols;
using BadgerCommonLibrary.utils;
using IWshRuntimeLibrary;
using Path = System.IO.Path;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour MoreDetailsView.xaml
    /// </summary>
    public partial class MoreDetailsView : Window
    {
        public EnumTypesJournees TyJourneeRef { get; private set; }

        public int EtatBadgeageRef { get; private set; }

        public TimesBadgerDto TimesRef { get; private set; }

        private List<LabelledDateTime> ListIvlDay { get; set; }
        private AppOptions PrgOptions { get; set; }
        public bool IsMustLoadsModTimeView { get; set; }

        public MoreDetailsView(TimesBadgerDto times, int etatBadgeage, EnumTypesJournees typesJournees, AppOptions prgOptions)
        {
            InitializeComponent();

            TimesRef = times;
            EtatBadgeageRef = etatBadgeage;
            TyJourneeRef = typesJournees;
            PrgOptions = prgOptions;


            ListIvlDay = new List<LabelledDateTime>(4);

            SetListValue(times, etatBadgeage);

            InitStackPanel();


            lblTyJournee.Content = TyJourneeRef.Libelle;
            lblCurrentDay.Content = TimesRef.TimeRef.ToString("D");

            bool isMaxDepass = false;
            var a = TimesUtils.GetTempsTravaille(AppDateUtils.DtNow(), EtatBadgeageRef, TimesRef, prgOptions, TyJourneeRef, true,
                ref isMaxDepass);
            lblTempsTrav.Content = a.ToString(Cst.TimeSpanFormatWithH);

        }



        private void InitStackPanel()
        {
            stackBadgeage.Children.Clear();
            foreach (LabelledDateTime lblDtime in ListIvlDay)
            {
                FriseDayControl fr = new FriseDayControl(lblDtime.Time, lblDtime.BadgeageName, lblDtime.BadgeageInfo);
                fr.LTag = lblDtime.LTag;
                fr.Color = lblDtime.Color;

                fr.Margin = new Thickness(0, 0, 0, 10);

                fr.RefreshUi();
                fr.OnClickBtnSeeScreenshot += time =>
                {
                    string fileScreenshot = MiscAppUtils.GetFileNameScreenshot(time, EtatBadgeageRef + "");

                    ProcessStartInfo processStartInfo = new ProcessStartInfo("explorer.exe", Path.GetFullPath(Path.Combine(Cst.ScreenshotDir, fileScreenshot)));
                    Process.Start(processStartInfo);
                    // FileUtils.(Path.Combine(Cst.ScreenshotDir, fileScreenshot));
                };

                string fileSshot = MiscAppUtils.GetFileNameScreenshot(lblDtime.Time, EtatBadgeageRef + "");

                fr.SetBtnScreenshotVisible(System.IO.File.Exists(Path.Combine(Cst.ScreenshotDir, fileSshot)));


                stackBadgeage.Children.Add(fr);

                if (!StringUtils.IsNullOrWhiteSpace(fr.LTag) && fr.LTag.Equals("FinMatin"))
                {
                    Label lbl = new Label();
                    lbl.Content = String.Format("Temps travaillé le matin : {0}", TimesRef.PlageTravMatin.GetDuration().ToString(Cst.TimeSpanFormatWithH));
                    stackBadgeage.Children.Add(lbl);
                }

                if (!StringUtils.IsNullOrWhiteSpace(fr.LTag) && fr.LTag.Equals("FinAprem"))
                {
                    Label lbl = new Label();
                    lbl.Content = String.Format("Temps travaillé l'après-midi : {0}", TimesRef.PlageTravAprem.GetDuration().ToString(Cst.TimeSpanFormatWithH));
                    stackBadgeage.Children.Add(lbl);
                }
            }
        }

        private void SetListValue(TimesBadgerDto times, int etatBadgeage)
        {
            LabelledDateTime labelledDateTime = null;
            if (etatBadgeage >= 0 && times.IsStartMatinBadged())
            {
                labelledDateTime = new LabelledDateTime("Premier badgeage de la journée", "Début de la matinée", times.PlageTravMatin.Start);
                labelledDateTime.Color = Colors.DodgerBlue;
                ListIvlDay.Add(labelledDateTime);
            }


            if (etatBadgeage >= 1 && times.IsEndMatinBadged())
            {
                labelledDateTime = new LabelledDateTime("Badgeage de fin de matinée", "Fin de la matinée",
                   times.PlageTravMatin.EndOrDft);
                labelledDateTime.Color = Colors.DodgerBlue;
                labelledDateTime.LTag = "FinMatin";
                ListIvlDay.Add(labelledDateTime);


            }

            if (etatBadgeage >= 2 && times.IsStartApremBadged())
            {
                labelledDateTime = new LabelledDateTime("Badgeage du début de l'après-midi", "Début de l'après-midi",
                   times.PlageTravAprem.Start);
                labelledDateTime.Color = Colors.Goldenrod;
                ListIvlDay.Add(labelledDateTime);
            }
            if (etatBadgeage >= 3 && times.IsEndApremBadged())
            {
                labelledDateTime = new LabelledDateTime("Badgeage de fin de la journée", "Fin de l'après-midi",
                   times.PlageTravAprem.EndOrDft);
                labelledDateTime.Color = Colors.Goldenrod;
                labelledDateTime.LTag = "FinAprem";
                ListIvlDay.Add(labelledDateTime);
            }

            foreach (IntervalTemps pause in times.PausesHorsDelai)
            {
                string pausePlusStr = pause.IsIntervalComplet() ? "pause terminée" : "pause en cours";

                LabelledDateTime lStart = new LabelledDateTime(
                    String.Format("Pause commencée à {0}", pause.Start.ToString(Cst.TimeSpanFormatWithH)),
                    "",
                    pause.Start);
                lStart.Color = Colors.DarkRed;

                ListIvlDay.Add(lStart);

                if (pause.IsIntervalComplet())
                {
                    LabelledDateTime lEnd = new LabelledDateTime(
                        String.Format("Fin de la pause commencée à {0}", pause.Start.ToString(Cst.TimeSpanFormatWithH)),
                        pausePlusStr,
                        pause.EndOrDft);
                    lEnd.Color = Colors.DarkRed;
                    ListIvlDay.Add(lEnd);
                }
            }

            ListIvlDay = ListIvlDay.OrderBy(r => r.Time).ToList();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnModTimes_Click(object sender, RoutedEventArgs e)
        {
            IsMustLoadsModTimeView = true;
            Close();
        }


        private class LabelledDateTime
        {
            public string BadgeageName { get; set; }
            public string BadgeageInfo { get; set; }
            public DateTime Time { get; set; }
            public Color Color { get; set; }
            public string LTag { get; set; }

            public LabelledDateTime(string badgeageName, string badgeageInfo, DateTime time)
            {
                BadgeageName = badgeageName;
                BadgeageInfo = badgeageInfo;
                Time = time;
                Color = Colors.DeepSkyBlue;
            }
        }




    }
}
