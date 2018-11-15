using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace SharpDb
{
    public class Program
    {
        public static void Main()
        {
        }

        public static void Test02()
        {
            var node = new InternalNode();
            node.AddNode(100, 5);
            node.AddNode(101, 10);
            node.AddNode(102, 15);
            node.AddNode(103, 20);
            node.AddNode(104, 25);
            node.AddNode(105, 30);
            node.AddNode(106, 28);
            Console.WriteLine(node.GetUpperKey());
            Console.WriteLine(node.GetNodeIndexForKey(18));
            var serialized = node.Serialize();
            node = new InternalNode(5, serialized);
            Console.WriteLine(node.GetUpperKey());
            Console.WriteLine(node.GetNodeIndexForKey(18));
        }

        public static void Test01()
        {
            using (var dbFile = MemoryMappedFile.CreateNew("Db", Constants.DbMaxSize))
            {
                Pager.Init(dbFile);
                var pager = Pager.Get();
                var stubNodes = new LeafNode[]
                {
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>(),
                    pager.NewNode<LeafNode>()
                };
                var node = new LeafNode(stubNodes[2].PageIndex, data);
                pager.SaveNode(node);
                var emptyNode = pager.LoadNode(1);
                var populatedNode = pager.LoadNode(2);
            }
        }

        public static byte[] data = new byte[]
        {
            1, // Is leaf
            0,
            0,
            0,
            1, // key
            0,
            0,
            0,
            7, // size
            15,
            44,
            12,
            33,
            11,
            67,
            123, // data
            0,
            0,
            0,
            2, // key
            0,
            0,
            0,
            1, // size
            56, // data
            0,
            0,
            0,
            3, // key
            0,
            0,
            0,
            6, // size
            34,
            55,
            12,
            90,
            byte.MaxValue,
            211,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        };
    }
}
