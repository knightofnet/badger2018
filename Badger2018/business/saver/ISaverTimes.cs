using System;
using Badger2018.constants;
using Badger2018.dto;

namespace Badger2018.business.saver
{
    internal interface ISaverTimes
    {
        void SaveCurrentDayTimes();

        bool MustReloadIncomplete();

        PointageElt LoadIncomplete();

        void SaveAnotherDayTime(DateTime dayToMod, TimesBadgerDto times, EnumTypesJournees typeJournee, int etatBadger);
    }
}
