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

        public static void Test07()
        {
            var randomiser = new KeyRandomiser();
            for (var i = -1000000; i < 1000000; i++)
            {
                var nodeKey = randomiser.GetKeyFromId(i);
                var hopefullyI = randomiser.GetIdFromNodeKey(nodeKey);
                if (i != hopefullyI)
                {
                    Console.WriteLine(i + " became " + hopefullyI);
                    break;
                }
            }
        }

        public static void Test06()
        {
            using (var dbFile = MemoryMappedFile.CreateFromFile("Db", FileMode.OpenOrCreate, "Db", Constants.DbMaxSize))
            {
                Pager.Init(dbFile);
                var reader = new BinaryReader(dbFile.CreateViewStream(0 * Constants.PageSize, Constants.PageSize));
                var data = reader.ReadBytes(Constants.PageSize);
                Console.WriteLine(data[0]);
                var index = 1;
                while (index + 4 < data.Length)
                {
                    var result = DeserializeInt(data, ref index);
                    if (result != 0)
                    {
                        Console.Write(result + ",");
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static byte[] SerializeInt(int value)
        {
            return BitConverter.GetBytes(value);
        }

        private static int DeserializeInt(byte[] data, ref int index)
        {
            var result = BitConverter.ToInt32(data, index);
            index += 4;
            return result;
        }

        public static void Test05()
        {
            using (var dbFile = MemoryMappedFile.CreateFromFile("Db", FileMode.OpenOrCreate, "Db", Constants.DbMaxSize))
            {
                Pager.Init(dbFile);
                var reader = new BinaryReader(dbFile.CreateViewStream(0, Constants.PageSize));
                var data = reader.ReadBytes(Constants.PageSize);
                var root = new InternalNode(0, data);
                var bTree = new BTree(root);
                data = bTree.GetData(876726186);
                Console.WriteLine(data[3]);
            }
        }

        public static void Test04()
        {
            using (var dbFile = MemoryMappedFile.CreateFromFile("Db", FileMode.OpenOrCreate, "Db", Constants.DbMaxSize))
            {
                Pager.Init(dbFile);
                var pager = Pager.Get();
                var rng = new Random();
                var root = new LeafNode();
                root.PageIndex = pager.NewNodeIndex();
                pager.SaveNode(root);
                var bTree = new BTree(root);
                var usedKeys = new HashSet<int>();
                var chosenKey = 0;
                for (var i = 0; i < 2000000; i++)
                {
                    if (i % 100000 == 0)
                    {
                        Console.WriteLine(i);
                    }

                    try
                    {
                        var key = rng.Next(1, int.MaxValue);
                        while (usedKeys.Contains(key))
                        {
                            key = rng.Next(1, int.MaxValue);
                        }

                        var byteValue = (byte)(key % 256);
                        if (i == 313284)
                        {
                            chosenKey = key;
                        }
                        bTree.InsertData(key, new byte[] { 0, 0, 0,  byteValue, 0, 0, 0, byteValue, 0, 0, 0, byteValue, 0, 0, 0, byteValue, 0, 0, 0, byteValue, 0, 0, 0, byteValue, 0, 0, 0, byteValue, 0, 0, 0, byteValue });
                        usedKeys.Add(key);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Iteration: " + i);
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        break;
                    }
                }

                bTree.SaveNodes();

                var data = bTree.GetData(chosenKey);
                Console.WriteLine(chosenKey);
                Console.WriteLine(chosenKey % 256);
                Console.WriteLine(string.Join(",", data));
            }
        }

        public static void Test03()
        {
            var intNode = new InternalNode();
            intNode.AddNode(100, 5);
            intNode.AddNode(101, 10);
            intNode.AddNode(102, 15);
            intNode.AddNode(103, 20);
            intNode.AddNode(104, 25);
            intNode.AddNode(105, 30);
            var split = intNode.Split();
            Console.WriteLine(intNode.GetNodeIndexForKey(28));
            Console.WriteLine(split.GetNodeIndexForKey(28));
            var leafNode = new LeafNode();
            leafNode.AddDataRow(50, new byte[] { 1 });
            leafNode.AddDataRow(100, new byte[] { 2 });
            leafNode.AddDataRow(150, new byte[] { 3 });
            leafNode.AddDataRow(200, new byte[] { 4 });
            leafNode.AddDataRow(250, new byte[] { 5 });
            leafNode.AddDataRow(300, new byte[] { 6 });
            leafNode.AddDataRow(350, new byte[] { 7 });
            leafNode.AddDataRow(400, new byte[] { 8 });
            Console.WriteLine(leafNode.GetDataRow(350)[0]);
            var leafSplit = leafNode.Split();
            Console.WriteLine(leafNode.GetDataRow(350)[0]);
            Console.WriteLine(leafSplit.GetDataRow(350).Length);
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
            Console.WriteLine(node.GetNodeIndexForKey(18));
            var serialized = node.Serialize();
            node = new InternalNode(5, serialized);
            Console.WriteLine(node.GetNodeIndexForKey(18));
        }

        public static void Test01()
        {
            using (var dbFile = MemoryMappedFile.CreateNew("Db", Constants.DbMaxSize))
            {
                Pager.Init(dbFile);
                var pager = Pager.Get();
                var indices = new PageIndex[]
                {
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex(),
                    pager.NewNodeIndex()
                };
                var node = new LeafNode(indices[2], data);
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
