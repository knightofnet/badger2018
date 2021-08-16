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
using AryxDevLibrary.extensions;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.dto.converter;
using Badger2018.services;
using Badger2018.utils;
using Badger2018.views.usercontrols.calculatecd;
using BadgerCommonLibrary.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour CalculateCDView.xaml
    /// </summary>
    public partial class CalculateCDView : Window
    {
        private static String bilanTpl;

        private BadgeagesServices bServices = ServicesMgr.Instance.BadgeagesServices;
        private JoursServices jServices = ServicesMgr.Instance.JoursServices;
        private AppOptions PrgOptions { get; set; }

        private List<DataGridTextColumn> listBadgeageCols;

        private CcdDayControl ccdDayControlSelected = null;

        private bool isEventChangeEnabled = false;

        private TimeSpan _cdAtStart;
        private TimeSpan _tpsSupplCalc;
        private TimeSpan? _lastCdCalc;

        private bool isShowEmpty = true;

        public CalculateCDView(AppOptions prgOptions)
        {
            InitializeComponent();
            PrgOptions = prgOptions;

            gpModDay.IsEnabled = false;

            InitEvents();

            bilanTpl = l4Content.Text;
            l4Content.Text = null;

            ((Label)lienApplyLastCd.Parent).Visibility = Visibility.Hidden;

            chkOptShowEmptyDay.IsChecked = true;
            chkOptShowEmptyDay.Click += (sender, args) =>
            {
                bool value = chkOptShowEmptyDay.IsChecked ?? false;
                isShowEmpty = value;

                bool isStripped = true;
                foreach (CcdDayControl ccdDayControl in wp.Children.OfType<CcdDayControl>())
                {
                    if (!ccdDayControl.Elt.IsDayActive)
                    {
                        ccdDayControl.Visibility = isShowEmpty ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        ccdDayControl.IsStripped = isStripped;
                        isStripped = !isStripped;
                    }

                    ccdDayControl.AdaptUi();
                }
            };

            _tpsSupplCalc = TimeSpan.Zero;
            tBoxMoreTime.Text = _tpsSupplCalc.ToStrSignedhhmm();

            dtMin.DisplayDateEnd = AppDateUtils.DtNow().AddDays(-1);
            dtMin.DisplayDateStart = jServices.GetFirstDayOfHistory();
            dtMax.DisplayDateEnd = AppDateUtils.DtNow().AddDays(-1);
            dtMax.DisplayDateStart = jServices.GetFirstDayOfHistory();
        }

        private void InitEvents()
        {
            lienMesBadgeages.Click += (sender, args) => MiscAppUtils.GoTo(PrgOptions.UrlMesPointages);
            lienApplyLastCd.Click += (sender, args) =>
            {
                if (_lastCdCalc != null)
                {
                    PrgOptions.LastCdSeen = _lastCdCalc.Value;
                }
            };

            sliderMatin.ValueChanged += (sender, args) =>
            {
                //if (!isEventChangeEnabled) return;

                sliderMatin.Value = Math.Round(sliderMatin.Value);
                switch (sliderMatin.Value)
                {
                    case 0:
                        lblMatinState.Content = "Matin non comptée";
                        lblMatinState.ToolTip =
                            "Non travaillée : période prise en compte mais avec aucun temps travaillé (déficit de temps)";
                        break;
                    case 1:
                        lblMatinState.Content = "Matin travaillée";
                        lblMatinState.ToolTip = "Travaillée : période prise en compte selon les horaires";
                        break;
                    case 2:
                        lblMatinState.Content = "Matin non travaillée";
                        lblMatinState.ToolTip =
                            "Non comptée : cette période n'est pas à prendre en compte dans les calculs (la pause du midi non plus par conséquent)";
                        break;
                }

                if (ccdDayControlSelected != null)
                {
                    ccdDayControlSelected.EtatMatin = (StateDay)sliderMatin.Value;
                    ccdDayControlSelected.AdaptUi();
                    Calc();
                }
            };

            sliderAprem.ValueChanged += (sender, args) =>
            {
                //if (!isEventChangeEnabled) return;

                sliderAprem.Value = Math.Round(sliderAprem.Value);
                switch (sliderAprem.Value)
                {
                    case 0:
                        lblApremState.Content = "Après-midi non comptée";
                        lblApremState.ToolTip =
                            "Non travaillée : période prise en compte mais avec aucun temps travaillé (déficit de temps)";
                        break;
                    case 1:
                        lblApremState.Content = "Après-midi travaillée";
                        lblApremState.ToolTip = "Travaillée : période prise en compte selon les horaires";
                        break;
                    case 2:
                        lblApremState.Content = "Après-midi non travaillée";
                        lblApremState.ToolTip =
                            "Non comptée : cette période n'est pas à prendre en compte dans les calculs (la pause du midi non plus par conséquent)";
                        break;
                }

                if (ccdDayControlSelected != null)
                {
                    ccdDayControlSelected.EtatAprem = (StateDay)sliderAprem.Value;
                    ccdDayControlSelected.AdaptUi();
                    Calc();
                }
            };


            void OnTboxModB0Change(object sender, RoutedEventArgs args)
            {
                if (!isEventChangeEnabled) return;

                TextBox tbox = tboxModB0;
                String msgFormatFail = "L'heure de début de la journée doit être au format HH:mm";
                TimeSpan? newTs = TsParse(tbox, msgFormatFail);
                if (ccdDayControlSelected != null && newTs != null)
                {
                    ccdDayControlSelected.Times.PlageTravMatin.Start =
                        AppDateUtils.ChangeTime(ccdDayControlSelected.Times.PlageTravMatin.Start, newTs.Value);
                    Calc();
                }
            }

            void OnTboxModB1Change(object sender, RoutedEventArgs args)
            {
                if (!isEventChangeEnabled) return;

                TextBox tbox = tboxModB1;
                String msgFormatFail = "L'heure de fin de matinéee doit être au format HH:mm";
                TimeSpan? newTs = TsParse(tbox, msgFormatFail);
                if (ccdDayControlSelected != null && newTs != null)
                {
                    ccdDayControlSelected.Times.PlageTravMatin.End =
                        AppDateUtils.ChangeTime(ccdDayControlSelected.Times.PlageTravMatin.Start, newTs.Value);
                    Calc();
                }
            }

            void OnTboxModB2Change(object sender, RoutedEventArgs args)
            {
                if (!isEventChangeEnabled) return;

                TextBox tbox = tboxModB2;
                String msgFormatFail = "L'heure de début de l'après-midi doit être au format HH:mm";
                TimeSpan? newTs = TsParse(tbox, msgFormatFail);
                if (ccdDayControlSelected != null && newTs != null)
                {
                    ccdDayControlSelected.Times.PlageTravAprem.Start =
                        AppDateUtils.ChangeTime(ccdDayControlSelected.Times.PlageTravAprem.Start, newTs.Value);
                    Calc();
                }
            }

            void OnTboxModB3Change(object sender, RoutedEventArgs args)
            {
                if (!isEventChangeEnabled) return;

                TextBox tbox = tboxModB3;
                String msgFormatFail = "L'heure de fin de la journée doit être au format HH:mm";
                TimeSpan? newTs = TsParse(tbox, msgFormatFail);
                if (ccdDayControlSelected != null && newTs != null)
                {
                    ccdDayControlSelected.Times.PlageTravAprem.End =
                        AppDateUtils.ChangeTime(ccdDayControlSelected.Times.PlageTravAprem.Start, newTs.Value);
                    Calc();
                }
            }

            void OnTboxTpsPauseChange(object sender, RoutedEventArgs args)
            {
                if (!isEventChangeEnabled) return;

                TextBox tbox = tboxTpsPause;
                String msgFormatFail = "Le temps de pause doit être au format HH:mm";
                TimeSpan? newTs = TsParse(tbox, msgFormatFail);
                if (ccdDayControlSelected != null && newTs != null)
                {
                    ccdDayControlSelected.Elt.TimePause = newTs.Value;
                    ccdDayControlSelected.AdaptUi();
                    Calc();
                }
            }

            void OnTBoxCdStartChange(object sender, RoutedEventArgs args)
            {
                if (!isEventChangeEnabled) return;

                TextBox tbox = tBoxCdStart;
                String msgFormatFail = "Le C/D doit être au format HH:mm ou -HH:mm";
                TimeSpan? newTs = TsParse(tbox, msgFormatFail);
                if (newTs != null)
                {
                    _cdAtStart = newTs.Value;
                    Calc();
                }
            }

            void OnTBoxMoreTimeChange(object sender, RoutedEventArgs args)
            {
                if (!isEventChangeEnabled) return;

                TextBox tbox = tBoxMoreTime;
                String msgFormatFail = "Le temps supplémentaire doit être au format HH:mm";
                TimeSpan? newTs = TsParse(tbox, msgFormatFail);
                if (newTs != null)
                {
                    _tpsSupplCalc = newTs.Value;
                    Calc();
                }
            }

            tboxModB0.LostFocus += OnTboxModB0Change;
            tboxModB0.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) OnTboxModB0Change(sender, args);
            };

            tboxModB1.LostFocus += OnTboxModB1Change;
            tboxModB1.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) OnTboxModB1Change(sender, args);
            };

            tboxModB2.LostFocus += OnTboxModB2Change;
            tboxModB2.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) OnTboxModB2Change(sender, args);
            };

            tboxModB3.LostFocus += OnTboxModB3Change;
            tboxModB3.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) OnTboxModB3Change(sender, args);
            };

            tboxTpsPause.LostFocus += OnTboxTpsPauseChange;
            tboxTpsPause.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) OnTboxTpsPauseChange(sender, args);
            };

            tBoxCdStart.LostFocus += OnTBoxCdStartChange;
            tBoxCdStart.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) OnTBoxCdStartChange(sender, args);
            };

            tBoxMoreTime.LostFocus += OnTBoxMoreTimeChange;
            tBoxMoreTime.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) OnTBoxMoreTimeChange(sender, args);
            };


            dtMin.SelectedDateChanged += (sender, args) =>
            {
                if (!dtMin.SelectedDate.HasValue) return;

                if (dtMin.SelectedDate.Value.CompareTo(AppDateUtils.DtNow()) >= 0)
                {
                    MessageBox.Show("La date minimale doit être inférieure stricte à la date du jour");
                    dtMin.SelectedDate = AppDateUtils.DtNow().Date.AddDays(-1);
                    return;
                }

                if (dtMax.SelectedDate.HasValue &&
                    dtMax.SelectedDate.Value.CompareTo(dtMin.SelectedDate.Value) >= 0) return;

                dtMax.SelectedDate = dtMin.SelectedDate.Value.AddDays(1);

                if (dtMax.SelectedDate.Value.DayOfWeek == DayOfWeek.Sunday)
                {
                    dtMax.SelectedDate = dtMin.SelectedDate.Value.AddDays(2);
                }
                else if (dtMax.SelectedDate.Value.DayOfWeek == DayOfWeek.Saturday)
                {
                    dtMax.SelectedDate = dtMin.SelectedDate.Value.AddDays(3);
                }
            };

            dtMax.SelectedDateChanged += (sender, args) =>
            {
                if (!dtMax.SelectedDate.HasValue) return;

                if (dtMax.SelectedDate.Value.CompareTo(AppDateUtils.DtNow()) >= 0)
                {
                    MessageBox.Show("La date maximale doit être inférieure stricte à la date du jour");
                    dtMax.SelectedDate = AppDateUtils.DtNow().Date.AddDays(-1);
                    return;
                }
            };
        }


        private void btnCalc_Click(object sender, RoutedEventArgs e)
        {
            if (!dtMin.SelectedDate.HasValue || !dtMax.SelectedDate.HasValue) return;

            DateTime dateMin = dtMin.SelectedDate.Value;
            DateTime dateMax = dtMax.SelectedDate.Value;
            int nbJourTrav = 0;

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // vérid dateMax > datemin

                nbJourTrav = GatherDatas(dateMin, dateMax);
            }

            Calc();
            isEventChangeEnabled = true;

            l4Content.Text = String.Format(bilanTpl,
                dateMin.ToShortDateString(),
                dateMax.ToShortDateString(),
                nbJourTrav,
                dateMin.ToShortDateString(),
                _cdAtStart.ToStrSignedhhmm()
            );
        }

        private void Calc()
        {
            if (wp.Children.Count > 0)
            {
                TimeSpan cd = CalcFromUiElt();
                _lastCdCalc = cd;
                cdCalc.Content = cd.ToStrSignedhhmm();
                ((Label)lienApplyLastCd.Parent).Visibility = Visibility.Visible;
            }
        }

        private int GatherDatas(DateTime dateMin, DateTime dateMax)
        {
            int nbJourTrav = 0;

            


            List<DgElt> lstDays = new List<DgElt>();


            double nbJours = (dateMax - dateMin).TotalDays;

            wp.Children.Clear();
            wp.UpdateLayout();

            for (int i = 0; i <= nbJours; i++)
            {
                DateTime currIxDay = dateMin.AddDays(i);


                JourEntryDto jourBdd = jServices.GetJourData(currIxDay);

                DgElt infoDay = new DgElt();
                infoDay.Date = currIxDay;
                if (jourBdd.IsHydrated)
                {
                    infoDay.TypesJournees = jourBdd.TypeJour;
                    infoDay.EtatBadger = jourBdd.EtatBadger;
                }
                else
                {
                    if (currIxDay.DayOfWeek == DayOfWeek.Sunday || currIxDay.DayOfWeek == DayOfWeek.Saturday)
                    {
                        infoDay.IsDayActive = false;
                        //lstDays.Add(infoDay);
                        //continue;
                    }

                    infoDay.EtatBadger = -1;
                }

                infoDay.IsDayActive = true;

                List<BadgeageEntryDto> badgeageEntryDtos = bServices.GetAllBadgeageOfDay(currIxDay);
                infoDay.Times = TimesUtils.HydrateTimesLogicalOrder(infoDay.TypesJournees, infoDay.EtatBadger, badgeageEntryDtos);


                infoDay.Times.PausesHorsDelai = bServices.GetPauses(currIxDay);
                infoDay.TimePause = infoDay.Times.GetTpsPause();

                if (i == 0)
                {
                    if (badgeageEntryDtos.Any())
                    {
                        TimeSpan? cdAtTime = badgeageEntryDtos
                            .FirstOrDefault(r => r.TypeBadge == EnumBadgeageType.PLAGE_TRAV_APREM_END)?.CdAtTime;
                        _cdAtStart = cdAtTime ?? TimeSpan.Zero;
                    }
                    else
                    {
                        _cdAtStart = TimeSpan.Zero;
                    }
                }

                lstDays.Add(infoDay);
                nbJourTrav++;
            }


            foreach (DgElt elt in lstDays)
            {
                CcdDayControl cElt = new CcdDayControl(elt, wp.Children.Count % 2 == 0);
                wp.Children.Add(cElt);


                if (!elt.IsDayActive)
                {
                    cElt.AdaptUi();
                    continue;
                }

                if (elt.TypesJournees == EnumTypesJournees.Matin)
                {
                    cElt.EtatAprem = StateDay.NonTrav;
                    cElt.AdaptUi();
                }
                else if (elt.TypesJournees == EnumTypesJournees.ApresMidi)
                {
                    cElt.EtatMatin = StateDay.NonTrav;
                    cElt.AdaptUi();
                }
                else if (elt.EtatBadger == -1)
                {
                    cElt.EtatMatin = StateDay.NonTrav;
                    cElt.EtatAprem = StateDay.NonTrav;
                    cElt.AdaptUi();
                }


                cElt.MouseUp += CEltOnMouseUp;
                cElt.OnChange += CEltOnMouseUp;
            }

            tBoxCdStart.Text = _cdAtStart.ToStrSignedhhmm();

            return nbJourTrav;
        }

        private void CEltOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            CcdDayControl cElt = sender as CcdDayControl;
            if (cElt == null) return;

            if (!cElt.Elt.IsDayActive)
            {
                ccdDayControlSelected = null;
                gpModDay.IsEnabled = false;
                return;
            }

            gpModDay.Header = "Modifier le " + cElt.Date.ToLongDateString();
            ccdDayControlSelected = cElt;

            gpModDay.IsEnabled = true;
            isEventChangeEnabled = false;

            tboxModB0.Text = cElt.Elt.Times.PlageTravMatin.Start.TimeOfDay.ToStrSignedhhmm();
            tboxModB1.Text = cElt.Elt.Times.PlageTravMatin.EndOrDft.TimeOfDay.ToStrSignedhhmm();
            tboxModB2.Text = cElt.Elt.Times.PlageTravAprem.Start.TimeOfDay.ToStrSignedhhmm();
            tboxModB3.Text = cElt.Elt.Times.PlageTravAprem.EndOrDft.TimeOfDay.ToStrSignedhhmm();
            tboxTpsPause.Text = cElt.Elt.TimePause.ToStrSignedhhmm();

            sliderMatin.Value = (int)cElt.EtatMatin;
            sliderAprem.Value = (int)cElt.EtatAprem;


            isEventChangeEnabled = true;
        }

        private TimeSpan CalcFromUiElt()
        {
            TimeSpan retTs = new TimeSpan(0, 0, 0, 0);
            if (_cdAtStart != null)
            {
                retTs = _cdAtStart;
            }

            retTs = retTs + _tpsSupplCalc;


            foreach (CcdDayControl cElt in wp.Children.Cast<CcdDayControl>())
            {
                if (!cElt.Elt.IsDayActive) continue;


                DgElt e = cElt.Elt;

                TimeSpan normalTpsTravMatin = PrgOptions.TempsDemieJournee;
                TimeSpan normalTpsTravAprem = PrgOptions.TempsDemieJournee;
                TimeSpan normalTpsPauseMidi = PrgOptions.TempsMinPause;
                bool is5minAdded = false;


                is5minAdded = PrgOptions.IsAdd5minCpt;
                if (cElt.EtatMatin == StateDay.NonTrav)
                {
                    normalTpsTravMatin = TimeSpan.Zero;
                    normalTpsPauseMidi = TimeSpan.Zero;
                }

                if (cElt.EtatAprem == StateDay.NonTrav)
                {
                    normalTpsTravAprem = TimeSpan.Zero;
                    normalTpsPauseMidi = TimeSpan.Zero;
                }



                if (cElt.EtatMatin == StateDay.NonTrav && cElt.EtatAprem == StateDay.NonTrav)
                {
                    // La journée n'est pas à compter : pas de diff sur le C/D
                    continue;
                }


                TimeSpan normalDayTs = normalTpsTravMatin + normalTpsTravAprem;

                TimeSpan tpsTravMatin = TimeSpan.Zero;

                if (cElt.EtatMatin == StateDay.ParHoraire)
                {
                    tpsTravMatin = e.Times.PlageTravMatin.GetDuration();
                }


                TimeSpan tpsTravAprem = TimeSpan.Zero;
                TimeSpan tpsPauseMidi = TimeSpan.Zero;

                if (cElt.EtatAprem == StateDay.ParHoraire)
                {
                    tpsTravAprem = e.Times.PlageTravAprem.GetDuration();
                }

                TimeSpan tpsMoreAprem = TimeSpan.Zero;
                if (normalTpsPauseMidi != TimeSpan.Zero && e.EtatBadger >= 2 &&
                    e.TypesJournees == EnumTypesJournees.Complete)
                {
                    tpsPauseMidi = e.Times.PlageTravAprem.Start - e.Times.PlageTravMatin.EndOrDft;

                    if (tpsPauseMidi.CompareTo(PrgOptions.TempsMinPause) < 0)
                    {
                        tpsMoreAprem = PrgOptions.TempsMinPause - tpsPauseMidi;
                        tpsPauseMidi = TimeSpan.Zero;
                    }
                    else
                    {
                        tpsPauseMidi = TimeSpan.Zero;
                    }
                }

                tpsTravAprem = tpsTravAprem - tpsMoreAprem;
                //tpsPauseMidi = tpsPauseMidi - PrgOptions.TempsMinPause;


                TimeSpan tpsPauseHd = e.TimePause;

                TimeSpan cdDay =
                    tpsTravMatin + tpsTravAprem - tpsPauseMidi +
                    (is5minAdded ? new TimeSpan(0, 5, 0) : TimeSpan.Zero) -
                    tpsPauseHd - normalDayTs;
                cElt.UpdateTimesTrav(tpsTravMatin, tpsPauseMidi, tpsTravAprem, tpsPauseHd, normalDayTs,
                    PrgOptions.IsAdd5minCpt);

                retTs += cdDay;
            }

            return retTs;
        }

        private static TimeSpan? TsParse(TextBox tbox, string msgFormatFail)
        {
            TimeSpan parsableTs;
            if (MiscAppUtils.TryParseAlt(tbox.Text, out parsableTs))
            {
                TimeSpan? retTs = parsableTs;
                return retTs;
            }

            MessageBox.Show(msgFormatFail);
            return null;
        }

        public void SetDtMin(DateTime currentShowDay)
        {
            dtMin.SelectedDate = currentShowDay;
            dtMax.SelectedDate = AppDateUtils.DtNow().AddDays(-1);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }


    public class DgElt
    {
        public DateTime Date { get; set; }

        public TimesBadgerDto Times { get; set; }

        public EnumTypesJournees TypesJournees { get; set; }
        public int EtatBadger { get; internal set; }
        public TimeSpan TimePause { get; set; }
        public bool IsDayActive { get; set; }
    }
}