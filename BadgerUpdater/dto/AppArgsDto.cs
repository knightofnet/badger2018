using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BadgerUpdater.dto
{
    public class AppArgsDto
    {
        public string VergionTarget { get; set; }
        public string XmlUpdateFile { get; set; }
        public string BadgerAppExe { get; set; }

        public bool LaunchAppIfSucess { get; set; }
    }
}
