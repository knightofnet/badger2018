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

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour BadgeageZeroConfirmView.xaml
    /// </summary>
    public partial class BadgeageZeroConfirmView : Window
    {
        private MainWindow.IAppOptionsProvider prgOptRef;

        public bool IsChoiceDone { get; private set; }
        public bool IsSetChoiceToMemory { get; private set; }
        public EnumBadgeageZeroAction Action { get; private set; }

        public BadgeageZeroConfirmView(MainWindow.IAppOptionsProvider optProvider)
        {
            InitializeComponent();
            
            prgOptRef = optProvider;
            Action = EnumBadgeageZeroAction.NO_CHOICE;
            
        }

        private void btnBadge_Click(object sender, RoutedEventArgs e)
        {
            Action = EnumBadgeageZeroAction.BADGER;
            CloseView();
        }


        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            Action = EnumBadgeageZeroAction.REPORT_HEURE;
            CloseView();
        }


        private void CloseView()
        {
            IsChoiceDone = true;
            IsSetChoiceToMemory = chkBoxMemory.IsChecked ?? false;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
