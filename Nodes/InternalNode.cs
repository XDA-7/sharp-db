namespace SharpDb
{
    public class InternalNode : Node
    {
        private int[] upperKeyValues;
        
        private uint[] nodeIndices;

        public InternalNode()
        {
            upperKeyValues = new int[(Constants.PageSize / 8) - 1];
            nodeIndices = new uint[Constants.PageSize / 8];
        }

        public InternalNode(uint pageIndex, byte[] data)
        {
            PageIndex = pageIndex;
            Deserialize(data);
        }

        public uint GetNodeIndexForKey(int key)
        {
            for (var i = 0; i < upperKeyValues.Length; i++)
            {
                if (upperKeyValues[i] > key)
                {
                    return nodeIndices[i];
                }
            }

            return nodeIndices[upperKeyValues.Length];
        }

        public bool IsFull() => nodeIndices[nodeIndices.Length + 1] != 0;

        public int GetUpperKey()
        {
            for (var i = upperKeyValues.Length; i >= 0; i--)
            {
                if (upperKeyValues[i] != 0)
                {
                    return upperKeyValues[i];
                }
            }

            return 0;
        }

        public void AddNode(uint pageIndex, int key)
        {
            var i = 0;
            uint shiftIndex = 0;
            var shiftKey = 0;
            for (; i < nodeIndices.Length; i++)
            {
                if (key > upperKeyValues[i])
                {
                    shiftIndex = ind
                }
            }
        }

        protected override void DeserializeData(byte[] data, int index)
        {
        }

        public override byte[] Serialize()
        {
            return new byte[Constants.PageSize];
        }
    }
}