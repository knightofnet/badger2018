using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;
using BadgerPluginExtender;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Navigation;


namespace Badger2018.business
{
    public class NoticationsManager
    {

        public PluginManager PluginMgrRef { get; internal set; }

        private readonly NotifyIcon _notifyIcon;

        private ConcurrentBag<NotificationDto> listNotifRealTime = new ConcurrentBag<NotificationDto>();

        private ConcurrentBag<String> listNotifAlreadyShown = new ConcurrentBag<String>();

        public EnumTypesJournees TypeJournee { get; set; }

        public int EtatBadger { get; set; }

        public bool UseAlternateNotification { get; set; }
        public Action<NotificationDto> AfterShowNotif { get; internal set; }

        public NoticationsManager(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon;

        }

        internal NotificationDto RegisterNotificationOnRealTimeNow(string notifName, String notifTitle, string notifMessage, EnumTypesJournees typeJournee, int? etatBadger, TimeSpan timePivot)
        {
            return RegisterNotification(notifName, notifTitle, notifMessage, typeJournee, etatBadger, timePivot, EnumTypesTemps.RealTime);
        }

        internal NotificationDto RegisterNotification(string notifName, String notifTitle, string notifMessage, EnumTypesJournees typeJournee, int? etatBadger, TimeSpan timePivot, EnumTypesTemps typeTemps)
        {
            NotificationDto n = new NotificationDto
            {
                Name = notifName,
                Title = notifTitle,
                Message = notifMessage,
                TypeJournee = typeJournee,
                EtatBadger = etatBadger,
                TypeTemps = typeTemps,
                TimePivot = () => timePivot,
            };


            RemoveNotification(notifName);

            listNotifRealTime.Add(n);

            return n;
        }



        internal NotificationDto RegisterNotification(string notifName, String notifTitle, string notifMessage, EnumTypesJournees typeJournee, int? etatBadger, Func<TimeSpan> timePivot, EnumTypesTemps typeTemps)
        {
            NotificationDto n = new NotificationDto
            {
                Name = notifName,
                Title = notifTitle,
                Message = notifMessage,
                TypeJournee = typeJournee,
                EtatBadger = etatBadger,
                TypeTemps = typeTemps,
            };
            n.TimePivot += timePivot;


            RemoveNotification(notifName);

            listNotifRealTime.Add(n);

            return n;
        }


        public void DoNotification(TimeSpan nowTs, EnumTypesTemps typeTempsNotif, bool isShowNotif)
        {

            EventHandler actionHandler = delegate(object sender, EventArgs args)
            {
                //
            };

            foreach (NotificationDto notif in listNotifRealTime.Where(r => r.TypeTemps == typeTempsNotif))
            {

                if (nowTs.CompareTo(notif.TimePivot()) >= 0
                    && (!notif.EtatBadger.HasValue || (notif.EtatBadger == EtatBadger))
                    && (notif.TypeJournee == null || notif.TypeJournee == TypeJournee)
                    && !listNotifAlreadyShown.Contains(notif.Name)
                    )
                {
                    String nTitle = notif.Title;
                    if (notif.Title == null)
                    {
                        nTitle = String.Format("Il est {0}", nowTs.ToString(Cst.TimeSpanFormat));
                    }
                    
                    if (isShowNotif)
                    {
                        MiscAppUtils.ShowNotificationBaloon(_notifyIcon, nTitle,
                            notif.Message,
                            actionHandler, 3000, useAlternate: UseAlternateNotification);
                        PluginMgrRef.PlayHook("OnNotifSend", new object[] { notif.TimePivot() , nTitle, notif.Message});
                    }
                    notif.TimeShowed = nowTs;

                    if (AfterShowNotif != null) {
                        AfterShowNotif(notif);
                    }

                    if (notif.DtoAfterShowNotif != null)
                    {
                        notif.DtoAfterShowNotif(notif);
                    }

                    listNotifAlreadyShown.Add(notif.Name);
                }

            }


        }

        internal void RemoveNotification(string notifName)
        {
            List<NotificationDto> tmpList = listNotifRealTime.ToList();
            listNotifRealTime = new ConcurrentBag<NotificationDto>();
            foreach (NotificationDto n in tmpList)
            {
                if (!n.Name.Equals(notifName))
                {
                    listNotifRealTime.Add(n);
                }


            }


            SetNotifShow(notifName, false);
        }

        internal void RemoveNotificationsSeries(string notifSerieName)
        {
            foreach (NotificationDto n in listNotifRealTime.Where(r => r.Name.StartsWith(notifSerieName) && r.Name.Contains(":")))
            {
                RemoveNotification(n.Name);
            }
        }

        internal void SetNotifShow(string notificationName, bool isNotificationShow)
        {
            if (isNotificationShow)
            {
                listNotifAlreadyShown.Add(notificationName);
            }
            else
            {
                List<String> tmpLstr = listNotifAlreadyShown.ToList();
                listNotifAlreadyShown = new ConcurrentBag<String>();
                foreach (String s in tmpLstr)
                {
                    if (!s.Equals(notificationName))
                    {
                        listNotifAlreadyShown.Add(notificationName);
                    }
                }
            }
        }

        internal bool IsNotifShow(string notificationName)
        {
            return listNotifAlreadyShown.Contains(notificationName);
        }

        internal void ResetNotificationShow()
        {
            listNotifAlreadyShown = new ConcurrentBag<string>();
        }
    }
}
