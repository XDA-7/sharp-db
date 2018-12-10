using System.Collections.Generic;

namespace SharpDb
{
    public class NodeList
    {
        private List<LinkedNode> linkedNodes = new List<LinkedNode>();

        private Pager pager = Pager.Get();

        public NodeList(PageIndex rootIndex)
        {
        }
    }
}