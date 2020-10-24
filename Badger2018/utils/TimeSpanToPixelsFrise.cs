using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace Badger2018.utils
{
    public class TimeSpanToPixelsFrise
    {

        public double StartDayPosition { get; private set; }
        public double EndDayPosition { get; private set; }

        private readonly double dPix = 0;

        public TimeSpan StartDayTs { get; private set; }
        public TimeSpan EndDayTs { get; private set; }

        private readonly TimeSpan dTs = new TimeSpan();

        private readonly double factor;

        public TimeSpanToPixelsFrise(double sDayPos, double eDayPos, TimeSpan sDayTs, TimeSpan eDayTs)
        {
            StartDayPosition = sDayPos;
            EndDayPosition = eDayPos;
            StartDayTs = sDayTs;
            EndDayTs = eDayTs;

            dPix = EndDayPosition - StartDayPosition;
            dTs = EndDayTs - StartDayTs;

            factor = dPix / dTs.TotalMinutes;
        }

        public Rectangle RectStretchLeftAlignFromTs(TimeSpan start, TimeSpan end)
        {

            Rectangle r = RectTopLeftAlignFromTs(start, end);
            r.VerticalAlignment = VerticalAlignment.Stretch;

            return r;
        }

        public Rectangle RectTopLeftAlignFromTs(TimeSpan start, TimeSpan end)
        {
            Rectangle rect = new Rectangle();
            TimeSpan relTsStart = start - StartDayTs;
            double rLeftPos = relTsStart.TotalMinutes * factor + StartDayPosition;
            rect.Margin = new Thickness(rLeftPos, 0, 0, 0);

            TimeSpan relDuration = end - start;
            double width = relDuration.TotalMinutes * factor;
            rect.Width = width;

            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.VerticalAlignment = VerticalAlignment.Top;

            return rect;
        }

        internal Line LineTopLeftAlignFromTs(TimeSpan start)
        {
            Line line = new Line();
                        
            double rLeftPos = CalcDateToPixel(start) ;

            line.Margin = new Thickness(rLeftPos, 0, 0, 0);

            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Top;
            line.Width = 1;
            line.X1 = rLeftPos;
            line.X2 = rLeftPos;
            line.Y1 = 0;
            line.Y2 = 26;

            line.SnapsToDevicePixels = true;

            

            return line;
        }

        internal double CalcDateToPixel(TimeSpan start)
        {
            TimeSpan relTsStart = start - StartDayTs;
            double rLeftPos = relTsStart.TotalMinutes * factor + StartDayPosition;

            return rLeftPos;
        }

        public Rectangle RectStretchLeftAlignFromTs(TimeSpan ts)
        {
            Rectangle r = RectStretchLeftAlignFromTs(ts, ts.Add(new TimeSpan(0, 0, 1, 0)));
            r.Width = 1;
            r.SnapsToDevicePixels = true;

            return r;
        }
    }
}
