using System;
using System.ComponentModel;
using System.Configuration;
using Badger2018.constants;
using Badger2018.dto.converter;

namespace Badger2018.dto
{
    [TypeConverter(typeof(CustomNotificationConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.String)]
    public class CustomNotificationDto
    {

        public bool IsActive { get; set; }

        public EnumHeurePersoNotif HeurePersoNotif { get; set; }

        public TimeSpan HeureRef { get; set; }

        public TimeSpan Delta { get; set; }

        public int CompSign { get; set; }

        public String Message { get; set; }

        public TimeSpan GetRealTimeSpan(AppOptions prgOptions, TimesBadgerDto times)
        {
            TimeSpan realHeureRef = HeureRef;
            if (HeurePersoNotif.Equals(EnumHeurePersoNotif.END_PF_MATIN))
            {
                realHeureRef = prgOptions.PlageFixeMatinFin;
            }
            else if (HeurePersoNotif.Equals(EnumHeurePersoNotif.START_PF_APREM))
            {
                realHeureRef = prgOptions.PlageFixeApremStart;
            }
            else if (HeurePersoNotif.Equals(EnumHeurePersoNotif.END_PF_APREM))
            {
                realHeureRef = prgOptions.PlageFixeApremFin;
            }
            else if (HeurePersoNotif.Equals(EnumHeurePersoNotif.TPS_TRAV_THEO))
            {
                realHeureRef = times.EndTheoDateTime.TimeOfDay;
            }
            else if (HeurePersoNotif.Equals(EnumHeurePersoNotif.HEURE_END_MOY_MATIN))
            {
                realHeureRef = times.EndMoyPfMatin;
            }
            else if (HeurePersoNotif.Equals(EnumHeurePersoNotif.HEURE_END_MOY_APREM))
            {
                realHeureRef = times.EndMoyPfAprem;
            }

            if (CompSign <= 0)
            {
                realHeureRef = realHeureRef - Delta;
            }
            else
            {
                realHeureRef = realHeureRef + Delta;
            }

            return realHeureRef;

        }


        public void HydrateFrom(CustomNotificationDto n)
        {
            IsActive = n.IsActive;
            HeurePersoNotif = n.HeurePersoNotif;
            HeureRef = n.HeureRef;
            Delta = n.Delta;
            CompSign = n.CompSign;
            Message = n.Message;
        }
    }
}
