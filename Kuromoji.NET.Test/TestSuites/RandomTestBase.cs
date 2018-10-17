using Kuromoji.NET.Test.TestSuites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Test.TestSuits
{
    public class RandomTestBase
    {
        protected Random Random { get; } = new Random();

        RealisticUnicodeGenerator RealisticUnicodeGenerator { get; } = new RealisticUnicodeGenerator();

        UnicodeGenerator UnicodeGenerator { get; } = new UnicodeGenerator();

        AsciiLettersGenerator AsciiLettersGenerator { get; } = new AsciiLettersGenerator();

        protected string RandomUnicodeOfLength(int length)
        {
            return UnicodeGenerator.OfCodeUnitsLength(Random, length, length);
        }

        protected string RandomRealisticUnicodeOfLength(int length)
        {
            return RealisticUnicodeGenerator.OfCodeUnitsLength(Random, length, length);
        }

        protected string RandomAsciiOfLength(int length)
        {
            return AsciiLettersGenerator.OfCodeUnitsLength(Random, length, length);
        }
    }
}
