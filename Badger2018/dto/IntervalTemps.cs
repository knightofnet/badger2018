using AryxDevLibrary.extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
