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
            nodeCache.Add(root.PageIndex, root);
        }

        public void InsertData(int key, byte[] data)
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
                // pager.SaveNode(leafNode);
            }
            else // Split the leaf node in two and find a place for the lower-ordered one
            {
                var largestKey = leafNode.GetLargestKey();
                leafNode.AddDataRow(key, data);

                var splitLeaf = leafNode.Split();
                // pager.SaveNode(leafNode);
                splitLeaf.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(splitLeaf.PageIndex, splitLeaf);
                // pager.SaveNode(splitLeaf);

                var currentInternalNode = nodeStack.Pop(); // Take the parent to the leaf nodes
                if (!currentInternalNode.IsFull())
                {
                    currentInternalNode.AddNode(splitLeaf.PageIndex, splitLeaf.GetLargestKey());
                    // pager.SaveNode(currentInternalNode);
                }
                else // Keep splitting internal nodes and adding them to the parent until reaching one that can fit a new node without splitting
                {
                    currentInternalNode.AddNode(splitLeaf.PageIndex, splitLeaf.GetLargestKey());
                    var splitInternalNode = currentInternalNode.Split();
                    // pager.SaveNode(currentInternalNode);
                    splitInternalNode.PageIndex = pager.NewNodeIndex();
                    nodeCache.Add(splitInternalNode.PageIndex, splitInternalNode);
                    // pager.SaveNode(splitInternalNode);

                    currentInternalNode = GetParentNode(nodeStack, currentInternalNode);
                    while (currentInternalNode.IsFull())
                    {
                        currentInternalNode.AddNode(splitInternalNode.PageIndex, splitInternalNode.GetLargestKey());
                        splitInternalNode = currentInternalNode.Split();
                        // pager.SaveNode(currentInternalNode);
                        splitInternalNode.PageIndex = pager.NewNodeIndex();
                        nodeCache.Add(splitInternalNode.PageIndex, splitInternalNode);
                        // pager.SaveNode(splitInternalNode);

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
            else // The root node has been reached, create a new root node
            {
                root = new InternalNode();
                root.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(root.PageIndex, root);

                ((InternalNode)root).AddNode(currentInternalNode.PageIndex, currentInternalNode.GetLargestKey());
                return (InternalNode)root;
            }
        }

        private void InsertDataInLeafRoot(int key, byte[] data)
        {
            var leafRoot = (LeafNode)root;
            if (leafRoot.CanFitDataRow(data))
            {
                leafRoot.AddDataRow(key, data);
                // pager.SaveNode(leafRoot);
            }
            else
            {
                leafRoot.AddDataRow(key, data);

                var splitNode = leafRoot.Split();
                // pager.SaveNode(leafRoot);
                splitNode.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(splitNode.PageIndex, splitNode);
                // pager.SaveNode(splitNode);

                root = new InternalNode();
                root.PageIndex = pager.NewNodeIndex();
                nodeCache.Add(root.PageIndex, root);

                ((InternalNode)root).AddNode(leafRoot.PageIndex, int.MaxValue); // We need any key larger than anything we currently have to fit in the right node
                ((InternalNode)root).AddNode(splitNode.PageIndex, splitNode.GetLargestKey());
                // pager.SaveNode(root);
            }
        }

        public byte[] GetData(int key)
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

        private Node GetNode(uint pageIndex) =>
            nodeCache.ContainsKey(pageIndex) ? nodeCache[pageIndex] : pager.LoadNode(pageIndex);
    }
}