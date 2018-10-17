using Kuromoji.NET.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public interface ITokenFactory<T> where T : TokenBase
    {
        T CreateToken(int wordId, string surface, ViterbiNode.NodeType type, int position, IDictionary dictionary);
    }
}
