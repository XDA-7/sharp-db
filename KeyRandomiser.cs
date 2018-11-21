using System;
using System.Collections.Generic;

namespace SharpDb
{
    public class KeyRandomiser
    {
        private readonly int rngSeed = 19881781;

        private readonly int[] bitOrder;

        public KeyRandomiser()
        {
            var rng = new Random(rngSeed);
            var bitDict = new Dictionary<int, int>();
            for (var i = 0; i < 32; i++)
            {
                var randomNumber = rng.Next();
                bitDict.Add(randomNumber, i);
            }

            var orderedKeys = new SortedSet<int>(bitDict.Keys);
            bitOrder = new int[32];
            var arrayIndex = 0;
            foreach (var key in orderedKeys)
            {
                bitOrder[arrayIndex] = bitDict[key];
                arrayIndex++;
            }
        }

        public NodeKey GetKeyFromId(int id)
        {
            var idBits = ConvertIntToBits(id);
            var nodeKeyBits = new int[32];

            for (var i = 0; i < 32; i++)
            {
                nodeKeyBits[i] = idBits[bitOrder[i]];
            }

            return ConvertBitsToInt(nodeKeyBits);
        }

        public int GetIdFromNodeKey(NodeKey nodeKey)
        {
            var keyBits = ConvertIntToBits((int)nodeKey);
            var idBits = new int[32];

            for (var i = 0; i < 32; i++)
            {
                idBits[bitOrder[i]] = keyBits[i];
            }

            return ConvertBitsToInt(idBits);
        }

        private int[] ConvertIntToBits(int value)
        {
            var result = new int[32];
            var valueBytes = BitConverter.GetBytes(value);
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    result[(i * 8) + j] = (valueBytes[i] >> j) & 1;
                }
            }

            return result;
        }

        private int ConvertBitsToInt(int[] bits)
        {
            var resultBytes = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    resultBytes[i] |= (byte)(bits[(i * 8) + j] << j);
                }
            }

            return BitConverter.ToInt32(resultBytes, 0);
        }
    }
}