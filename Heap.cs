using System.Collections.Generic;

namespace SharpDb
{
    public class Heap
    {
        private BTree bTree;

        private int heapBlobCount;

        private KeyRandomiser keyRandomiser = new KeyRandomiser();

        public Heap(Node treeRoot, int heapBlobCount)
        {
            bTree = new BTree(treeRoot);
            this.heapBlobCount = heapBlobCount;
        }

        public NodeKey ModifyHeapBlob(NodeKey nodeKey, byte[] data)
        {
            var blobs = GetBlobs(nodeKey);
            // To do this we need to have a way of modifying data on the B-Tree
        }

        public NodeKey AddHeapBlob(byte[] data)
        {
            heapBlobCount++;
            var nodeKey = keyRandomiser.GetKeyFromId(heapBlobCount);
            bTree.InsertData(nodeKey, data);
            return nodeKey;
        }

        public Blob GetHeapBlob(NodeKey nodeKey)
        {
            var blobs = GetBlobs(nodeKey);
            var bytes = new List<byte>();
            foreach (var blob in blobs)
            {
                bytes.AddRange(blob.ProduceByteArray());
            }

            return new Blob(bytes.ToArray());
        }

        private List<Blob> GetBlobs(NodeKey nodeKey)
        {
            var baseBlob = bTree.GetData(nodeKey);
            var result = new List<Blob>();
            result.Add(baseBlob);
            var nextNode = GetNextNodeKey(baseBlob);
            while (nextNode != 0)
            {
                var nextBlob = bTree.GetData(nextNode);
                result.Add(nextBlob);
                nextNode = GetNextNodeKey(nextBlob);
            }

            return result;
        }

        private NodeKey GetNextNodeKey(Blob blob)
        {
            var linkData = blob.ProduceByteArray(0, 5);
            var index = 0;
            var hasNextBlob = Serializer.DeserilizeBool(linkData, ref index);
            if (hasNextBlob)
            {
                return Serializer.DeserializeInt(linkData, ref index);
            }
            else
            {
                return 0;
            }
        }
    }
}