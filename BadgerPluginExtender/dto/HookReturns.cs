using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils;

namespace BadgerPluginExtender.dto
{
    public class HookReturns
    {

        List<HookReturn> _listReturns = new List<HookReturn>();

        public List<HookReturn> ListReturns
        {
            get { return _listReturns; }
            set { _listReturns = value; }
        }

        public Type ReturnType { get; set; }

        public bool IsOneResult()
        {
            return _listReturns.Count == 1;
        }

        public bool HasResult()
        {
            return _listReturns.Any();
        }

        public object ReturnFirstResultObject()
        {
            return HasResult() ? _listReturns[0].ReturnedObject : null;
        }

        public object ReturnFirstOrDefaultResultObject()
        {
            return HasResult() ? _listReturns[0].ReturnedObject : ReflexionUtils.GetDefaultValue(ReturnType);
        }

        public void Add(HookReturn hookReturn)
        {
            _listReturns.Add(hookReturn);
        }


    }
}
