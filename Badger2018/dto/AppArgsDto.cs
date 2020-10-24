using System;

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
