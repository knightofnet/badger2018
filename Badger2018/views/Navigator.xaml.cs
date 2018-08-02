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
using Badger2018.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour Navigator.xaml
    /// </summary>
    public partial class Navigator : Window
    {
        public Navigator()
        {
            InitializeComponent();
        }

        public void Navigate(string url)
        {

            IeUtils.Navigate(webB, url, 3);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
