using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Threading;
using BadgerCommonLibrary.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour AskForTtView.xaml
    /// </summary>
    public partial class AskForTtView : Window
    {
        private readonly MainWindow.IAppOptionsProvider prgOptRef;
        public bool IsDayTt { get; private set; }

        public bool HasChanged { get; private set; }
        public Action<AskForTtView> ActionOnClose { get; set; }

        private DispatcherTimer _timer = null;

        private const int TimeoutInit = 300;
        private int _timeoutCurr = 0;

        public AskForTtView(MainWindow.IAppOptionsProvider options)
        {
            InitializeComponent();
            prgOptRef = options;

            Loaded += (sender, args) =>
            {
                pbarTimeout.Value = 100;
                _timeoutCurr = TimeoutInit;
                _timer = new DispatcherTimer(DispatcherPriority.ContextIdle);
                _timer.Interval = new TimeSpan(0, 0, 1);
                _timer.Tick += (o, eventArgs) =>
                {
                    _timeoutCurr--;
                    if (_timeoutCurr > 0)
                    {
                        pbarTimeout.Value = (double)(_timeoutCurr * 100) / TimeoutInit;
                    }
                    else
                    {
                        _timer.Stop();
                        Close();
                    }
                };
                _timer.Start();
                Topmost = true;
            };

            pbarTimeout.MouseDoubleClick += (sender, args) =>
            {
                if (_timer == null) return;

                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                }

                _timeoutCurr = TimeoutInit;

                _timer.Start();
            };

            MouseEnter += (sender, args) =>
            {
                Topmost = false;
            };
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            IsDayTt = true;
            CloseWindowAndModOptions();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            IsDayTt = false;
            CloseWindowAndModOptions();
        }

        private void CloseWindowAndModOptions()
        {
            int nbWeek = 0;
            if (chkBoxNotAskForThisWeek.IsChecked ?? false)
            {
                nbWeek = new GregorianCalendar(GregorianCalendarTypes.Localized).GetWeekOfYear(AppDateUtils.DtNow(),
                   CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            }
            if (prgOptRef.PrgOptions.LastWeekNbrTtChecked != nbWeek)
            {
                prgOptRef.PrgOptions.LastWeekNbrTtChecked = nbWeek;
                HasChanged = true;
            }

            bool isCanAskForTt = !(chkBoxNotAsk.IsChecked ?? false);
            if (isCanAskForTt != prgOptRef.PrgOptions.IsCanAskForTT)
            {
                prgOptRef.PrgOptions.IsCanAskForTT = isCanAskForTt;
                HasChanged = true;
            }

            ActionOnClose?.Invoke(this);

            Close();
        }


        public void StopTimer()
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
            }
        }
    }
}
