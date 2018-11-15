namespace SharpDb
{
    public class InternalNode : Node
    {
        private static readonly int upperKeysLength = (Constants.PageSize / 8) - 1;

        private static readonly int nodeIndicesLength = Constants.PageSize / 8;

        private int[] upperKeyValues = new int[10];
        
        private uint[] nodeIndices = new uint[10];

        public InternalNode()
        {
        }

        public InternalNode(uint pageIndex, byte[] data)
        {
            PageIndex = pageIndex;
            Deserialize(data);
        }

        public InternalNode(uint[] nodeIndices, int[] upperKeyValues)
        {
            upperKeyValues.CopyTo(this.upperKeyValues, 0);
            nodeIndices.CopyTo(this.nodeIndices, 0);
        }

        public uint GetNodeIndexForKey(int key)
        {
            for (var i = 0; i < upperKeyValues.Length; i++)
            {
                if (upperKeyValues[i] >= key)
                {
                    return nodeIndices[i];
                }
            }

            return nodeIndices[upperKeyValues.Length];
        }

        public bool IsFull() => nodeIndices[nodeIndices.Length - 1] != 0;

        public int GetUpperKey()
        {
            for (var i = 0; i < nodeIndices.Length; i++)
            {
                if (nodeIndices[i] == 0)
                {
                    return upperKeyValues[i - 1];
                }
            }

            return upperKeyValues[nodeIndices.Length];
        }

        // Invariant: The last node index will be 0, this can be guaranteed by a check against IsFull
        public void AddNode(uint pageIndex, int upperKey)
        {
            var desiredNodePosition = 0;
            for (; desiredNodePosition <= upperKeyValues.Length; desiredNodePosition++)
            {
                // If we're on the final node then just assign it to the end, the upper value goes nowhere
                if (desiredNodePosition == upperKeyValues.Length)
                {
                    nodeIndices[desiredNodePosition] = pageIndex;
                    return;  
                }
                else if (nodeIndices[desiredNodePosition] == 0 || upperKeyValues[desiredNodePosition] > upperKey)
                {
                    break;
                }
            }

            // Shift every node-value pair right of the desired position up one
            uint shiftedIndex = nodeIndices[desiredNodePosition];
            int shiftedUpperValue = upperKeyValues[desiredNodePosition];
            // Insert the value into the newly vacated spot
            nodeIndices[desiredNodePosition] = pageIndex;
            upperKeyValues[desiredNodePosition] = upperKey;

            var nodeCursor = desiredNodePosition + 1;
            while(nodeIndices[nodeCursor] != 0)
            {
                uint tempIndex = nodeIndices[nodeCursor];
                int tempValue = upperKeyValues[nodeCursor];
                nodeIndices[nodeCursor] = shiftedIndex;
                upperKeyValues[nodeCursor] = shiftedUpperValue;
                shiftedIndex = tempIndex;
                shiftedUpperValue = tempValue;
                nodeCursor++;
            }

            // The final value shift.
            // If the end of the node has been reached then the upper value falls off the edge
            nodeIndices[nodeCursor] = shiftedIndex;
            if (nodeCursor < upperKeyValues.Length)
            {
                upperKeyValues[nodeCursor] = shiftedUpperValue;
            }
        }

        public InternalNode Split(int maxKey)
        {
            var midPosition = nodeIndices.Length / 2;
            var splitNodeIndices = new uint[nodeIndicesLength];
            var splitUpperValues = new int[upperKeysLength];

            var splitIndex = 0;
            for (var i = midPosition + 1; i < upperKeyValues.Length; i++)
            {
                splitNodeIndices[splitIndex] = nodeIndices[i];
                splitUpperValues[splitIndex] = upperKeyValues[i];
                nodeIndices[i] = 0;
                upperKeyValues[i] = 0;
                splitIndex++;
            }

            splitNodeIndices[splitIndex] = nodeIndices[upperKeyValues.Length];
            splitUpperValues[splitIndex] = maxKey;

            return new InternalNode(splitNodeIndices, splitUpperValues);
        }

        protected override void DeserializeData(byte[] data, int index)
        {
            for (var i = 0; i < upperKeyValues.Length; i++)
            {
                nodeIndices[i] = (uint)DeserializeInt(data, ref index);
                upperKeyValues[i] = DeserializeInt(data, ref index);
            }

            nodeIndices[upperKeyValues.Length] = (uint)DeserializeInt(data, ref index);
        }

        public override byte[] Serialize()
        {
            var result = new byte[Constants.PageSize];
            result[0] = 0; // Is not leaf
            var resultIndex = 1;
            var copyArray = new byte[4];
            for (var i = 0; i < upperKeyValues.Length; i++)
            {
                copyArray = SerializeInt((int)nodeIndices[i]);
                copyArray.CopyTo(result, resultIndex);
                resultIndex += 4;
                copyArray = SerializeInt(upperKeyValues[i]);
                copyArray.CopyTo(result, resultIndex);
                resultIndex += 4;
            }

            copyArray = SerializeInt((int)nodeIndices[upperKeyValues.Length]);
            copyArray.CopyTo(result, resultIndex);
            
            return result;
        }
    }
}