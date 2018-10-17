using Kuromoji.NET.Tokenizers.UniDic;
using System;

namespace Kuromoji.NET.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "お寿司が食べたい。";
            var tokenizer = new Tokenizer(@"C:\Users\mes\Desktop\Kuromoji.NET\Kuromoji.NET.Test\Resources\UniDic\unidic.zip");

            Console.WriteLine($"input:\t{input}");
            foreach (var token in tokenizer.Tokenize(input))
            {
                Console.WriteLine($"{token.Surface}\t{token.GetAllFeatures()}");
            }
        }
    }
}
