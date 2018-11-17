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
            var nodeStack = new Stack<InternalNode>();
            var currentNode = root;
            while (currentNode is InternalNode)
            {
                var currentInternalNode = (InternalNode)currentNode;
                nodeStack.Push(currentInternalNode);
                var childNodeIndex = currentInternalNode.GetNodeIndexForKey(key);
                currentNode = GetNode(childNodeIndex);
            }

            var leafNode = (LeafNode)currentNode;
            if (leafNode.CanFitDataRow(data))
            {
                leafNode.AddDataRow(key, data);
            }
            else
            {
            }
        }

        public byte[] GetData(int key)
        {
            return new byte[0];
        }

        private Node GetNode(uint pageIndex) =>
            nodeCache.ContainsKey(pageIndex) ? nodeCache[pageIndex] : pager.LoadNode(pageIndex);
    }
}