using Kuromoji.NET.Compile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Tokenizers.UniDicKanaAccent.Compile
{
    public class DictionaryCompiler : DictionaryCompilerBase<DictionaryEntry>
    {
        protected override TokenInfoDictionaryCompilerBase<DictionaryEntry> GetTokenInfoDictionaryCompiler(Encoding encoding)
        {
            return new TokenInfoDictionaryCompiler(encoding);
        }
    }
}
