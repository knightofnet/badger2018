using System;
using System.Windows;
using System.Windows.Controls;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;

namespace Badger2018.views.usercontrols
{
    /// <summary>
    /// Logique d'interaction pour CustNotifControl.xaml
    /// </summary>
    public partial class CustNotifControl : UserControl
    {
        public CustomNotificationDto CnotifObj { get; private set; }

        public AppOptions OptionsRef { get; private set; }

        public DateTime EndTheoDateTime { get; private set; }
        public TimeSpan EndMoyPfMatin { get; private set; }
        public TimeSpan EndMoyPfAprem { get; private set; }

        public CustNotifControl()
        {
            InitializeComponent();

            foreach (EnumHeurePersoNotif heurePersoNotif in EnumHeurePersoNotif.Values)
            {
                cboxListHeureTypeNotifA.Items.Add(heurePersoNotif.Libelle);
            }

            cboxListHeureTypeNotifA.SelectionChanged += CboxListHeureTypeNotifOnSelectionChanged;

            cboxEltCompNotifA.Items.Add("Avant");
            cboxEltCompNotifA.Items.Add("Après");
        }

        private void CboxListHeureTypeNotifOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = sender as ComboBox;
            if (cbox == null) return;

            string valChg = cbox.SelectedItem as string;
            if (valChg == null) return;

            EnumHeurePersoNotif typeHeure = EnumHeurePersoNotif.GetFromLibelle(valChg);
            if (typeHeure == EnumHeurePersoNotif.HEURE_PERSO)
            {
                tboxHeureRefNotifA.IsEnabled = true;
                tboxHeureRefNotifA.Text = CnotifObj.HeureRef.ToString(Cst.TimeSpanFormatWithH);
            }
            else if (typeHeure == EnumHeurePersoNotif.END_PF_MATIN)
            {
                tboxHeureRefNotifA.IsEnabled = false;
                tboxHeureRefNotifA.Text = OptionsRef.PlageFixeMatinFin.ToString(Cst.TimeSpanFormatWithH);
            }
            else if (typeHeure == EnumHeurePersoNotif.END_PF_APREM)
            {
                tboxHeureRefNotifA.IsEnabled = false;
                tboxHeureRefNotifA.Text = OptionsRef.PlageFixeApremFin.ToString(Cst.TimeSpanFormatWithH);
            }
            else if (typeHeure == EnumHeurePersoNotif.START_PF_APREM)
            {
                tboxHeureRefNotifA.IsEnabled = false;
                tboxHeureRefNotifA.Text = OptionsRef.PlageFixeApremStart.ToString(Cst.TimeSpanFormatWithH);
            }
            else if (typeHeure == EnumHeurePersoNotif.TPS_TRAV_THEO)
            {
                tboxHeureRefNotifA.IsEnabled = false;
                tboxHeureRefNotifA.Text = EndTheoDateTime.TimeOfDay.ToString(Cst.TimeSpanFormatWithH);
            }

            else if (typeHeure == EnumHeurePersoNotif.HEURE_END_MOY_MATIN)
            {
                tboxHeureRefNotifA.IsEnabled = false;
                tboxHeureRefNotifA.Text = EndMoyPfMatin.ToString(Cst.TimeSpanFormatWithH);
            }
            else if (typeHeure == EnumHeurePersoNotif.HEURE_END_MOY_APREM)
            {
                tboxHeureRefNotifA.IsEnabled = false;
                tboxHeureRefNotifA.Text = EndMoyPfAprem.ToString(Cst.TimeSpanFormatWithH);
            }
        }

        public void LoadsUi(CustomNotificationDto customNotificationDto, AppOptions options, DateTime endTheoDateTime, TimeSpan endMoyPfMatin, TimeSpan endMoyPfAprem)
        {
            CnotifObj = customNotificationDto;
            OptionsRef = options;
            EndTheoDateTime = endTheoDateTime;
            EndMoyPfMatin = endMoyPfMatin;
            EndMoyPfAprem = endMoyPfAprem;

            if (CnotifObj != null)
            {
                tboxHeureDeltaNotifA.Text = CnotifObj.Delta.ToString(Cst.TimeSpanFormatWithH);
                tboxHeureRefNotifA.Text = CnotifObj.HeureRef.ToString(Cst.TimeSpanFormatWithH);
                cboxListHeureTypeNotifA.SelectedItem = CnotifObj.HeurePersoNotif.Libelle;
                cboxEltCompNotifA.SelectedIndex = CnotifObj.CompSign;
                chkActiveNotifA.IsChecked = CnotifObj.IsActive;
                tboxMsg.Text = CnotifObj.Message;
            }
        }



        public bool IsControlOk()
        {
            CustomNotificationDto newNotifA = new CustomNotificationDto();


            newNotifA.CompSign = cboxEltCompNotifA.SelectedIndex;

            string heureTypeRaw = cboxListHeureTypeNotifA.SelectedItem as string;
            newNotifA.HeurePersoNotif = EnumHeurePersoNotif.GetFromLibelle(heureTypeRaw);


            //  HeureRef
            TimeSpan newHeureRef = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxHeureRefNotifA.Text, out newHeureRef))
            {
                newNotifA.HeureRef = newHeureRef;
            }
            else
            {
                MessageBox.Show("L'heure doit être au format HH:mm.");
                tboxHeureRefNotifA.Focus();

                return false;
            }

            //  Deleta
            TimeSpan newDeltaHr = new TimeSpan();
            if (MiscAppUtils.TryParseAlt(tboxHeureDeltaNotifA.Text, out newDeltaHr))
            {
                newNotifA.Delta = newDeltaHr;
            }
            else
            {
                MessageBox.Show("L'heure doit être au format HH:mm.");
                tboxHeureDeltaNotifA.Focus();

                return false;
            }

            newNotifA.IsActive = chkActiveNotifA.IsChecked ?? false;

            newNotifA.Message = tboxMsg.Text;

            CnotifObj.HydrateFrom(newNotifA);

            return true;
        }


    }
}
