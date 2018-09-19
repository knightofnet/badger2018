using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BadgerCommonLibrary.utils
{
    public interface IEnumSerializableWithIndex<T>
    {
        T GetFromIndex(int index);
        int GetIndex();
    }
}
