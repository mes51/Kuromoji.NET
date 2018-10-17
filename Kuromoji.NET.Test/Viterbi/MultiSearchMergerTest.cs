using System;
using System.Collections.Generic;
using System.Text;
using Kuromoji.NET.Viterbi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Viterbi
{
    [TestClass]
    public class MultiSearchMergerTest
    {
        [TestMethod]
        public void TestMerger()
        {
            const int maxCount = 3;
            const int costSlack = 8;
            var merger = new MultiSearchMerger(maxCount, costSlack);
            var results = new List<MultiSearchResult>();

            var surfaces1 = new string[][] { new string[] { "a", "b" }, new string[] { "c", "d" }, new string[] { "e", "f" } };
            var costs1 = new int[] { 1, 2, 3 };
            results.Add(MakeResult(surfaces1, costs1));

            var surfaces2 = new string[][] { new string[] { "a", "b" }, new string[] { "c", "d" } };
            var costs2 = new int[] { 1, 2 };
            results.Add(MakeResult(surfaces2, costs2));

            var mergedResult = merger.Merge(results);
            mergedResult.Count.Is(3);
            mergedResult.GetCost(0).Is(2);
            mergedResult.GetCost(1).Is(3);
            mergedResult.GetCost(2).Is(3);
            GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(0)).Is("a b a b");
            GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(1)).Is("c d a b");
            GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(2)).Is("a b c d");
        }

        [TestMethod]
        public void TestMergerTooFew()
        {
            const int maxCount = 5;
            const int costSlack = 3;
            var merger = new MultiSearchMerger(maxCount, costSlack);
            var results = new List<MultiSearchResult>();

            var surfaces1 = new string[][] { new string[] { "a", "b" }, new string[] { "c", "d" }, new string[] { "e", "f" } };
            var costs1 = new int[] { 1, 2, 5 };
            results.Add(MakeResult(surfaces1, costs1));

            var surfaces2 = new string[][] { new string[] { "a", "b" }, new string[] { "c", "d" } };
            var costs2 = new int[] { 1, 2 };
            results.Add(MakeResult(surfaces2, costs2));

            var surfaces3 = new string[][] { new string[] { "a", "b" } };
            var costs3 = new int[] { 5 };
            results.Add(MakeResult(surfaces3, costs3));

            MultiSearchResult mergedResult = merger.Merge(results);
            mergedResult.Count.Is(4);
            mergedResult.GetCost(0).Is(7);
            mergedResult.GetCost(1).Is(8);
            mergedResult.GetCost(2).Is(8);
            mergedResult.GetCost(3).Is(9);
            GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(0)).Is("a b a b a b");
            GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(1)).Is("c d a b a b");
            GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(2)).Is("a b c d a b");
            GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(3)).Is("c d c d a b");
        }

        MultiSearchResult MakeResult(string[][] surfaces, int[] cost)
        {
            var ret = new MultiSearchResult();
            for (int i = 0; i < surfaces.Length; i++)
            {
                ret.Add(makeNodes(surfaces[i]), cost[i]);
            }
            return ret;
        }

        List<ViterbiNode> makeNodes(String[] surfaces)
        {
            var ret = new List<ViterbiNode>();
            foreach (string s in surfaces)
            {
                ret.Add(new ViterbiNode(0, s, 0, 0, 0, 0, ViterbiNode.NodeType.Known));
            }
            return ret;
        }

        string GetSpaceSeparatedTokens(List<ViterbiNode> nodes)
        {
            if (nodes.Count == 0)
            {
                return "";
            }

            var builder = new StringBuilder();
            builder.Append(nodes[0].Surface);
            for (var i = 1; i < nodes.Count; i++)
            {
                builder.Append(" ");
                builder.Append(nodes[i].Surface);
            }

            return builder.ToString();
        }
    }
}
