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
using Badger2018.dto;
using Badger2018.utils;

namespace Badger2018.views.usercontrols.semaine
{
    /// <summary>
    /// Logique d'interaction pour JourSemaineControl.xaml
    /// </summary>
    public partial class JourSemaineControl : UserControl
    {

        public interface IPresenterJsCtrl
        {
            void MouseEnterZone(PeriodeG periodeG, double AbsoluteTop);
            void MouseLeaveZone(PeriodeG periodeG);
        }


        public string Header { get; set; }

        public double AbsoluteTop { get; set; }

        public TimeSpanToPixelsFrise TsToPixRef { get; private set; }
        private IPresenterJsCtrl Presenter { get; }
        public InfosDay InfosDay { get; internal set; }

        private readonly List<PeriodeG> _listPeriodes = new List<PeriodeG>();

        public JourSemaineControl()
        {
            InitializeComponent();


            Height = 26;
        }

        public JourSemaineControl(IPresenterJsCtrl presenter,TimeSpanToPixelsFrise tsToPix)
        {
            InitializeComponent();
            Background = null;

            TsToPixRef = tsToPix;
            Presenter = presenter;

            Width = tsToPix.EndDayPosition + 50;
        }

        public PeriodeG AddPeriode(String name, TimeSpan start, TimeSpan end, Color color)
        {

            PeriodeG periodeG = AddPeriodeGeneric(name, start, end, color);

            periodeG.PerRectangle.MouseEnter += (s, a) =>
            {
                periodeG.PerRectangle.Stroke = new SolidColorBrush(MiscAppUtils.Opacify(1, color));
                periodeG.PerRectangle.StrokeThickness = 2;

                Presenter.MouseEnterZone(periodeG, AbsoluteTop);
            };
            periodeG.PerRectangle.MouseLeave += (s, a) =>
            {
                periodeG.PerRectangle.Stroke = null;
                periodeG.PerRectangle.StrokeThickness = 0;

                Presenter.MouseLeaveZone(periodeG);
            };

            periodeG.TypePeriode = 0;

            return periodeG;
        }

        internal PeriodeG AddEmptyPeriode(String name, TimeSpan start, TimeSpan end)
        {
            PeriodeG pG = AddPeriodeGeneric(name, start, end, Colors.White);


            pG.PerRectangle.Stroke = new SolidColorBrush(MiscAppUtils.Opacify(0.5, Colors.DodgerBlue));
            pG.PerRectangle.StrokeThickness = 2;

            return pG;
        }

        public PeriodeG AddPeriodeGeneric(String name, TimeSpan start, TimeSpan end, Color color )
        {

            // heure/pos départ



            PeriodeG per = new PeriodeG();
            per.InfosDay = InfosDay;
            per.StartTs = start;
            per.EndTs = end;

            string tooltip = null;
            if (start.Equals(end))
            {
                end = end.Add(new TimeSpan(0, 1, 0));

            } else
            {

            }
          

            Rectangle rect = TsToPixRef.RectTopLeftAlignFromTs(start, end);
            rect.SnapsToDevicePixels = true;
            rect.Height = 26;
            rect.Fill = new SolidColorBrush(color);
            if (rect.Width < 1)
            {
                rect.Width = 1;
            }

            rect.Cursor = Cursors.Hand;

          



            per.PerRectangle = rect;
            
            
            
                

            per.Name = name;

            if (_listPeriodes.Any(r => r.Name.Equals(name)))
            {
                _listPeriodes.RemoveAll(r => r.Name.Equals(name));
            }

            _listPeriodes.Add(per);

            return per;
        }

        public void DrawPeriodes()
        {
            foreach (PeriodeG per in _listPeriodes)
            {
                mainGrid.Children.Add(per.PerRectangle);
            }

            lblJour.Content = Header;
        }


        public class PeriodeG
        {

            public String Name { get; set; }
            public Shape PerRectangle { get; set; }

            public TimeSpan StartTs { get; set; }
            public TimeSpan EndTs { get; set; }

            public InfosDay InfosDay { get; set; }
            public int TypePeriode { get; internal set; }

            public PeriodeG()
            {




            }

        }

 
    }
}
