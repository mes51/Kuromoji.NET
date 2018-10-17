using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Compile;
using Kuromoji.NET.Extentions;
using Kuromoji.NET.Util;

namespace Kuromoji.NET.Trie
{
    public class DoubleArrayTrie
    {
        public const string DoubleArrayTrieFileName = "doubleArrayTrie.bin";

        public const char TerminatingCharacter = '\u0001';

        const int BaseCheckInitialSize = 2800000;

        const int TailInitialSize = 200000;

        const int TailOffset = 100000000;

        const float BufferGrowthPercentage = 0.25F;

        public int[] BaseBuffer { get; private set; }

        public int[] CheckBuffer { get; private set; }

        public char[] TailBuffer { get; private set; }

        int TailIndex { get; set; } = TailOffset;

        int MaxBaseCheckIndex { get; set; }

        bool Compact { get; }

        public DoubleArrayTrie() : this(false) { }

        public DoubleArrayTrie(bool compactTrie)
        {
            Compact = compactTrie;
        }

        /// <summary>
        /// Construct double array trie which is equivalent to input trie
        /// </summary>
        /// <param name="trie">normal trie, which contains all dictionary words</param>
        public void Build(Trie trie)
        {
            ProgressLog.Begin("building " + (Compact ? "compact" : "sparse") + " trie");

            BaseBuffer = new int[BaseCheckInitialSize];
            BaseBuffer[0] = 1;
            CheckBuffer = new int[BaseCheckInitialSize];
            TailBuffer = new char[TailInitialSize];
            Add(-1, 0, trie.Root);
            ReportUtilizationRate();

            ProgressLog.End();
        }

        public void Write(Stream output)
        {
            var baseCheckSize = Math.Min(MaxBaseCheckIndex + 64, BaseBuffer.Length);
            var tailSize = Math.Min(TailIndex - TailOffset + 64, TailBuffer.Length);

            output.Write(Compact);
            output.Write(baseCheckSize);
            output.Write(tailSize);

            output.Write(BaseBuffer, baseCheckSize);
            output.Write(CheckBuffer, baseCheckSize);
            output.Write(TailBuffer, tailSize);
        }

        /// <summary>
        /// Match input keyword.
        /// </summary>
        /// <param name="key">key to match</param>
        /// <returns>index value of last character in baseBuffer(double array id) if it is complete match. Negative value if it doesn't match. 0 if it is prefix match.</returns>
        public int Lookup(string key)
        {
            return Lookup(key, 0, 0);
        }

        public int Lookup(string key, int index, int j)
        {
            var baseIndex = 1;
            if (index != 0)
            {
                baseIndex = BaseBuffer[index];
            }

            for (var i = j; i < key.Length; i++)
            {
                var previous = index;
                if (Compact)
                {
                    index = baseIndex + key[i];
                }
                else
                {
                    index = index + baseIndex + key[i];
                }
                if (index >= BaseBuffer.Length)
                {
                    // Too long
                    return -1;
                }

                baseIndex = BaseBuffer[index];

                if (baseIndex == 0)
                {
                    // Didn't find match
                    return -1;
                }

                if (CheckBuffer[index] != previous)
                {
                    // check doesn't match
                    return -1;
                }

                if (baseIndex >= TailOffset)
                {
                    // If base is bigger than TAIL_OFFSET, start processing "tail"
                    return MatchTail(baseIndex, index, key.Substring(i + 1));
                }
            }

            // If we reach at the end of input keyword, check if it is complete match by looking for following terminating character
            int endIndex;
            if (Compact)
            {
                endIndex = baseIndex + TerminatingCharacter;
            }
            else
            {
                endIndex = index + baseIndex + TerminatingCharacter;
            }

            return CheckBuffer[endIndex] == index ? index : 0;
        }

        /// <summary>
        /// Check match in tail array
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="index"></param>
        /// <param name="key"></param>
        /// <returns>index if it is complete match. 0 if it is prefix match. negative value if it doesn't match</returns>
        int MatchTail(int baseIndex, int index, string key)
        {
            var positionInTailArr = baseIndex - TailOffset;

            int keyLength = key.Length;
            for (int i = 0; i < keyLength; i++)
            {
                if (key[i] != TailBuffer[positionInTailArr + i])
                {
                    return -1;
                }
            }
            return TailBuffer[positionInTailArr + keyLength] == TerminatingCharacter ? index : 0;
        }

