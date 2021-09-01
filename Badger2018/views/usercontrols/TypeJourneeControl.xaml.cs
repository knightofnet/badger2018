using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;

namespace Badger2018.views.usercontrols
{
    /// <summary>
    /// Logique d'interaction pour TypeJourneeControl.xaml
    /// </summary>
    public partial class TypeJourneeControl : UserControl
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private EnumTypesJournees _typeJournee;
        public Action<EnumTypesJournees> OnTypeJourneeChange;
        public Action<double> OnValTtChange;

        private bool _isEnabledChange;
        private double _valTt;

        public EnumTypesJournees TypeJournee
        {
            get { return _typeJournee; }
            set
            {
                _typeJournee = value;
                AdaptUiTypeJournee(value, _valTt);
                if (OnTypeJourneeChange != null)
                {
                    OnTypeJourneeChange(value);
                }
            }
        }

        public double ValTt
        {
            get { return _valTt; }
            set
            {
                _valTt = value;
                AdaptUiTypeJournee(_typeJournee, value);
                if (OnValTtChange != null)
                {
                    OnValTtChange(value);
                }
            }
        }

        public bool IsEnabledChange
        {
            get { return _isEnabledChange; }
            set
            {
                _logger.Debug("IsEnabledChange : {0}", value);
                _isEnabledChange = value;
                AdaptUiEnableState(value);
            }
        }



        public TypeJourneeControl()
        {
            InitializeComponent();


            foreach (EnumTypesJournees enumTyJ in EnumTypesJournees.Values)
            {
                cboxTypeJournee.Items.Add(enumTyJ.Libelle);
            }


            gridMod.Visibility = Visibility.Collapsed;


            lblTypeJournee.MouseEnter += (sender, args) =>
            {
                if (gridMod.Visibility == Visibility.Collapsed && IsEnabledChange)
                {
                    lblTypeJournee.Visibility = Visibility.Collapsed;
                    gridMod.Visibility = Visibility.Visible;

                    //cboxTypeJournee.Focus();
                }
            };


            gridMod.MouseLeave += (sender, args) =>
            {
                if (cboxTypeJournee.Visibility == Visibility.Visible && IsEnabledChange)
                {
                    lblTypeJournee.Visibility = Visibility.Visible;
                    gridMod.Visibility = Visibility.Collapsed;

                }
            };

            cboxTypeJournee.SelectionChanged += (sender, args) =>
            {

                string valSel = cboxTypeJournee.SelectedItem as string;
                if (valSel == null) { return; };

                TypeJournee = EnumTypesJournees.GetFromLibelle(valSel);


            };
            chkBoxTT.Click += (sender, args) =>
            {
                bool isChecked = chkBoxTT.IsChecked ?? false;

                if (isChecked)
                {
                    ValTt = EnumTypesJournees.IsDemiJournee(TypeJournee) ? 0.5 : 1;
                } else
                {
                    ValTt = 0;
                }

            };

        }

        private void AdaptUiTypeJournee(EnumTypesJournees value, double valTt)
        {
            String strToShow = value.Libelle;
            if (valTt > 0)
            {
                strToShow += " (TT)";
                chkBoxTT.IsChecked = true;
            }
            else
            {
                chkBoxTT.IsChecked = false;
            }

            lblTypeJournee.Content = strToShow;

            string valSel = cboxTypeJournee.SelectedItem as string;

            if (valSel == null || !value.Libelle.Equals(valSel))
            {
                cboxTypeJournee.SelectedItem = value.Libelle;
            }
        }

        private void AdaptUiEnableState(bool value)
        {
            if (!value)
            {
                gridMod.Visibility = Visibility.Collapsed;
                lblTypeJournee.Visibility = Visibility.Visible;


            }
        }

        internal void ChangeTypeJourneeWithoutAction(EnumTypesJournees tyJournee, double pWorkAtHomeCpt)
        {
            _typeJournee = tyJournee;
            _valTt = pWorkAtHomeCpt;
            AdaptUiTypeJournee(tyJournee, pWorkAtHomeCpt);
        }
    }
}
