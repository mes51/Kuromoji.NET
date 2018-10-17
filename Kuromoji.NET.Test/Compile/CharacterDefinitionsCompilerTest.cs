using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Kuromoji.NET.Compile;
using Kuromoji.NET.Dict;
using Kuromoji.NET.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Compile
{
    [TestClass]
    [DeploymentItem("Resources\\char.def", "Dict")]
    public class CharacterDefinitionsCompilerTest
    {
        string CharDefFileName { get; set; }

        Dictionary<int, string> CategoryIdMap { get; set; }

        CharacterDefinitions CharacterDefinition { get; set; }

        [TestInitialize]
        public void Setup()
        {
            CharDefFileName = Path.GetTempFileName();

            using (var fs = new FileStream(CharDefFileName, FileMode.Create, FileAccess.ReadWrite))
            using (var resource = GetResource("char.def"))
            {
                var compiler = new CharacterDefinitionsCompiler(fs);
                compiler.ReadCharacterDefinition(resource, Encoding.GetEncoding("euc-jp"));
                CategoryIdMap = Invert(compiler.MakeCharacterCategoryMap());
                compiler.Compile();
            }

            using (var fs = new FileStream(CharDefFileName, FileMode.Open, FileAccess.Read))
            {
                var definitions = IntArrayIO.ReadSparseArray2D(fs);
                var mappings = IntArrayIO.ReadSparseArray2D(fs);
                var symbols = StringArrayIO.ReadArray(fs);
                CharacterDefinition = new CharacterDefinitions(definitions, mappings, symbols);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(CharDefFileName);
        }

        [TestMethod]
        public void TestCharacterCategories()
        {
            // Non-defined characters get the default definition
            AssertCharacterCategories(CharacterDefinition, '\u0000', "DEFAULT");
            AssertCharacterCategories(CharacterDefinition, '〇', "SYMBOL", "KANJI", "KANJINUMERIC");
            AssertCharacterCategories(CharacterDefinition, ' ', "SPACE");
            AssertCharacterCategories(CharacterDefinition, '。', "SYMBOL");
            AssertCharacterCategories(CharacterDefinition, 'A', "ALPHA");
            AssertCharacterCategories(CharacterDefinition, 'Ａ', "ALPHA");
        }

        [TestMethod]
        public void TestAddCategoryDefinitions()
        {
            AssertCharacterCategories(CharacterDefinition, '・', "KATAKANA");

            CharacterDefinition.SetCategories('・', new string[] { "SYMBOL", "KATAKANA" });

            AssertCharacterCategories(CharacterDefinition, '・', "KATAKANA", "SYMBOL");
            AssertCharacterCategories(CharacterDefinition, '・', "SYMBOL", "KATAKANA");
        }

        void AssertCharacterCategories(CharacterDefinitions characterDefinition, char c, params string[] categories)
        {
            var categoryIds = characterDefinition.LookupCategories(c);

            if (categoryIds == null)
            {
                categories.IsNull();
                return;
            }

            categoryIds.Length.Is(categories.Length);

            foreach (var categoryId in categoryIds)
            {
                var category = CategoryIdMap[categoryId];
                categories.Contains(category).IsTrue();
            }
        }

        Stream GetResource(string file)
        {
            return new FileStream("Dict\\" + file, FileMode.Open, FileAccess.Read);
        }

        Dictionary<int, string> Invert(IDictionary<string, int> map)
        {
            var inverted = new Dictionary<int, string>();

            foreach (var key in map.Keys)
            {
                inverted.Add(map[key], key);
            }

            return inverted;
        }
    }
}
