using System;

namespace Badger2018.utils.sqlite
{
    public class TableDef
    {
        public int ColumnId { get; set; }
        public String Name { get; set; }

        public String Type { get; set; }

        public bool IsNotNull { get; set; }

        public Object DefaultValue { get; set; }

        public int PrimaryKey { get; set; }
    }
}