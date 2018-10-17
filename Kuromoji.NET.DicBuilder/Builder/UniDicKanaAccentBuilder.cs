using Kuromoji.NET.Compile;
using Kuromoji.NET.Tokenizers.UniDicKanaAccent.Compile;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.DicBuilder.Builder
{
    class UniDicKanaAccentBuilder : BuilderBase<DictionaryEntry>
    {
        protected override string DirectoryName => "UniDicKanaAccent";

        protected override string OutputDicFileName => "unidic-kanaaccent.zip";

        protected override Encoding DicEncoding => Encoding.UTF8;

        public UniDicKanaAccentBuilder()
        {
            DicUrl = "http://atilika.com/releases/unidic-mecab/unidic-mecab_kana-accent-2.1.2_src.zip";
        }

        protected override DictionaryCompilerBase<DictionaryEntry> CreateCompiler()
        {
            return new DictionaryCompiler();
        }
    }
}
