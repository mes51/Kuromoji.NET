using Kuromoji.NET.Compile;
using Kuromoji.NET.Tokenizers.UniDic.Compile;
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
    class UniDicBuilder : BuilderBase<DictionaryEntry>
    {
        protected override string DirectoryName => "UniDic";

        protected override string OutputDicFileName => "unidic.zip";

        protected override Encoding DicEncoding => Encoding.UTF8;

        public UniDicBuilder()
        {
            DicUrl = "http://atilika.com/releases/unidic-mecab/unidic-mecab-2.1.2_src.zip";
        }

        protected override DictionaryCompilerBase<DictionaryEntry> CreateCompiler()
        {
            return new DictionaryCompiler();
        }
    }
}
