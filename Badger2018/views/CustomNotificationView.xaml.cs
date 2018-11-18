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
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour CustomNotificationView.xaml
    /// </summary>
    public partial class CustomNotificationView : Window
    {


        public CustomNotificationDto NotifB { get; private set; }
        public CustomNotificationDto NotifA { get; private set; }

        public bool IsOkClose { get; private set; }

        public CustomNotificationView(CustomNotificationDto notifA, CustomNotificationDto notifB, AppOptions options, TimesBadgerDto times)
        {
            InitializeComponent();

            NotifA = notifA;
            NotifB = notifB;


            custNotifA.LoadsUi(NotifA, options, times.EndTheoDateTime, times.EndMoyPfMatin, times.EndMoyPfAprem);
            custNotifB.LoadsUi(NotifB, options, times.EndTheoDateTime, times.EndMoyPfMatin, times.EndMoyPfAprem);


        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            IsOkClose = true;

            bool cAisControlOK = custNotifA.IsControlOk();
            bool cBisControlOK = custNotifB.IsControlOk();

            if (cAisControlOK && cBisControlOK)
            {
                Close();

            }

        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
