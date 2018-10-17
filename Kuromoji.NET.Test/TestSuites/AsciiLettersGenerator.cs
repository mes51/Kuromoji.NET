using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Test.TestSuites
{
    class AsciiLettersGenerator : StringGenerator
    {
        static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public override string OfCodePointsLength(Random r, int minCodePoints, int maxCodePoints)
        {
            var length = r.Next(minCodePoints, maxCodePoints + 1);
            return new string(Enumerable.Range(0, length).Select(_ => chars[r.Next(chars.Length)]).ToArray());
        }

        public override string OfCodeUnitsLength(Random r, int minCodeUnits, int maxCodeUnits)
        {
            return OfCodePointsLength(r, minCodeUnits, maxCodeUnits);
        }
    }
}
