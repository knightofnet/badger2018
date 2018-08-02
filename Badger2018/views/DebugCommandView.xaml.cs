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
    /// Logique d'interaction pour DebugCommandView.xaml
    /// </summary>
    public partial class DebugCommandView : Window
    {
        public DebugCommandView()
        {
            InitializeComponent();

            tbox.Text = AppDateUtils.DtNow().ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime newTboxTmin = new DateTime();
            if (DateTime.TryParse(tbox.Text, out newTboxTmin))
            {
                AppDateUtils.ForceDtNow(newTboxTmin);

            }
            else
            {
                AppDateUtils.ForceDtNow(null);
            }
        }
    }
}
