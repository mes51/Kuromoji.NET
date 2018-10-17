using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Trie;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Trie
{
    [TestClass]
    public class PatriciaTrieTest
    {
        [TestMethod]
        public void TestRomaji()
        {
            var trie = new PatriciaTrie<string>();
            trie.Add("a", "a");
            trie.Add("b", "b");
            trie.Add("ab", "ab");
            trie.Add("bac", "bac");
            trie["a"].Is("a");
            trie["bac"].Is("bac");
            trie["b"].Is("b");
            trie["ab"].Is("ab");
            Assert.ThrowsException<KeyNotFoundException>(() => trie["nonexistant"]);
        }

        [TestMethod]
        public void TestJapanese()
        {
            var trie = new PatriciaTrie<string>();
            trie.Add("寿司", "sushi");
            trie.Add("刺身", "sashimi");
            trie["寿司"].Is("sushi");
            trie["刺身"].Is("sashimi");
        }

        [TestMethod]
        public void TestNull()
        {
            var trie = new PatriciaTrie<string>();
            trie.Add("null", null);
            trie["null"].IsNull();
            Assert.ThrowsException<ArgumentNullException>(() => trie.Add(null, "null"));
        }

        [TestMethod]
        public void TestRandom()
        {
            // Generate random strings
            var randoms = Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid().ToString()).ToArray();

            // Insert them
            var trie = new PatriciaTrie<string>();
            foreach (var random in randoms)
            {
                trie.Add(random, random);
            }

            // Get and test them
            foreach (var random in randoms)
            {
                trie[random].Is(random);
                trie.ContainsKey(random).IsTrue();
            }
        }

        [TestMethod]
        public void TestAddRange()
        {
            // Generate random strings
            var randoms = Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid().ToString()).Select(r => new KeyValuePair<string, string>(r, r)).ToArray();

            // Insert them
            var trie = new PatriciaTrie<string>();
            trie.AddRange(randoms);

            // Get and test them
            foreach (var random in randoms)
            {
                trie[random.Key].Is(random.Value);
                trie.ContainsKey(random.Key).IsTrue();
            }
        }

        [TestMethod]
        public void TestLongString()
        {
            const string Value = "found it";
            var longMovieTitle = "マルキ・ド・サドの演出のもとにシャラントン精神病院患者たちによって演じられたジャン＝ポール・マラーの迫害と暗殺";

            var trie = new PatriciaTrie<string>();
            trie.Add(longMovieTitle, Value);

            trie[longMovieTitle].Is(Value);
        }

        [TestMethod]
        public void TestIsEmpty()
        {
            var trie = new PatriciaTrie<string>();
            trie.IsEmpty.IsTrue();
            trie.Add("hello", "world");
            trie.IsEmpty.IsFalse();
        }

        [TestMethod]
        public void TestCount()
        {
            var trie = new PatriciaTrie<string>();
            trie.Count.Is(0);
            trie.Add("hello", "world");
            trie.Count.Is(1);
        }

        [TestMethod]
        public void TestEmptyInsert()
        {
            var trie = new PatriciaTrie<string>();
            trie.Count.Is(0);
            trie.Add("", "i am empty bottle of beer!");
            trie.Count.Is(1);
            trie[""].Is("i am empty bottle of beer!");
            trie.Add("", "...and i'm an empty bottle of sake");
            trie[""].Is("...and i'm an empty bottle of sake");
        }

        [TestMethod]
        public void TestClear()
        {
            var trie = new PatriciaTrie<string>();
            trie.Count.Is(0);
            trie.Add("hello", "world");
            trie.Add("world", "hello");
            trie.Count.Is(2);
            trie.Clear();
            trie.Count.Is(0);
        }

        [TestMethod]
        public void TestNaiveCollections()
        {
            var trie = new PatriciaTrie<string>();
            trie.Add("寿司", "sushi");
            trie.Add("刺身", "sashimi");
            trie.Add("そば", "soba");
            trie.Add("ラーメン", "ramen");

            trie.Keys.Count.Is(4);
            trie.Keys.Concat(new string[] { "寿司", "そば", "ラーメン", "刺身" }).Distinct().Count().Is(4);

            trie.Values.Count.Is(4);
            trie.Values.Concat(new string[] { "sushi", "soba", "ramen", "sashimi" }).Distinct().Count().Is(4);
        }

        [TestMethod]
        public void TestEscapeChars()
        {
            var trie = new PatriciaTrie<string>();
            trie.Add("new", "no error");

            trie.ContainsKeyPrefix("new\na").IsFalse();
            trie.ContainsKeyPrefix("\n").IsFalse();
            trie.ContainsKeyPrefix("\t").IsFalse();
        }

        [TestMethod]
        public void TestPrefix()
        {
            var trie = new PatriciaTrie<string>();
            var tokyoPlaces = new string[]{
                "Hachiōji",
                "Tachikawa",
                "Musashino",
                "Mitaka",
                "Ōme",
                "Fuchū",
                "Akishima",
                "Chōfu",
                "Machida",
                "Koganei",
                "Kodaira",
                "Hino",
                "Higashimurayama",
                "Kokubunji",
                "Kunitachi",
                "Fussa",
                "Komae",
                "Higashiyamato",
                "Kiyose",
                "Higashikurume",
                "Musashimurayama",
                "Tama",
                "Inagi",
                "Hamura",
                "Akiruno",
                "Nishitōkyō"
            };

            foreach (var place in tokyoPlaces)
            {
                trie.Add(place, place);
            }

            // Prefixes of Kodaira
            trie.ContainsKeyPrefix("K").IsTrue();
            trie.ContainsKeyPrefix("Ko").IsTrue();
            trie.ContainsKeyPrefix("Kod").IsTrue();
            trie.ContainsKeyPrefix("Koda").IsTrue();
            trie.ContainsKeyPrefix("Kodai").IsTrue();
            trie.ContainsKeyPrefix("Kodair").IsTrue();
            trie.ContainsKeyPrefix("Kodaira").IsTrue();
            trie.ContainsKeyPrefix("Kodaira ").IsFalse();
            trie.ContainsKeyPrefix("Kodaira  ").IsFalse();
            trie["Kodaira"].IsNotNull();

            // Prefixes of Fussa
            trie.ContainsKeyPrefix("fu").IsFalse();
            trie.ContainsKeyPrefix("Fu").IsTrue();
            trie.ContainsKeyPrefix("Fus").IsTrue();
        }

        [TestMethod]
        public void TestTextScan()
        {
            var trie = new PatriciaTrie<string>();
            var terms = new string[]{
                "お寿司", "sushi",
                "美味しい", "tasty",
                "日本", "japan",
                "だと思います", "i think",
                "料理", "food",
                "日本料理", "japanese food",
                "一番", "first and foremost",
            };
            for (var i = 0; i < terms.Length; i += 2)
            {
                trie.Add(terms[i], terms[i + 1]);
            }

            var text = "日本料理の中で、一番美味しいのはお寿司だと思います。すぐ日本に帰りたいです。";
            var builder = new StringBuilder();

            var startIndex = 0;
            while (startIndex < text.Length)
            {
                int matchLength = 0;
                while (trie.ContainsKeyPrefix(text.Substring(startIndex, matchLength + 1)))
                {
                    matchLength++;
                }
                if (matchLength > 0)
                {
                    var match = text.Substring(startIndex, matchLength);
                    builder.Append("[");
                    builder.Append(match);
                    builder.Append("|");
                    builder.Append(trie[match]);
                    builder.Append("]");
                    startIndex += matchLength;
                }
                else
                {
                    builder.Append(text[startIndex]);
                    startIndex++;
                }
            }
            builder.ToString().Is("[日本料理|japanese food]の中で、[一番|first and foremost][美味しい|tasty]のは[お寿司|sushi][だと思います|i think]。すぐ[日本|japan]に帰りたいです。");
        }

        [TestMethod]
        public void TestMultiThreadedTrie()
        {
            var numThreads = 10;
            var perThreadRuns = 50000;
            var keySetSize = 1000;

            var threads = new List<Task>();
            var randoms = Enumerable.Range(0, keySetSize).Select(_ => Guid.NewGuid().ToString()).ToArray();

            var trie = new PatriciaTrie<int>();

            for (var i = 0; i < keySetSize; i++)
            {
                trie.Add(randoms[i], i);
            }

            for (int i = 0; i < numThreads; i++)
            {
                var thread = Task.Run(() =>
                {
                    var rand = new Random();
                    for (int run = 0; run < perThreadRuns; run++)
                    {
                        int randomIndex = rand.Next(randoms.Length);
                        var random = randoms[randomIndex];

                        // Test retrieve
                        trie[random].Is(randomIndex);

                        int randomPrefixLength = rand.Next(random.Length);

                        // Test random prefix length prefix match
                        trie.ContainsKeyPrefix(random.Substring(0, randomPrefixLength)).IsTrue();
                    }
                });
                threads.Add(thread);
            }

            Task.WaitAll(threads.ToArray());

            true.IsTrue();
        }

        [TestMethod]
        public void TestSimpleKey()
        {
            var keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            var key = "abc";

            // a = U+0061 = 0000 0000 0110 0001
            keyMapper.IsSet(0, key).IsFalse();
            keyMapper.IsSet(1, key).IsFalse();
            keyMapper.IsSet(2, key).IsFalse();
            keyMapper.IsSet(3, key).IsFalse();

            keyMapper.IsSet(4, key).IsFalse();
            keyMapper.IsSet(5, key).IsFalse();
            keyMapper.IsSet(6, key).IsFalse();
            keyMapper.IsSet(7, key).IsFalse();

            keyMapper.IsSet(8, key).IsFalse();
            keyMapper.IsSet(9, key).IsTrue();
            keyMapper.IsSet(10, key).IsTrue();
            keyMapper.IsSet(11, key).IsFalse();

            keyMapper.IsSet(12, key).IsFalse();
            keyMapper.IsSet(13, key).IsFalse();
            keyMapper.IsSet(14, key).IsFalse();
            keyMapper.IsSet(15, key).IsTrue();

            // b = U+0062 = 0000 0000 0110 0010
            keyMapper.IsSet(16, key).IsFalse();
            keyMapper.IsSet(17, key).IsFalse();
            keyMapper.IsSet(18, key).IsFalse();
            keyMapper.IsSet(19, key).IsFalse();

            keyMapper.IsSet(20, key).IsFalse();
            keyMapper.IsSet(21, key).IsFalse();
            keyMapper.IsSet(22, key).IsFalse();
            keyMapper.IsSet(23, key).IsFalse();

            keyMapper.IsSet(24, key).IsFalse();
            keyMapper.IsSet(25, key).IsTrue();
            keyMapper.IsSet(26, key).IsTrue();
            keyMapper.IsSet(27, key).IsFalse();

            keyMapper.IsSet(28, key).IsFalse();
            keyMapper.IsSet(29, key).IsFalse();
            keyMapper.IsSet(30, key).IsTrue();
            keyMapper.IsSet(31, key).IsFalse();

            // c = U+0063 = 0000 0000 0110 0011
            keyMapper.IsSet(32, key).IsFalse();
            keyMapper.IsSet(33, key).IsFalse();
            keyMapper.IsSet(34, key).IsFalse();
            keyMapper.IsSet(35, key).IsFalse();

            keyMapper.IsSet(36, key).IsFalse();
            keyMapper.IsSet(37, key).IsFalse();
            keyMapper.IsSet(38, key).IsFalse();
            keyMapper.IsSet(39, key).IsFalse();

            keyMapper.IsSet(40, key).IsFalse();
            keyMapper.IsSet(41, key).IsTrue();
            keyMapper.IsSet(42, key).IsTrue();
            keyMapper.IsSet(43, key).IsFalse();

            keyMapper.IsSet(44, key).IsFalse();
            keyMapper.IsSet(45, key).IsFalse();
            keyMapper.IsSet(46, key).IsTrue();
            keyMapper.IsSet(47, key).IsTrue();
        }

        [TestMethod]
        public void TestNullKeyMap()
        {
            var keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            keyMapper.IsSet(0, null).IsFalse();
            keyMapper.IsSet(100, null).IsFalse();
            keyMapper.IsSet(1000, null).IsFalse();
        }

        [TestMethod]
        public void TestEmptyKeyMap()
        {
            var keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            // Note: this is a special case handled in PatriciaTrie
            keyMapper.IsSet(0, "").IsTrue();
            keyMapper.IsSet(100, "").IsTrue();
            keyMapper.IsSet(1000, "").IsTrue();
        }

        [TestMethod]
        public void TestOverflowBit()
        {
            var keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            var key = "a";

            // a = U+0061 = 0000 0000 0110 0001
            keyMapper.IsSet(0, key).IsFalse();
            keyMapper.IsSet(1, key).IsFalse();
            keyMapper.IsSet(2, key).IsFalse();
            keyMapper.IsSet(3, key).IsFalse();

            keyMapper.IsSet(4, key).IsFalse();
            keyMapper.IsSet(5, key).IsFalse();
            keyMapper.IsSet(6, key).IsFalse();
            keyMapper.IsSet(7, key).IsFalse();

            keyMapper.IsSet(8, key).IsFalse();
            keyMapper.IsSet(9, key).IsTrue();
            keyMapper.IsSet(10, key).IsTrue();
            keyMapper.IsSet(11, key).IsFalse();

            keyMapper.IsSet(12, key).IsFalse();
            keyMapper.IsSet(13, key).IsFalse();
            keyMapper.IsSet(14, key).IsFalse();
            keyMapper.IsSet(15, key).IsTrue();

            // Asking for overflow bits should return 1
            keyMapper.IsSet(16, key).IsTrue();
            keyMapper.IsSet(17, key).IsTrue();
            keyMapper.IsSet(100, key).IsTrue();
        }
    }
}
