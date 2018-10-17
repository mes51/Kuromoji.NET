using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;

namespace Kuromoji.NET.IO
{
    public static class IntArrayIO
    {
        const int IntBytes = sizeof(int);

        public static int[][] ReadArrays(Stream input, int arrayCount)
        {
            int[][] arrays = new int[arrayCount][];
            for (int i = 0; i < arrayCount; i++)
            {
                arrays[i] = ReadArrayFromChannel(input);
            }
            return arrays;
        }

        public static void WriteArray(Stream output, int[] array)
        {
            output.Write(array.Length);
            output.Write(array);
        }

        public static int[][] ReadArray2D(Stream input)
        {
            return ReadArrays(input, input.ReadInt32());
        }

        public static void WriteArray2D(Stream output, int[][] array)
        {
            output.Write(array.Length);
            foreach (var a in array)
            {
                WriteArray(output, a);
            }
        }

        public static int[][] ReadSparseArray2D(Stream input)
        {
            int[][] arrays = new int[input.ReadInt32()][];

            int index;

            while ((index = input.ReadInt32()) >= 0)
            {
                arrays[index] = ReadArrayFromChannel(input);
            }
            return arrays;
        }

        public static void WriteSparseArray2D(Stream output, int[][] array)
        {
            output.Write(array.Length);

            for (int i = 0; i < array.Length; i++)
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


        static int[] ReadArrayFromChannel(Stream channel)
        {
            int length = channel.ReadInt32();

            var result = new int[length];
            channel.ReadIntArray(result);

            return result;
        }
    }
}
