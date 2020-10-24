using System;
using Badger2018.constants;

namespace Badger2018.dto
{
    public class NotificationDto
    {
        public string Name { get; set; }

        public EnumTypesJournees TypeJournee { get; set; }

        public int? EtatBadger { get; set; }

        public String Title { get; set; }

        public string Message { get; set; }

        public Func<TimeSpan> TimePivot { get; set; }

        public TimeSpan TimeShowed { get; set; }

        public EnumTypesTemps TypeTemps { get; set; }

        public bool IsPushToInfo { get; set; }

        public Action<NotificationDto> DtoAfterShowNotif { get; set; }
    }
}
