using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Extentions
{
    public static class StringExtension
    {
        static Int32Converter Int32Converter = new Int32Converter();

        public static int CodePointAt(this string str, int index)
        {
            if (char.IsSurrogatePair(str, index))
            {
                return char.ConvertToUtf32(str, index);
            }
            else
            {
                return str[index];
            }
        }

        public static int DecodeToInt32(this string str)
        {
            return (int)Int32Converter.ConvertFromString(str);
        }
    }
}
