using Badger2018.dto;
using Badger2018.utils;
using BadgerCommonLibrary.utils;
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


namespace Badger2018.views.usercontrols.semaine
{
    /// <summary>
    /// Logique d'interaction pour HoverInfoControl.xaml
    /// </summary>
    public partial class HoverInfoControl : UserControl
    {
        public HoverInfoControl()
        {
            InitializeComponent();

            frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
        }

        public void LoadsFromPeriodeG(JourSemaineControl.PeriodeG pG, AppOptions appOptions)
        {

            switch (pG.TypePeriode)
            {
                case 0:
                    HoverPeriodePage p = new HoverPeriodePage();
                    p.Loads(pG, appOptions);
                    frame.Navigate(p);
                    break;
                case 1:
                    HoverPointPage pt = new HoverPointPage();
                    pt.Loads(pG, appOptions);
                    frame.Navigate(pt);
                    break;
            }

        }
    }
}
