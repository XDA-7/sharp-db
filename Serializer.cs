using System;
using System.Text;

namespace SharpDb
{
    public static class Serializer
    {
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