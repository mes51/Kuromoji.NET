using System;
using System.Linq;
using Kuromoji.NET.Dict;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Dict
{
    [TestClass]
    public class InsertedDictionaryTest
    {
        [TestMethod]
        public void TestFeatureSize()
        {
            var dictionary1 = new InsertedDictionary(9);
            var dictionary2 = new InsertedDictionary(5);

            dictionary1.GetAllFeatures(0).Is("*,*,*,*,*,*,*,*,*");
            dictionary2.GetAllFeatures(0).Is("*,*,*,*,*");

            dictionary1.GetAllFeaturesArray(0).SequenceEqual(new string[] { "*", "*", "*", "*", "*", "*", "*", "*", "*" }).IsTrue();
            dictionary2.GetAllFeaturesArray(0).SequenceEqual(new string[] { "*", "*", "*", "*", "*" }).IsTrue();
        }
    }
}
