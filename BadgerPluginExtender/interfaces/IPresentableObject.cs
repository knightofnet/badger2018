using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BadgerPluginExtender.dto;

namespace BadgerPluginExtender.interfaces
{
    public interface IPresentableObject
    {

        MethodRecordWithInstance[] GetMethodToPresents();

    }
}
