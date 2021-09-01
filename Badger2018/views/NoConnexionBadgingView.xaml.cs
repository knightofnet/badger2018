using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using Badger2018.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour NoConnexionBadgingView.xaml
    /// </summary>
    public partial class NoConnexionBadgingView : Window
    {
        private MainWindow.IAppOptionsProvider prgOptRef;
        public MessageBoxResult Result { get; set; }

        private string url;
        private readonly DispatcherTimer _timerClose;
        private BackgroundWorker testConnexionBackgroundWorker;

        private readonly int nbSecondeMaxTimeout;
        private int nbSecondeTimeout;

        public NoConnexionBadgingView(int timeout, string urlToTest)
        {
            InitializeComponent();
            Result = MessageBoxResult.Cancel;

            pbarTimeout.Value = 1;
            lblCptArebour.Content = null;
            url = urlToTest;

            nbSecondeMaxTimeout = timeout;
            nbSecondeTimeout = timeout;

            Closing += (sender, args) =>
            {
                if (_timerClose != null)
                {
                    _timerClose.Stop();
                }
            };


            testConnexionBackgroundWorker = new BackgroundWorker();
            testConnexionBackgroundWorker.WorkerSupportsCancellation = true;
            testConnexionBackgroundWorker.DoWork += TestConnexionBackgroundWorkerOnDoWork;
            testConnexionBackgroundWorker.RunWorkerAsync();

            _timerClose = new DispatcherTimer();
            _timerClose.Interval = new TimeSpan(0, 0, 1);
            _timerClose.Tick += (sender, args) =>
            {
                if (nbSecondeTimeout > 0 && testConnexionBackgroundWorker.IsBusy)
                {
                    if (nbSecondeTimeout < 1000)
                    {
                        lblCptArebour.Content = nbSecondeTimeout + "s";
                    }
                    nbSecondeTimeout--;

                    pbarTimeout.Value = (double)nbSecondeTimeout * 100 / nbSecondeMaxTimeout;
                }
                else
                {
                    _timerClose.Stop();
                    Result = MessageBoxResult.OK;
                    Close();
                }
            };
            _timerClose.Start();

            Loaded += (sender, args) =>
            {
                Topmost = true;
            };
            MouseEnter += (sender, args) =>
            {
                Topmost = false;
            };
        }

        private void TestConnexionBackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = sender as BackgroundWorker;

            bool isOk = false;

            while (!bg.CancellationPending)
            {
                if (!BadgingUtils.IsValidWebResponse(url))
                {
                    Thread.Sleep(500);
                }
                else
                {
                    if (isOk)
                    {
                        break;
                    }
                    Thread.Sleep(3500);
                    isOk = BadgingUtils.IsValidWebResponse(url);
                }
            }
            
            e.Result = true;
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
