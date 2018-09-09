using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BadgerPluginExtender.dto
{
    public class HookReturn
    {

        public PluginInfo PluginInfo { get; set; }

        public MethodRecord MethodRecord { get; set; }

        public object ReturnedObject { get; set; }


    }
}
