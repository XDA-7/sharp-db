using System.Collections.Generic;

namespace SharpDb
{
    public class InternalNode : Node
    {
        private static readonly int maxKeys = Constants.PageSize / 8;

        private Dictionary<int, uint> nodeIndices = new Dictionary<int, uint>();

        private SortedSet<int> upperKeyValues = new SortedSet<int>();

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
            for (var i = 0; i < nodeIndices.Length; i++)
            {
                this.nodeIndices.Add(upperKeyValues[i], nodeIndices[i]);
            }
        }

        public uint GetNodeIndexForKey(int key)
        {
            foreach (var upperValue in upperKeyValues)
            {
                if (upperValue > key)
                {
                    return nodeIndices[upperValue];
                }
            }

            return 0;
        }

        public bool IsFull() => nodeIndices.Count == maxKeys;

        public void AddNode(uint pageIndex, int upperKey)
        {
            upperKeyValues.Add(upperKey);
            nodeIndices.Add(upperKey, pageIndex);
        }

        // Split off and return the lower half in a new node
        // We split off the lower half so that data about the upper key value of this node does not change
        public InternalNode Split()
        {
            var nodeCount = upperKeyValues.Count / 2;
            var splitKeys = new int[nodeCount];
            upperKeyValues.CopyTo(splitKeys, 0, nodeCount);
            var splitNode = new InternalNode();
            foreach (var upperKey in splitKeys)
            {
                splitNode.AddNode(nodeIndices[upperKey], upperKey);
                nodeIndices.Remove(upperKey);
                upperKeyValues.Remove(upperKey);
            }

            return splitNode;
        }

        public int GetLargestKey() => upperKeyValues.Max;

        protected override void DeserializeData(byte[] data, int index)
        {
            for (var i = 0; i < maxKeys; i++)
            {
                var pageIndex = (uint)DeserializeInt(data, ref index);
                var upperKey = DeserializeInt(data, ref index);
                if (upperKey != 0)
                {
                    nodeIndices.Add(upperKey, pageIndex);
                    upperKeyValues.Add(upperKey);
                }
                else
                {
                    nodeIndices.Add(int.MaxValue, pageIndex);
                    upperKeyValues.Add(int.MaxValue);
                    break;
                }
            }
        }

        public override byte[] Serialize()
        {
            var result = new byte[Constants.PageSize];
            result[0] = 0; // Is not leaf
            var resultIndex = 1;
            var copyArray = new byte[4];
            var maxKey = upperKeyValues.Max; // Do the last node separately as we don't serialize the upper key for it
            upperKeyValues.Remove(maxKey);
            foreach (var keyValue in upperKeyValues)
            {
                copyArray = SerializeInt((int)nodeIndices[keyValue]);
                copyArray.CopyTo(result, resultIndex);
                resultIndex += 4;
                copyArray = SerializeInt(keyValue);
                copyArray.CopyTo(result, resultIndex);
                resultIndex += 4;
            }

            copyArray = SerializeInt((int)nodeIndices[maxKey]);
            copyArray.CopyTo(result, resultIndex);
            
            return result;
        }
    }
}