using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace SharpDb
{
    public class Pager
    {
        private MemoryMappedFile dbFile;

        private uint pagesUsed;

        private static Pager pager;

        public static void Init(MemoryMappedFile dbFile) => pager = new Pager(dbFile);

        public static Pager Get() => pager;

        private Pager(MemoryMappedFile dbFile)
        {
            this.dbFile = dbFile;
        }

        public T NewNode<T>() where T : Node, new()
        {
            var result = new T();
            result.PageIndex = pagesUsed;
            pagesUsed++;
            return result;
        }

        public void SaveNode(Node node)
        {
            using (var stream = dbFile.CreateViewStream(node.PageIndex * Constants.PageSize, Constants.PageSize))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(node.Serialize());
            }
        }

        public Node LoadNode(uint pageIndex)
        {
            using (var stream = dbFile.CreateViewStream(pageIndex * Constants.PageSize, Constants.PageSize))
            {
                var reader = new BinaryReader(stream);
                var data = reader.ReadBytes(Constants.PageSize);
                return NodeFactory.DeserilizeNode(pageIndex, data);
            }
        }
    }
}