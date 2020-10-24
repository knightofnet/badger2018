using System;
using System.Reflection;
using System.Windows;
using Badger2018.dto;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {

        private LicenceInfo _licenceRef;

        public LicenceInfo LicenceRef
        {
            get
            {
                return _licenceRef;
            }

            set
            {
                _licenceRef = value;
                AdapteUi();
            }

        }

        public AboutView()
        {
            InitializeComponent();
            lblVersion.Content = String.Format(lblVersion.Content.ToString(), Assembly.GetExecutingAssembly().GetName().Version, Properties.Resources.versionName);
            lblYearVersion.Content = String.Format(lblYearVersion.Content.ToString(), DateTime.Now.Year);

        }

 

        private void AdapteUi()
        {

            lblLicenceUser.Content = LicenceRef.NiceName.Trim();

            lblLicence.Content = String.Format("Licence {0}. ", LicenceRef.TypeUser == 0 ? "ambassadeur" : "utilisateur");
            if (LicenceRef.TypeUser == 0)
            {
                lblLicence.Content += "Validitée perpétuelle.";                
            } else
            {
                lblLicence.Content += String.Format("Valide jusqu'au {0}.", LicenceRef.DateExpiration.ToShortDateString());
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
