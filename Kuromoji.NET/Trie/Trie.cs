using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Trie
{
    /// <summary>
    /// Simple Trie used to build the DoubleArrayTrie
    /// </summary>
    public class Trie
    {
        /// <summary>
        /// Root node
        /// </summary>
        public Node Root { get; } = new Node();

        /// <summary>
        /// Adds an input value to this trie
        /// 
        /// Before the value is added, a terminating character (U+0001) is appended to the input string
        /// </summary>
        /// <param name="value">value to add to this trie</param>
        public void Add(string value)
        {
            Root.Add(value, true);
        }

        /// <summary>
        /// Trie Node
        /// </summary>
        public class Node
        {
            public char Key { get; }

            public List<Node> Children { get; } = new List<Node>();

            /// <summary>
            /// Constructor
            /// </summary>
            public Node() : this((char)0) { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="key">this node's key</param>
            public Node(char key)
            {
                Key = key;
            }

            /// <summary>
            /// Add string to add to this node
            /// </summary>
            /// <param name="value">string value, not null</param>
            public void Add(string value)
            {
                Add(value, false);
            }

            public void Add(string value, bool terminate)
            {
                if (value.Length == 0)
                {
                    return;
                }

                var node = value.Aggregate(this, (n, c) => n.AddChild(new Node(c)));

                if (terminate && node != null)
                {
                    node.AddChild(new Node(DoubleArrayTrie.TerminatingCharacter));
                }
            }

            /// <summary>
            /// Adds a new child node to this node
            /// </summary>
            /// <param name="newNode">new child to add</param>
            /// <returns>the child node added, or, if a node with same key already exists, that node</returns>
            public Node AddChild(Node newNode)
            {
                var child = GetChild(newNode.Key);
                if (child == null)
                {
                    Children.Add(newNode);
                    child = newNode;
                }

                return child;
            }

            /// <summary>
            /// Predicate indicating if children following this node forms single key path (no branching)
            /// 
            /// For example, if we have "abcde" and "abfgh" in the trie, calling this method on node "a" and "b" returns false.
            /// However, this method on "c", "d", "e", "f", "g" and "h" returns true.
            /// </summary>
            /// <returns>true if this node has a single key path. false otherwise.</returns>
            public bool HasSinglePath()
            {
                switch (Children.Count)
                {
                    case 0:
                        return true;
                    case 1:
                        return Children[0].HasSinglePath();
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Searches this nodes for a child with a specific key
            /// </summary>
            /// <param name="key">key to search for</param>
            /// <returns>node matching the input key if it exists, otherwise null</returns>
            Node GetChild(char key)
            {
                return Children.Find(c => c.Key == key);
            }
        }
    }
}
