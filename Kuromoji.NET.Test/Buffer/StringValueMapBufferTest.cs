using System;
using System.Collections.Generic;
using Kuromoji.NET.Buffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Buffer
{
    [TestClass]
    public class StringValueMapBufferTest
    {
        [TestMethod]
        public void TestInsertIntoMap()
        {
            var input = new SortedDictionary<int, string>();
            input.Add(1, "hello");
            input.Add(2, "日本");
            input.Add(3, "カタカナ");
            input.Add(0, "Bye");

            var values = new StringValueMapBuffer(input);
            Assert.AreEqual("Bye", values[0]);
            Assert.AreEqual("hello", values[1]);
            Assert.AreEqual("日本", values[2]);
            Assert.AreEqual("カタカナ", values[3]);
        }
    }
}
