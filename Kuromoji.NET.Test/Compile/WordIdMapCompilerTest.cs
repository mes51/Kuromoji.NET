using System;
using System.IO;
using System.Linq;
using Kuromoji.NET.Buffer;
using Kuromoji.NET.Compile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Compile
{
    [TestClass]
    public class WordIdMapCompilerTest
    {
        [TestMethod]
        public void TestGrowableArray()
        {
            var array = new WordIdMapCompiler.GrowableIntArray(5);
            array[3] = 1;
            string.Join(", ", array.GetArray()).Is("0, 0, 0, 1");
            array[0] = 2;
            array[10] = 3;
            string.Join(", ", array.GetArray()).Is("2, 0, 0, 1, 0, 0, 0, 0, 0, 0, 3");
        }

        [TestMethod]
        public void TestCompiler()
        {
            var compiler = new WordIdMapCompiler();
            compiler.AddMapping(3, 1);
            compiler.AddMapping(3, 2);
            compiler.AddMapping(3, 3);
            compiler.AddMapping(10, 0);

            using (var ms = new MemoryStream())
            {
                compiler.Write(ms);

                ms.Seek(0, SeekOrigin.Begin);

                var wordIds = new WordIdMap(ms);
                string.Join(", ", wordIds.LookUp(3)).Is("1, 2, 3");
                string.Join(", ", wordIds.LookUp(10)).Is("0");
                string.Join(", ", wordIds.LookUp(1)).Is("");
            }
        }
    }
}
