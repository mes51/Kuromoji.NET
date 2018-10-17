using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public class MultiSearchMerger
    {
        int BaseCost { get; set; }

        List<int> SuffixCostLowerBounds { get; } = new List<int>();

        int MaxCount { get; }

        int CostSlack { get; }

        public MultiSearchMerger(int maxCount, int costSlack)
        {
            MaxCount = maxCount;
            CostSlack = costSlack;
        }

        public MultiSearchResult Merge(List<MultiSearchResult> results)
        {
            if (results.Count == 0)
            {
                return new MultiSearchResult();
            }

            SuffixCostLowerBounds.Clear();
            SuffixCostLowerBounds.AddRange(Enumerable.Repeat(0, results.Count));
            SuffixCostLowerBounds[SuffixCostLowerBounds.Count - 1] = results[results.Count - 1].GetCost(0);
            for (var i = results.Count - 2; i >= 0; i--)
            {
                SuffixCostLowerBounds[i] = results[i].GetCost(0) + SuffixCostLowerBounds[i + 1];
            }
            BaseCost = SuffixCostLowerBounds[0];

            var ret = new MultiSearchResult();
            var builders = new List<MergeBuilder>();
            for (var i = 0; i < results[0].Count; i++)
            {
                if (GetCostLowerBound(results[0].GetCost(i), 0) - BaseCost > CostSlack || i == MaxCount)
                {
                    break;
                }

                var newBuilder = new MergeBuilder(results);
                newBuilder.Add(i);
                builders.Add(newBuilder);
            }

            for (var i = 1; i < results.Count; i++)
            {
                builders = MergeStep(builders, results, i);
            }

            foreach (var builder in builders)
            {
                ret.Add(builder.BuildList(), builder.Cost);
            }

            return ret;
        }

        List<MergeBuilder> MergeStep(List<MergeBuilder> builders, List<MultiSearchResult> results, int currentIndex)
        {
            var nextResult = results[currentIndex];
            var pairHeap = new PriorityQueue<MergePair>();
            var ret = new List<MergeBuilder>();

            if (builders.Count == 0 || nextResult.Count == 0)
            {
                return ret;
            }

            pairHeap.Enqueue(new MergePair(0, 0, builders[0].Cost + nextResult.GetCost(0)));

            var visited = new HashSet<int>();

            while (ret.Count < MaxCount && pairHeap.Count > 0)
            {
                var top = pairHeap.Dequeue();

                if (GetCostLowerBound(top.Cost, currentIndex) - BaseCost > CostSlack)
                {
                    break;
                }

                var i = top.LeftIndex;
                var j = top.RightIndex;

                var nextBuilder = new MergeBuilder(results, builders[i].Indices);
                nextBuilder.Add(j);
                ret.Add(nextBuilder);

                if (i + 1 < builders.Count)
                {
                    var newMergePair = new MergePair(i + 1, j, builders[i + 1].Cost + nextResult.GetCost(j));
                    var positionValue = GetPositionValue(i + 1, j);
                    if (!visited.Contains(positionValue))
                    {
                        pairHeap.Add(newMergePair);
                        visited.Add(positionValue);
                    }
                }
                if (j + 1 < nextResult.Count)
                {
                    var newMergePair = new MergePair(i, j + 1, builders[i].Cost + nextResult.GetCost(j + 1));
                    var positionValue = GetPositionValue(i, j + 1);
                    if (!visited.Contains(positionValue))
                    {
                        pairHeap.Add(newMergePair);
                        visited.Add(positionValue);
                    }
                }
            }

            return ret;
        }

        int GetPositionValue(int i, int j)
        {
            return (MaxCount + 1) * i + j;
        }

        int GetCostLowerBound(int currentCost, int index)
        {
            if (index + 1 < SuffixCostLowerBounds.Count)
            {
                return currentCost + SuffixCostLowerBounds[index + 1];
            }
            return currentCost;
        }

        class MergeBuilder : IComparable<MergeBuilder>
        {
            public int Cost { get; private set; }

            public List<int> Indices { get; } = new List<int>();

            List<MultiSearchResult> Results { get; }

            public MergeBuilder(List<MultiSearchResult> results)
            {
                Results = results;
            }

            public MergeBuilder(List<MultiSearchResult> results, List<int> indices) : this(results)
            {
                indices.ForEach(Add);
            }

            public List<ViterbiNode> BuildList()
            {
                return Enumerable.Range(0, Indices.Count)
                    .SelectMany(i => Results[i].GetTokenizedResult(Indices[i]))
                    .ToList();
            }

            public void Add(int index)
            {
                Indices.Add(index);
                Cost += Results[Indices.Count - 1].GetCost(index);
            }

            public int CompareTo(MergeBuilder other)
            {
                return Cost - other.Cost;
            }
        }

        class MergePair : IComparable<MergePair>
        {
            public int LeftIndex { get; }

            public int RightIndex { get; }

            public int Cost { get; }

            public MergePair(int leftIndex, int rightIndex, int cost)
            {
                LeftIndex = leftIndex;
                RightIndex = rightIndex;
                Cost = cost;
            }

            public int CompareTo(MergePair other)
            {
                return Cost - other.Cost;
            }
        }
    }
}
