using System.Collections.Generic;

namespace SharpDb
{
    public class LeafNode : Node
    {
        private Dictionary<NodeKey, Blob> dataRows = new Dictionary<NodeKey, Blob>();

        private SortedSet<NodeKey> dataKeys = new SortedSet<NodeKey>();

        private readonly int dataRowHeaderSize = 8; // 4 for the key, 4 for the data length

        private int bytesUsed = 1; // First byte is reserved for the node type

        public LeafNode() : base()
        {
        }

        public LeafNode(PageIndex pageIndex, byte[] data) : base(pageIndex, data)
        {
        }

        public byte[] GetDataRow(NodeKey key) => dataRows.ContainsKey(key) ? dataRows[key].ProduceByteArray() : new byte[0];

        public void AddDataRow(NodeKey key, byte[] data) => AddDataRow(key, new Blob(data));

        public void AddDataRow(NodeKey key, Blob data)
        {
            bytesUsed += dataRowHeaderSize + data.Length;
            dataRows.Add(key, data);
            dataKeys.Add(key);
        }

        public bool CanFitDataRow(byte[] data) => (bytesUsed + data.Length + dataRowHeaderSize) < Constants.PageSize;

        // Split off and return the lower half in a new node
        // We split off the lower half so that data about the upper key value of this node does not change
        public LeafNode Split()
        {
            var keyCount = dataKeys.Count / 2;
            var upperKeys = new NodeKey[keyCount];
            dataKeys.CopyTo(upperKeys, 0, keyCount);
            var splitNode = new LeafNode();
            foreach (var upperKey in upperKeys)
            {
                splitNode.AddDataRow(upperKey, dataRows[upperKey]);
                dataKeys.Remove(upperKey);
                dataRows.Remove(upperKey);
            }

            return splitNode;
        }

        public NodeKey GetLargestKey() => dataKeys.Max;

        protected override void Deserialize(byte[] data)
        {
            var index = 1; // First byte indicates the node's type
            bytesUsed = data.Length;
            var key = Serializer.DeserializeInt(data, ref index);
            while(key != 0)
            {
                dataRows.Add(key, new Blob(data, ref index));
                dataKeys.Add(key);
                if (index + 4 < data.Length)
                {
                    key = Serializer.DeserializeInt(data, ref index);
                }
                else
                {
                    key = 0;
                }
            }
        }

        public override byte[] Serialize()
        {
            var result = new byte[Constants.PageSize];
            result[0] = 1; // Is leaf
            var index = 1;
            foreach (var rowKey in dataKeys)
            {
                var key = Serializer.SerializeInt((int)rowKey);
                var blob = dataRows[rowKey].Serialize();

                key.CopyTo(result, index);
                index += 4;
                blob.CopyTo(result, index);
                index += blob.Length;
            }

            return result;
        }
    }
}