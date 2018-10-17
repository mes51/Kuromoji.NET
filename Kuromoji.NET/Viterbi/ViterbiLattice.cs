using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public class ViterbiLattice
    {
        const string BOS = "BOS";

        const string EOS = "EOS";

        int Dimension { get; }

        public ViterbiNode[][] StartIndexArr { get; }

        public ViterbiNode[][] EndIndexArr { get; }

        public int[] StartSizeArr { get; }

        public int[] EndSizeArr { get; }

        public ViterbiLattice(int dimension)
        {
            Dimension = dimension;
            StartIndexArr = new ViterbiNode[dimension][];
            EndIndexArr = new ViterbiNode[dimension][];
            StartSizeArr = new int[dimension];
            EndSizeArr = new int[dimension];
        }

        public void AddBos()
        {
            var bosNode = new ViterbiNode(-1, BOS, 0, 0, 0, -1, ViterbiNode.NodeType.Known);
            AddNode(bosNode, 0, 1);
        }

        public void AddEos()
        {
            var eosNode = new ViterbiNode(-1, EOS, 0, 0, 0, Dimension - 1, ViterbiNode.NodeType.Known);
            AddNode(eosNode, Dimension - 1, 0);
        }

        public void AddNode(ViterbiNode node, int start, int end)
        {
            AddNodeToArray(node, start, StartIndexArr, StartSizeArr);
            AddNodeToArray(node, end, EndIndexArr, EndSizeArr);
        }

        void AddNodeToArray(ViterbiNode node, int index, ViterbiNode[][] arr, int[] sizes)
        {
            var count = sizes[index];

            ExpandIfNeeded(index, arr, count);

            arr[index][count] = node;
            sizes[index] = count + 1;
        }

        void ExpandIfNeeded(int index, ViterbiNode[][] arr, int count)
        {
            if (count == 0)
            {
                arr[index] = new ViterbiNode[10];
            }

            if (arr[index].Length <= count)
            {
                arr[index] = ExtendArray(arr[index]);
            }
        }

        ViterbiNode[] ExtendArray(ViterbiNode[] array)
        {
            var newArray = new ViterbiNode[array.Length * 2];
            Array.Copy(array, newArray, array.Length);
            return newArray;
        }

        internal bool TokenEndsWhereCurrentTokenStarts(int startIndex)
        {
            return EndSizeArr[startIndex + 1] != 0;
        }
    }
}
