using System;
using System.Text;

namespace SharpDb
{
    public static class Serializer
    {
        public static byte[] DeserializeBlob(byte[] data, ref int index)
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

        public static byte[] SerializeBlob(byte[] value)
        {
            var result = new byte[value.Length + 4];
            var size = SerializeInt(value.Length);
            for (var i = 0; i < 4; i++) result[i] = size[i];
            for (var i = 0; i < value.Length; i++) result[i + 4] = value[i];
            return result;
        }

        public static bool DeserilizeBool(byte[] data, ref int index)
        {
            var boolData = data[index];
            index++;
            return boolData != 0;
        }

        public static byte[] SerializeBool(bool value)
        {
            return new byte[] { value ? (byte)1 : (byte)0 };
        }

        public static int DeserializeInt(byte[] data, ref int index)
        {
            var result = BitConverter.ToInt32(data, index);
            index += 4;
            return result;
        }

        public static byte[] SerializeInt(int value) => BitConverter.GetBytes(value);

        public static float DeserialiseFloat(byte[] data, ref int index)
        {
            var result = BitConverter.ToSingle(data, index);
            index += 4;
            return result;
        }

        public static byte[] SerializeFloat(float value) => BitConverter.GetBytes(value);

        public static string DeserilizeChar(byte[] data, int charSize, ref int index)
        {
            var charArray = new byte[charSize];
            for (var i = 0; i < charSize; i++)
            {
                charArray[i] = data[index + i];
            }

            index += charSize;
            return Encoding.UTF8.GetString(charArray);
        }

        public static byte[] SerializeChar(string value) => Encoding.UTF8.GetBytes(value);
    }
}