        /// <summary>
        /// Find base value for current node, which contains input nodes. They are children of current node.
        /// Set default base value , which is one, at the index of each input node.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nodes"></param>
        /// <returns>base value for current node</returns>
        int FindBase(int index, List<Trie.Node> nodes)
        {
            var baseIndex = BaseBuffer[index];
            if (baseIndex < 0)
            {
                return baseIndex;
            }

            while (true)
            {
                var collision = false;    // already taken?
                foreach (var node in nodes)
                {
                    var nextIndex = index + baseIndex + node.Key;
                    MaxBaseCheckIndex = Math.Max(MaxBaseCheckIndex, nextIndex);

                    if (BaseBuffer.Length <= nextIndex)
                    {
                        ExtendBuffers(nextIndex);
                    }

                    if (BaseBuffer[nextIndex] != 0)
                    {
                        // already taken
                        baseIndex++;    // check next base value
                        collision = true;
                        break;
                    }
                }

                if (!collision)
                {
                    break;    // if there is no collision, found proper base value. Break the while loop.
                }

            }

            foreach (var node in nodes)
            {
                BaseBuffer[index + baseIndex + node.Key] = node.Key == TerminatingCharacter ? -1 : 1;    // Set -1 if key is terminating character. Set default base value 1 if not.
            }

            return baseIndex;
        }

        void ExtendBuffers(int nextIndex)
        {
            var newLength = nextIndex + (int)(BaseBuffer.Length * BufferGrowthPercentage);
            ProgressLog.Println("Buffers extended to " + BaseBuffer.Length + " entries");

            var tmp = BaseBuffer;
            Array.Resize(ref tmp, newLength);
            BaseBuffer = tmp;

            tmp = CheckBuffer;
            Array.Resize(ref tmp, newLength);
            CheckBuffer = tmp;
        }

        /// <summary>
        /// Add characters(nodes) to tail array
        /// </summary>
        /// <param name="node"></param>
        void AddToTail(Trie.Node node)
        {
            while (true)
            {
                if (TailBuffer.Length < TailIndex - TailOffset + 1)
                {
                    var tmp = TailBuffer;
                    Array.Resize(ref tmp, TailBuffer.Length + (int)(TailBuffer.Length * BufferGrowthPercentage));
                    TailBuffer = tmp;
                }
                TailBuffer[TailIndex++ - TailOffset] =  node.Key;// set character of current node

                if (!node.Children.Any())
                {
                    // if it reached the end of input, break.
                    break;
                }
                node = node.Children[0]; // Move to next node
            }
        }

        void ReportUtilizationRate()
        {
            var zeros = 0;

            for (int i = 0; i < MaxBaseCheckIndex; i++)
            {
                if (BaseBuffer[i] == 0)
                {
                    zeros++;
                }
            }

            ProgressLog.Println("trie memory utilization ratio (" + (!Compact ? "not " : "") + "compacted): " + ((MaxBaseCheckIndex - zeros) / (float)MaxBaseCheckIndex));
        }

        /// <summary>
        /// Recursively add Nodes(characters) to double array trie
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="index"></param>
        /// <param name="node"></param>
        void Add(int previous, int index, Trie.Node node)
        {
            if (node.Children.Any() && node.HasSinglePath() && node.Children[0].Key != TerminatingCharacter)
            {
                // If node has only one path, put the rest in tail array
                BaseBuffer[index] = TailIndex;    // current index of tail array
                AddToTail(node.Children[0]);
                CheckBuffer[index] =  previous;
                return;    // No more child to process
            }

            var startIndex = (Compact ? 0 : index);
            var baseIndex = FindBase(startIndex, node.Children);

            BaseBuffer[index] =  baseIndex;

            if (previous >= 0)
            {
                CheckBuffer[index] =  previous;    // Set check value
            }

            foreach (var child in node.Children)
            {
                // For each child to double array trie
                if (Compact)
                {
                    Add(index, baseIndex + child.Key, child);
                }
                else
                {
                    Add(index, index + baseIndex + child.Key, child);
                }
            }
        }

        public static DoubleArrayTrie NewInstance(IResourceResolver resolver)
        {
            using (var stream = resolver.Resolve(DoubleArrayTrieFileName))
            {
                return Read(stream);
            }
        }

        public static DoubleArrayTrie Read(Stream input)
        {
            var trie = new DoubleArrayTrie(input.ReadBool());

            var baseCheckSize = input.ReadInt32();
            var tailSize = input.ReadInt32();

            trie.BaseBuffer = new int[baseCheckSize];
            trie.CheckBuffer = new int[baseCheckSize];
            trie.TailBuffer = new char[tailSize];

            input.ReadIntArray(trie.BaseBuffer);
            input.ReadIntArray(trie.CheckBuffer);
            input.ReadCharArray(trie.TailBuffer);

            return trie;
        }
    }
}
