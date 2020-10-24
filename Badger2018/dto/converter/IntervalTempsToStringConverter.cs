using Badger2018.constants;
using BadgerCommonLibrary.utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Badger2018.dto.converter
{
    class IntervalTempsToStringConverter : IValueConverter
    {
        public DataGrid DataGridRef { get; internal set; }

        public int Mode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String outStr = "";
            if (Mode == 1 )
            {
                outStr = "pause non terminée";
                if (value != null && value is DateTime?)
                {
                    DateTime? dt = (DateTime?)value;
                    outStr = dt.Value.TimeOfDay.ToString(Cst.TimeSpanFormat);
                }          

               
            } else if (value is DateTime)
            {
                outStr = ((DateTime)value).TimeOfDay.ToString(Cst.TimeSpanFormat);
            }

            return outStr;
            

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            /*
            IntervalTemps cData = DataGridRef.SelectedItem as IntervalTemps;
            if (cData == null) return null;
            */
            string c = value as string;
         //   if (c == null) return cData;

            TimeSpan result;


            if (TimeSpan.TryParse(c, out result))
            {
                return AppDateUtils.DtNow().ChangeTime(result);
            }

            return null;
        }
    }
}
