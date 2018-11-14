using System;

namespace SharpDb
{
    public abstract class Node
    {
        public uint PageIndex { get; set; }

        public abstract byte[] Serialize();

        protected abstract void DeserializeData(byte[] data, int index);

        protected void Deserialize(byte[] data)
        {
            var index = 1; // First byte indicates the node's type
            DeserializeData(data, index);
        }

        protected byte[] DeserializeBlob(byte[] data, ref int index)
        {
            var blobSize = DeserializeInt(data, ref index);
            var blob = new byte[blobSize];
            for (var i = 0; i < blobSize; i++)
            {
                blob[i] = data[index + i];
            }

            index += blobSize;
            return blob;
        }

        protected byte[] SerializeBlob(byte[] value)
        {
            var result = new byte[value.Length + 4];
            var size = SerializeInt(value.Length);
            for (var i = 0; i < 4; i++) result[i] = size[i];
            for (var i = 0; i < value.Length; i++) result[i + 4] = value[i];
            return result;
        }

        protected bool DeserilizeBool(byte[] data, ref int index)
        {
            var boolData = data[index];
            index++;
            return boolData != 0;
        }

        protected byte[] SerializeBool(bool value)
        {
            return new byte[] { value ? (byte)1 : (byte)0 };
        }

        protected int DeserializeInt(byte[] data, ref int index)
        {
            var intBytes = new int[]
            {
                data[index] << 12,
                data[index + 1] << 8,
                data[index + 2] << 4,
                data[index + 3]
            };

            index += 4;

            return intBytes[0] | intBytes[1] | intBytes[2] | intBytes[3];
        }

        protected byte[] SerializeInt(int value)
        {
            return new byte[]
            {
                (byte)((value >> 12)),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 4) & 0xff),
                (byte)(value & 0xff)
            };
        }
    }
}