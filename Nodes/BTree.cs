using System.Collections.Generic;

namespace SharpDb
{
    public class BTree
    {
        private Node root;

        private Pager pager = Pager.Get();

        private Dictionary<uint, Node> nodeCache = new Dictionary<uint, Node>();

        public BTree(Node root)
        {
            this.root = root;
        }

        public void InsertData(int key, byte[] data)
        {
        }

        public byte[] GetData(int key)
        {
            return new byte[0];
        }

        private void AddValueToTree(InternalNode internalNode, int key, byte[] data)
        {
            var keyValueCount = internalNode.upperKeyValues.Length;
            var leafIndex = 0;
            for (; leafIndex < keyValueCount; leafIndex++)
            {
                if (internalNode.upperKeyValues[leafIndex] > key)
                {
                    // the correct leaf index is currently assigned
                    break;
                }
            }

            var correctNode = GetNode(internalNode.nodeIndices[leafIndex]);
            if (correctNode is InternalNode)
            {
                AddValueToTree(correctNode as InternalNode, key, data);
            }
            else
            {
                var correctLeafNode = correctNode as LeafNode;
                if (correctLeafNode.CanFitDataRow(data))
                {
                    correctLeafNode.AddDataRow(key, data);
                    if (key > internalNode.upperKeyValues[leafIndex])
                    {
                        internalNode.upperKeyValues[leafIndex] = key;
                    }
                }
                // Can't fit the data into the leaf, split the node
                else
                {
                }
            }
        }

        private void AddLeafNodeToInternalNode(InternalNode internalNode, int leafIndex, int key, byte[] data)
        {
            var newNode = pager.NewNode<LeafNode>();
            nodeCache.Add(newNode.PageIndex, newNode);
            newNode.AddDataRow(key, data);
            internalNode.nodeIndices[leafIndex] = newNode.PageIndex;
            if (leafIndex < internalNode.upperKeyValues.Length)
            {
                internalNode.upperKeyValues[leafIndex] = key;
            }
        }

        private Node GetNode(uint pageIndex) =>
            nodeCache.ContainsKey(pageIndex) ? nodeCache[pageIndex] : pager.LoadNode(pageIndex);
    }
}