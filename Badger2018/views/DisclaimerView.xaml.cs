using AryxDevLibrary.utils;
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
using Badger2018.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour DisclaimerView.xaml
    /// </summary>
    public partial class DisclaimerView : Window
    {
        readonly DispatcherTimer clockUpdTimer = new DispatcherTimer();

        public DisclaimerView(bool isFirstRun)
        {
            InitializeComponent();

            if (isFirstRun)
            {
                lblFirst.Visibility = Visibility.Visible;
            }
            else
            {
                lblFirst.Visibility = Visibility.Collapsed;
            }

            DateTime endTimer = AppDateUtils.DtNow().AddSeconds(20);

            clockUpdTimer.Interval = new TimeSpan(0, 0, 1);
            clockUpdTimer.Tick += (sender, args) =>
            {
                TimeSpan remainingTimer = endTimer - AppDateUtils.DtNow();
                if (remainingTimer < TimeSpan.Zero)
                {
                    btnOK.IsEnabled = true;
                    clockUpdTimer.Stop();

                }
            };


            clockUpdTimer.Start();

        }

        public bool IsConsent { get; internal set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsConsent = false;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                FileUtils.ShowFileInWindowsExplorer(@"C:\Users\" + Environment.UserName + @"\AppData\Local");

            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                IsConsent = true;
            }

            Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            IsConsent = true;
            Close();
        }
    }
}
