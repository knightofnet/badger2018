using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.dto.converter;
using Badger2018.services;
using Badger2018.utils;
using BadgerCommonLibrary.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour ModTimeView.xaml
    /// </summary>
    public partial class ModTimeView : Window
    {
        public TimesBadgerDto Times { get; private set; }

        public TimeSpan LastCdSeen { get; private set; }

        public EnumTypesJournees TypeJournee { get; private set; }

        public AppOptions PrgOptionsRef { get; private set; }

        public bool HasChanged { get; private set; }

        public int EtatBadger { get; private set; }

        public DateTime CurrentModDay { get; private set; }
        public double ValTt { get; set; }

        private bool isCurrentDay;
        private ObservableCollection<IntervalTemps> obsListPause = new ObservableCollection<IntervalTemps>();

        private CoupleCboxTbox c0;
        private CoupleCboxTbox c1;
        private CoupleCboxTbox c2;
        private CoupleCboxTbox c3;
        private DataGridTextColumn _colColumnStart;
        private DataGridTextColumn _colColumnEnd;

        public ModTimeView(DateTime dayToMod, string urlMesPointages, AppOptions appOptions)
        {

            PrgOptionsRef = appOptions;

            InitializeComponent();
            InitDg();

            Loaded += OnLoaded;

            c0 = new CoupleCboxTbox(chkB0Tick, tboxB0Time);
            c1 = new CoupleCboxTbox(chkB1Tick, tboxB1Time);
            c2 = new CoupleCboxTbox(chkB2Tick, tboxB2Time);
            c3 = new CoupleCboxTbox(chkB3Tick, tboxB3Time);

            foreach (EnumTypesJournees enumTyJ in EnumTypesJournees.Values)
            {
                cboxTyJournee.Items.Add(enumTyJ.Libelle);
            }

            lienMesBadgeages.Click += (sender, args) => MiscAppUtils.GoTo(urlMesPointages);
        }

        public void SetCurrentDay(DateTime day, TimesBadgerDto times, EnumTypesJournees tyJournee, int etatBadger, double valTt, bool pIsCurrentDay)
        {
            CurrentModDay = day;
            if (pIsCurrentDay)
            {
                Times = times;
                TypeJournee = tyJournee;
                EtatBadger = etatBadger;
                LastCdSeen = PrgOptionsRef.LastCdSeen;
                ValTt = valTt;
            }
            else
            {
                Times = new TimesBadgerDto();
            }

            this.isCurrentDay = pIsCurrentDay;
        }

        private void OnLoaded(object objSender, RoutedEventArgs a)
        {
            if (!isCurrentDay)
            {
                BadgeagesServices bServices = ServicesMgr.Instance.BadgeagesServices;
                JoursServices jServices = ServicesMgr.Instance.JoursServices;

                JourEntryDto jour = jServices.GetJourData(CurrentModDay);
                TypeJournee = jour.TypeJour;
                EtatBadger = jour.EtatBadger;
                ValTt = jour.WorkAtHomeCpt;

                Times.PlageTravMatin.Start = DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_START, CurrentModDay)).AtSec(Cst.SecondeOffset);
                Times.PlageTravMatin.End = DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_END, CurrentModDay)).AtSec(Cst.SecondeOffset);
                Times.PlageTravAprem.Start = DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_START, CurrentModDay)).AtSec(Cst.SecondeOffset);
                Times.PlageTravAprem.End = DateTime.Parse(bServices.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_END, CurrentModDay)).AtSec(Cst.SecondeOffset);

                Times.PausesHorsDelai = new System.Collections.Generic.List<IntervalTemps>();
                Times.PausesHorsDelai.AddRange(bServices.GetPauses(CurrentModDay));

                TimeSpan? tsLastCd = bServices.GetLastCD(CurrentModDay);
                if (tsLastCd.HasValue)
                {
                    LastCdSeen = tsLastCd.Value;
                }
            }


            cboxTyJournee.SelectedItem = TypeJournee.Libelle;
            EnumTypesJournees locTyJournees = TypeJournee;

            // On ajoute les 4 temps badgés si dispo
            c0.SetTextTime(Times.PlageTravMatin.Start);
            c1.SetTextTime(Times.PlageTravMatin.EndOrDft);
            c2.SetTextTime(Times.PlageTravAprem.Start);
            c3.SetTextTime(Times.PlageTravAprem.EndOrDft);


            // On ajoute le dernier C/D relevé
            tboxLastCdSeen.Text = LastCdSeen.ToStrSignedhhmm();

            // On ajoute les pauses si dispo            
            Times.PausesHorsDelai.ForEach(r => obsListPause.Add(r.Clone()));

            // ValTt
            chkTt.IsChecked = ValTt > 0;


            ChangeUiToEtatBadgerTyJournee(EtatBadger, locTyJournees);

            cboxTyJournee.SelectionChanged += (sender, args) =>
            {
                String selItem = cboxTyJournee.SelectedItem as String;
                if (selItem == null) return;

                EnumTypesJournees eTypesJournees = EnumTypesJournees.GetFromLibelle(selItem);
                ChangeUiToEtatBadgerTyJournee(EtatBadger, eTypesJournees);

                TypeJournee = eTypesJournees;
            };

        }

        private void InitDg()
        {
            dgPause.AutoGenerateColumns = false;
            dgPause.CanUserAddRows = true;

            dgPause.ItemsSource = obsListPause;

            _colColumnStart = new DataGridTextColumn();
            _colColumnStart.Header = "Début";
            IntervalTempsToStringConverter convColDeb = new IntervalTempsToStringConverter();
            convColDeb.DataGridRef = dgPause;
            Binding bindgColumnName = new Binding("Start") { Converter = convColDeb };
            _colColumnStart.Binding = bindgColumnName;
            //  _colColumnStart.CellStyle = sCenteredCell;
            dgPause.Columns.Add(_colColumnStart);

            _colColumnEnd = new DataGridTextColumn();
            _colColumnEnd.Header = "Fin";
            convColDeb = new IntervalTempsToStringConverter();
            convColDeb.DataGridRef = dgPause;
            convColDeb.Mode = 1;
            bindgColumnName = new Binding("End") { Converter = convColDeb };
            _colColumnEnd.Binding = bindgColumnName;
            //  _colColumnStart.CellStyle = sCenteredCell;
            dgPause.Columns.Add(_colColumnEnd);

        }

        private void ChangeUiToEtatBadgerTyJournee(int etatBadger, EnumTypesJournees locTyJournees)
        {

            if (locTyJournees == EnumTypesJournees.Complete)
            {
                c0.IsEnabled = true;
                c1.IsEnabled = true;
                c2.IsEnabled = true;
                c3.IsEnabled = true;

            }
            else if (locTyJournees == EnumTypesJournees.Matin)
            {
                c0.IsEnabled = true;
                c1.IsEnabled = false;
                c2.IsEnabled = false;
                c3.IsEnabled = true;
            }
            else if (locTyJournees == EnumTypesJournees.ApresMidi)
            {
                c0.IsEnabled = true;
                c1.IsEnabled = false;
                c2.IsEnabled = false;
                c3.IsEnabled = true;
            }


            switch (etatBadger)
            {
                case -1:
                    c0.IsChecked = false;
                    c1.IsChecked = false;
                    c2.IsChecked = false;
                    c3.IsChecked = false;

                    break;
                case 0:

                    c0.IsChecked = true;
                    c1.IsChecked = false;
                    c2.IsChecked = false;
                    c3.IsChecked = false;

                    break;
                case 1:

                    c0.IsChecked = true;
                    c1.IsChecked = true;
                    c2.IsChecked = false;
                    c3.IsChecked = false;

                    break;
                case 2:

                    c0.IsChecked = true;
                    c1.IsChecked = true;
                    c2.IsChecked = true;
                    c3.IsChecked = false;

                    break;
                case 3:

                    c0.IsChecked = true;
                    c1.IsChecked = true;
                    c2.IsChecked = true;
                    c3.IsChecked = true;

                    break;
            }
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (VerifyDatas() && VerifyCoherence())
            {
                HasChanged = true;

                DateTime dtNow = CurrentModDay;

                Times.PlageTravMatin.Start = c0.GetStrictDateTime().ChangeDate(dtNow);
                Times.PlageTravMatin.End = c1.GetStrictDateTime().ChangeDate(dtNow);
                Times.PlageTravAprem.Start = c2.GetStrictDateTime().ChangeDate(dtNow);
                Times.PlageTravAprem.End = c3.GetStrictDateTime().ChangeDate(dtNow);



                if (TypeJournee == EnumTypesJournees.Complete)
                {
                    ValTt = (chkTt.IsChecked ?? false) ? 1 : 0;
                    if (!c0.IsEnabled && !c1.IsEnabled && !c2.IsEnabled && !c3.IsEnabled)
                    {
                        EtatBadger = -1;
                    }
                    else if (c0.IsEnabled && !c1.IsEnabled && !c2.IsEnabled && !c3.IsEnabled)
                    {
                        EtatBadger = 0;
                    }
                    else if (c0.IsEnabled && c1.IsEnabled && !c2.IsEnabled && !c3.IsEnabled)
                    {
                        EtatBadger = 1;
                    }
                    else if (c0.IsEnabled && c1.IsEnabled && c2.IsEnabled && !c3.IsEnabled)
                    {
                        EtatBadger = 2;
                    }
                    else if (c0.IsEnabled && c1.IsEnabled && c2.IsEnabled && c3.IsEnabled)
                    {
                        EtatBadger = 3;
                    }
                }
                else 
                {
                    ValTt = (chkTt.IsChecked ?? false) ? 0.5 : 0;
                    if (!c0.IsEnabled && !c3.IsEnabled)
                    {
                        EtatBadger = -1;
                    }
                    else if (c0.IsEnabled && !c3.IsEnabled)
                    {
                        EtatBadger = 0;
                    }
                    else if (c0.IsEnabled && c3.IsEnabled)
                    {
                        EtatBadger = 3;
                    }
                }


                TimeSpan ts;
                string tboxLastSeenRaw = tboxLastCdSeen.Text.Trim();
                bool isNegateLastSeen = false;
                if (tboxLastSeenRaw.StartsWith("-"))
                {
                    isNegateLastSeen = true;
                    tboxLastSeenRaw = tboxLastSeenRaw.Substring(1);
                }
                if (MiscAppUtils.TryParseAlt(tboxLastSeenRaw, out ts))
                {
                    if (isNegateLastSeen)
                    {
                        ts = ts.Negate();
                    }

                    LastCdSeen = ts;
                }

                Times.PausesHorsDelai.Clear();
                foreach (IntervalTemps pause in obsListPause)
                {
                    
                    TimeSpan pStart = pause.Start.TimeOfDay;
                    pause.Start = pause.Start.ChangeDate(dtNow).ChangeTime(pStart);
                    if (pause.IsIntervalComplet())
                    {
                        TimeSpan pEnd = pause.EndOrDft.TimeOfDay;
                        pause.EndOrDft = pause.EndOrDft.ChangeDate(dtNow).ChangeTime(pEnd);
                    }
                    Times.PausesHorsDelai.Add(pause);
                }

                Close();
            }
            else
            {
                HasChanged = false;
            }


        }



        private bool VerifyDatas()
        {

            TimeSpan ts;
            string tboxLastSeenRaw = tboxLastCdSeen.Text.Trim();
            //bool isNegateLastSeen = false;
            if (tboxLastSeenRaw.StartsWith("-"))
            {
                //  isNegateLastSeen = true;
                tboxLastSeenRaw = tboxLastSeenRaw.Substring(1);
            }
            if ((!tboxLastSeenRaw.StartsWith("-") && !TimeSpan.TryParse(tboxLastSeenRaw, out ts))
                && (tboxLastSeenRaw.StartsWith("-") && !TimeSpan.TryParse(tboxLastSeenRaw.Substring(1), out ts)))
            {
                MessageBox.Show("Le C/D doit être au format HH:mm.");
                tboxLastCdSeen.Focus();
                return false;
            }

            if (!c0.IsValidTextTime())
            {
                MessageBox.Show("L'heure de premier badgeage de la journée doit être au format HH:mm.");
                c0.FocusTbox();
                return false;

            }

            if (!c1.IsValidTextTime())
            {
                MessageBox.Show("L'heure de la fin de la matinée doit être au format HH:mm.");
                c1.FocusTbox();
                return false;

            }

            if (!c2.IsValidTextTime())
            {
                MessageBox.Show("L'heure du début de l'après-midi doit être au format HH:mm.");
                c2.FocusTbox();
                return false;

            }

            if (!c3.IsValidTextTime())
            {
                MessageBox.Show("L'heure de fin de l'après-midi doit être au format HH:mm.");
                c3.FocusTbox();
                return false;

            }



            if (TypeJournee == EnumTypesJournees.Complete)
            {
                if (c2.IsEnabled && (!c1.IsEnabled || !c0.IsEnabled))
                {
                    MessageBox.Show("Si l'heure du début de l'après-midi est badgée, les heures du début de la journée et de fin de la matinée doivent être badgées");

                    return false;
                }

                if (c3.IsEnabled && (!c2.IsEnabled || !c1.IsEnabled || !c0.IsEnabled))
                {
                    MessageBox.Show("Si l'heure de fin de l'après-midi est badgée, les heures du début de la journée, de fin de la matinée et de début de l'après-midi doivent être badgées");

                    return false;
                }
            }

            return true;
        }

        private bool VerifyCoherence()
        {
            if (c0.IsEnabled && c1.IsEnabled)
            {
                if (c0.GetStrictTextTime() > c1.GetStrictTextTime())
                {
                    MessageBox.Show("L'heure de premier badgeage de la journée doit être inférieure à l'heure de la fin de la matinée.");
                    c1.FocusTbox();
                    return false;
                }
            }

            if (c1.IsEnabled && c2.IsEnabled)
            {
                if (c1.GetStrictTextTime() > c2.GetStrictTextTime())
                {
                    MessageBox.Show("L'heure de la fin de la matinée doit être inférieure à l'heure du début de l'après-midi.");
                    c2.FocusTbox();
                    return false;
                }
            }

            if (c2.IsEnabled && c3.IsEnabled)
            {
                if (c2.GetStrictTextTime() > c3.GetStrictTextTime())
                {
                    MessageBox.Show("L'heure du début de l'après-midi doit être inférieure à l'heure de fin de l'après-midi.");
                    c3.FocusTbox();
                    return false;
                }
            }

            if (c0.IsEnabled && c3.IsEnabled)
            {
                if (c0.GetStrictTextTime() > c3.GetStrictTextTime())
                {
                    MessageBox.Show("L'heure de premier badgeage de la journée doit être inférieure à l'heure de fin de l'après-midi.");
                    c3.FocusTbox();
                    return false;
                }
            }

            return true;
        }
    }





    class CoupleCboxTbox
    {
        public bool IsEnabled
        {
            get { return Cbox.IsEnabled && Tbox.IsEnabled; }
            set
            {
                Cbox.IsEnabled = value;
                Tbox.IsEnabled = value;
            }
        }

        public bool IsChecked
        {
            get { return Cbox.IsChecked ?? false; }
            set
            {
                Cbox.IsChecked = value;
                Tbox.IsEnabled = value;
            }
        }

        private readonly CheckBox Cbox;

        private readonly TextBox Tbox;

        private DateTime internalDateTime;

        public CoupleCboxTbox(CheckBox cbox, TextBox tbox)
        {
            Cbox = cbox;
            Tbox = tbox;

            Cbox.Click += (sender, args) =>
            {
                tbox.IsEnabled = Cbox.IsChecked.HasValue && Cbox.IsChecked.Value;
            };
        }



        public void SetTextTime(DateTime times)
        {
            internalDateTime = times;
            Tbox.Text = times.TimeOfDay.ToString(Cst.TimeSpanFormat);
        }


        public bool IsValidTextTime()
        {
            TimeSpan newTboxTs = new TimeSpan();
            return TimeSpan.TryParse(Tbox.Text, out newTboxTs);

        }

        public DateTime GetStrictDateTime()
        {
            TimeSpan newTboxTs = new TimeSpan();
            if (TimeSpan.TryParse(Tbox.Text, out newTboxTs))
            {
                return internalDateTime.ChangeTime(newTboxTs);
            }

            throw new Exception("Erreur lors de la lecture du Temps");
        }

        public TimeSpan GetStrictTextTime()
        {
            TimeSpan newTboxTs = new TimeSpan();
            if (TimeSpan.TryParse(Tbox.Text, out newTboxTs))
            {
                return newTboxTs;
            }

            throw new Exception("Erreur lors de la lecture du Temps");
        }

        public void FocusTbox()
        {
            Tbox.Focus();
        }
    }
}
