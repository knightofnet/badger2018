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
using Badger2018.utils;
using BadgerCommonLibrary.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour SaisieManuelleTsView.xaml
    /// </summary>
    public partial class SaisieManuelleTsView : Window
    {

        public DateTime DateManuelle { get; set; }
        public bool IsRealClose { get; set; }

        public SaisieManuelleTsView()
        {
            InitializeComponent();

            tboxMhours.Text = AppDateUtils.DtNow().ToString("HH:mm");
        }

        private void btnValider_Click(object sender, RoutedEventArgs e)
        {
            ValideAndClose();
        }

        private void ValideAndClose()
        {
            String tboxTsStr = tboxMhours.Text;
            TimeSpan tboxTs;
            if (TimeSpan.TryParse(tboxTsStr, out tboxTs))
            {
                DateManuelle = AppDateUtils.DtNow().ChangeTime(tboxTs);
                IsRealClose = true;
                Close();
            }
            else
            {
                MessageBox.Show("L'heure de saisie manuelle doit être au format HH:mm.");
                tboxMhours.Focus();

            }
        }

        public static DateTime? ShowAskForDateTime()
        {
            SaisieManuelleTsView s = new SaisieManuelleTsView();
            s.ShowDialog();

            if (s.IsRealClose)
            {

                return s.DateManuelle;
            }

            return null;
        }

        private void tboxMhours_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValideAndClose();
            }
        }
    }
}
