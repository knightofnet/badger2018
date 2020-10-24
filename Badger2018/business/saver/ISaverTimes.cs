using Badger2018.dto;

namespace Badger2018.business.saver
{
    internal interface ISaverTimes
    {
        void SaveCurrentDayTimes();

        bool MustReloadIncomplete();

        PointageElt LoadIncomplete();
    }
}
