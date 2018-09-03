using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BadgerCommonLibrary.dto
{
    public class UpdateInfoDto
    {

        public Version Version { get; set; }

        public string Title { get; set; }

        public String Authors { get; set; }

        public string Description { get; set; }
        public string FileUpdate { get; set; }
        public int LevelUpdate { get; set; }
        public bool NeedIntermediateLaunch { get; set; }
        public bool IsActive { get; set; }
    }
}
