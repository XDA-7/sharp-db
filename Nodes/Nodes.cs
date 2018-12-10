namespace SharpDb
{
    public abstract class Node
    {
        public PageIndex PageIndex { get; set; }

        public abstract byte[] Serialize();

        protected abstract void DeserializeData(byte[] data, int index);

        protected void Deserialize(byte[] data)
        {
            var index = 1; // First byte indicates the node's type
            DeserializeData(data, index);
        }
    }
}