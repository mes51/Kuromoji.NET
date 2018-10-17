using System;
using System.IO;
using System.Text;
using Kuromoji.NET.Dict;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Dict
{
    [TestClass]
    [DeploymentItem("Resources\\userdict.txt", "Dict")]
    public class UserDictionaryTest
    {
        [TestMethod]
        public void TestLookup()
        {
            UserDictionary dictionary;
            using (var fs = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(fs, 9, 7, 0);
            }

            var matches = dictionary.FindUserDictionaryMatches("関西国際空港に行った");

            // Length should be three 関西, 国際, 空港
            matches.Count.Is(3);

            // Test positions
            matches[0].MatchStartIndex.Is(0); // index of 関西
            matches[1].MatchStartIndex.Is(2); // index of 国際
            matches[2].MatchStartIndex.Is(4); // index of 空港

            // Test lengths
            matches[0].MatchLength.Is(2); // length of 関西
            matches[1].MatchLength.Is(2); // length of 国際
            matches[2].MatchLength.Is(2); // length of 空港

            var matches2 = dictionary.FindUserDictionaryMatches("関西国際空港と関西国際空港に行った");
            matches2.Count.Is(6);
        }

        [TestMethod]
        public void TestIpadicFeatures()
        {
            UserDictionary dictionary;
            using (var fs = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(fs, 9, 7, 0);
            }

            dictionary.GetAllFeatures(0).Is("カスタム名詞,*,*,*,*,*,*,ニホン,*");
        }

        [TestMethod]
        public void TestJumanDicFeatures()
        {
            UserDictionary dictionary;
            using (var fs = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(fs, 7, 5, 0);
            }

            dictionary.GetAllFeatures(0).Is("カスタム名詞,*,*,*,*,ニホン,*");
        }

        [TestMethod]
        public void TestNaistJDicFeatures()
        {
            UserDictionary dictionary;
            using (var fs = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(fs, 11, 7, 0);
            }

            // This is a sample naist-jdic entry:
            //
            //   葦登,1358,1358,4975,名詞,一般,*,*,*,*,葦登,ヨシノボリ,ヨシノボリ,,
            //
            // How should we treat the last features in the user dictionary?  They seem empty, but we return * for them...
            dictionary.GetAllFeatures(0).Is("カスタム名詞,*,*,*,*,*,*,ニホン,*,*,*");
        }

        [TestMethod]
        public void TestUniDicFeatures()
        {
            UserDictionary dictionary;
            using (var fs = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(fs, 13, 7, 0);
            }

            dictionary.GetAllFeatures(0).Is("カスタム名詞,*,*,*,*,*,*,ニホン,*,*,*,*,*");
        }

        [TestMethod]
        public void TestUniDicExtendedFeatures()
        {
            UserDictionary dictionary;
            using (var fs = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(fs, 22, 13, 0);
            }

            dictionary.GetAllFeatures(0).Is("カスタム名詞,*,*,*,*,*,*,*,*,*,*,*,*,ニホン,*,*,*,*,*,*,*,*");
        }

        [TestMethod]
        public void TestUserDictionaryEntries()
        {
            var userDictionaryEntry = "クロ,クロ,クロ,カスタム名詞";
            UserDictionary dictionary;
            using (var fs = new MemoryStream(Encoding.UTF8.GetBytes(userDictionaryEntry)))
            {
                dictionary = new UserDictionary(fs, 9, 7, 0);
            }

            var matches = dictionary.FindUserDictionaryMatches("この丘はアクロポリスと呼ばれている");
            matches.Count.Is(1);
            matches[0].MatchStartIndex.Is(5);
        }

        [TestMethod]
        public void TestOverlappingUserDictionaryEntries()
        {
            var userDictionaryEntries = "クロ,クロ,クロ,カスタム名詞\n" +
                "アクロ,アクロ,アクロ,カスタム名詞";
            UserDictionary dictionary;
            using (var fs = new MemoryStream(Encoding.UTF8.GetBytes(userDictionaryEntries)))
            {
                dictionary = new UserDictionary(fs, 9, 7, 0);
            }

            var positions = dictionary.FindUserDictionaryMatches("この丘はアクロポリスと呼ばれている");
            positions[0].MatchStartIndex.Is(4);
            positions.Count.Is(2);
        }

        Stream GetResource(string file)
        {
            return new FileStream("Dict\\" + file, FileMode.Open, FileAccess.Read);
        }
    }
}
