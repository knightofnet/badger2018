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
using Badger2018.constants;

namespace Badger2018.views.usercontrols
{
    /// <summary>
    /// Logique d'interaction pour FourCornerProgressbar.xaml
    /// </summary>
    public partial class FourCornerProgressbar : UserControl
    {
        private double _thickness;
        private double _value;
        private bool _isIndeterminate;
        private SolidColorBrush _color;
        private SolidColorBrush _backgroundColor;

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                SetIndeterminate(value);
            }
        }


        public double Thickness
        {
            get { return _thickness; }
            set { _thickness = value; SetThickness(value); }
        }

        public double Value
        {
            get { return _value; }
            set { _value = value; SetValue(value); }
        }

        public SolidColorBrush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                SetColor(value);
            }
        }

        public SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; SetBackgroundColor(value); }
        }


        public FourCornerProgressbar()
        {
            InitializeComponent();
            Background = null;
        }

        private void SetThickness(double thick)
        {
            pbTop.Height = thick;
            pbLeft.Width = thick;
            pbBottom.Width = thick;
            pbRight.Width = thick;

            pbTop.Margin = new Thickness(thick, 0, 0, 0);
            pbLeft.Margin = new Thickness(0, 0, 0, thick);
            pbBottom.Margin = new Thickness(0, 0, thick, 0);
            pbRight.Margin = new Thickness(0, thick, 0, thick);
        }

        private void SetValue(double value)
        {
            double c = value;
            if (value >= 75)
            {
                pbTop.Value = (value - 75) * 4;
                c = value - (value - 75);
            }
            else
            {
                pbTop.Value = 0;
            }


            if (c >= 50)
            {
                pbLeft.Value = (c - 50) * 4;
                c = c - (c - 50);
            }
            else
            {
                pbLeft.Value = 0;
            }

            if (c >= 25)
            {
                pbBottom.Value = (c - 25) * 4;
                c = c - (c - 25);
            }
            else
            {
                pbBottom.Value = 0;
            }


            pbRight.Value = c * 4;
        }


        private void SetIndeterminate(bool value)
        {
            pbTop.IsIndeterminate = value;
            pbLeft.IsIndeterminate = value;
            pbBottom.IsIndeterminate = value;
            pbRight.IsIndeterminate = value;
        }


        private void SetColor(SolidColorBrush value)
        {
            if (value == null)
            {
                value = Cst.SCBGreenPbar;
                _color = value;
            }

            pbTop.Foreground = value;
            pbLeft.Foreground = value;
            pbBottom.Foreground = value;
            pbRight.Foreground = value;
        }

        private void SetBackgroundColor(SolidColorBrush value)
        {

            pbTop.Background = value;
            pbLeft.Background = value;
            pbBottom.Background = value;
            pbRight.Background = value;
        }
    }
}
