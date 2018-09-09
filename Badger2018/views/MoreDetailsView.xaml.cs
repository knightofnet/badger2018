using System;
using System.Collections.Generic;
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
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;
using Badger2018.views.usercontrols;
using BadgerCommonLibrary.utils;

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
                FriseDayControl fr = FriseDayControl.NewInstance(lblDtime.Time, lblDtime.BadgeageName, lblDtime.BadgeageInfo, lblDtime.Color);
                stackBadgeage.Children.Add(fr);
                fr.Margin = new Thickness(0, 0, 0, 10);
            }
        }

        private void SetListValue(TimesBadgerDto times, int etatBadgeage)
        {
            if (etatBadgeage >= 0)
            {
                LabelledDateTime labelledDateTime = new LabelledDateTime("Premier badgeage du jour", "matinée", times.PlageTravMatin.Start);
                labelledDateTime.Color = Colors.DodgerBlue;
                ListIvlDay.Add(labelledDateTime);
            }


            if (etatBadgeage >= 1 && times.PlageTravMatin.End.HasValue)
            {
                LabelledDateTime labelledDateTime = new LabelledDateTime("Badgeage de fin de matinée", "matinée",
                    times.PlageTravMatin.EndOrDft);
                labelledDateTime.Color = Colors.DeepSkyBlue;
                ListIvlDay.Add(labelledDateTime);


            }

            if (etatBadgeage >= 2)
            {
                LabelledDateTime labelledDateTime = new LabelledDateTime("Badgeage du début de l'après-midi", "après-midi",
                    times.PlageTravAprem.Start);
                labelledDateTime.Color = Colors.Goldenrod;
                ListIvlDay.Add(labelledDateTime);
            }
            if (etatBadgeage >= 3 && times.PlageTravAprem.End.HasValue)
            {
                LabelledDateTime labelledDateTime = new LabelledDateTime("Badgeage de fin de la journée", "après-midi",
                    times.PlageTravAprem.EndOrDft);
                labelledDateTime.Color = Colors.DarkGoldenrod;
                ListIvlDay.Add(labelledDateTime);
            }

            foreach (IntervalTemps pause in times.PausesHorsDelai)
            {
                string pausePlusStr = pause.IsIntervalComplet() ? "pause terminée" : "pause en cours";

                LabelledDateTime lStart = new LabelledDateTime(
                    String.Format("Pause commencée à {0}", pause.Start.ToString(Cst.TimeSpanFormatWithH)),
                    "",
                    pause.Start);
                lStart.Color = Colors.Crimson;

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

        private class LabelledDateTime
        {
            public string BadgeageName { get; set; }
            public string BadgeageInfo { get; set; }
            public DateTime Time { get; set; }
            public Color Color { get; set; }

            public LabelledDateTime(string badgeageName, string badgeageInfo, DateTime time)
            {
                BadgeageName = badgeageName;
                BadgeageInfo = badgeageInfo;
                Time = time;
                Color = Colors.DeepSkyBlue;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
