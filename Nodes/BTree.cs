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
        }

        private void AddLeafNodeToInternalNode(InternalNode internalNode, int leafIndex, int key, byte[] data)
        {
        }

        private Node GetNode(uint pageIndex) =>
            nodeCache.ContainsKey(pageIndex) ? nodeCache[pageIndex] : pager.LoadNode(pageIndex);
    }
}