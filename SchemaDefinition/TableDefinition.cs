using System.Linq;

namespace SharpDb
{
    public class TableDefinition
    {
        public string Name { get; }

        public ColumnDefinition[] ColumnDefinitions { get; }

        public int SizeInBytes { get; }

        public TableDefinition(string name, ColumnDefinition[] columnDefinitions)
        {
            Name = name;
            ColumnDefinitions = columnDefinitions;
            SizeInBytes = ColumnDefinitions.Sum(def => def.SizeInBytes);
        }
    }
}