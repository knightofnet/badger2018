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
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;

namespace Badger2018.views.usercontrols
{
    /// <summary>
    /// Logique d'interaction pour CompteurControl.xaml
    /// </summary>
    public partial class CompteurControl : UserControl
    {
        private CompteurState _currentState;

        public enum CompteurState
        {
            TempsTravailDuJour,
            TempsRestantJour,
            TempsCdRealTime,
            CustumExt
        }

        public enum CompteurVisibility
        {
            Hidden,
            OnlyMain,
            Full
        }

        public CompteurState CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                OnChangeCurrentState(value);

            }
        }



        public bool IsEnabledCtrl { get; set; }

        public TimesBadgerDto TimesRef { get; set; }
        public AppOptions OptionsRef { get; set; }
        public AppSwitchs PrgSwitchRef { get; set; }
        public RealTimesObj RealTimesRef { get; set; }
        public EnumTypesJournees TyJourneeRef { get; set; }

        public CompteurControl()
        {
            InitializeComponent();
            CurrentState = CompteurState.TempsTravailDuJour;
            lblTpsTravReel.ContentShortTime(TimeSpan.Zero);

            lblTpsTravReel.MouseDoubleClick += OnMouseDblClick;
            lblTpsTravReelLbl.MouseDoubleClick += OnMouseDblClick;
            lblTpsTravReelSuppl.MouseDoubleClick += OnMouseDblClick;

            //  AdaptUiAtState(CurrentState);
        }

        private void OnMouseDblClick(object sender, MouseButtonEventArgs e)
        {
            if (CurrentState == CompteurState.CustumExt)
            {
                return;
            }

            if (CurrentState == CompteurState.TempsTravailDuJour)
            {
                CurrentState = CompteurState.TempsRestantJour;

            }
            else if (CurrentState == CompteurState.TempsRestantJour)
            {
                CurrentState = CompteurState.TempsCdRealTime;
            }
            else if (CurrentState == CompteurState.TempsCdRealTime)
            {
                CurrentState = CompteurState.TempsTravailDuJour;
            }


            UpdateInfos();
        }

        private ContextMenu GetLblCtxMenu()
        {
            if (!IsEnabledCtrl)
            {
                return null;
            }

            ContextMenu ctxMenu = new ContextMenu();

            MenuItem tpsTravaJourMenuItem = new MenuItem();
            tpsTravaJourMenuItem.Header = "Temps travaillé du jour";
            tpsTravaJourMenuItem.Click += (sender, args) =>
            {
                if (CurrentState != CompteurState.CustumExt)
                {
                    CurrentState = CompteurState.TempsTravailDuJour;
                    UpdateInfos();
                }
            };
            tpsTravaJourMenuItem.IsChecked = CurrentState == CompteurState.TempsTravailDuJour;
            tpsTravaJourMenuItem.IsEnabled = CurrentState != CompteurState.CustumExt;
            ctxMenu.Items.Add(tpsTravaJourMenuItem);

            MenuItem tpsRestantMenuItem = new MenuItem();
            tpsRestantMenuItem.Header = "Temps restant";
            tpsRestantMenuItem.Click += (sender, args) =>
            {
                if (CurrentState != CompteurState.CustumExt)
                {
                    CurrentState = CompteurState.TempsRestantJour;
                    UpdateInfos();
                }
            };
            tpsRestantMenuItem.IsChecked = CurrentState == CompteurState.TempsRestantJour;
            tpsRestantMenuItem.IsEnabled = CurrentState != CompteurState.CustumExt;
            ctxMenu.Items.Add(tpsRestantMenuItem);


            MenuItem tpsCdRealTimeItem = new MenuItem();
            tpsCdRealTimeItem.Header = "C/D en temps réel";
            tpsCdRealTimeItem.Click += (sender, args) =>
            {
                if (CurrentState != CompteurState.CustumExt)
                {
                    CurrentState = CompteurState.TempsCdRealTime;
                    UpdateInfos();
                }
            };
            tpsCdRealTimeItem.IsChecked = CurrentState == CompteurState.TempsCdRealTime;
            tpsCdRealTimeItem.IsEnabled = CurrentState != CompteurState.CustumExt;
            ctxMenu.Items.Add(tpsCdRealTimeItem);

            return ctxMenu;
        }

        public void UpdateInfos()
        {
            UpdateInfos(CurrentState);
        }

        public void UpdateInfos(CompteurState currentState)
        {
            if (!IsEnabledCtrl)
            {
                return;
            }

            lblTpsTravReelLbl.ContextMenu = GetLblCtxMenu();

            if (CompteurState.TempsTravailDuJour == currentState)
            {

                UpdateInfosTempsTravailDuJour();
                return;
            }

            if (CompteurState.TempsRestantJour == currentState)
            {
                UpdateInfosTempsTpsRestant();
                return;
            }

            if (CompteurState.TempsCdRealTime == currentState)
            {
                UpdateInfosTempsCdRealTime();
                return;
            }

            if (CompteurState.CustumExt == currentState)
            {

                return;
            }


        }




        private void UpdateInfosTempsTravailDuJour()
        {
            SolidColorBrush colFont = Cst.SCBBlack;

            TimeSpan tpsTravRestant = TimesRef.EndTheoDateTime - RealTimesRef.RealTimeDtNow;

            // Tps trav pour une journée ou une demie-journée.
            TimeSpan tTravTheo = TimesUtils.GetTpsTravTheoriqueOneDay(OptionsRef, TyJourneeRef);


            String lblTpsTrav = "Compteur temps travaillé du jour :";
            String msgTooltip = String.Format("{0}Double-cliquer pour afficher le temps de travail restant",
                OptionsRef.IsAdd5minCpt ? "Le temps travaillé prend en compte les 5 min supplémentaires." + Environment.NewLine : "");



            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(RealTimesRef.RealTimeTempsTravaille);
            /*
            if (PrgSwitchRef.IsTimeRemainingNotTimeWork)
            {

                msgTooltip = "Double-cliquer pour afficher le compteur temps travaillé du jour";
                lblTpsTrav = RealTimesRef.RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0
                    ? "Temps supplémentaire pour la journée :"
                    : "Temps restant pour la journée :";
                strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(tpsTravRestant);
            }
            else
            {
             * */
            if (TimesRef.IsTherePauseAprem() || TimesRef.IsTherePauseMatin())
            {
                strTpsTrav += "*";
                msgTooltip += Environment.NewLine + "Prend en compte les pauses effectuées durant la journée";
            }
            /*
            }
             * */

            lblTpsTravReelLbl.Content = lblTpsTrav;
            lblTpsTravReel.Content = strTpsTrav;
             lblTpsTravReel.ToolTip = null;





            if (RealTimesRef.RealTimeTsNow.CompareTo(OptionsRef.PlageFixeApremFin) >= 0)
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Visible;


                string tplTpsReelSuppl = "({0})";
                if (RealTimesRef.RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0)
                {
                    tplTpsReelSuppl = "(+{0})";

                    if (!PrgSwitchRef.IsMoreThanTpsTheo && RealTimesRef.RealTimeTempsTravaille.CompareTo(OptionsRef.TempsMaxJournee) < 0)
                    {
                        PrgSwitchRef.IsMoreThanTpsTheo = true;
                        colFont = Cst.SCBDarkGreen;

                    }
                    else if (RealTimesRef.RealTimeTempsTravaille.CompareTo(OptionsRef.TempsMaxJournee) >= 0)
                    {
                        PrgSwitchRef.IsMoreThanTpsTheo = false;
                        colFont = Cst.SCBDarkRed;
                    }
                }
                lblTpsTravReelSuppl.Content = String.Format(tplTpsReelSuppl, MiscAppUtils.TimeSpanShortStrFormat((RealTimesRef.RealTimeTempsTravaille - tTravTheo)));


            }

            lblTpsTravReel.Foreground = colFont;
        }


        private void UpdateInfosTempsTpsRestant()
        {
            TimeSpan tpsTravRestant = TimesRef.EndTheoDateTime - RealTimesRef.RealTimeDtNow;

            // Tps trav pour une journée ou une demie-journée.
            TimeSpan tTravTheo = TimesUtils.GetTpsTravTheoriqueOneDay(OptionsRef, TyJourneeRef);



            String lblTpsTrav = RealTimesRef.RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0
                ? "Temps supplémentaire pour la journée :"
                : "Temps restant pour la journée :";
            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(tpsTravRestant.TotalSeconds < 0 ? tpsTravRestant.Negate() : tpsTravRestant);



            lblTpsTravReelLbl.Content = lblTpsTrav;
            lblTpsTravReel.Content = strTpsTrav;
            lblTpsTravReel.ToolTip = null;
            lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;

            lblTpsTravReel.Foreground = Cst.SCBBlack;


        }

        private void UpdateInfosTempsCdRealTime()
        {

            if (!PrgSwitchRef.PbarMainTimerActif)
            {
                return;
            }

            TimeSpan tpsTravRestant = TimesRef.EndTheoDateTime - RealTimesRef.RealTimeDtNow;
            TimeSpan tpsCdReal = OptionsRef.LastCdSeen.Subtract(tpsTravRestant);


            SolidColorBrush colFont = Cst.SCBBlack;
            if (tpsCdReal.Absolute() > OptionsRef.CompteurCDMaxAbs)
            {
                colFont = Cst.SCBDarkRed;
                tpsCdReal = OptionsRef.CompteurCDMaxAbs;
            } else if (tpsCdReal.TotalMinutes > 0)
            {
                colFont = Cst.SCBDarkGreen;
            }


            String lblTpsTrav = "Crédit/débit actuel :";


            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(tpsCdReal);
            string msgToolTip = String.Format("C/D relevé lors du dernier badgeage : {0}", MiscAppUtils.TimeSpanShortStrFormat(OptionsRef.LastCdSeen));


            lblTpsTravReelLbl.Content = lblTpsTrav;
            lblTpsTravReel.Content = strTpsTrav;
            lblTpsTravReel.Foreground = colFont;
            lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;


            lblTpsTravReel.ToolTip = "C/D calculé si départ maintenant (dernier C/D officiel relevé - temps restant)";
            lblTpsTravReelLbl.ToolTip = msgToolTip;



            /*

            if (RealTimesRef.RealTimeTsNow.CompareTo(OptionsRef.PlageFixeApremFin) >= 0)
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Visible;


                string tplTpsReelSuppl = "({0})";
                if (RealTimesRef.RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0)
                {
                    tplTpsReelSuppl = "(+{0})";

                    if (!PrgSwitchRef.IsMoreThanTpsTheo && RealTimesRef.RealTimeTempsTravaille.CompareTo(OptionsRef.TempsMaxJournee) < 0)
                    {
                        PrgSwitchRef.IsMoreThanTpsTheo = true;
                        lblTpsTravReel.Foreground = Cst.SCBDarkGreen;

                    }
                    else if (RealTimesRef.RealTimeTempsTravaille.CompareTo(OptionsRef.TempsMaxJournee) >= 0)
                    {
                        PrgSwitchRef.IsMoreThanTpsTheo = false;
                        lblTpsTravReel.Foreground = Cst.SCBDarkRed;
                    }
                }
                lblTpsTravReelSuppl.Content = String.Format(tplTpsReelSuppl, MiscAppUtils.TimeSpanShortStrFormat((RealTimesRef.RealTimeTempsTravaille - tTravTheo)));


            }
             * */
        }


        private void OnChangeCurrentState(CompteurState value)
        {
            lblTpsTravReelLbl.ContextMenu = GetLblCtxMenu();
        }


        public void SetVisibility(CompteurVisibility visibilityState)
        {
            switch (visibilityState)
            {
                case CompteurVisibility.Hidden:
                    lblTpsTravReel.Visibility = Visibility.Collapsed;
                    lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;
                    return;

                case CompteurVisibility.OnlyMain:
                    lblTpsTravReel.Visibility = Visibility.Visible;
                    lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;
                    return;

                case CompteurVisibility.Full:
                    lblTpsTravReel.Visibility = Visibility.Visible;
                    lblTpsTravReelSuppl.Visibility = Visibility.Visible;
                    return;
            }

        }

        public void SetFontColor(SolidColorBrush mainColor, SolidColorBrush secondeColor = null)
        {
            lblTpsTravReel.Foreground = mainColor;
            if (secondeColor != null)
            {
                lblTpsTravReelSuppl.Foreground = secondeColor;
            }
        }

        public void SetToolTip(string mainTooltip, string secondeTooltip = null)
        {
            lblTpsTravReel.ToolTip = mainTooltip;
            if (secondeTooltip != null)
            {
                lblTpsTravReelSuppl.ToolTip = secondeTooltip;
            }
        }

        public void SetText(string mainText, string secondeText = null)
        {
            lblTpsTravReel.Content = mainText;
            if (secondeText != null)
            {
                lblTpsTravReelSuppl.Content = secondeText;
            }
        }

        public void SetTitle(string title)
        {
            lblTpsTravReelLbl.Content = title;
        }
    }
}
