using Kuromoji.NET.Compile;
using Kuromoji.NET.Tokenizers.UniDicNEologd.Compile;
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
    class UniDicNEologdBuilder : BuilderBase<DictionaryEntry>
    {
        protected override string DirectoryName => "UniDicNEologd";

        protected override string OutputDicFileName => "unidic-neologd.zip";

        protected override Encoding DicEncoding => Encoding.UTF8;

        public UniDicNEologdBuilder()
        {
            DicUrl = "http://atilika.com/releases/unidic-mecab-neologd/unidic-mecab-2.1.2_src-neologd-20171002.zip";
        }

        protected override DictionaryCompilerBase<DictionaryEntry> CreateCompiler()
        {
            return new DictionaryCompiler();
        }
    }
}
