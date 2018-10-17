using Kuromoji.NET.Dict;
using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    /// <summary>
    /// An instance of MultiSearcher can be used to extend the search functionality of ViterbiSearcher to find multiple paths ordered by cost.
    /// Note that the MultiSearcher uses the value of ViterbiNode.getPathCost() to evaluate the cost of possible path.
    /// Therefore, the ViterbiLattice should be updated by ViterbiSearcher.calculatePathCosts() before being used by the MultiSearcher.
    /// 
    /// The implementation is based on Eppstein's algorithm for finding n shortest paths in a weighted directed graph.
    /// </summary>
    public class MultiSearcher
    {
        ConnectionCosts Costs { get; }

        TokenizerMode Mode { get; }

        ViterbiSearcher ViterbiSearcher { get; }

        int BaseCost { get; set; }

        List<int> PathCosts { get; } = new List<int>();

        Dictionary<ViterbiNode, SidetrackEdge> Sidetracks { get; } = new Dictionary<ViterbiNode, SidetrackEdge>();

        public MultiSearcher(ConnectionCosts costs, TokenizerMode mode, ViterbiSearcher viterbiSearcher)
        {
            Costs = costs;
            Mode = mode;
            ViterbiSearcher = viterbiSearcher;
        }

        /// <summary>
        /// Get up to maxCount shortest paths with cost at most OPT + costSlack, where OPT is the optimal solution. The results are ordered in ascending order by cost.
        /// </summary>
        /// <param name="lattice">an instance of ViterbiLattice prosecced by a ViterbiSearcher</param>
        /// <param name="maxCount">the maximum number of results</param>
        /// <param name="costSlack">the maximum cost slack of a path</param>
        /// <returns>the shortest paths and their costs</returns>
        public MultiSearchResult GetShortestPaths(ViterbiLattice lattice, int maxCount, int costSlack)
        {
            PathCosts.Clear();
            Sidetracks.Clear();
            var multiSearchResult = new MultiSearchResult();
            BuildSidetracks(lattice);
            var eos = lattice.EndIndexArr[0][0];
            BaseCost = eos.PathCost;
            var paths = GetPaths(eos, maxCount, costSlack);

            foreach (var (path, cost) in paths.Zip(Enumerable.Range(0, paths.Count), (p, i) => Tuple.Create(p, PathCosts[i])))
            {
                var nodes = GeneratePath(eos, path);
                multiSearchResult.Add(nodes, cost);
            }

            return multiSearchResult;
        }

        List<ViterbiNode> GeneratePath(ViterbiNode eos, SidetrackEdge sidetrackEdge)
        {
            var result = new LinkedList<ViterbiNode>();
            var node = eos;
            result.AddFirst(eos);

            while (node.LeftNode != null)
            {
                var leftNode = node.LeftNode;
                if (sidetrackEdge != null && sidetrackEdge.Head == node)
                {
                    leftNode = sidetrackEdge.Tail;
                    sidetrackEdge = sidetrackEdge.Parent;
                }
                node = leftNode;
                result.AddFirst(node);
            }

            return result.ToList();
        }

        void BuildSidetracks(ViterbiLattice lattice)
        {
            var startIndexArr = lattice.StartIndexArr;
            var endIndexArr = lattice.EndIndexArr;

            for (int i = 1; i < startIndexArr.Length; i++)
            {
                if (startIndexArr[i] == null || endIndexArr[i] == null)
                {
                    continue;
                }

                foreach (var node in startIndexArr[i])
                {
                    if (node == null)
                    {
                        break;
                    }

                    BuildSidetracksForNode(endIndexArr[i], node);
                }
            }
        }

        void BuildSidetracksForNode(ViterbiNode[] leftNodes, ViterbiNode node)
        {
            var backwardConnectionId = node.LeftId;
            var wordCost = node.WordCost;

            var sidetrackEdges = new List<SidetrackEdge>();
            SidetrackEdge nextOption;//
            Sidetracks.TryGetValue(node.LeftNode, out nextOption);

            foreach (var leftNode in leftNodes)
            {
                if (leftNode == null)
                {
                    break;
                }

                if (leftNode.Type == ViterbiNode.NodeType.Known && leftNode.WordId == -1)
                {
                    // Ignore BOS
                    continue;
                }

                var sideTrackCost = leftNode.PathCost - node.PathCost + wordCost + Costs[leftNode.RightId, backwardConnectionId];
                if (Mode == TokenizerMode.Search || Mode == TokenizerMode.Extended)
                {
                    sideTrackCost += ViterbiSearcher.GetPenaltyCost(node);
                }

                if (leftNode != node.LeftNode)
                {
                    sidetrackEdges.Add(new SidetrackEdge(sideTrackCost, leftNode, node));
                }
            }

            if (sidetrackEdges.Count == 0)
            {
                Sidetracks.Add(node, nextOption);
            }
            else
            {
                for (var i = 0; i < sidetrackEdges.Count - 1; i++)
                {
                    sidetrackEdges[i].NextOption = sidetrackEdges[i + 1];
                }
                sidetrackEdges[sidetrackEdges.Count - 1].NextOption = nextOption;
                Sidetracks.Add(node, sidetrackEdges[0]);
            }
        }

        List<SidetrackEdge> GetPaths(ViterbiNode eos, int maxCount, int costSlack)
        {
            var result = new List<SidetrackEdge>();
            result.Add(null);
            PathCosts.Add(BaseCost);
            var sidetrackHeap = new PriorityQueue<SidetrackEdge>();

            var sideTrackEdge = Sidetracks[eos];
            while (sideTrackEdge != null)
            {
                sidetrackHeap.Add(sideTrackEdge);
                sideTrackEdge = sideTrackEdge.NextOption;
            }

            for (var i = 1; i < maxCount; i++)
            {
                if (sidetrackHeap.Count == 0)
                {
                    break;
                }

                sideTrackEdge = sidetrackHeap.Dequeue();
                if (sideTrackEdge.Cost > costSlack)
                {
                    break;
                }
                result.Add(sideTrackEdge);
                PathCosts.Add(BaseCost + sideTrackEdge.Cost);
                var nextSidetrack = Sidetracks[sideTrackEdge.Tail];

                while (nextSidetrack != null)
                {
                    var next = new SidetrackEdge(nextSidetrack.Cost, nextSidetrack.Tail, nextSidetrack.Head);
                    next.Parent = sideTrackEdge;
                    sidetrackHeap.Add(next);
                    nextSidetrack = nextSidetrack.NextOption;
                }
            }

            return result;
        }

        class SidetrackEdge : IComparable<SidetrackEdge>
        {
            public int Cost { get; set; }

            public ViterbiNode Tail { get; }

            public ViterbiNode Head { get; }

            public SidetrackEdge NextOption { get; set; }

            private SidetrackEdge parent;
            public SidetrackEdge Parent
            {
                get
                {
                    return parent;
                }
                set
                {
                    parent = value;
                    Cost += parent.Cost;
                }
            }

            public SidetrackEdge(int cost, ViterbiNode tail, ViterbiNode head)
            {
                Cost = cost;
                Tail = tail;
                Head = head;
            }

            public int CompareTo(SidetrackEdge other)
            {
                return Cost - other.Cost;
            }
        }
    }
}
