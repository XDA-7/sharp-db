using System;
using System.Collections.Generic;

namespace SharpDb
{
    public class Heap
    {
        private BTree bTree;

        private int heapBlobCount;

        public const int MaxBlobSize = Constants.PageSize - 13; // 1 for isleaf, 4 for key, 4 for blob size, 4 for next blob pointer. This is going to become painful to maintain

        public int HeapBlobCount { get => heapBlobCount; }

        public Heap(Node treeRoot, int heapBlobCount)
        {
            bTree = new BTree(treeRoot);
            this.heapBlobCount = heapBlobCount;
        }

        public NodeKey ModifyHeapBlob(NodeKey nodeKey, byte[] data)
        {
            var blobList = GetBlobs(nodeKey);
            var blobs = blobList.Blobs;
            if (data.Length > blobList.ByteCount)
            {
                var neededBytes = data.Length - blobList.ByteCount;
                var neededPages = neededBytes / MaxBlobSize;
                for (var i = 0; i < neededPages; i++)
                {
                    // TODO
                }
            }

            return 0;
        }

        public NodeKey InsertData(byte[] data)
        {
            var neededFullBlobs = (data.Length / MaxBlobSize);
            var lastBlobSize = data.Length % MaxBlobSize;
            heapBlobCount++;
            NodeKey startKey = heapBlobCount;
            NodeKey currentKey = startKey;
            NodeKey nextKey = 0;
            var blobData = neededFullBlobs > 0 ? new byte[MaxBlobSize + 4] : null;
            byte[] nextKeyBytes = null;
            for (var i = 0; i < neededFullBlobs; i++)
            {
                heapBlobCount++;
                nextKey = heapBlobCount;
                nextKeyBytes = Serializer.SerializeInt((int)nextKey);
                nextKeyBytes.CopyTo(blobData, 0);
                Array.Copy(data, i * MaxBlobSize, blobData, 4, MaxBlobSize);
                bTree.InsertData(currentKey, blobData);
                currentKey = nextKey;
            }

            blobData = new byte[lastBlobSize + 4];
            nextKeyBytes = Serializer.SerializeInt(0);
            nextKeyBytes.CopyTo(blobData, 0);
            Array.Copy(data, neededFullBlobs * MaxBlobSize, blobData, 4, lastBlobSize);
            bTree.InsertData(currentKey, blobData);
            return startKey;
        }

        public Blob GetHeapBlob(NodeKey nodeKey)
        {
            var blobList = GetBlobs(nodeKey);
            var bytes = new List<byte>();
            foreach (var blob in blobList.Blobs)
            {
                bytes.AddRange(blob.ProduceByteArray(4));
            }

            return new Blob(bytes.ToArray());
        }

        public void SaveHeap() => bTree.SaveNodes();

        private BlobList GetBlobs(NodeKey nodeKey)
        {
            var baseBlob = bTree.GetData(nodeKey);
            var blobs = new List<Blob>();
            var byteCount = 0;
            blobs.Add(baseBlob);
            var nextNode = GetNextNodeKey(baseBlob);
            while (nextNode != 0)
            {
                var nextBlob = bTree.GetData(nextNode);
                blobs.Add(nextBlob);
                byteCount += nextBlob.ByteCount;
                nextNode = GetNextNodeKey(nextBlob);
            }

            return new BlobList { Blobs = blobs, ByteCount = byteCount };
        }

        private NodeKey GetNextNodeKey(Blob blob)
        {
            var linkData = blob.ProduceByteArray(0, 4);
            var index = 0;
            return Serializer.DeserializeInt(linkData, ref index);
        }

        private class BlobList
        {
            public List<Blob> Blobs { get; set; }

            public int ByteCount { get; set; }
        }
    }
}