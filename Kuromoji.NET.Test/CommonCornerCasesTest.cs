using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Test
{
    public static class CommonCornerCasesTest
    {
        public static void TestPunctuation<T>(TokenizerBase<T> tokenizer) where T : TokenBase
        {
            var gerryNoHanaNoHanashi = "僕の鼻はちょっと\r\n長いよ。";

            tokenizer.Tokenize(gerryNoHanaNoHanashi).IsSameTokenSurfaces(new string[] { "僕", "の", "鼻", "は", "ちょっと", "\r", "\n", "長い", "よ", "。" });
        }
    }
}
