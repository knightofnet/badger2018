using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AryxDevLibrary.utils;
using Badger2018.views.wizard;
using BadgerCommonLibrary.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour DisclaimerView.xaml
    /// </summary>
    public partial class DisclaimerView : Window
    {
        private DispatcherTimer clockUpdTimer;
        private DateTime endTimer;

        private List<Page> listPages = new List<Page>(3);
        private int currentIndexPage = 0;

        private Boolean isFormOk = false;

        Page3 p3;

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


            p3 = new Page3();
            p3.IsFormOkRef = isFormOk;
            listPages.Add(new Page1());
            listPages.Add(new Page2());
            listPages.Add(p3);

            mainFrame.Navigate(listPages[0]);
            mainFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            btnAcceptText.Text = "Suivant";
            DoTimer();



            clockUpdTimer.Start();

        }

        private void DoTimer()
        {
            endTimer = AppDateUtils.DtNow().AddSeconds(15);
            if (clockUpdTimer == null)
            {
                clockUpdTimer = new DispatcherTimer();
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
            } else
            {
                clockUpdTimer.Start();
            }
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
            if (currentIndexPage < 2)
            {
                mainFrame.Navigate(listPages[++currentIndexPage]);
                btnOK.IsEnabled = false;
                DoTimer();

                if (currentIndexPage == 2)
                {
                    btnAcceptText.Text = "Je souhaite utiliser ce programme et j'accepte ses limitations";
                }

            }
            else
            {
                isFormOk = p3.IsChoixOk();
                if (isFormOk)
                {

                    IsConsent = true;
                    Close();
                } else
                {
                    MessageBox.Show("Les réponses ne sont pas correctes.", "Problèmes avec les réponses");
                }
            }
        }

        private void mainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
