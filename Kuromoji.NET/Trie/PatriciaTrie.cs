using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;

namespace Kuromoji.NET.Trie
{
    public class PatriciaTrie<T> : IDictionary<string, T>
    {
        /// <summary>
        /// Should only be used by PatriciaTrieFormatter
        /// </summary>
        public PatriciaNode Root { get; private set; }

        /// <summary>
        /// Should only be used by PatriciaTrieFormatter
        /// </summary>
        public IKeyMapper<string> KeyMapper { get; } = new StringKeyMapper();

        public PatriciaTrie()
        {
            Clear();
        }

        /// <summary>
        /// Get value associated with specified key in this trie
        /// </summary>
        /// <param name="key">key to retrieve value for</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">in case key is null</exception>
        public T this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                // Empty keys are stored in the root
                if (string.IsNullOrEmpty(key))
                {
                    if (Root.Right == null)
                    {
                        throw new KeyNotFoundException($"not found: {key}");
                    }
                    else
                    {
                        return Root.Right.Value;
                    }
                }

                // Find nearest node
                var nearest = FindNearestNode(key);

                // If the nearest node matches key, we have a match
                if (key == nearest.Key)
                {
                    return nearest.Value;
                }
                else
                {
                    throw new KeyNotFoundException($"not found: {key}");
                }
            }
            set
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// Returns a copy of the keys contained in this trie as a Set
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                var keys = new HashSet<string>();
                KeysR(Root.Left, -1, keys);
                return keys;
            }
        }

        /// <summary>
        /// Returns a copy of the values contained in this trie as a Set
        /// </summary>
        public ICollection<T> Values
        {
            get
            {
                var values = new List<T>();
                ValuesR(Root.Left, -1, values);
                return values;
            }
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Test key prefix membership in this trie (prefix search using key)
        /// </summary>
        /// <param name="prefix">key prefix to search</param>
        /// <returns>true if trie contains key prefix</returns>
        /// <exception cref="ArgumentNullException">in case key is null</exception>
        public bool ContainsKeyPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (string.IsNullOrEmpty(prefix))
            {
                return true;
            }

            var nearest = FindNearestNode(prefix);

            if (nearest == null || nearest.Key == null)
            {
                return false;
            }

            return nearest.Key.StartsWith(prefix);
        }

        /// <summary>
        /// Puts value into trie identifiable by key into this trie (key should be non-null)
        /// </summary>
        /// <param name="key">key to associate with value</param>
        /// <param name="value">value be inserted</param>
        /// <exception cref="ArgumentNullException">in case key is null</exception>
        public void Add(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Empty keys are stored in the root
            if (string.IsNullOrEmpty(key))
            {
                Root.Right = new PatriciaNode(key, value, -1);
                Count++;
                return;
            }

            // Find nearest node
            var nearest = FindNearestNode(key);

            // Key already exist, replace value and return
            if (key == nearest.Key)
            {
                nearest.Value = value;
                return;
            }

            // Find differing bit
            var bit = FindFirstDifferingBit(key, nearest.Key);

            // Insert new node
            InsertNode(new PatriciaNode(key, value, bit));
            Count++;
        }

        /// <summary>
        /// Puts value into trie identifiable by key into this trie (key should be non-null)
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException">in case key is null</exception>
        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Inserts all key and value entries in a map into this trie
        /// </summary>
        /// <param name="items">items with entries to insert</param>
        public void AddRange(IEnumerable<KeyValuePair<string, T>> items)
        {
            foreach (var (key, value) in items)
            {
                Add(key, value);
            }
        }

        public void Clear()
        {
            Root = new PatriciaNode(null, default, -1);
            Root.Left = Root;
            Count = 0;
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return this.AsEnumerable().Contains(item);
        }

        /// <summary>
        /// Test membership in this trie
        /// </summary>
        /// <param name="key">to test if exists</param>
        /// <returns>true if trie contains key</returns>
        /// <exception cref="ArgumentNullException">in case key is null</exception>
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Empty keys are stored in the root
            if (string.IsNullOrEmpty(key))
            {
                return Root.Right != null;
            }
            
            // Find nearest node
            var nearest = FindNearestNode(key);

            // If the nearest node matches key, we have a match
            return key == nearest.Key;
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            foreach (var entry in this)
            {
                array[arrayIndex] = entry;
                arrayIndex++;
            }
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            var result = new Dictionary<string, T>();
            EntriesR(Root, -1, result);
            return result.GetEnumerator();
        }

        /// <summary>
        /// Removes entry identified by key from this trie (currently unsupported)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes entry identified by key from this trie (currently unsupported)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, T> item)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out T value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Finds the closest node in the trie matching key
        /// </summary>
        /// <param name="key">key to look up</param>
        /// <returns>closest node, null null</returns>
        PatriciaNode FindNearestNode(string key)
        {
            var current = Root.Left;
            var parent = Root;

            while (parent.Bit < current.Bit)
            {
                parent = current;
                if (!KeyMapper.IsSet(current.Bit, key))
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            return current;
        }

        /// <summary>
        /// Returns the leftmost differing bit index when doing a bitwise comparison of key1 and key2
        /// </summary>
        /// <param name="key1">first key to compare</param>
        /// <param name="key2">second key to compare</param>
        /// <returns>bit index of first different bit</returns>
        int FindFirstDifferingBit(string key1, string key2)
        {
            var bit = 0;

            while (KeyMapper.IsSet(bit, key1) == KeyMapper.IsSet(bit, key2))
            {
                bit++;
            }

            return bit;
        }

        /// <summary>
        /// Inserts a node into this trie
        /// </summary>
        /// <param name="node">node to insert</param>
        void InsertNode(PatriciaNode node)
        {
            var current = Root.Left;
            var parent = Root;

            while (parent.Bit < current.Bit && current.Bit < node.Bit)
            {
                parent = current;
                if (!KeyMapper.IsSet(current.Bit, node.Key))
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            if (!KeyMapper.IsSet(node.Bit, node.Key))
            {
                node.Left = node;
                node.Right = current;
            }
            else
            {
                node.Left = current;
                node.Right = node;
            }

            if (!KeyMapper.IsSet(parent.Bit, node.Key))
            {
                parent.Left = node;
            }
            else
            {
                parent.Right = node;
            }
        }

        void ValuesR(PatriciaNode node, int bit, List<T> list)
        {
            if (node.Bit > bit)
            {
                ValuesR(node.Left, node.Bit, list);
                ValuesR(node.Right, node.Bit, list);
                list.Add(node.Value);
            }
        }

        void KeysR(PatriciaNode node, int bit, ISet<string> keys)
        {
            if (node.Bit > bit)
            {
                KeysR(node.Left, node.Bit, keys);
                KeysR(node.Right, node.Bit, keys);
                keys.Add(node.Key);
            }
        }

        void EntriesR(PatriciaNode node, int bit, Dictionary<string, T> entries)
        {
            if (node.Bit > bit)
            {
                EntriesR(node.Left, node.Bit, entries);
                EntriesR(node.Right, node.Bit, entries);
                entries.Add(node.Key, node.Value);
            }
        }

        /// <summary>
        /// Generic interface to map a key to bits
        /// </summary>
        /// <typeparam name="K">key type</typeparam>
        public interface IKeyMapper<K>
        {
            /// <summary>
            /// Tests a bit in a key
            /// </summary>
            /// <param name="bit">bit to test</param>
            /// <param name="key">key to use as a base for testing</param>
            /// <returns>true if the specified bit is set in the provided key</returns>
            bool IsSet(int bit, K key);

            /// <summary>
            /// Formats a key as a String
            /// </summary>
            /// <param name="key">key to format to a String</param>
            /// <returns>key formatted as a String, not null</returns>
            string ToBitString(string key);
        }

        /// <summary>
        /// A KeyMapper mapping Strings to bits
        /// </summary>
        public class StringKeyMapper : IKeyMapper<string>
        {
            const int CharBits = sizeof(char) * 8;

            public bool IsSet(int bit, string key)
            {
                if (key == null)
                {
                    return false;
                }

                if (bit >= key.Length * CharBits)
                {
                    return true;
                }

                var codePoint = key.CodePointAt(bit / CharBits);
                var mask = 1 << (CharBits - 1 - (bit % CharBits));

                return (codePoint & mask) != 0;
            }

            public string ToBitString(string key)
            {
                var length = key.Length * CharBits;
                var builder = new StringBuilder();

                for (var i = 0; i < length; i++)
                {
                    builder.Append(IsSet(i, key) ? "1" : "0");
                    if (((i + 1) % 4) == 0 && i < length)
                    {
                        builder.Append(" ");
                    }
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Nodes used in a PatriciaTrie containing a String key and associated value data
        /// </summary>
        public class PatriciaNode
        {
            public string Key { get; }

            public T Value { get; set; }

            public int Bit { get; }

            public PatriciaNode Left { get; set; }

            public PatriciaNode Right { get; set; }

            public PatriciaNode(string key, T value, int bit)
            {
                Key = key;
                Value = value;
                Bit = bit;

            }

            public override string ToString()
            {
                return $"key: {Key}, bit: {Bit}, value: {Value}, left: { (Left == null ? "null" : Left.Key) }, right: { (Right == null ? "null" : Right.Key) }";
            }
        }
    }
}