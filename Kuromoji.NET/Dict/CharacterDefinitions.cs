using Kuromoji.NET.IO;
using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public class CharacterDefinitions
    {
        public const string CharacterDefinitionsFileName = "characterDefinitions.bin";

        public const int Invoke = 0;

        public const int Group = 1;

        const string DefaultCategoryName = "DEFAULT";

        const int Length = 2; // Not used as of now

        int[][] CategoryDefinitions { get; }

        int[][] CodepointMappings { get; }

        string[] CategorySymbols { get; }

        int[] DefaultCategory { get; }

        public CharacterDefinitions(int[][] categoryDefinitions, int[][] codepointMappings, string[] categorySymbols)
        {
            CategoryDefinitions = categoryDefinitions;
            CodepointMappings = codepointMappings;
            CategorySymbols = categorySymbols;
            DefaultCategory = LookupCategories(new string[] { DefaultCategoryName });
        }

        public int[] LookupCategories(char c)
        {
            var mappings = CodepointMappings[c];

            if (mappings == null)
            {
                return DefaultCategory;
            }

            return mappings;
        }

        public int[] LookupDefinition(int category)
        {
            return CategoryDefinitions[category];
        }

        public void SetCategories(char c, string[] categoryNames)
        {
            CodepointMappings[c] = LookupCategories(categoryNames);
        }

        int[] LookupCategories(string[] categoryNames)
        {
            var categories = new int[categoryNames.Length];

            for (var i = 0; i < categories.Length; i++)
            {
                var category = categoryNames[i];
                var categoryIndex = -1;

                for (int j = 0; j < CategorySymbols.Length; j++)
                {
                    if (category == CategorySymbols[j])
                    {
                        categoryIndex = j;
                    }
                }

                if (categoryIndex < 0)
                {
                    throw new ArgumentException("No category '" + category + "' found");
                }

                categories[i] = categoryIndex;
            }

            return categories;
        }

        public static CharacterDefinitions NewInstance(IResourceResolver resolver)
        {
            using (var charDefInput = resolver.Resolve(CharacterDefinitionsFileName))
            {
                var definitions = IntArrayIO.ReadSparseArray2D(charDefInput);
                var mappings = IntArrayIO.ReadSparseArray2D(charDefInput);
                var symbols = StringArrayIO.ReadArray(charDefInput);

                return new CharacterDefinitions(definitions, mappings, symbols);
            }
        }
    }
}