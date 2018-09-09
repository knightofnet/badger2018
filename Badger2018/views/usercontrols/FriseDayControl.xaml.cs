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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Badger2018.utils;

namespace Badger2018.views.usercontrols
{
    /// <summary>
    /// Logique d'interaction pour FriseDayControl.xaml
    /// </summary>
    public partial class FriseDayControl : UserControl
    {
        public static FriseDayControl NewInstance(DateTime hour, string name, string more, Color color)
        {
            FriseDayControl f = new FriseDayControl(hour, name, more);
            f.rectColor.Fill = new SolidColorBrush(color);
            f.rectSep.Fill = new SolidColorBrush(color);

            return f;
        }

        public FriseDayControl(DateTime hour, string name, string more)
        {
            InitializeComponent();
            mainGrid.Background = null;

            lblHours.ContentShortTime(hour);
            lblName.Content = name;
            lblMoreStr.Content = more;


        }
    }
}
