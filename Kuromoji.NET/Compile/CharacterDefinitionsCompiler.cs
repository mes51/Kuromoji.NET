using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;
using Kuromoji.NET.IO;

namespace Kuromoji.NET.Compile
{
    public class CharacterDefinitionsCompiler : ICompiler, IDisposable
    {
        public SortedDictionary<string, int[]> CategoryDefinitions { get; } = new SortedDictionary<string, int[]>();

        public List<SortedSet<string>> CodepointCategories { get; } = Enumerable.Repeat<SortedSet<string>>(null, 65536).ToList();

        Stream Output { get; }

        bool LeaveOpen { get; }

        bool Disposed { get; set; }

        public CharacterDefinitionsCompiler(Stream output) : this(output, false) { }

        public CharacterDefinitionsCompiler(Stream output, bool leaveOpen)
        {
            Output = output;
            LeaveOpen = leaveOpen;
        }

        public void ReadCharacterDefinition(Stream input, Encoding encoding)
        {
            foreach (var l in input.ReadLines(encoding))
            {
                // Strip comments
                var line = Regexs.Comment.Replace(l, "").TrimEnd();
                // Skip empty line or comment line
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (IsCategoryEntry(line))
                {
                    ParseCategory(line);
                }
                else
                {
                    ParseMapping(line);
                }
            }
        }

        void ParseCategory(string line)
        {
            var values = Regexs.CustomSegmentationSpliter.Split(line);

            var classname = values[0];
            var invoke = int.Parse(values[1]);
            var group = int.Parse(values[2]);
            var length = int.Parse(values[3]);

            CategoryDefinitions.Add(classname, new int[] { invoke, group, length });
        }

        void ParseMapping(string line)
        {
            var values = Regexs.CustomSegmentationSpliter.Split(line);

            var codepointString = values[0];
            var categories = GetCategories(values);

            if (codepointString.Contains(".."))
            {
                var codepoints = codepointString.Split(new string[] { ".." }, StringSplitOptions.None);

                var lowerCodepoint = codepoints[0].DecodeToInt32();
                var upperCodepoint = codepoints[1].DecodeToInt32();

                for (var i = lowerCodepoint; i <= upperCodepoint; i++)
                {
                    AddMapping(i, categories);
                }
            }
            else
            {
                var codepoint = codepointString.DecodeToInt32();

                AddMapping(codepoint, categories);
            }
        }

        List<string> GetCategories(string[] values)
        {
            return values.Skip(1).ToList();
        }

        void AddMapping(int codepoint, List<string> categories)
        {
            foreach (var c in categories)
            {
                AddMapping(codepoint, c);
            }
        }

        void AddMapping(int codepoint, string category)
        {
            var categories = CodepointCategories[codepoint];

            if (categories == null)
            {
                CodepointCategories[codepoint] = new SortedSet<string>();
                categories = CodepointCategories[codepoint];
            }

            categories.Add(category);
        }

        bool IsCategoryEntry(string line)
        {
            return !line.StartsWith("0x");
        }

        public SortedDictionary<string, int> MakeCharacterCategoryMap()
        {
            var classMapping = new SortedDictionary<string, int>();
            var i = 0;

            foreach (var category in CategoryDefinitions.Keys)
            {
                classMapping.Add(category, i++);
            }

            return classMapping;
        }

        int[][] MakeCharacterDefinitions()
        {
            var categoryMap = MakeCharacterCategoryMap();
            var array = new int[categoryMap.Count][];

            foreach (var category in categoryMap.Keys)
            {
                array[categoryMap[category]] = CategoryDefinitions[category];
            }

            return array;
        }

        int[][] MakeCharacterMappings()
        {
            var categoryMap = MakeCharacterCategoryMap();
            var array = new int[CodepointCategories.Count][];

            for (var i = 0; i < array.Length; i++)
            {
                var categories = CodepointCategories[i];

                if (categories != null)
                {
                    array[i] = categories
                        .Select(c => (categoryMap.TryGetValue(c, out int q)) ? q : 0)
                        .ToArray();
                }
            }

            return array;
        }

        string[] MakeCharacterCategorySymbols()
        {
            var categoryMap = MakeCharacterCategoryMap();
            var inverted = new SortedDictionary<int, string>();

            foreach (var (k, v) in categoryMap)
            {
                inverted.Add(v, k);
            }

            return inverted.Keys.Select(i => inverted[i]).ToArray();
        }

        public void Compile()
        {
            IntArrayIO.WriteSparseArray2D(Output, MakeCharacterDefinitions());
            IntArrayIO.WriteSparseArray2D(Output, MakeCharacterMappings());
            StringArrayIO.WriteArray(Output, MakeCharacterCategorySymbols());
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
