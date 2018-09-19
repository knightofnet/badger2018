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

        public event Action<DateTime> OnClickBtnSeeScreenshot;

        public DateTime DtTimeIn { get; set; }

        public String Title { get; set; }
        public String SubTitle { get; set; }

        public String LTag { get; set; }
        public Color Color { get; set; }



        public FriseDayControl()
        {
            InitializeComponent();
            mainGrid.Background = null;


        }
        public FriseDayControl(DateTime hour, string title, string subtitle)
            : this()
        {

            DtTimeIn = hour;
            Title = title;
            SubTitle = subtitle;

            RefreshUi();
        }

        public void RefreshUi()
        {
            lblHours.ContentShortTime(DtTimeIn);
            lblName.Content = Title;
            lblMoreStr.Content = SubTitle;
            rectColor.Fill = new SolidColorBrush(Color);
            rectSep.Fill = new SolidColorBrush(Color);
        }

        public void SetBtnScreenshotVisible(bool isVisible)
        {
            lblBtnSeeScreenShot.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnSeeScreenShot_Click(object sender, RoutedEventArgs e)
        {
            if (OnClickBtnSeeScreenshot != null)
            {
                OnClickBtnSeeScreenshot(DtTimeIn);
            }
        }


    }
}
