using System.Windows;
using Badger2018.dto;

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
