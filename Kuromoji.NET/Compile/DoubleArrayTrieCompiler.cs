using Kuromoji.NET.Trie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Compile
{
    public static class DoubleArrayTrieCompiler
    {
        public static DoubleArrayTrie Build(List<string> surfaces, bool compact)
        {
            var trie = new Trie.Trie();

            foreach (var surface in surfaces)
            {
                trie.Add(surface);
            }
            var doubleArrayTrie = new DoubleArrayTrie(compact);
            doubleArrayTrie.Build(trie);

            return doubleArrayTrie;
        }
    }
}
