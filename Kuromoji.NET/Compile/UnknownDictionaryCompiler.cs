using Kuromoji.NET.Dict;
using Kuromoji.NET.IO;
using Kuromoji.NET.Util;
using Kuromoji.NET.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Compile
{
    public class UnknownDictionaryCompiler : ICompiler, IDisposable
    {
        SortedDictionary<string, int> CategoryMap { get; }

        public List<GenericDictionaryEntry> DictionaryEntries { get; } = new List<GenericDictionaryEntry>();

        Stream Output { get; }

        bool LeaveOpen { get; }

        bool Disposed { get; set; }

        public UnknownDictionaryCompiler(Stream output, SortedDictionary<string, int> categoryMap) : this(output, categoryMap, false) { }

        public UnknownDictionaryCompiler(Stream output, SortedDictionary<string, int> categoryMap, bool leaveOpen)
        {
            CategoryMap = categoryMap;
            Output = output;
            LeaveOpen = leaveOpen;
        }

        public void ReadUnknownDefinition(Stream input, Encoding encoding)
        {
            foreach (var line in input.ReadLines(encoding))
            {
                var entry = UnknownDictionaryEntryParser.Parse(line);
                DictionaryEntries.Add(entry);
            }
        }

        public int[][] MakeCosts()
        {
            var costs = new int[DictionaryEntries.Count][];

            for (int i = 0; i < DictionaryEntries.Count; i++)
            {
                var entry = DictionaryEntries[i];

                costs[i] = new int[] { entry.LeftId, entry.RightId, entry.WordCost };
            }

            return costs;
        }

        public string[][] MakeFeatures()
        {
            var features = new string[DictionaryEntries.Count][];

            for (int i = 0; i < DictionaryEntries.Count; i++)
            {
                var entry = DictionaryEntries[i];
                features[i] = entry.PartOfSpeechFeatures.Concat(entry.OtherFeatures).ToArray();
            }

            return features;
        }

        public int[][] MakeCategoryReferences()
        {
            var entries = new int[CategoryMap.Count][];

            foreach (var category in CategoryMap.Keys)
            {
                var categoryId = CategoryMap[category];
                entries[categoryId] = GetEntryIndices(category);
            }

            return entries;
        }

        public void PrintFeatures(string[][] features)
        {
            for (var i = 0; i < features.Length; i++)
            {
                Console.WriteLine(i);

                var array = features[i];
                foreach (var f in features[i])
                {
                    Console.WriteLine("\t" + f);
                }
            }
        }

        public int[] GetEntryIndices(string surface)
        {
            var indices = new List<int>();

            for (int i = 0; i < DictionaryEntries.Count; i++)
            {
                var entry = DictionaryEntries[i];

                if (entry.Surface == surface)
                {
                    indices.Add(i);
                }
            }

            return indices.ToArray();
        }

        public void Compile()
        {
            IntArrayIO.WriteArray2D(Output, MakeCosts());
            IntArrayIO.WriteArray2D(Output, MakeCategoryReferences());
            StringArrayIO.WriteArray2D(Output, MakeFeatures());
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (!LeaveOpen)
                {
                    Output.Dispose();
                }
                Disposed = true;
            }
        }
    }
}
