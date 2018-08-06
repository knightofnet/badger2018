using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.constants
{
    interface IEnumSerializableWithIndex<T>
    {
        T GetFromIndex(int index);
        int GetIndex();
    }
}
