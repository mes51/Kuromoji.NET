using Kuromoji.NET.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public class ViterbiSearcher
    {
        const int DefaultCost = int.MaxValue;

        ConnectionCosts Costs { get; }

        UnknownDictionary UnknownDictionary { get; }

        int KanjiPenaltyLengthThreshold { get; }

        int OtherPenaltyLengthThreshold { get; }

        int KanjiPenalty { get; }

        int OtherPenalty { get; }

        TokenizerMode Mode { get; }

        MultiSearcher MultiSearcher { get; }

        public ViterbiSearcher(TokenizerMode mode, ConnectionCosts costs, UnknownDictionary unknownDictionary, List<int> penalties)
        {
            if (penalties.Count != 0)
            {
                KanjiPenaltyLengthThreshold = penalties[0];
                KanjiPenalty = penalties[1];
                OtherPenaltyLengthThreshold = penalties[2];
                OtherPenalty = penalties[3];
            }

            Mode = mode;
            Costs = costs;
            UnknownDictionary = unknownDictionary;
            MultiSearcher = new MultiSearcher(costs, mode, this);
        }

        /// <summary>
        /// Find best path from input lattice.
        /// </summary>
        /// <param name="lattice">the result of build method</param>
        /// <returns>List of ViterbiNode which consist best path</returns>
        public List<ViterbiNode> Search(ViterbiLattice lattice)
        {
            var endIndexArr = CalculatePathCosts(lattice);
            return BacktrackBestPath(endIndexArr[0][0]);
        }

        /// <summary>
        /// Find the best paths with cost at most OPT + costSlack, where OPT is the optimal solution. At most maxCount paths will be returned. The paths are ordered by cost in ascending order.
        /// </summary>
        /// <param name="lattice">the result of a build method</param>
        /// <param name="maxCount">the maximum number of paths to find</param>
        /// <param name="costSlack">the maximum cost slack of a path</param>
        /// <returns>MultiSearchResult containing the shortest paths and their costs</returns>
        public MultiSearchResult SearchMultiple(ViterbiLattice lattice, int maxCount, int costSlack)
        {
            CalculatePathCosts(lattice);
            return MultiSearcher.GetShortestPaths(lattice, maxCount, costSlack);
        }

        ViterbiNode[][] CalculatePathCosts(ViterbiLattice lattice)
        {
            var startIndexArr = lattice.StartIndexArr;
            var endIndexArr = lattice.EndIndexArr;

            for (var i = 1; i < startIndexArr.Length; i++)
            {
                if (startIndexArr[i] == null || endIndexArr[i] == null)
                {
                    // continue since no array which contains ViterbiNodes exists. Or no previous node exists.
                    continue;
                }

                foreach (var node in startIndexArr[i])
                {
                    if (node == null)
                    {
                        // If array doesn't contain ViterbiNode any more, continue to next index
                        break;
                    }

                    UpdateNode(endIndexArr[i], node);
                }
            }

            return endIndexArr;
        }

        void UpdateNode(ViterbiNode[] viterbiNodes, ViterbiNode node)
        {
            var backwardConnectionId = node.LeftId;
            var wordCost = node.WordCost;
            var leastPathCost = DefaultCost;

            foreach (var leftNode in viterbiNodes)
            {
                // If array doesn't contain any more ViterbiNodes, continue to next index
                if (leftNode == null)
                {
                    return;
                }

                // cost = [total cost from BOS to previous node] + [connection cost between previous node and current node] + [word cost]
                var pathCost = leftNode.PathCost +
                    Costs[leftNode.RightId, backwardConnectionId] +
                    wordCost;

                // Add extra cost for long nodes in "Search mode".
                if (Mode == TokenizerMode.Search || Mode == TokenizerMode.Extended)
                {
                    pathCost += GetPenaltyCost(node);
                }

                // If total cost is lower than before, set current previous node as best left node (previous means left).
                if (pathCost < leastPathCost)
                {
                    leastPathCost = pathCost;
                    node.PathCost = leastPathCost;
                    node.LeftNode = leftNode;
                }
            }
        }

        internal int GetPenaltyCost(ViterbiNode node)
        {
            var pathCost = 0;
            var surface = node.Surface;
            var length = surface.Length;

            if (length > KanjiPenaltyLengthThreshold)
            {
                if (IsKanjiOnly(surface))
                {
                    // Process only Kanji keywords
                    pathCost += (length - KanjiPenaltyLengthThreshold) * KanjiPenalty;
                }
                else if (length > OtherPenaltyLengthThreshold)
                {
                    pathCost += (length - OtherPenaltyLengthThreshold) * OtherPenalty;
                }
            }

            return pathCost;
        }

        bool IsKanjiOnly(string surface)
        {
            for (var i = 0; i < surface.Length; i++)
            {
                var c = surface[i];

                // 4E00..9FFF; CJK Unified Ideographs
                // see: https://github.com/openjdk/jdk/blob/master/src/java.base/share/classes/java/lang/Character.java#L3224
                if (0x4E00 > c || c > 0x9FFF)
                {
                    return false;
                }
            }

            return true;
        }

        List<ViterbiNode> BacktrackBestPath(ViterbiNode eos)
        {
            var node = eos;
            var result = new List<ViterbiNode>();

            result.Add(node);

            while (true)
            {
                var leftNode = node.LeftNode;

                if (leftNode == null)
                {
                    break;
                }

                // Extended mode converts unknown word into unigram nodes
                if (Mode == TokenizerMode.Extended && leftNode.Type == ViterbiNode.NodeType.Unknown)
                {
                    var uniGramNodes = ConvertUnknownWordToUnigramNode(leftNode);
                    result.AddRange(uniGramNodes);
                }
                else
                {
                    result.Insert(0, leftNode);
                }

                node = leftNode;
            }

            return result;
        }

        List<ViterbiNode> ConvertUnknownWordToUnigramNode(ViterbiNode node)
        {
            var uniGramNodes = new List<ViterbiNode>();
            var unigramWordId = 0;
            var surface = node.Surface;

            for (var i = surface.Length; i > 0; i--)
            {
                var word = surface.Substring(i - 1, 1); // NOTE: original is surface.substring(i - 1, i); 
                var startIndex = node.StartIndex + i - 1;

                var uniGramNode = new ViterbiNode(unigramWordId, word, UnknownDictionary, startIndex, ViterbiNode.NodeType.Unknown);
                uniGramNodes.Add(uniGramNode);
            }

            return uniGramNodes;
        }
    }
}
