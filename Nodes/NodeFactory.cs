namespace SharpDb
{
    public static class NodeFactory
    {
        public static Node DeserilizeNode(PageIndex pageIndex, byte[] data)
        {
            var isLeaf = data[0];
            if (isLeaf == 0)
            {
                return new InternalNode(pageIndex, data);
            }
            else
            {
                return new LeafNode(pageIndex, data);
            }
        }
    }
}