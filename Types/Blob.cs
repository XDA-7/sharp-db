using System;

namespace SharpDb
{
    public class Blob
    {
        private byte[] data;

        public int Length { get => data.Length; }

        public Blob(byte[] data)
        {
            this.data = new byte[data.Length];
            data.CopyTo(this.data, 0);
        }

        public Blob(byte[] data, ref int index)
        {
            Deserialize(data, ref index);
        }

        private void Deserialize(byte[] data, ref int index)
        {
            var blobSize = Serializer.DeserializeInt(data, ref index);
            this.data = new byte[blobSize];
            for (var i = 0; i < blobSize; i++)
            {
                this.data[i] = data[index + i];
            }

            index += blobSize;
        }

        public byte[] Serialize()
        {
            var result = new byte[data.Length + 4];
            var size = Serializer.SerializeInt(data.Length);
            for (var i = 0; i < 4; i++) result[i] = size[i];
            for (var i = 0; i < data.Length; i++) result[i + 4] = data[i];
            return result;
        }

        public byte[] ProduceByteArray()
        {
            var result = new byte[data.Length];
            data.CopyTo(result, 0);
            return result;
        }
    }
}