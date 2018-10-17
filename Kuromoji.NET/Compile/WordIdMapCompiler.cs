using Kuromoji.NET.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Compile
{
    public class WordIdMapCompiler : ICompiler
    {
        int[][] WordIds { get; set; } = new int[1][];

        int[] Indices { get; set; }

        GrowableIntArray WordIdArray { get; } = new GrowableIntArray();

        public void AddMapping(int sourceId, int wordId)
        {
            if (WordIds.Length <= sourceId)
            {
                var newArray = WordIds;
                Array.Resize(ref newArray, sourceId + 1);
                WordIds = newArray;
            }

            // Prepare array -- extend the length of array by one
            var current = WordIds[sourceId];
            if (current == null)
            {
                current = new int[1];
            }
            else
            {
                Array.Resize(ref current, current.Length + 1);
            }
            WordIds[sourceId] = current;

            var targets = WordIds[sourceId];
            targets[targets.Length - 1] = wordId;
        }

        public void Write(Stream output)
        {
            Compile();
            IntArrayIO.WriteArray(output, Indices);
            IntArrayIO.WriteArray(output, WordIdArray.GetArray());
        }

        public void Compile()
        {
            Indices = new int[WordIds.Length];
            var wordIdIndex = 0;

            for (var i = 0; i < WordIds.Length; i++)
            {
                var inner = WordIds[i];

                if (inner == null)
                {
                    Indices[i] = -1;
                }
                else
                {
                    Indices[i] = wordIdIndex;
                    WordIdArray[wordIdIndex++] = inner.Length;

                    for (int j = 0; j < inner.Length; j++)
                    {
                        WordIdArray[wordIdIndex++] = inner[j];
                    }
                }
            }
        }

        public class GrowableIntArray
        {
            const float GrowRate = 1.25F;

            const int InitialSize = 1024;

            public int this[int index]
            {
                get => Array[index];
                set
                {
                    if (index >= Array.Length)
                    {
                        Grow(GetNewLength(index));
                    }

                    MaxIndex = Math.Max(index, MaxIndex);

                    Array[index] = value;
                }
            }

            int MaxIndex { get; set; }

            int[] Array { get; set; }

            public GrowableIntArray() : this(InitialSize) { }

            public GrowableIntArray(int size)
            {
                Array = new int[size];
            }

            public int[] GetArray()
            {
                var length = MaxIndex + 1;
                var result = new int[length];
                System.Buffer.BlockCopy(Array, 0, result, 0, Math.Min(Array.Length, length) * sizeof(int));
                return result;
            }

            void Grow(int newLength)
            {
                var tmp = Array;
                System.Array.Resize(ref tmp, newLength);
                Array = tmp;
            }

            int GetNewLength(int index)
            {
                return Math.Max(index + 1, (int)(Array.Length * GrowRate));
            }
        }
    }
}
