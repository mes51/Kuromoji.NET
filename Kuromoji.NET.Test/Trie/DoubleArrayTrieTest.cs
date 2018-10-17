using System;
using System.IO;
using Kuromoji.NET.Trie;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Trie
{
    [TestClass]
    public class DoubleArrayTrieTest
    {
        [TestMethod]
        public void TestSparseTrie()
        {
            TestSimpleTrie(false);
        }

        [TestMethod]
        public void TestCompactTrie()
        {
            TestSimpleTrie(true);
        }

        void TestSimpleTrie(bool compact)
        {
            var trie = MakeTrie();

            var doubleArrayTrie = new DoubleArrayTrie(compact);
            doubleArrayTrie.Build(trie);

            using (var ms = new MemoryStream())
            {
                doubleArrayTrie.Write(ms);

                ms.Seek(0, SeekOrigin.Begin);

                doubleArrayTrie = DoubleArrayTrie.Read(ms);
            }

            doubleArrayTrie.Lookup("a").Is(0);
            (doubleArrayTrie.Lookup("abc") > 0).IsTrue();
            (doubleArrayTrie.Lookup("あいう") > 0).IsTrue();
            (doubleArrayTrie.Lookup("xyz") < 0).IsTrue();
        }

        NET.Trie.Trie MakeTrie()
        {
            var trie = new NET.Trie.Trie();
            trie.Add("abc");
            trie.Add("abd");
            trie.Add("あああ");
            trie.Add("あいう");
            return trie;
        }
    }
}
