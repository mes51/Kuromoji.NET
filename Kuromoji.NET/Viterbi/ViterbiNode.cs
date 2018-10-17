using Kuromoji.NET.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public class ViterbiNode
    {
        public enum NodeType
        {
            Known,
            Unknown,
            User,
            Inserted
        }

        public int WordId { get; }

        public string Surface { get; }

        public int LeftId { get; }

        public int RightId { get; }

        /// <summary>
        /// word cost for this node
        /// </summary>
        public int WordCost { get; }

        /// <summary>
        /// minimum path cost found thus far
        /// </summary>
        public int PathCost { get; set; }

        public ViterbiNode LeftNode { get; set; }

        public NodeType Type { get; }

        public int StartIndex { get; }

        public ViterbiNode(int wordId, string surface, int leftId, int rightId, int wordCost, int startIndex, NodeType type)
        {
            WordId = wordId;
            Surface = surface;
            LeftId = leftId;
            RightId = rightId;
            WordCost = wordCost;
            StartIndex = startIndex;
            Type = type;
        }

        public ViterbiNode(int wordId, string word, IDictionary dictionary, int startIndex, NodeType type) :
            this(wordId, word, dictionary.GetLeftId(wordId), dictionary.GetRightId(wordId), dictionary.GetWordCost(wordId), startIndex, type) { }
    }
}
