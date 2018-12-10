using System.Collections.Generic;

namespace SharpDb
{
    public class InternalNode : Node
    {
        private static readonly int maxKeys = Constants.PageSize / 8;

        private Dictionary<NodeKey, PageIndex> nodeIndices = new Dictionary<NodeKey, PageIndex>();

        private SortedSet<NodeKey> upperKeyValues = new SortedSet<NodeKey>();

        public InternalNode()
        {
        }

        public InternalNode(PageIndex pageIndex, byte[] data)
        {
            PageIndex = pageIndex;
            Deserialize(data);
        }

        public InternalNode(PageIndex[] nodeIndices, NodeKey[] upperKeyValues)
        {
            for (var i = 0; i < nodeIndices.Length; i++)
            {
                this.nodeIndices.Add(upperKeyValues[i], nodeIndices[i]);
            }
        }

        public PageIndex GetNodeIndexForKey(NodeKey key)
        {
            var upperRange = upperKeyValues.GetViewBetween(key, upperKeyValues.Max);
            return nodeIndices[upperRange.Min];
        }

        public bool IsFull() => nodeIndices.Count == maxKeys;

        public void AddNode(PageIndex pageIndex, NodeKey upperKey)
        {
            upperKeyValues.Add(upperKey);
            nodeIndices.Add(upperKey, pageIndex);
        }

        // Split off and return the lower half in a new node
        // We split off the lower half so that data about the upper key value of this node does not change
        public InternalNode Split()
        {
            var nodeCount = upperKeyValues.Count / 2;
            var splitKeys = new NodeKey[nodeCount];
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

        public NodeKey GetLargestKey() => upperKeyValues.Max;

        protected override void DeserializeData(byte[] data, int index)
        {
            for (var i = 0; i < maxKeys; i++)
            {
                SharpDb.PageIndex pageIndex = Serializer.DeserializeInt(data, ref index);
                NodeKey upperKey = Serializer.DeserializeInt(data, ref index);
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
                copyArray = Serializer.SerializeInt((int)nodeIndices[keyValue]);
                copyArray.CopyTo(result, resultIndex);
                resultIndex += 4;
                copyArray = Serializer.SerializeInt((int)keyValue);
                copyArray.CopyTo(result, resultIndex);
                resultIndex += 4;
            }

            upperKeyValues.Add(maxKey);
            copyArray = Serializer.SerializeInt((int)nodeIndices[maxKey]);
            copyArray.CopyTo(result, resultIndex);
            
            return result;
        }
    }
}