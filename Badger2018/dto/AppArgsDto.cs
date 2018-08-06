using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.dto
{
    public class AppArgsDto
    {

        public String ImportConfFilePath { get; set; }
        public String ExportConfFilePath { get; set; }

        public bool IsForceLogDebug { get; set; }
        public bool LoadAfterImportExport { get; set; }



    }
}
