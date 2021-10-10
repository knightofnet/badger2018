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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AryxDevLibrary.utils;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;
using BadgerCommonLibrary.utils;

namespace Badger2018.views.usercontrols.calculatecd
{

    public enum StateDay
    {
        /// <summary>
        /// Sans horaire - DispPfix
        /// </summary>
        NonCompte,

        /// <summary>
        /// Trav - par horaire
        /// </summary>
        ParHoraire,

        /// <summary>
        /// CP
        /// </summary>
        NonTrav
    }
    /// <summary>
    /// Logique d'interaction pour CcdDayControl.xaml
    /// </summary>
    public partial class CcdDayControl : UserControl
    {
        private static SolidColorBrush ColorFillMatin =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E90FF"));

        private static SolidColorBrush ColorFillAprem =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAA520"));

        private static SolidColorBrush ColorStrokeMatin =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1E90FF"));

        private static SolidColorBrush ColorStrokeAprem =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FDAA520"));



        private static SolidColorBrush ColorMouseEnter =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEFEFEF"));

        private static SolidColorBrush ColorStripped =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCFCFCF"));


        private SolidColorBrush ColorDftBg = null;

        public DgElt Elt { get; set; }

        public DateTime Date { get; private set; }
        public TimesBadgerDto Times { get; private set; }

        public StateDay EtatMatin { get; set; }
        public StateDay EtatAprem { get; set; }

        public bool IsStripped { get; set; }



        public CcdDayControl(DgElt elt, bool isStripped = false)
        {
            InitializeComponent();

            IsStripped = isStripped;


            Date = elt.Date;
            Elt = elt;
            Times = elt.Times;



            if (elt.IsDayActive)
            {

                EtatMatin = StateDay.NonCompte;
                EtatAprem = StateDay.NonCompte;
                if (Times.PlageTravMatin.IsIntervalComplet())
                {
                    EtatMatin = StateDay.ParHoraire;
                }

                if (Times.PlageTravAprem.IsIntervalComplet())
                {
                    EtatAprem = StateDay.ParHoraire;
                }
            }

            AdaptUi();
            InitEvents();

        }


        public void AdaptUi()
        {

            Background = IsStripped ? ColorStripped : null;
            ColorDftBg = IsStripped ? ColorStripped : null;

            lblJ.Content = Date.Day;
            lblM.Content = Date.Month;
            lblDay.Content = String.Format("{0}. {1} {2}.",
                AppDateUtils.StrDayOfWeek(Date.DayOfWeek).Substring(0, 3),
                Date.Day,
                AppDateUtils.StrMonthOfYear(Date.Month).Substring(0, 3)
            );


            if (!Elt.IsDayActive)
            {
                rPauses.Visibility = Visibility.Collapsed;
                rMatin.Fill = null;
                rMatin.Stroke = null;
                rAprem.Fill = null;
                rAprem.Stroke = null;
                lblJ.Foreground = new SolidColorBrush(Colors.Black);
                lblM.Foreground = new SolidColorBrush(Colors.Black);


                return;
            }

            Cursor = Cursors.Hand;

            rPauses.Visibility = Elt.TimePause.CompareTo(TimeSpan.Zero) > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            switch (EtatMatin)
            {
                case StateDay.NonTrav:
                    rMatin.Fill = new SolidColorBrush(Colors.White);
                    rMatin.Stroke = new SolidColorBrush(Colors.White); ;
                    lblJ.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                case StateDay.NonCompte:
                    rMatin.Fill = new SolidColorBrush(Colors.White); ;
                    rMatin.Stroke = ColorStrokeMatin;
                    lblJ.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                case StateDay.ParHoraire:
                    rMatin.Fill = ColorFillMatin;
                    rMatin.Stroke = ColorStrokeMatin;
                    lblJ.Foreground = new SolidColorBrush(Colors.White);
                    break;
            }

            switch (EtatAprem)
            {
                case StateDay.NonTrav:
                    rAprem.Fill = new SolidColorBrush(Colors.White); ;
                    rAprem.Stroke = new SolidColorBrush(Colors.White); ;
                    lblM.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                case StateDay.NonCompte:
                    rAprem.Fill = new SolidColorBrush(Colors.White); ;
                    rAprem.Stroke = ColorStrokeAprem;
                    lblM.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                case StateDay.ParHoraire:
                    rAprem.Fill = ColorFillAprem;
                    rAprem.Stroke = ColorStrokeAprem;
                    lblM.Foreground = new SolidColorBrush(Colors.White);
                    break;
            }
        }
        private void InitEvents()
        {
            lblJ.MouseUp += (sender, args) =>
            {
                if (!Elt.IsDayActive) return;

                EtatMatin++;
                if (3 == (int)EtatMatin)
                {
                    EtatMatin = 0;
                }
                AdaptUi();
                OnChange?.Invoke(this, null);
            };

            lblM.MouseUp += (sender, args) =>
            {
                if (!Elt.IsDayActive) return;

                EtatAprem++;
                if (3 == (int)EtatAprem)
                {
                    EtatAprem = 0;
                }
                AdaptUi();
                OnChange?.Invoke(this, null);
            };

            MouseEnter += (sender, args) => Background = ColorMouseEnter;
            MouseLeave += (sender, args) => Background = ColorDftBg;

        }

        public void UpdateTimesTrav(TimeSpan tpsTravMatin,  TimeSpan tpsTravAprem,
            TimeSpan tpsPauseHd, TimeSpan tpsNormalJournee,
            bool prgOptionsIsAdd5MinCpt, TimeSpan prgOptionsTempsMaxJournee, double valTt)
        {
            StringBuilder strB = new StringBuilder();
            strB.AppendLine(Date.ToShortDateString() + (valTt > 0 ? " (" + valTt + " TT)" : "") + " :");
            strB.AppendLine("------");

            strB.AppendLine("Matin : " + tpsTravMatin.ToStrSignedhhmm());
            strB.AppendLine("Après-midi : " + tpsTravAprem.ToStrSignedhhmm());

            if (tpsPauseHd.CompareTo(TimeSpan.Zero) > 0)
            {
                strB.AppendLine("Autres pauses : " + tpsPauseHd.ToStrSignedhhmm());

            }

            TimeSpan tpsTravDay = tpsTravMatin + tpsTravAprem + (prgOptionsIsAdd5MinCpt ? new TimeSpan(0, 5, 0) : TimeSpan.Zero) - tpsPauseHd;
            if (tpsTravDay.CompareTo(prgOptionsTempsMaxJournee) > 0)
            {
                tpsTravDay = prgOptionsTempsMaxJournee;
                prgOptionsIsAdd5MinCpt = false;
            }

            TimeSpan total = tpsTravDay;
            strB.AppendLine();
            strB.AppendLine("Total : " + total.ToStrSignedhhmm() + "/" + tpsNormalJournee.ToStrSignedhhmm());
            strB.AppendLine("C/D : " + (total - tpsNormalJournee).ToStrSignedhhmm());


            ToolTip = strB.ToString();

        }

        public event EventHandler<MouseButtonEventArgs> OnChange;
    }
}
