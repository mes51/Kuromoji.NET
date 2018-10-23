using Kuromoji.NET.Tokenizers.UniDic;
using System;
using System.IO;

namespace Kuromoji.NET.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "お寿司が食べたい。";
            var dictionaryPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), @"Resources\unidic.zip");
            var tokenizer = new Tokenizer(dictionaryPath);

            Console.WriteLine($"input:\t{input}");
            foreach (var token in tokenizer.Tokenize(input))
            {
                Console.WriteLine($"{token.Surface}\t{token.GetAllFeatures()}");
            }
        }
    }
}
