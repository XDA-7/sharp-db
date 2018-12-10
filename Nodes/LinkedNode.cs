namespace SharpDb
{
    public class LinkedNode : Node
    {
        private PageIndex nextNode = 0;

        private byte[] data;

        public byte[] Data { get => data; }

        public LinkedNode() : base()
        {
        }

        public LinkedNode(PageIndex pageIndex, byte[] data) : base(pageIndex, data)
        {
        }

        public override byte[] Serialize()
        {
            var result = new byte[Constants.PageSize];
            var nextNodeBytes = Serializer.SerializeInt((int)nextNode);
            nextNodeBytes.CopyTo(result, 0);
            data.CopyTo(result, 4);
            return result;
        }

        protected override void Deserialize(byte[] data)
        {
            var index = 0;
            nextNode = Serializer.DeserializeInt(data, ref index);
            this.data = new byte[data.Length];
            for (var i = 0; i < this.data.Length; i++)
            {
                this.data[i] = data[i + 4];
            }
        }
    }
}