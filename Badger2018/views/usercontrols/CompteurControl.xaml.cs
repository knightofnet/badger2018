using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Badger2018.business;
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
        private MainWindow _pwinRef;

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
                if (PwinRef != null)
                {
                    PwinRef.PrgOptions.CptCtrlStateShowned = (int)_currentState;
                    // OptionManager.SaveOptions(PwinRef.PrgOptions);
                }
                OnChangeCurrentState(value);


            }
        }



        public bool IsEnabledCtrl { get; set; }

        //public TimesBadgerDto TimesRef { get; set; }
        //public AppOptions OptionsRef { get; set; }
        // public AppSwitchs PrgSwitchRef { get; set; }
        //public RealTimesObj RealTimesRef { get; set; }
        // public EnumTypesJournees TyJourneeRef { get; set; }

        public MainWindow PwinRef
        {
            get { return _pwinRef; }
            set
            {
                _pwinRef = value;
                CurrentState = (CompteurState)value.PrgOptions.CptCtrlStateShowned;
                UpdateInfos();
            }
        }

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

            //TimeSpan tpsTravRestant = PwinRef.Times.EndTheoDateTime - PwinRef.RealTimes.RealTimeDtNow;

            // Tps trav pour une journée ou une demie-journée.
            TimeSpan tTravTheo = TimesUtils.GetTpsTravTheoriqueOneDay(PwinRef.PrgOptions, PwinRef.TypeJournee);


            String lblTpsTrav = "Compteur temps travaillé du jour :";
            String msgTooltip = String.Format("{0}Double-cliquer pour afficher le temps de travail restant",
                PwinRef.PrgOptions.IsAdd5minCpt ? "Le temps travaillé prend en compte les 5 min supplémentaires." + Environment.NewLine : "");



            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(PwinRef.RealTimes.RealTimeTempsTravaille);
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
            if (PwinRef.Times.IsTherePauseAprem() || PwinRef.Times.IsTherePauseMatin())
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





            if (PwinRef.RealTimes.RealTimeTsNow.CompareTo(PwinRef.PrgOptions.PlageFixeApremFin) >= 0)
            {
                lblTpsTravReelSuppl.Visibility = Visibility.Visible;


                string tplTpsReelSuppl = "({0})";
                if (PwinRef.RealTimes.RealTimeTempsTravaille.CompareTo(tTravTheo) >= 0)
                {
                    tplTpsReelSuppl = "(+{0})";

                    if (!PwinRef.PrgSwitch.IsMoreThanTpsTheo && PwinRef.RealTimes.RealTimeTempsTravaille.CompareTo(PwinRef.PrgOptions.TempsMaxJournee) < 0)
                    {
                        PwinRef.PrgSwitch.IsMoreThanTpsTheo = true;
                        colFont = Cst.SCBDarkGreen;

                    }
                    else if (PwinRef.RealTimes.RealTimeTempsTravaille.CompareTo(PwinRef.PrgOptions.TempsMaxJournee) >= 0)
                    {
                        PwinRef.PrgSwitch.IsMoreThanTpsTheo = false;
                        colFont = Cst.SCBDarkRed;
                    }
                }
                lblTpsTravReelSuppl.Content = String.Format(tplTpsReelSuppl, MiscAppUtils.TimeSpanShortStrFormat((PwinRef.RealTimes.RealTimeTempsTravaille - tTravTheo)));


            }

            lblTpsTravReel.Foreground = colFont;
        }


        private void UpdateInfosTempsTpsRestant()
        {
            TimeSpan tpsTravRestant = PwinRef.Times.EndTheoDateTime - PwinRef.RealTimes.RealTimeDtNow;

            String lblTpsTrav = "Temps restant pour la journée :";
            lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;
            if (tpsTravRestant.TotalSeconds < 0)
            {
                DateTime endRaw = PwinRef.Times.EndRawDateTime -
                                  (PwinRef.PrgOptions.IsAdd5minCpt ? new TimeSpan(0, 5, 0) : TimeSpan.Zero);
                tpsTravRestant = PwinRef.RealTimes.RealTimeDtNow - endRaw;
                lblTpsTrav = "Temps supplémentaire pour la journée :";
                TimeSpan tpsRestant = PwinRef.Times.MaxTimeForOneDay - PwinRef.RealTimes.RealTimeDtNow;
                var supplLblTpsTrav = String.Format(" ({0})", MiscAppUtils.TimeSpanShortStrFormat(tpsRestant.Negate()));
                lblTpsTravReelSuppl.Content = supplLblTpsTrav;
                if (tpsRestant.TotalSeconds > 0)
                {
                    lblTpsTravReelSuppl.Visibility = Visibility.Visible;
                }

            }

            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(tpsTravRestant);

            lblTpsTravReelLbl.Content = lblTpsTrav;
            lblTpsTravReel.Content = strTpsTrav;
            lblTpsTravReel.ToolTip = null;


            lblTpsTravReel.Foreground = Cst.SCBBlack;


        }

        private void UpdateInfosTempsCdRealTime()
        {
            /*
            if (!PwinRef.PrgSwitch.PbarMainTimerActif)
            {
                return;
            }*/

            TimeSpan tpsTravRestant = PwinRef.RealTimes.RealTimeMinTpsTravRestant;
            TimeSpan tpsCdReal = PwinRef.PrgOptions.LastCdSeen.Subtract(tpsTravRestant);


            SolidColorBrush colFont = Cst.SCBBlack;
            if (tpsCdReal.Absolute() > PwinRef.PrgOptions.CompteurCDMaxAbs)
            {
                colFont = Cst.SCBDarkRed;
                tpsCdReal = PwinRef.PrgOptions.CompteurCDMaxAbs;
            }
            else if (tpsCdReal.TotalMinutes > 0)
            {
                colFont = Cst.SCBDarkGreen;
            }


            String lblTpsTrav = "Crédit/débit actuel :";


            String strTpsTrav = MiscAppUtils.TimeSpanShortStrFormat(tpsCdReal);
            string msgToolTip = String.Format("C/D relevé lors du dernier badgeage : {0}", MiscAppUtils.TimeSpanShortStrFormat(PwinRef.PrgOptions.LastCdSeen));


            lblTpsTravReelLbl.Content = lblTpsTrav;
            lblTpsTravReel.Content = strTpsTrav;
            lblTpsTravReel.Foreground = colFont;
            lblTpsTravReelSuppl.Visibility = Visibility.Collapsed;


            lblTpsTravReel.ToolTip = "C/D calculé si départ maintenant (dernier C/D officiel relevé - temps restant)";
            lblTpsTravReelLbl.ToolTip = msgToolTip;



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
