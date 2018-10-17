using System;
using System.IO;
using System.Text;
using Kuromoji.NET.Compile;
using Kuromoji.NET.Dict;
using Kuromoji.NET.Extentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Compile
{
    [TestClass]
    public class ConnectionCostsCompilerTest
    {
        ConnectionCosts ConnectionCosts { get; set; }

        [TestInitialize]
        public void Setup()
        {
            var costs = "" +
                "3 3\n" +
                "0 0 1\n" +
                "0 1 2\n" +
                "0 2 3\n" +
                "1 0 4\n" +
                "1 1 5\n" +
                "1 2 6\n" +
                "2 0 7\n" +
                "2 1 8\n" +
                "2 2 9\n";

            using (var ms = new MemoryStream())
            using (var costStream = new MemoryStream(Encoding.UTF8.GetBytes(costs)))
            {
                var compiler = new ConnectionCostsCompiler(ms);
                compiler.ReadCosts(costStream);
                compiler.Compile();

                ms.Seek(0, SeekOrigin.Begin);

                var size = ms.ReadInt32();
                var costSize = ms.ReadInt32();
                var costValues = new short[costSize / sizeof(short)];
                ms.ReadShortArray(costValues);
                ConnectionCosts = new ConnectionCosts(size, costValues);
            }
        }

        [TestMethod]
        public void TestCosts()
        {
            var cost = 1;

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    ConnectionCosts[i, j].Is(cost++);
                }
            }
        }
    }
}
