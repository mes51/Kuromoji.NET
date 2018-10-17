using System;
using Kuromoji.NET.Test.TestSuits;
using Kuromoji.NET.Tokenizers.UniDicKanaAccent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Tokenizers.UniDicKanaAccent
{
    [TestClass]
    [DeploymentItem("Resources\\UniDicKanaAccent\\unidic-kanaaccent.zip", "Dict")]
    public class RandomizedInputTest : RandomTestBase
    {
        const int Length = 512;

        const string DicFileName = @"Dict\unidic-kanaaccent.zip";

        static Tokenizer Tokenizer { get; set; }

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            Tokenizer = new Tokenizer(DicFileName);
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            Tokenizer = null;
        }

        [TestMethod]
        public void TestRandomizedUnicodeInput()
        {
            for (var i = 0; i < 10; i++)
            {
                Tokenizer.IsCanTokenizeString(RandomUnicodeOfLength(Length));
            }
        }

        [TestMethod]
        public void TestRandomizedRealisticUnicodeInput()
        {
            for (var i = 0; i < 10; i++)
            {
                Tokenizer.IsCanTokenizeString(RandomRealisticUnicodeOfLength(Length));
            }
        }

        [TestMethod]
        public void TestRandomizedAsciiInput()
        {
            for (var i = 0; i < 10; i++)
            {
                Tokenizer.IsCanTokenizeString(RandomAsciiOfLength(Length));
            }
        }

        [TestMethod]
        public void TestRandomizedUnicodeInputMultiTokenize()
        {
            for (var i = 0; i < 10; i++)
            {
                Tokenizer.IsCanMultiTokenizeString(RandomUnicodeOfLength(Length), Random.Next(2, 1000), Random.Next(100000));
            }
        }

        [TestMethod]
        public void TestRandomizedRealisticUnicodeInputMultiTokenize()
        {
            for (var i = 0; i < 10; i++)
            {
                Tokenizer.IsCanMultiTokenizeString(RandomRealisticUnicodeOfLength(Length), Random.Next(2, 1000), Random.Next(100000));
            }
        }

        [TestMethod]
        public void TestRandomizedAsciiInputMultiTokenize()
        {
            for (var i = 0; i < 10; i++)
            {
                Tokenizer.IsCanMultiTokenizeString(RandomAsciiOfLength(Length), Random.Next(2, 1000), Random.Next(100000));
            }
        }
    }
}
