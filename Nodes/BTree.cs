using System.Collections.Generic;

namespace SharpDb
{
    public class BTree
    {
        private Node root;

        private Pager pager = Pager.Get();

        private Dictionary<PageIndex, Node> nodeCache = new Dictionary<PageIndex, Node>();

        public BTree(Node root)
        {
            this.root = root;
            nodeCache.Add(root.PageIndex, root);
        }

        public void InsertData(NodeKey key, byte[] data)
        {
            if (root is LeafNode)
            {
                InsertDataInLeafRoot(key, data);
                return;
            }

            var nodeStack = new Stack<InternalNode>();
            var currentNode = root;
            // Descend the btree, recording the path from root to leaf
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
                var largestKey = leafNode.GetLargestKey();
                leafNode.AddDataRow(key, data);
            }
            else // Split the leaf node in two and find a place for the lower-ordered one
            {
                var largestKey = leafNode.GetLargestKey();
                leafNode.AddDataRow(key, data);

                var splitLeaf = leafNode.Split();
                splitLeaf.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(splitLeaf.PageIndex, splitLeaf);

                var currentInternalNode = nodeStack.Pop(); // Take the parent to the leaf nodes
                if (!currentInternalNode.IsFull())
                {
                    currentInternalNode.AddNode(splitLeaf.PageIndex, splitLeaf.GetLargestKey());
                }
                else // Keep splitting internal nodes and adding them to the parent until reaching one that can fit a new node without splitting
                {
                    currentInternalNode.AddNode(splitLeaf.PageIndex, splitLeaf.GetLargestKey());
                    var splitInternalNode = currentInternalNode.Split();
                    splitInternalNode.PageIndex = pager.NewNodeIndex();
                    nodeCache.Add(splitInternalNode.PageIndex, splitInternalNode);

                    currentInternalNode = GetParentNode(nodeStack, currentInternalNode);
                    while (currentInternalNode.IsFull())
                    {
                        currentInternalNode.AddNode(splitInternalNode.PageIndex, splitInternalNode.GetLargestKey());
                        splitInternalNode = currentInternalNode.Split();
                        splitInternalNode.PageIndex = pager.NewNodeIndex();
                        nodeCache.Add(splitInternalNode.PageIndex, splitInternalNode);

                        currentInternalNode = GetParentNode(nodeStack, currentInternalNode);
                    }

                    currentInternalNode.AddNode(splitInternalNode.PageIndex, splitInternalNode.GetLargestKey());
                }
            }
        }

        private InternalNode GetParentNode(Stack<InternalNode> nodeStack, InternalNode currentInternalNode)
        {
            if (nodeStack.Count != 0)
            {
                return nodeStack.Pop();
            }
            else // The root node has been reached, create a new root node, however the index of the root should always remain the same
            {
                var rootIndex = currentInternalNode.PageIndex;
                nodeCache.Remove(rootIndex);
                currentInternalNode.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(currentInternalNode.PageIndex, currentInternalNode);

                root = new InternalNode();
                root.PageIndex = rootIndex;
                nodeCache.Add(root.PageIndex, root);

                ((InternalNode)root).AddNode(currentInternalNode.PageIndex, currentInternalNode.GetLargestKey());
                return (InternalNode)root;
            }
        }

        private void InsertDataInLeafRoot(NodeKey key, byte[] data)
        {
            var leafRoot = (LeafNode)root;
            if (leafRoot.CanFitDataRow(data))
            {
                leafRoot.AddDataRow(key, data);
            }
            else
            {
                var rootIndex = root.PageIndex; // The index of the root should always remain the same
                nodeCache.Remove(rootIndex);
                leafRoot.AddDataRow(key, data);

                // Move the soon to be former root into a new position
                leafRoot.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(leafRoot.PageIndex, leafRoot);

                var splitNode = leafRoot.Split();
                splitNode.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(splitNode.PageIndex, splitNode);

                root = new InternalNode();
                root.PageIndex = rootIndex;
                nodeCache.Add(root.PageIndex, root);

                ((InternalNode)root).AddNode(leafRoot.PageIndex, int.MaxValue); // We need any key larger than anything we currently have to fit in the right node
                ((InternalNode)root).AddNode(splitNode.PageIndex, splitNode.GetLargestKey());
            }
        }

        public byte[] GetData(NodeKey key)
        {
            var currentNode = root;
            while (currentNode is InternalNode)
            {
                var childIndex = ((InternalNode)currentNode).GetNodeIndexForKey(key);
                currentNode = GetNode(childIndex);
            }

            return ((LeafNode)currentNode).GetDataRow(key);
        }

        public void SaveNodes()
        {
            foreach (var node in nodeCache.Values)
            {
                pager.SaveNode(node);
            }
        }

        private Node GetNode(PageIndex pageIndex) =>
            nodeCache.ContainsKey(pageIndex) ? nodeCache[pageIndex] : pager.LoadNode(pageIndex);
    }
}