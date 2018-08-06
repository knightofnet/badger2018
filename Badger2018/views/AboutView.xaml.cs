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

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {
        public AboutView()
        {
            InitializeComponent();
            lblVersion.Content = String.Format(lblVersion.Content.ToString(), System.Reflection.Assembly.GetExecutingAssembly().GetName().Version, Properties.Resources.versionName);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
