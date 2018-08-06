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

        public EnumTypesJournees TypeJournee { get; private set; }

        public bool HasChanged { get; private set; }

        public int EtatBadger { get; private set; }

        private CoupleCboxTbox c0;
        private CoupleCboxTbox c1;
        private CoupleCboxTbox c2;
        private CoupleCboxTbox c3;

        public ModTimeView(TimesBadgerDto times, EnumTypesJournees tyJournees, int etatBadger, string urlMesPointages)
        {
            Times = times;
            TypeJournee = tyJournees;
            EtatBadger = etatBadger;

            InitializeComponent();
            c0 = new CoupleCboxTbox(chkB0Tick, tboxB0Time);
            c1 = new CoupleCboxTbox(chkB1Tick, tboxB1Time);
            c2 = new CoupleCboxTbox(chkB2Tick, tboxB2Time);
            c3 = new CoupleCboxTbox(chkB3Tick, tboxB3Time);

            foreach (EnumTypesJournees enumTyJ in EnumTypesJournees.Values)
            {
                cboxTyJournee.Items.Add(enumTyJ.Libelle);
            }
            cboxTyJournee.SelectedItem = tyJournees.Libelle;

            c0.SetTextTime(Times.StartDateTime);
            c1.SetTextTime(Times.PauseStartDateTime);
            c2.SetTextTime(Times.PauseEndDateTime);
            c3.SetTextTime(Times.EndDateTime);


            EnumTypesJournees locTyJournees = TypeJournee;

            ChangeUiToEtatBadgerTyJournee(etatBadger, locTyJournees);

            cboxTyJournee.SelectionChanged += (sender, args) =>
            {
                String selItem = cboxTyJournee.SelectedItem as String;
                if (selItem == null) return;

                EnumTypesJournees eTypesJournees = EnumTypesJournees.GetFromLibelle(selItem);
                ChangeUiToEtatBadgerTyJournee(EtatBadger, eTypesJournees);

                TypeJournee = eTypesJournees;
            };


            lienMesBadgeages.Click += (sender, args) => MiscAppUtils.GoTo(urlMesPointages);
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
                c1.IsEnabled = true;
                c2.IsEnabled = false;
                c3.IsEnabled = false;
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

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (VerifyDatas() && VerifyCoherence())
            {
                HasChanged = true;

                DateTime dtNow = AppDateUtils.DtNow();

                Times.StartDateTime = c0.GetStrictDateTime().ChangeDate(dtNow);
                Times.PauseStartDateTime = c1.GetStrictDateTime().ChangeDate(dtNow);
                Times.PauseEndDateTime = c2.GetStrictDateTime().ChangeDate(dtNow);
                Times.EndDateTime = c3.GetStrictDateTime().ChangeDate(dtNow);

                if (TypeJournee == EnumTypesJournees.Complete)
                {
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
                else if (TypeJournee == EnumTypesJournees.Matin)
                {
                    if (!c0.IsEnabled && !c1.IsEnabled)
                    {
                        EtatBadger = -1;
                    }
                    else if (c0.IsEnabled && !c1.IsEnabled)
                    {
                        EtatBadger = 0;
                    }
                    else if (c0.IsEnabled && c1.IsEnabled)
                    {
                        EtatBadger = 3;
                    }

                }
                else if (TypeJournee == EnumTypesJournees.ApresMidi)
                {
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

                Close();
            }
            else
            {
                HasChanged = false;
            }


        }

        private bool VerifyDatas()
        {
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
}
