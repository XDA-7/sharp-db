using System;

namespace SharpDb
{
    public class RowConverter
    {
        private TableDefinition tableDefinition;

        public RowConverter(TableDefinition tableDefinition)
        {
            this.tableDefinition = tableDefinition;
        }

        public object[] ReadRow(byte[] data)
        {
            var result = new object[tableDefinition.ColumnDefinitions.Length];
            var dataIndex = 0;
            for (var i = 0; i < tableDefinition.ColumnDefinitions.Length; i++)
            {
                var columnDefinition = tableDefinition.ColumnDefinitions[i];
                object columnObj = null;
                switch(columnDefinition.ColumnType)
                {
                    case ColumnType.Integer:
                        columnObj = Serializer.DeserializeInt(data, ref dataIndex);
                        break;
                    case ColumnType.Float:
                        columnObj = Serializer.DeserialiseFloat(data, ref dataIndex);
                        break;
                    case ColumnType.Bit:
                        columnObj = Serializer.DeserilizeBool(data, ref dataIndex);
                        break;
                    case ColumnType.Char:
                        columnObj = Serializer.DeserilizeChar(data, columnDefinition.ElementCount, ref dataIndex);
                        break;
                }

                result[i] = columnObj;
            }

            return result;
        }

        public byte[] WriteRow(object[] data)
        {
            var result = new byte[tableDefinition.SizeInBytes];
            var resultIndex = 0;
            for (var i = 0; i < tableDefinition.ColumnDefinitions.Length; i++)
            {
                byte[] serializedObject = null;
                var column = tableDefinition.ColumnDefinitions[i];
                var datum = data[i];
                switch(column.ColumnType)
                {
                    case ColumnType.Bit:
                        serializedObject = Serializer.SerializeBool((bool)datum);
                        break;
                    case ColumnType.Char:
                        serializedObject = Serializer.SerializeChar((string)datum);
                        break;
                    case ColumnType.Float:
                        serializedObject = Serializer.SerializeFloat((float)datum);
                        break;
                    case ColumnType.Integer:
                        serializedObject = Serializer.SerializeInt((int)datum);
                        break;
                }

                serializedObject.CopyTo(result, resultIndex);
                resultIndex += column.SizeInBytes;
            }

            return result;
        }
    }
}