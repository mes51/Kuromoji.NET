using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuromoji.NET.Buffer;
using Kuromoji.NET.Compile;
using Kuromoji.NET.Extentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Compile
{
    [TestClass]
    public class TokenInfoBufferCompilerTest
    {
        [TestMethod]
        public void TestReadAndWriteFromBuffer()
        {
            var shorts = Enumerable.Range(0, 10).Select(i => (short)i).ToList();

            using (var ms = new MemoryStream())
            {
                ms.Write((short)shorts.Count);
                foreach (var s in shorts)
                {
                    ms.Write(s);
                }

                ms.Seek(0, SeekOrigin.Begin);

                var count = ms.ReadInt16();
                var readShorts = new List<short>();
                for (var i = 0; i < count; i++)
                {
                    readShorts.Add(ms.ReadInt16());
                }

                readShorts.SequenceEqual(shorts).IsTrue();
            }
        }

        [TestMethod]
        public void TestReadAndLookUpTokenInfo()
        {
            var tokenInfo = new List<short>();
            var features = new List<int>();

            var tokenInfos = new short[] { 1, 2, 3 };

            var featureInfos = new int[] { 73, 99 };

            tokenInfo.Add(1);
            tokenInfo.Add(2);
            tokenInfo.Add(3);

            features.Add(73);
            features.Add(99);

            var entry = new BufferEntry();
            entry.TokenInfo = tokenInfo;
            entry.Features = features;

            entry.TokenInfos = tokenInfos;
            entry.FeatureInfos = featureInfos;

            var bufferEntries = new List<BufferEntry>();
            bufferEntries.Add(entry);

            using (var ms = new MemoryStream())
            {
                var compiler = new TokenInfoBufferCompiler(ms, bufferEntries);
                compiler.Compile();

                ms.Seek(0, SeekOrigin.Begin);

                var tokenInfoBuffer2 = new TokenInfoBuffer(ms);

                tokenInfoBuffer2.LookupFeature(0, 1).Is(99);
                tokenInfoBuffer2.LookupFeature(0, 0).Is(73);
            }
        }

        [TestMethod]
        public void TestCompleteLookUp()
        {
            var resultMap = new Dictionary<int, string>();

            resultMap.Add(73, "hello");
            resultMap.Add(42, "今日は");
            resultMap.Add(99, "素敵な世界");

            var tokenInfo = new List<short>();
            var features = new List<int>();

            tokenInfo.Add(1);
            tokenInfo.Add(2);
            tokenInfo.Add(3);

            features.Add(73);
            features.Add(99);

            var entry = new BufferEntry();
            entry.TokenInfo = tokenInfo;
            entry.Features = features;

            var bufferEntries = new List<BufferEntry>();
            bufferEntries.Add(entry);

            using (var ms = new MemoryStream())
            {
                var compiler = new TokenInfoBufferCompiler(ms, bufferEntries);
                compiler.Compile();

                ms.Seek(0, SeekOrigin.Begin);

                var tokenInfoBuffer = new TokenInfoBuffer(ms);

                var result = tokenInfoBuffer.LookupEntry(0);
                resultMap[result.FeatureInfos[0]].Is("hello");
                resultMap[result.FeatureInfos[1]].Is("素敵な世界");
            }
        }
    }
}
