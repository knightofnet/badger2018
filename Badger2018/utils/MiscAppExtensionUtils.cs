using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Badger2018.constants;

namespace Badger2018.utils
{
    public static class MiscAppExtensionUtils
    {

        public static void ContentShortTime(this Label label, DateTime dateTime)
        {
            label.Content = dateTime.TimeOfDay.ToString(Cst.TimeSpanFormatWithH);
        }

        public static void ContentShortTime(this Label label, TimeSpan ts)
        {
            label.Content = ts.ToString(Cst.TimeSpanFormatWithH);
        }

    }
}
