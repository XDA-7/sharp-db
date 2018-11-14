namespace SharpDb
{
    public class ColumnDefinition
    {
        public string Name { get; set; }

        public ColumnType ColumnType { get; set; }

        public int ElementCount { get; set; }
    }
}