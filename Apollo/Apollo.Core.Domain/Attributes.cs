using System;

namespace Apollo.Core.Domain
{
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string columnName, bool autoIncrement = false)
        {
            this.ColumnName = columnName;
            this.AutoIncrement = autoIncrement;
        }

        public string ColumnName { get; set; }
        public bool AutoIncrement { get; }
    }

    public class PKAttribute : Attribute
    {
        public PKAttribute() { }
    }

    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string TableName)
        {
            this.TableName = TableName;
        }

        public string TableName { get; set; }
    }

}