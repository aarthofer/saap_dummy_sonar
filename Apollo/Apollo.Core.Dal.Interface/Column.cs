using Apollo.Core.Domain;
using System;
using System.Linq;

namespace Apollo.Core.Dal.Interface
{
    public class Column
    {
        public Column(Type domainObject, string column, string alias = "")
        {
            DomainObject = domainObject;
            ColumnName = column;
            Alias = alias;
        }

        public Type DomainObject { get; set; }
        public string ColumnName { get; set; }
        public string Alias { get; set; }

        public static Column Create<T>(string name, string alias = "")
        {
            CheckAttributeExists(typeof(T), name);
            return new Column(typeof(T), name, alias);
        }

        private static void CheckAttributeExists(Type domainObject, string columnName)
        {
            if ("*".Equals(columnName))
            {
                return;
            }

            var properties = domainObject.GetProperties();

            ColumnAttribute column = null;

            foreach (var property in properties)
            {
                ColumnAttribute[] Columns = (ColumnAttribute[])Attribute.GetCustomAttributes(property, typeof(ColumnAttribute));
                column = Columns.Where(col => col.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (column != null) { break; }
            }

            if (column == null)
            {
                throw new Exception($"Column {columnName} not found in {domainObject}!");
            }
        }
    }
}
