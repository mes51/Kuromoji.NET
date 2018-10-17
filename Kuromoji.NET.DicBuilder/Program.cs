using Kuromoji.NET.DicBuilder.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.DicBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("1: unidic");
            Console.WriteLine("2: unidic kana accent");
            Console.WriteLine("3: unidic neologd");
            Console.Write("please choice create dictionary: ");

            var builderNumber = 0;
            while (true)
            {
                if (!int.TryParse(Console.ReadLine().Trim().Trim('\r', '\n'), out builderNumber) || builderNumber < 1 || builderNumber > 3)
                {
                    Console.WriteLine("please input 1 to 3");
                }
                break;
            }

            Console.Write("use compact trie?(y/n): ");
            var compactTrie = Console.ReadLine().Trim().Trim('\r', '\n') == "y";

            switch (builderNumber)
            {
                case 1:
                    new UniDicBuilder().Build(compactTrie);
                    break;
                case 2:
                    new UniDicKanaAccentBuilder().Build(compactTrie);
                    break;
                case 3:
                    new UniDicNEologdBuilder().Build(compactTrie);
                    break;
            }
        }
    }
}
