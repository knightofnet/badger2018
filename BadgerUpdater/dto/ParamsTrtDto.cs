using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BadgerUpdater.dto
{
    public class ParamsTrtDto
    {
        public AppArgsDto InArgs { get; set; }
        public FileInfo BadgerExeFileInfo { get; set; }
        public Version BadgerExeVersion { get; set; }
    }
}
