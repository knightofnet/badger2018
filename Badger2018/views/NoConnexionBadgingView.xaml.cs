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
using System.Windows.Threading;
using Badger2018.constants;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour NoConnexionBadgingView.xaml
    /// </summary>
    public partial class NoConnexionBadgingView : Window
    {
        public MessageBoxResult Result { get; set; }

        private DispatcherTimer _timerClose;

        private int nbSecondeMaxTimeout;
        private int nbSecondeTimeout;

        public NoConnexionBadgingView(int timeout)
        {
            InitializeComponent();
            Result = MessageBoxResult.Cancel;

            nbSecondeMaxTimeout = timeout;
            nbSecondeTimeout = timeout;

            Closing += (sender, args) =>
            {
                if (_timerClose != null)
                {
                    _timerClose.Stop();
                }
            };


            _timerClose = new DispatcherTimer();
            _timerClose.Interval = new TimeSpan(0, 0, 1);
            _timerClose.Tick += (sender, args) =>
            {
                if (nbSecondeTimeout > 0)
                {
                    lblCptArebour.Content = nbSecondeTimeout + "s";
                    nbSecondeTimeout--;

                    pbarTimeout.Value = nbSecondeTimeout * 100 / nbSecondeMaxTimeout;
                }
                else
                {
                    _timerClose.Stop();
                    Result = MessageBoxResult.OK;
                    Close();
                }
            };
            _timerClose.Start();
        }



        private void btnBadger_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void lblCptArebour_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_timerClose != null)
            {
                if (_timerClose.IsEnabled)
                {
                    _timerClose.Stop();
                    lblCptArebour.Foreground = Cst.SCBDarkGreen;
                }
                else
                {
                    _timerClose.Start();
                    lblCptArebour.Foreground = Cst.SCBBlack;
                }
            }
        }
    }
}
