using Badger2018.views.usercontrols.semaine;
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
using Badger2018.dto.bdd;
using Badger2018.services;
using Badger2018.utils;
using BadgerCommonLibrary.utils;



namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour SemaineView.xaml
    /// </summary>
    public partial class SemaineView : Window, JourSemaineControl.IPresenterJsCtrl
    {
        private DateTime currentShownedDateTime;


        private readonly AppOptions _prgOptionsRef;
        private TimesBadgerDto _timesRef;

        private readonly TimeSpanToPixelsFrise _tsToPixRef;

        private Dictionary<DateTime, JourSemaineControl> joursByDate = new Dictionary<DateTime, JourSemaineControl>(5);

        private Rectangle lineDtNow;
        private Rectangle rectWeekPfMatin;
        private Rectangle rectWeekPfAprem;
        private Rectangle lineMoyMat;
        private Rectangle lineMoyAprem;

        public SemaineView(AppOptions prgOptions, TimesBadgerDto times)
        {
            InitializeComponent();
            hoverInfo.Visibility = Visibility.Collapsed;

            _prgOptionsRef = prgOptions;
            _timesRef = times;

            _tsToPixRef = new TimeSpanToPixelsFrise(lTime07.Margin.Left, lTime20.Margin.Left, new TimeSpan(7, 0, 0), new TimeSpan(20, 0, 0));

            InitElementsWeek();
            InitCtxMenu();


            currentShownedDateTime = DateTime.Now.WithFirstDayOfWeek();
            InitWeekWorked(currentShownedDateTime);

            Activated += SemaineView_Activated;
            SemaineView_Activated(null, null);



        }

        private void InitCtxMenu()
        {
            ContextMenu ctxGrid = new ContextMenu();

            MenuItem tglPfMenuItem = new MenuItem();
            tglPfMenuItem.Header = "Afficher les plages fixes";
            tglPfMenuItem.IsChecked = true;
            tglPfMenuItem.Click += (s, a) =>
            {
                if (tglPfMenuItem.IsChecked == true)
                {
                    rectWeekPfMatin.Visibility = Visibility.Collapsed;
                    rectWeekPfAprem.Visibility = Visibility.Collapsed;

                }
                else
                {
                    rectWeekPfMatin.Visibility = Visibility.Visible;
                    rectWeekPfAprem.Visibility = Visibility.Visible;
                }

                tglPfMenuItem.IsChecked = !tglPfMenuItem.IsChecked;
            };


            MenuItem tglHmMenuItem = new MenuItem();
            tglHmMenuItem.Header = "Afficher les horaires moyens";
            tglHmMenuItem.IsChecked = true;
            tglHmMenuItem.Click += (s, a) =>
            {
                if (tglHmMenuItem.IsChecked == true)
                {
                    lineMoyMat.Visibility = Visibility.Collapsed;
                    lineMoyAprem.Visibility = Visibility.Collapsed;

                }
                else
                {
                    lineMoyMat.Visibility = Visibility.Visible;
                    lineMoyAprem.Visibility = Visibility.Visible;
                }

                tglHmMenuItem.IsChecked = !tglHmMenuItem.IsChecked;
            };


            ctxGrid.Items.Add(tglPfMenuItem);
            ctxGrid.Items.Add(tglHmMenuItem);


            weekGrid.ContextMenu = ctxGrid;
        }

        private void SemaineView_Activated(object sender, EventArgs e)
        {

            if (lineDtNow == null)
            {

                lineDtNow = _tsToPixRef.RectStretchLeftAlignFromTs(AppDateUtils.DtNow().TimeOfDay);
                lineDtNow.Fill = new SolidColorBrush(Colors.Green);
                lineDtNow.Margin = new Thickness(lineDtNow.Margin.Left, 23, lineDtNow.Margin.Right, lineDtNow.Margin.Bottom);
                weekGrid.Children.Insert(weekGrid.Children.IndexOf(dayGrid), lineDtNow);
            }
            else
            {
                double left = _tsToPixRef.CalcDateToPixel(AppDateUtils.DtNow().TimeOfDay);
                lineDtNow.Margin = new Thickness(left, 23, lineDtNow.Margin.Right, lineDtNow.Margin.Bottom);
            }

        }

        private void InitWeekWorked(DateTime dateTime)
        {
            DateTime dateMonday = dateTime.WithFirstDayOfWeek();
            int nbJour = 5;

            BadgeagesServices bServices = ServicesMgr.Instance.BadgeagesServices;
            JoursServices jServices = ServicesMgr.Instance.JoursServices;

            lblWeek.Content = String.Format("Semaine du {0} au {1}", dateMonday.ToShortDateString(), dateMonday.AddDays(5).ToShortDateString());


            for (int i = 0; i < nbJour; i++)
            {
                DateTime currentDate = dateMonday.AddDays(i);
                JourEntryDto jourData = jServices.GetJourData(currentDate);
                InfosDay infoDay = new InfosDay();

                infoDay.EtatBadger = jourData.EtatBadger;
                infoDay.TypesJournees = jourData.TypeJour;

                bool isNewDrawing = false;

                if (!joursByDate.ContainsKey(currentDate))
                {
                    joursByDate.Add(currentDate, CreateJourSemaineControl(currentDate));

                    isNewDrawing = true;
                }
                else
                {

                }

                JourSemaineControl jrCtrl = joursByDate[currentDate];
                jrCtrl.InfosDay = infoDay;
                dayGrid.Children.Add(jrCtrl);

                if (!isNewDrawing) continue;

                double opacityFactor = 0.4;
                if ((int)currentDate.ToOADate() == (int)AppDateUtils.DtNow().ToOADate())
                {
                    opacityFactor = 1;
                    jrCtrl.Background = new SolidColorBrush(MiscAppUtils.Opacify(0.1, Colors.Black));
                }

                jrCtrl.Margin = new Thickness(jrCtrl.Margin.Left, i * 50 + 20, jrCtrl.Margin.Right, jrCtrl.Margin.Bottom);
                jrCtrl.AbsoluteTop = i * 50 + 20;

                TimesBadgerDto timesMoy = new TimesBadgerDto();
                timesMoy.PlageTravMatin.Start = currentDate.ChangeTime(bServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_MATIN_START, 30) ?? _prgOptionsRef.HeureMinJournee);
                timesMoy.PlageTravMatin.End = currentDate.ChangeTime(bServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_MATIN_END, 30) ?? _prgOptionsRef.PlageFixeMatinFin);
                timesMoy.PlageTravAprem.Start = currentDate.ChangeTime(bServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_APREM_START, 30) ?? (_prgOptionsRef.PlageFixeMatinFin + _prgOptionsRef.TempsMinPause));
                timesMoy.PlageTravAprem.End = currentDate.ChangeTime(bServices.GetBadgeMoyenneTime(EnumBadgeageType.PLAGE_TRAV_APREM_END, 30) ?? _prgOptionsRef.PlageFixeApremFin);




                if (!jourData.IsHydrated)
                {
                    JourSemaineControl.PeriodeG pGm = jrCtrl.AddEmptyPeriode("journéeNonTravM", timesMoy.PlageTravMatin.Start.TimeOfDay, timesMoy.PlageTravMatin.EndOrDft.TimeOfDay);
                    JourSemaineControl.PeriodeG pGa = jrCtrl.AddEmptyPeriode("journéeNonTravA", timesMoy.PlageTravAprem.Start.TimeOfDay, timesMoy.PlageTravAprem.EndOrDft.TimeOfDay);

                    jrCtrl.DrawPeriodes();
                    continue;
                }

                String genericLblPer = "{0} du " + currentDate.ToShortDateString();

                TimesBadgerDto times = new TimesBadgerDto();
                times.PlageTravMatin.Start =
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_START, currentDate));
                times.PlageTravMatin.End =
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_END, currentDate));
                times.PlageTravAprem.Start =
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_START, currentDate));
                times.PlageTravAprem.End =
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_END, currentDate));
                infoDay.Times = times;


                if (jourData.EtatBadger == 0)
                {
                    JourSemaineControl.PeriodeG p = jrCtrl.AddPeriode(String.Format(genericLblPer, "1er badgeage"), times.PlageTravMatin.Start.TimeOfDay, times.PlageTravMatin.Start.AddMinutes(1).TimeOfDay, MiscAppUtils.Opacify(opacityFactor, Colors.DodgerBlue));
                    p.TypePeriode = 1;
                }
                else if (jourData.EtatBadger >= 1)
                {
                    String matineLblPer = String.Format(genericLblPer, "Matinée");

                    if (jourData.TypeJour == EnumTypesJournees.Complete)
                    {
                        jrCtrl.AddPeriode(matineLblPer, times.PlageTravMatin.Start.TimeOfDay, times.PlageTravMatin.EndOrDft.TimeOfDay, MiscAppUtils.Opacify(opacityFactor, Colors.DodgerBlue));
                    }
                    else if (jourData.TypeJour == EnumTypesJournees.Matin)
                    {
                        jrCtrl.AddPeriode(matineLblPer, times.PlageTravMatin.Start.TimeOfDay, times.PlageTravAprem.EndOrDft.TimeOfDay, MiscAppUtils.Opacify(opacityFactor, Colors.DodgerBlue));
                    }
                    else if (jourData.TypeJour == EnumTypesJournees.ApresMidi)
                    {
                        JourSemaineControl.PeriodeG pG = jrCtrl.AddEmptyPeriode("1emePerFictive", timesMoy.PlageTravMatin.Start.TimeOfDay, timesMoy.PlageTravMatin.EndOrDft.TimeOfDay);

                    }
                }

                if (jourData.EtatBadger == 2)
                {
                    JourSemaineControl.PeriodeG p = jrCtrl.AddPeriode("2emB", times.PlageTravAprem.Start.TimeOfDay, times.PlageTravAprem.Start.AddMinutes(1).TimeOfDay, MiscAppUtils.Opacify(opacityFactor, Colors.Goldenrod));
                    p.TypePeriode = 1;
                }
                else if (jourData.EtatBadger >= 3)
                {
                    String apremLblPer = String.Format(genericLblPer, "Après-midi");

                    if (jourData.TypeJour == EnumTypesJournees.Complete)
                    {
                        jrCtrl.AddPeriode(apremLblPer, times.PlageTravAprem.Start.TimeOfDay, times.PlageTravAprem.EndOrDft.TimeOfDay, MiscAppUtils.Opacify(opacityFactor, Colors.Goldenrod));
                    }
                    else if (jourData.TypeJour == EnumTypesJournees.ApresMidi)
                    {
                        jrCtrl.AddPeriode(apremLblPer, times.PlageTravMatin.Start.TimeOfDay, times.PlageTravAprem.EndOrDft.TimeOfDay, MiscAppUtils.Opacify(opacityFactor, Colors.Goldenrod));
                    }
                    else if (jourData.TypeJour == EnumTypesJournees.Matin)
                    {
                        JourSemaineControl.PeriodeG pG = jrCtrl.AddEmptyPeriode("1emePerFictive", timesMoy.PlageTravAprem.Start.TimeOfDay, timesMoy.PlageTravAprem.EndOrDft.TimeOfDay);
                    }


                }

                // Ajoute une zone si la pause est inf à 45min
                if (jourData.TypeJour == EnumTypesJournees.Complete && jourData.EtatBadger >= 2)
                {
                    if ((times.PlageTravAprem.Start - times.PlageTravMatin.EndOrDft).CompareTo(_prgOptionsRef.TempsMinPause) < 0)
                    {
                        jrCtrl.AddPeriode("diffPause", times.PlageTravAprem.Start.TimeOfDay, times.PlageTravMatin.EndOrDft.TimeOfDay + _prgOptionsRef.TempsMinPause, MiscAppUtils.Opacify(0.5, Colors.Red));
                    }
                }


                if (jourData.EtatBadger >= 0)
                {
                    DateTime endtheo = TimesUtils.GetDateTimeEndTravTheorique(times.PlageTravMatin.Start, _prgOptionsRef, jourData.TypeJour);
                    JourSemaineControl.PeriodeG p = jrCtrl.AddPeriode("Fin théorique de la journée", endtheo.TimeOfDay, endtheo.AddMinutes(1).TimeOfDay, Colors.Red);
                    p.TypePeriode = 1;


                }


                jrCtrl.DrawPeriodes();

            }

        }

        public JourSemaineControl CreateJourSemaineControl(DateTime date)
        {
            JourSemaineControl j = new JourSemaineControl(this, _tsToPixRef);
            j.Header = date.ToString("dddd");
            //j.AddPeriode("matin",jourData. new TimeSpan(8, 0, 0), new TimeSpan(12, 30, 0));
            //j.DrawPeriodes();
            j.Margin = new Thickness(0, 0, 0, 0);
            j.HorizontalAlignment = HorizontalAlignment.Left;
            j.VerticalAlignment = VerticalAlignment.Top;
            //dayGrid.Children.Add(j);

            return j;
        }



        private void InitElementsWeek()
        {

            // PlageFixe matin
            rectWeekPfMatin = _tsToPixRef.RectStretchLeftAlignFromTs(_prgOptionsRef.PlageFixeMatinStart,
                _prgOptionsRef.PlageFixeMatinFin);
            rectWeekPfMatin.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#7FDBFFDE"));
            rectWeekPfMatin.Margin = new Thickness(rectWeekPfMatin.Margin.Left, 23, rectWeekPfMatin.Margin.Right, rectWeekPfMatin.Margin.Bottom);
            weekGrid.Children.Insert(0, rectWeekPfMatin);

            rectWeekPfAprem = _tsToPixRef.RectStretchLeftAlignFromTs(_prgOptionsRef.PlageFixeApremStart,
            _prgOptionsRef.PlageFixeApremFin);
            rectWeekPfAprem.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#7FFFF47A"));
            rectWeekPfAprem.Margin = new Thickness(rectWeekPfAprem.Margin.Left, 23, rectWeekPfAprem.Margin.Right, rectWeekPfAprem.Margin.Bottom);
            weekGrid.Children.Insert(0, rectWeekPfAprem);


            lineMoyMat = _tsToPixRef.RectStretchLeftAlignFromTs(_timesRef.EndMoyPfMatin);
            lineMoyMat.Fill = new SolidColorBrush(Colors.BlueViolet);
            lineMoyMat.Margin = new Thickness(lineMoyMat.Margin.Left, 23, lineMoyMat.Margin.Right, lineMoyMat.Margin.Bottom);
            weekGrid.Children.Insert(weekGrid.Children.IndexOf(dayGrid), lineMoyMat);

            lineMoyAprem = _tsToPixRef.RectStretchLeftAlignFromTs(_timesRef.EndMoyPfAprem);
            lineMoyAprem.Fill = new SolidColorBrush(Colors.BlueViolet);
            lineMoyAprem.Margin = new Thickness(lineMoyAprem.Margin.Left, 23, lineMoyAprem.Margin.Right, lineMoyAprem.Margin.Bottom);
            weekGrid.Children.Insert(weekGrid.Children.IndexOf(dayGrid), lineMoyAprem);

        }

        private void btnNextDay_Click(object sender, RoutedEventArgs e)
        {

            ClearCurrentWeek(currentShownedDateTime);
            currentShownedDateTime = currentShownedDateTime.AddDays(7);
            InitWeekWorked(currentShownedDateTime);

        }

        private void btnPrevDay_Click(object sender, RoutedEventArgs e)
        {
            ClearCurrentWeek(currentShownedDateTime);
            currentShownedDateTime = currentShownedDateTime.AddDays(-7);
            InitWeekWorked(currentShownedDateTime);
        }

        private void ClearCurrentWeek(DateTime currentShownedDateTime)
        {
            dayGrid.Children.Clear();
        }

        public void MouseEnterZone(JourSemaineControl.PeriodeG periodeG, double AbsoluteTop)
        {
            hoverInfo.Visibility = Visibility.Visible;

            double left = periodeG.PerRectangle.Margin.Left + periodeG.PerRectangle.Width + weekGrid.Margin.Left + dayGrid.Margin.Left + 10;

            if (left + hoverInfo.Width > Width)
            {
                left = weekGrid.Margin.Left + dayGrid.Margin.Left + periodeG.PerRectangle.Margin.Left - 10 - hoverInfo.Width;
            }

            double top = periodeG.PerRectangle.Margin.Top + weekGrid.Margin.Top + dayGrid.Margin.Top + AbsoluteTop;

            if (top + hoverInfo.Height > Height)
            {
                top = top - hoverInfo.Height + periodeG.PerRectangle.Height;
            }


            hoverInfo.Margin = new Thickness(left, top, 0, 0);
            hoverInfo.LoadsFromPeriodeG(periodeG, _prgOptionsRef);
        }

        public void MouseLeaveZone(JourSemaineControl.PeriodeG periodeG)
        {
            hoverInfo.Visibility = Visibility.Collapsed;
        }
    }
}
