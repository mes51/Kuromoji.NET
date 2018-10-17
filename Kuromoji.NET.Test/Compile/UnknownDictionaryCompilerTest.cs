using System;
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
    [DeploymentItem("Resources\\unk.def", "Dict")]
    public class UnknownDictionaryCompilerTest
    {
        static UnknownDictionary UnknownDictionary { get; set; }

        static CharacterDefinitions CharacterDefinitions { get; set; }

        static int[][] Costs { get; set; }

        static int[][] References { get; set; }

        static string[][] Features { get; set; }

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            using (var charDefOutput = new MemoryStream())
            using (var unkDicOutput = new MemoryStream())
            using (var charDefResource = GetResource("char.def"))
            using (var unkDefResource = GetResource("unk.def"))
            {
                var charDefCompiler = new CharacterDefinitionsCompiler(charDefOutput);
                charDefCompiler.ReadCharacterDefinition(charDefResource, Encoding.GetEncoding("euc-jp"));
                charDefCompiler.Compile();

                var categoryMap = charDefCompiler.MakeCharacterCategoryMap();

                var unkDefCompiler = new UnknownDictionaryCompiler(unkDicOutput, categoryMap);
                unkDefCompiler.ReadUnknownDefinition(unkDefResource, Encoding.GetEncoding("euc-jp"));
                unkDefCompiler.Compile();

                charDefOutput.Seek(0, SeekOrigin.Begin);
                unkDicOutput.Seek(0, SeekOrigin.Begin);

                var definitions = IntArrayIO.ReadSparseArray2D(charDefOutput);
                var mappings = IntArrayIO.ReadSparseArray2D(charDefOutput);
                var symbols = StringArrayIO.ReadArray(charDefOutput);

                CharacterDefinitions = new CharacterDefinitions(definitions, mappings, symbols);

                Costs = IntArrayIO.ReadArray2D(unkDicOutput);
                References = IntArrayIO.ReadArray2D(unkDicOutput);
                Features = StringArrayIO.ReadArray2D(unkDicOutput);

                UnknownDictionary = new UnknownDictionary(CharacterDefinitions, References, Costs, Features);
            }
        }

        [TestMethod]
        public void TestCostsAndFeatures()
        {
            var categories = CharacterDefinitions.LookupCategories('一');

            // KANJI & KANJINUMERIC
            categories.Length.Is(2);

            categories.SequenceEqual(new int[] { 5, 6 }).IsTrue();

            // KANJI entries
            UnknownDictionary.LookupWordIds(categories[0]).SequenceEqual(new int[] { 2, 3, 4, 5, 6, 7 }).IsTrue();

            // KANJI feature variety
            UnknownDictionary.GetAllFeaturesArray(2).SequenceEqual(new string[] { "名詞", "一般", "*", "*", "*", "*", "*" }).IsTrue();

            UnknownDictionary.GetAllFeaturesArray(3).SequenceEqual(new string[] { "名詞", "サ変接続", "*", "*", "*", "*", "*" }).IsTrue();

            UnknownDictionary.GetAllFeaturesArray(4).SequenceEqual(new string[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "*" }).IsTrue();

            UnknownDictionary.GetAllFeaturesArray(5).SequenceEqual(new string[] { "名詞", "固有名詞", "組織", "*", "*", "*", "*" }).IsTrue();

            UnknownDictionary.GetAllFeaturesArray(6).SequenceEqual(new string[] { "名詞", "固有名詞", "人名", "一般", "*", "*", "*" }).IsTrue();

            UnknownDictionary.GetAllFeaturesArray(6).SequenceEqual(new string[] { "名詞", "固有名詞", "人名", "一般", "*", "*", "*" }).IsTrue();

            // KANJINUMERIC entry
            UnknownDictionary.LookupWordIds(categories[1]).SequenceEqual(new int[] { 29 }).IsTrue();

            // KANJINUMERIC costs
            UnknownDictionary.GetLeftId(29).Is(1295);
            UnknownDictionary.GetRightId(29).Is(1295);
            UnknownDictionary.GetWordCost(29).Is(27473);

            // KANJINUMERIC features
            UnknownDictionary.GetAllFeaturesArray(29).SequenceEqual(new string[] { "名詞", "数", "*", "*", "*", "*", "*" }).IsTrue();
        }

        static Stream GetResource(string file)
        {
            return new FileStream("Dict\\" + file, FileMode.Open, FileAccess.Read);
        }
    }
}
