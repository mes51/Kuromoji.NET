using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;

namespace Kuromoji.NET.IO
{
    public static class StringArrayIO
    {
        public static string[] ReadArray(Stream input)
        {
            var length = input.ReadInt32();
            var array = new string[length];

            using (var reader = new BinaryReader(input, Encoding.UTF8, true))
            {
                for (var i = 0; i < length; i++)
                {
                    array[i] = reader.ReadString();
                }
            }

            return array;
        }

        public static string[][] ReadArray2D(Stream input)
        {
            var length = input.ReadInt32();

            var array = new string[length][];

            for (var i = 0; i < length; i++)
            {
                array[i] = ReadArray(input);
            }

            return array;
        }

        public static string[][] ReadSparseArray2D(Stream input)
        {
            var length = input.ReadInt32();

            var array = new string[length][];

            var index = 0;
            while ((index = input.ReadInt32()) >= 0)
            {
                array[index] = ReadArray(input);
            }

            return array;
        }

        public static void WriteArray(Stream output, string[] array)
        {
            output.Write(array.Length);

            using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
            {
                foreach (var s in array)
                {
                    writer.Write(s);
                }
            }
        }

        public static void WriteArray2D(Stream output, string[][] array)
        {
            output.Write(array.Length);

            foreach (var a in array)
            {
                WriteArray(output, a);
            }
        }

        public static void WriteSparseArray2D(Stream output, string[][] array)
        {
            output.Write(array.Length);

            for (var i = 0; i < array.Length; i++)
            {
                var inner = array[i];
                if (inner != null)
                {
                    output.Write(i);
                    WriteArray(output, inner);
                }
            }

            // This negative index serves as an end-of-array marker
            output.Write(-1);
        }
    }
}
