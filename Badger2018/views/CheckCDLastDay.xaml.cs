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
using AryxDevLibrary.extensions;
using Badger2018.dto;
using Badger2018.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour CheckCDLastDay.xaml
    /// </summary>
    public partial class CheckCDLastDay : Window
    {
        private readonly MainWindow.IAppOptionsProvider _prgOptRef;

        public DateTime LastDay { get; set; }
        public TimeSpan CdVeille { get; set; }
        public TimeSpan TpsTravLastDay { get; set; }
        public TimeSpan CdVeilleAfter { get; set; }
        public DateTime CurrDay { get; set; }
        public TimeSpan CdToday { get; set; }
        public TimeSpan TpsTravDiff { get; internal set; }
        public MessageBoxResult Return { get; private set; }

        public bool HasChanged { get; private set; }

        public CheckCDLastDay(MainWindow.IAppOptionsProvider prgOptRef)
        {
            InitializeComponent();
            this._prgOptRef = prgOptRef;

            Return = MessageBoxResult.Cancel;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            runLastDayA.Text = LastDay.ToLongDateString();
            runLastDayB.Text = LastDay.ToLongDateString().FirstUpperOtherLower();
            runCurrDay.Text = CurrDay.ToLongDateString().FirstUpperOtherLower();

            runLastDayCdAtStart.Text = CdVeille.ToStrSignedhhmm() ;
            runLastDayCdAtEnd.Text = CdVeilleAfter.ToStrSignedhhmm() ;
            runLastDayTpsTrav.Text = TpsTravLastDay.ToStrSignedhhmm() + "(" + TpsTravDiff.ToStrSignedhhmm() + ")";

            runCurrDayCd.Text = CdToday.ToStrSignedhhmm();

        }

     

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            Return = MessageBoxResult.Yes;
            CommonClose();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            Return = MessageBoxResult.No;
            CommonClose();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Return = MessageBoxResult.Cancel;
            CommonClose();
        }

        private void CommonClose()
        {
            bool isCheckCdLastDay = !(chkBoxNotAsk.IsChecked ?? false);
            if (isCheckCdLastDay != _prgOptRef.PrgOptions.IsCheckCDLastDay)
            {
                _prgOptRef.PrgOptions.IsCheckCDLastDay = isCheckCdLastDay;
                HasChanged = true;
            }

            Close();
        }

  
    }
}
