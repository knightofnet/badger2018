using Badger2018.constants;
using System;
using System.Windows.Controls;

namespace Badger2018.utils
{
    public static class MiscAppExtensionUtils
    {

        public static void ContentShortTime(this Label label, DateTime dateTime)
        {
            label.ContentShortTime(dateTime.TimeOfDay);
        }

        public static void ContentShortTime(this Label label, TimeSpan ts)
        {
            label.Content = MiscAppUtils.TimeSpanShortStrFormat(ts);
        }

        /// <summary>
        /// Return absolute value (positive) of a TimeSpan.
        /// </summary>
        /// <param name="thisTs"></param>
        /// <returns></returns>
        public static TimeSpan Absolute(this TimeSpan thisTs)
        {
            if (thisTs.TotalMilliseconds < 0)
            {
                return thisTs.Negate();
            }
            return thisTs;
        }

        public static String ToStrSignedhhmm(this TimeSpan thisTs)
        {
            return String.Format("{0}{1}", thisTs.TotalMilliseconds < 0 ? "-" : "", thisTs.ToString(Cst.TimeSpanFormatWithH));
        }
    }
}
