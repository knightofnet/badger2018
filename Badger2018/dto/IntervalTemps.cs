using System;

namespace Badger2018.dto
{
    public class IntervalTemps
    {


        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public DateTime EndOrDft
        {
            get { return End.GetValueOrDefault(); }
            set { End = value; }
        }

        public IntervalTemps()
        {
            int a = 1;
        }

        public bool IsIntervalComplet()
        {
            return End.HasValue;
        }

        public TimeSpan GetDuration()
        {
            if (IsIntervalComplet() && End.HasValue)
            {
                return End.Value - Start;
            }
            return TimeSpan.Zero;
        }

        public override string ToString()
        {
            return string.Format("Start: {0}, End: {1}", Start, End);
        }

        internal IntervalTemps Clone()
        {
            IntervalTemps ivl = new IntervalTemps();
            ivl.Start = Start;
            ivl.End = End;

            return ivl;
        }
    }
}
