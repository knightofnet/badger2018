using System;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BadgerCommonLibrary.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour ModerNotifView.xaml
    /// </summary>
    public partial class ModerNotifView : Window
    {


        private ModerNotifView()
        {
            InitializeComponent();


        }

        public void SetTitle(string title)
        {
            lblTitle.Content = title;
        }

        public void SetText(string text)
        {
            lblText.Text = text;
        }

        public static void ShowNotif(string title, string text, int timeout)
        {
            ModerNotifView v = new ModerNotifView();
            v.WindowStartupLocation = WindowStartupLocation.Manual;
            v.Left = SystemParameters.PrimaryScreenWidth - v.Width ;
            v.SetTitle(title);
            v.SetText(text);

            v.Show();
            SystemSounds.Asterisk.Play();



            DateTime endTimer = AppDateUtils.DtNow().AddMilliseconds(timeout);
            DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100) };

            timer.Tick += (sender, args) =>
            {
                TimeSpan remainingTimer = endTimer - AppDateUtils.DtNow();
                if (remainingTimer >= TimeSpan.Zero) return;

                v.Hide();
                timer.Stop();
            };


            timer.Start();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
