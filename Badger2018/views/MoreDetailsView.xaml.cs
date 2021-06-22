using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AryxDevLibrary.utils;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services;
using Badger2018.utils;
using Badger2018.views.usercontrols;
using BadgerCommonLibrary.utils;
using ExceptionHandlingUtils = BadgerCommonLibrary.utils.ExceptionHandlingUtils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour MoreDetailsView.xaml
    /// </summary>
    public partial class MoreDetailsView : Window
    {
        //public EnumTypesJournees TyJourneeRef { get; private set; }

        //public int EtatBadgeageRef { get; private set; }

        //public TimesBadgerDto TimesRef { get; private set; }

        //private List<LabelledDateTime> ListIvlDay { get; set; }
        private AppOptions PrgOptions { get; set; }
        public bool IsMustLoadsModTimeView { get; set; }

        public DateTime CurrentShowDay { get; private set; }

        private Dictionary<String, List<LabelledDateTime>> _memoryDay;

        public MoreDetailsView(AppOptions prgOptions, DateTime? dayToShownAtInit = null)
        {
            InitializeComponent();
            btnNextDay.Visibility = Visibility.Collapsed;

            if (!dayToShownAtInit.HasValue)
            {
                CurrentShowDay = AppDateUtils.DtNow();
            } else
            {
                CurrentShowDay = dayToShownAtInit.Value;
            }

            _memoryDay = new Dictionary<string, List<LabelledDateTime>>(1);

            PrgOptions = prgOptions;

            Loaded += OnLoadUi; 

        }

        private void OnLoadUi(object Sender, RoutedEventArgs args)
        {
            BadgeagesServices bServices = ServicesMgr.Instance.BadgeagesServices;
            JoursServices jServices = ServicesMgr.Instance.JoursServices;

            AdaptUiToAnotherDay(CurrentShowDay, bServices, jServices);

            /*
            // On initiliase la fenetre avec le jours courant
            List<LabelledDateTime> listIvlDayForDay = SetListValueForCurrentDay(times, etatBadgeage, typesJournees);
            _memoryDay.Add(times.TimeRef.ToString("d"), listIvlDayForDay);


            InitStackPanel(listIvlDayForDay, times, typesJournees);


            lblTyJournee.Content = typesJournees.Libelle;

            dtPickCurrentDay.SelectedDateFormat = DatePickerFormat.Long;
            dtPickCurrentDay.SelectedDate = times.TimeRef;
            dtPickCurrentDay.DisplayDateStart = ServicesMgr.Instance.JoursServices.GetFirstDayOfHistory();
            dtPickCurrentDay.DisplayDateEnd = times.TimeRef;
            dtPickCurrentDay.DisplayDate = times.TimeRef;
            dtPickCurrentDay.SelectedDateChanged += DtPickCurrentDayOnSelectedDateChanged;

            bool isMaxDepass = false;
            var a = TimesUtils.GetTempsTravaille(AppDateUtils.DtNow(), etatBadgeage, times, prgOptions, typesJournees, true,
                ref isMaxDepass);
            lblTempsTrav.Content = a.ToString(Cst.TimeSpanFormatWithH);
            Add5MinTooltip();
            */
        }

        private void InitStackPanel(List<LabelledDateTime> listIvlForDay, TimesBadgerDto times, EnumTypesJournees typesJournees)
        {
            stackBadgeage.Children.Clear();
            foreach (LabelledDateTime lblDtime in listIvlForDay)
            {
                FriseDayControl fr = new FriseDayControl(lblDtime.Time, lblDtime.BadgeageName, lblDtime.BadgeageInfo);
                fr.LTag = lblDtime.LTag;
                fr.Color = lblDtime.Color;
                fr.EtatBadgeageAssociated = lblDtime.EtatBadgeage;

                fr.Margin = new Thickness(0, 0, 0, 10);

                fr.RefreshUi();
                fr.OnClickBtnSeeScreenshot += time =>
                {
                    string fileScreenshot = MiscAppUtils.GetFileNameScreenshot(time, fr.EtatBadgeageAssociated + "");

                    ProcessStartInfo processStartInfo = new ProcessStartInfo("explorer.exe", Path.GetFullPath(Path.Combine(Cst.ScreenshotDir, fileScreenshot)));
                    Process.Start(processStartInfo);
                    // FileUtils.(Path.Combine(Cst.ScreenshotDir, fileScreenshot));
                };

                string fileSshot = MiscAppUtils.GetFileNameScreenshot(lblDtime.Time, fr.EtatBadgeageAssociated + "");

                fr.SetBtnScreenshotVisible(File.Exists(Path.Combine(Cst.ScreenshotDir, fileSshot)));
                fr.SetEndBloc(lblDtime.IsEndBloc);

                stackBadgeage.Children.Add(fr);

                if (EnumTypesJournees.Complete == typesJournees && !StringUtils.IsNullOrWhiteSpace(fr.LTag) && fr.LTag.Equals("FinMatin"))
                {                    
                    

                    Label lbl = new Label();
                    lbl.Content = String.Format("Temps travaillé le matin : {0}", times.GetTpsTravMatin().ToString(Cst.TimeSpanFormatWithH));
                    stackBadgeage.Children.Add(lbl);
                }

                if (EnumTypesJournees.Complete == typesJournees && !StringUtils.IsNullOrWhiteSpace(fr.LTag) && fr.LTag.Equals("FinAprem"))
                {
                    Label lbl = new Label();
                    lbl.Content = String.Format("Temps travaillé l'après-midi : {0}", times.GetTpsTravAprem().ToString(Cst.TimeSpanFormatWithH));
                    stackBadgeage.Children.Add(lbl);
                }

                if (!StringUtils.IsNullOrWhiteSpace(fr.LTag) && fr.LTag.Equals("Pause"))
                {
                    fr.Margin = new Thickness(10, fr.Margin.Top, fr.Margin.Right, fr.Margin.Bottom);
                }
            }
        }

        public LabelledDateTime NewLabelledDt(string title, string subtitle, string tag, int etatBadgeage, bool isEndBloc,
            DateTime time, Color color)
        {
            LabelledDateTime labelledDateTime = new LabelledDateTime(title, subtitle,
                        time);
            labelledDateTime.Color = color;
            labelledDateTime.LTag = tag;
            labelledDateTime.EtatBadgeage = etatBadgeage;
            labelledDateTime.IsEndBloc = isEndBloc;
            return labelledDateTime;
            ;
        }

        private List<LabelledDateTime> SetListValueForCurrentDay(TimesBadgerDto times, int etatBadgeage, EnumTypesJournees typesJournees)
        {

            List<LabelledDateTime> listIvlDayForDay = new List<LabelledDateTime>(4);

            LabelledDateTime labelledDateTime = null;

            if (EnumTypesJournees.Complete == typesJournees || EnumTypesJournees.Matin == typesJournees)
            {

                if (etatBadgeage >= 0 && times.IsStartMatinBadged())
                {
                    labelledDateTime = NewLabelledDt("Premier badgeage de la journée", "Début de la matinée", null, -1,
                        false, times.PlageTravMatin.Start, Colors.DodgerBlue);
                    listIvlDayForDay.Add(labelledDateTime);
                }

                DateTime dt = new DateTime();
                int nextEtatBadgeage = 0;
                bool addNotif = false;

                if (EnumTypesJournees.Complete == typesJournees && etatBadgeage >= 1 && times.IsEndMatinBadged())
                {
                    dt = times.PlageTravMatin.EndOrDft;
                    addNotif = true;
                }
                if (EnumTypesJournees.Matin == typesJournees && etatBadgeage >= 3 && times.IsEndApremBadged())
                {
                    dt = times.PlageTravAprem.EndOrDft;
                    nextEtatBadgeage = 2;
                    addNotif = true;
                }

                if (addNotif)
                {
                    labelledDateTime = NewLabelledDt("Badgeage de fin de matinée", "Fin de la matinée", "FinMatin",
                        nextEtatBadgeage, true, dt, Colors.DodgerBlue);
                    listIvlDayForDay.Add(labelledDateTime);
                }

            }


            if (EnumTypesJournees.Complete == typesJournees || EnumTypesJournees.ApresMidi == typesJournees)
            {
                DateTime dt = new DateTime();
                int nextEtatBadgeage = 0;
                bool addNotif = false;

                if (EnumTypesJournees.Complete == typesJournees && etatBadgeage >= 2 && times.IsStartApremBadged())
                {
                    dt = times.PlageTravAprem.Start;
                    nextEtatBadgeage = 1;
                    addNotif = true;
                }
                if (EnumTypesJournees.ApresMidi == typesJournees && etatBadgeage >= 0 && times.IsStartMatinBadged())
                {
                    dt = times.PlageTravMatin.Start;
                    nextEtatBadgeage = -1;
                    addNotif = true;
                }
                if (addNotif)
                {
                    labelledDateTime = NewLabelledDt("Badgeage du début de l'après-midi", "Début de l'après-midi", null,
                        nextEtatBadgeage, false, dt, Colors.Goldenrod);
                    listIvlDayForDay.Add(labelledDateTime);
                }

                if (etatBadgeage >= 3 && times.IsEndApremBadged())
                {
                    labelledDateTime = NewLabelledDt("Badgeage de fin de la journée", "Fin de l'après-midi", "FinAprem",
                        2, true, times.PlageTravAprem.EndOrDft, Colors.Goldenrod);
                    listIvlDayForDay.Add(labelledDateTime);
                }
            }

            foreach (IntervalTemps pause in times.PausesHorsDelai)
            {
                string pausePlusStr = pause.IsIntervalComplet() ? "" : "Pause en cours";

                LabelledDateTime lStart = new LabelledDateTime(
                   "Début de la pause",
                    pausePlusStr,
                    pause.Start);
                lStart.Color = pause.IsIntervalComplet() ? Colors.DarkRed : Colors.IndianRed;
                lStart.LTag = "Pause";
                lStart.EtatBadgeage = -2;

                listIvlDayForDay.Add(lStart);

                if (pause.IsIntervalComplet())
                {
                    LabelledDateTime lEnd = new LabelledDateTime(
                        String.Format("Fin de la pause commencée à {0}", pause.Start.ToString(Cst.TimeSpanFormatWithH)),
                        String.Format("Durée de la pause : {0}", MiscAppUtils.TimeSpanShortStrFormat(pause.GetDuration())),
                        pause.EndOrDft);
                    lEnd.Color = Colors.DarkRed;
                    lEnd.LTag = "Pause";
                    lEnd.EtatBadgeage = -2;
                    lEnd.IsEndBloc = true;
                    listIvlDayForDay.Add(lEnd);
                }
            }

            return listIvlDayForDay.OrderBy(r => r.Time).ToList();
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

        private void btnPrevDay_Click(object sender, RoutedEventArgs e)
        {
            BadgeagesServices bServices = ServicesMgr.Instance.BadgeagesServices;
            JoursServices jServices = ServicesMgr.Instance.JoursServices;

            DateTime? dtLastDay = null;
            try
            {
                dtLastDay = jServices.GetPreviousDayOf(CurrentShowDay);
                if (!dtLastDay.HasValue)
                {
                    return;
                }

                if (dtLastDay.Value.ToShortDateString().Equals(AppDateUtils.DtNow().ToShortDateString()))
                {
                    btnNextDay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnNextDay.Visibility = Visibility.Visible;
                }

                CurrentShowDay = dtLastDay.Value;

                AdaptUiToAnotherDay(dtLastDay.Value, bServices, jServices);

                dtLastDay = jServices.GetPreviousDayOf(CurrentShowDay);
                btnPrevDay.IsEnabled = dtLastDay.HasValue;
            }
            catch (Exception ex)
            {
                ErrorChangeDayHandling(dtLastDay, ex);
            }
        }

        private void btnNextDay_Click(object sender, RoutedEventArgs e)
        {
            BadgeagesServices bServices = ServicesMgr.Instance.BadgeagesServices;
            JoursServices jServices = ServicesMgr.Instance.JoursServices;

            DateTime? dtLastDay = null;
            try
            {

                dtLastDay = jServices.GetNextDayOf(CurrentShowDay);
                if (!dtLastDay.HasValue)
                {
                    return;
                }

                if (dtLastDay.Value.ToShortDateString().Equals(AppDateUtils.DtNow().ToShortDateString()))
                {
                    btnNextDay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnNextDay.Visibility = Visibility.Visible;
                }

                CurrentShowDay = dtLastDay.Value;

                AdaptUiToAnotherDay(dtLastDay.Value, bServices, jServices);

                dtLastDay = jServices.GetNextDayOf(CurrentShowDay);
                btnNextDay.IsEnabled = dtLastDay.HasValue;
            }
            catch (Exception ex)
            {
                ErrorChangeDayHandling(dtLastDay, ex);
            }

        }

        private void DtPickCurrentDayOnSelectedDateChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            DateTime? dateSelected = null;
            try
            {
                BadgeagesServices bServices = ServicesMgr.Instance.BadgeagesServices;
                JoursServices jServices = ServicesMgr.Instance.JoursServices;


                dateSelected = dtPickCurrentDay.SelectedDate;
                if (!dateSelected.HasValue)
                {

                    return;
                }

                if (dateSelected.Value.ToShortDateString().Equals(AppDateUtils.DtNow().ToShortDateString()))
                {
                    btnNextDay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnNextDay.Visibility = Visibility.Visible;
                }

                CurrentShowDay = dateSelected.Value;

                AdaptUiToAnotherDay(dateSelected.Value, bServices, jServices);

                //   dateSelected = jServices.GetNextDayOf(currentShowDay);
                //   btnNextDay.IsEnabled = dateSelected.HasValue;
                //
                // 
            }
            catch (Exception ex)
            {
                ErrorChangeDayHandling(dateSelected, ex);
            }

        }

        private void AdaptUiToAnotherDay(DateTime dtLastDay, BadgeagesServices bServices, JoursServices jServices)
        {
            
            string dtLastDayStr = dtLastDay.ToString("d");

            TimesBadgerDto times = new TimesBadgerDto();
            times.TimeRef = dtLastDay;

            // désactive le bouton si un autre jour que le jour courant.
            // TODO btnModTimes.IsEnabled = dtLastDayStr.Equals(AppDateUtils.DtNow().ToString("d"));

            if (jServices.IsJourExistFor(dtLastDay))
            {
                JourEntryDto j = jServices.GetJourData(dtLastDay);
                if (!j.IsHydrated)
                {
                    return;
                }

                int etatBadgeage = j.EtatBadger;
                EnumTypesJournees typesJournees = j.TypeJour;

                times.PlageTravMatin.Start = 
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_START, dtLastDay));
                times.PlageTravMatin.End =
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_END, dtLastDay));
                times.PlageTravAprem.Start =
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_START, dtLastDay));
                times.PlageTravAprem.End =
                    DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_END, dtLastDay));

                times.PausesHorsDelai = bServices.GetPauses(dtLastDay);

                List<LabelledDateTime> listIvlDayForDay = SetListValueForCurrentDay(times, etatBadgeage, typesJournees);
                //_memoryDay.Add(dtLastDayStr, listIvlDayForDay);



                InitStackPanel(listIvlDayForDay, times, typesJournees);


                lblTyJournee.Content = typesJournees.Libelle;
                dtPickCurrentDay.SelectedDate = times.TimeRef;

                bool isMaxDepass = false;
                var a = TimesUtils.GetTempsTravaille(dtLastDay, etatBadgeage, times, PrgOptions, typesJournees, true,
                    ref isMaxDepass);
                lblTempsTrav.Content = a.ToString(Cst.TimeSpanFormatWithH);
                Add5MinTooltip();
            }
            else
            {
                if (!dtLastDay.ToString("d").Equals(AppDateUtils.DtNow().ToString("d")))
                {
                    MessageBox.Show(
                        String.Format("Aucun enregistrement a été trouvé pour le {0}", dtLastDay.ToString("D")),
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    dtPickCurrentDay.SelectedDate = AppDateUtils.DtNow();
                }
            }
        }

        private void Add5MinTooltip()
        {
            if (PrgOptions.IsAdd5minCpt)
            {
                lblTempsTrav.Content += "*";
                lblTempsTrav.ToolTip = "Ce temps prend en compte les 5 minutes supplémentaires";
            }
            else
            {
                lblTempsTrav.ToolTip = "Ce temps ne prend pas en compte les 5 minutes supplémentaires";
            }
        }

        private void ErrorChangeDayHandling(DateTime? dateSelected, Exception ex)
        {
            String cplText = "";
            if (dateSelected == null)
            {
                cplText =
                    "Une erreur non prévue s'est produite avec la fenêtre affichant les détails d'une journée. " +
                    "La fenêtre va maintenant se fermer, mais le programme peut continuer à fonctionner.";
            }
            else
            {
                cplText = String.Format("Une erreur non prévue s'est produite lors de la lecture de la date du {0}. " +
                                        "La fenêtre va maintenant se fermer, mais le programme peut continuer à fonctionner.",
                    dateSelected.Value.ToString("s"));
            }

            ExceptionHandlingUtils.LogAndHideException(ex);
            ExceptionMsgBoxView.ShowException(ex, null, cplText);
            Close();
        }

        public class LabelledDateTime
        {
            public string BadgeageName { get; set; }
            public string BadgeageInfo { get; set; }
            public DateTime Time { get; set; }
            public Color Color { get; set; }
            public string LTag { get; set; }
            public int EtatBadgeage { get; internal set; }
            public bool IsEndBloc { get; internal set; }

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
