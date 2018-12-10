namespace SharpDb
{
    public class ColumnDefinition
    {
        public string Name { get; }

        public ColumnType ColumnType { get; }

        public int ElementCount { get; }

        public int SizeInBytes { get; }

        public ColumnDefinition(string name, ColumnType columnType, int elementCount)
        {
            Name = name;
            ColumnType = columnType;
            ElementCount = elementCount;
            SizeInBytes = ElementCount * GetColumnTypeSize();
        }

        private int GetColumnTypeSize()
        {
            switch(ColumnType)
            {
                case ColumnType.Bit:
                case ColumnType.Char:
                    return 1;
                case ColumnType.Float:
                case ColumnType.Integer:
                    return 4;
                default:
                    return 0;
            }
        }
    }
}