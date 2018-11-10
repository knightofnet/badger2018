using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
