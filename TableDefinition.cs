using System.Linq;

namespace SharpDb
{
    public class TableDefinition
    {
        public string Name { get; set; }

        public ColumnDefinition[] ColumnDefinitions { get; set; }

        public int GetRecordSize()
        {
            // Holds as long as the size of each data type is 4 bytes(32 bits)
            return ColumnDefinitions.Sum(val => val.ElementCount * 4);
        }
    }
}