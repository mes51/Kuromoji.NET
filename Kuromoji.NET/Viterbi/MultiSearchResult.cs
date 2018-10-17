using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public class MultiSearchResult
    {
        public List<List<ViterbiNode>> TokenizedResults { get; } = new List<List<ViterbiNode>>();

        public int Count => Costs.Count;

        List<int> Costs { get; } = new List<int>();

        public void Add(List<ViterbiNode> tokenizedResult, int cost)
        {
            TokenizedResults.Add(tokenizedResult);
            Costs.Add(cost);
        }

        public List<ViterbiNode> GetTokenizedResult(int index)
        {
            return TokenizedResults[index];
        }

        public int GetCost(int index)
        {
            return Costs[index];
        }
    }
}