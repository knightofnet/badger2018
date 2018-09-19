using AryxDevLibrary.extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.dto
{
    public class IntervalPause
    {

        public DateTime PauseStart { get; set; }
        public DateTime? PauseFin { get; set; }

        public bool IsPauseEnded()
        {
            return PauseFin.HasValue ;
        }
    }
}
