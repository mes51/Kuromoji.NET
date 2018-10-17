using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Test.TestSuites
{
    class UnicodeGenerator : StringGenerator
    {
        const char MaxSurrogate = '\uDFFF';

        const char MinSurrogate = '\uD800';

        const int MaxCodePoint = 0x10FFFF;

        const int SurrogateRange = MaxSurrogate - MinSurrogate;

        const int CodePointRange = MaxCodePoint - SurrogateRange;

        public override string OfCodePointsLength(Random r, int minCodePoints, int maxCodePoints)
        {
            var length = r.Next(minCodePoints, maxCodePoints + 1);
            var chars = new char[length];

            for (var i = 0; i < length; i++)
            {
                var v = r.Next(0, CodePointRange + 1);
                if (v >= MinSurrogate)
                {
                    v += SurrogateRange;
                }
                chars[i] = (char)v;
            }

            return new string(chars);
        }

        public override string OfCodeUnitsLength(Random r, int minCodeUnits, int maxCodeUnits)
        {
            var length = r.Next(minCodeUnits, maxCodeUnits + 1);
            var chars = new char[length];
            for (var i = 0; i < length;)
            {
                var t = r.Next(0, 5);
                if (t == 0 && i < length - 1)
                {
                    chars[i++] = (char)r.Next(0xD800, 0xDC00); // high
                    chars[i++] = (char)r.Next(0xDC00, 0xE000); // low
                }
                else if (t <= 1)
                {
                    chars[i++] = (char)r.Next(0, 0x80);
                }
                else if (t == 2)
                {
                    chars[i++] = (char)r.Next(0x80, 0x800);
                }
                else if (t == 3)
                {
                    chars[i++] = (char)r.Next(0x800, 0xD800);
                }
                else if (t == 4)
                {
                    chars[i++] = (char)r.Next(0xE000, 0x10000);
                }
            }

            return new string(chars);
        }

        /// <summary>
        /// Returns a random string that will have a random UTF-8 representation length between
        /// <code>minUtf8Length</code> and <code>maxUtf8Length</code>.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="minUtf8Length">Minimum UTF-8 representation length (inclusive).</param>
        /// <param name="maxUtf8Length">Maximum UTF-8 representation length (inclusive).</param>
        /// <returns></returns>
        public string OfUtf8Length(Random r, int minUtf8Length, int maxUtf8Length)
        {
            var length = r.Next(minUtf8Length, maxUtf8Length + 1);
            var buffer = new char[length * 3];
            var bytes = length;
            var i = 0;

            for (; i < buffer.Length; i++)
            {
                var t = 0;
                if (bytes >= 4)
                {
                    t = r.Next(5);
                }
                if (bytes >= 3)
                {
                    t = r.Next(4);
                }
                if (bytes >= 2)
                {
                    t = r.Next(2);
                }

                switch (t)
                {
                    case 0:
                        buffer[i] = (char)r.Next(0, 0x80);
                        bytes--;
                        break;
                    case 1:
                        buffer[i] = (char)r.Next(0x80, 0x800);
                        bytes -= 2;
                        break;
                    case 2:
                        buffer[i] = (char)r.Next(0x800, 0xD800);
                        bytes -= 3;
                        break;
                    case 3:
                        buffer[i] = (char)r.Next(0xE000, 0x10000);
                        bytes -= 3;
                        break;
                    case 4:
                        buffer[i++] = (char)r.Next(0xD800, 0xDC00); // high
                        buffer[i] = (char)r.Next(0xDC00, 0xE000);   // low
                        bytes -= 4;
                        break;
                }
            }

            return new string(buffer, 0, i);
        }
    }
}
