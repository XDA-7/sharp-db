namespace SharpDb
{
    public abstract class Node
    {
        public PageIndex PageIndex { get; set; }

        public Node()
        {
        }

        public Node(PageIndex pageIndex, byte[] data)
        {
            PageIndex = pageIndex;
            Deserialize(data);
        }

        public abstract byte[] Serialize();

        protected abstract void Deserialize(byte[] data);
    }
}