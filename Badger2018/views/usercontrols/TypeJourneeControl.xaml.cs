using Badger2018.constants;
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

namespace Badger2018.views.usercontrols
{
    /// <summary>
    /// Logique d'interaction pour TypeJourneeControl.xaml
    /// </summary>
    public partial class TypeJourneeControl : UserControl
    {
        private EnumTypesJournees _typeJournee;
        public Action<EnumTypesJournees> OnTypeJourneeChange;
        private bool _isEnabledChange;

        public EnumTypesJournees TypeJournee
        {
            get { return _typeJournee; }
            set
            {
                _typeJournee = value;
                AdaptUiTypeJournee(value);
                if (OnTypeJourneeChange != null)
                {
                    OnTypeJourneeChange(value);
                }
            }
        }

        public bool IsEnabledChange
        {
            get { return _isEnabledChange; }
            set
            {
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


            cboxTypeJournee.Visibility = Visibility.Collapsed;

            lblTypeJournee.MouseEnter += (sender, args) =>
            {
                if (cboxTypeJournee.Visibility == Visibility.Collapsed && IsEnabledChange)
                {
                    lblTypeJournee.Visibility = Visibility.Collapsed;
                    cboxTypeJournee.Visibility = Visibility.Visible;
                    //cboxTypeJournee.Focus();
                }
            };


            cboxTypeJournee.MouseLeave += (sender, args) =>
            {
                if (cboxTypeJournee.Visibility == Visibility.Visible && IsEnabledChange)
                {
                    lblTypeJournee.Visibility = Visibility.Visible;
                    cboxTypeJournee.Visibility = Visibility.Collapsed;
                }
            };

            cboxTypeJournee.SelectionChanged += (sender, args) =>
            {

                string valSel = cboxTypeJournee.SelectedItem as string;
                if (valSel == null) { return; };

                TypeJournee = EnumTypesJournees.GetFromLibelle(valSel);


            };

        }

        private void AdaptUiTypeJournee(EnumTypesJournees value)
        {
            lblTypeJournee.Content = value.Libelle;

            string valSel = cboxTypeJournee.SelectedItem as string;

            if (valSel == null || !value.Libelle.Equals(valSel))
            {
                cboxTypeJournee.SelectedItem = value.Libelle;
            }
        }

        private void AdaptUiEnableState(bool value)
        {
            //throw new NotImplementedException();
        }

        internal void ChangeTypeJourneeWithoutAction(EnumTypesJournees tyJournee)
        {
            _typeJournee = tyJournee;
            AdaptUiTypeJournee(tyJournee);
        }
    }
}
