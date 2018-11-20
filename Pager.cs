using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;

namespace SharpDb
{
    public class Pager
    {
        private MemoryMappedFile dbFile;

        private int pagesUsed;

        private static Pager pager;

        public static void Init(MemoryMappedFile dbFile) => pager = new Pager(dbFile);

        public static Pager Get() => pager;

        private Pager(MemoryMappedFile dbFile)
        {
            this.dbFile = dbFile;
        }

        public PageIndex NewNodeIndex()
        {
            var result = pagesUsed;
            pagesUsed++;
            return result;
        }

        public void SaveNode(Node node)
        {
            using (var stream = dbFile.CreateViewStream((int)node.PageIndex * Constants.PageSize, Constants.PageSize))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(node.Serialize());
            }
        }

        public Node LoadNode(PageIndex pageIndex)
        {
            using (var stream = dbFile.CreateViewStream((int)pageIndex * Constants.PageSize, Constants.PageSize))
            {
                var reader = new BinaryReader(stream);
                var data = reader.ReadBytes(Constants.PageSize);
                return NodeFactory.DeserilizeNode(pageIndex, data);
            }
        }
    }
}