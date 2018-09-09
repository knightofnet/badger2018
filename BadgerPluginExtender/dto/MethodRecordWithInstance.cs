using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace BadgerPluginExtender.dto
{
    public class MethodRecordWithInstance : MethodRecord
    {

        public Object Instance { get; set; }

        public Dispatcher Dispatcher { get; set; }


        public Type StaticType { get; set; }
    }
}
