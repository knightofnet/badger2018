using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils;

namespace BadgerUpdater.dto
{
    public class AppArgsDto
    {
        public string VergionTarget { get; set; }
        public string XmlUpdateFile { get; set; }
        public string BadgerAppExe { get; set; }

        public bool LaunchAppIfSucess { get; set; }
        public string NumRunReprise { get; set; }
        public bool IsSideloadUpdate { get; internal set; }
        public string UpdateExeFile { get; internal set; }
        public bool IsForceDebug { get; internal set; }

        public bool IsReprise()
        {
            return !StringUtils.IsNullOrWhiteSpace(NumRunReprise);
        }
    }
}
