using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour ScreenProgressBarView.xaml
    /// </summary>
    public partial class ScreenProgressBarView : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        private int position = 0;

        public ScreenProgressBarView(int position=0)
        {
            InitializeComponent();

            this.position = position;

            rValue.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            rValue.SnapsToDevicePixels = true;


            WindowStartupLocation = WindowStartupLocation.Manual;

            InitPosition();

            // https://stackoverflow.com/questions/56645242/hide-a-wpf-form-from-alttab
            Loaded += (e, s) =>
            {
                //Variable to hold the handle for the form
                var helper = new WindowInteropHelper(this).Handle;
                //Performing some magic to hide the form from Alt+Tab
                SetWindowLong(helper, GWL_EX_STYLE, (GetWindowLong(helper, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

            };

            
        }

        private void InitPosition()
        {

            switch (position)
            {
                case 0:

                    Width = SystemParameters.PrimaryScreenWidth;
                    Height = 1;
                    /*
                    Top = System.Windows.SystemParameters.PrimaryScreenHeight - Height;
                    */
                    Top = 0;
                    Left = 0;
                    break;

                case 1:
                    Width = 1  ;
                    Height = SystemParameters.PrimaryScreenHeight;

                    Top = 0;
                    Left = (int) SystemParameters.PrimaryScreenWidth - 1 - 0.5;
                    break;

            }



        }

        private void setValuePbar(double value)
        {
            switch (position)
            {
                case 0:

                    rValue.Width = (value * Width) / 100; 
                    break;

                case 1:

                    rValue.Height = (value * Height) / 100; 

                    break;

            }

        }


            internal void PairWithProgressBar(ProgressBar pbarTime)
        {
            pbarTime.ValueChanged += (s,a) => {
                setValuePbar(pbarTime.Value);

            };
        }

        public void BringToFront()
        {
            Topmost = true;

            Topmost = false;
        }
    }
}
