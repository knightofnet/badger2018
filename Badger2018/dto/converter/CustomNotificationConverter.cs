using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using BadgerCommonLibrary.utils;

namespace Badger2018.dto.converter
{
    class CustomNotificationConverter : TypeConverter
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private const char CharSep = '#';

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            try
            {
                if (value is String)
                {
                    String[] splitted = ((string)value).Split(CharSep);

                    bool isActive = Boolean.Parse(splitted[0]);
                    EnumHeurePersoNotif heureType = EnumHeurePersoNotif.GetFromEnumRef(splitted[1]);
                    TimeSpan heureRef = TimeSpan.Parse(splitted[2]);
                    TimeSpan delta = TimeSpan.Parse(splitted[3]);
                    int compSign = Int16.Parse(splitted[4]);
                    String message = splitted[5];

                    CustomNotificationDto c = new CustomNotificationDto()
                    {
                        CompSign = compSign,
                        Delta = delta,
                        HeurePersoNotif = heureType,
                        HeureRef = heureRef,
                        IsActive = isActive,
                        Message = message
                    };

                    return c;


                }

            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);

            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                CustomNotificationDto c = value as CustomNotificationDto;
                if (c == null) return base.ConvertTo(context, culture, value, destinationType);

                String retStr = String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}",
                    CharSep,
                    c.IsActive, c.HeurePersoNotif.EnumRef, c.HeureRef, c.Delta, c.CompSign, c.Message);
                return retStr;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}
