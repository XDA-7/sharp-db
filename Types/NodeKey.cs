using System;

namespace SharpDb
{
    public class NodeKey : IComparable
    {
        private int value = 0;

        private NodeKey(int value) => this.value = value;

        public static implicit operator NodeKey(int value) => new NodeKey(value);

        public static explicit operator int(NodeKey nodeKey) => nodeKey.value;

        public static bool operator >(NodeKey left, NodeKey right) => left.value > right.value;

        public static bool operator <(NodeKey left, NodeKey right) => left.value < right.value;

        public static bool operator ==(NodeKey left, NodeKey right) => left.value == right.value;

        public static bool operator !=(NodeKey left, NodeKey right) => left.value != right.value;

        public override bool Equals(object obj) => this.value == ((NodeKey)obj).value;

        public override int GetHashCode() => this.value.GetHashCode();

        public int CompareTo(object obj) => this.value.CompareTo(((NodeKey)obj).value);
    }
}