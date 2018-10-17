using System;
using Kuromoji.NET.Tokenizers.UniDic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Tokenizers.UniDic
{
    [TestClass]
    [DeploymentItem("Resources\\UniDic\\unidic.zip", "Dict")]
    [DeploymentItem("Resources\\userdict.txt", "Dict")]
    [DeploymentItem("Resources\\UniDic\\bocchan-unidic-features.txt", "Input\\UniDic")]
    [DeploymentItem("Resources\\UniDic\\bocchan.txt", "Input\\UniDic")]
    [DeploymentItem("Resources\\UniDic\\jawikisentences-unidic-features.txt", "Input\\UniDic")]
    [DeploymentItem("Resources\\UniDic\\jawikisentences.txt", "Input\\UniDic")]
    public class MultiThreadedTokenizerTest
    {
        const string DicFileName = @"Dict\unidic.zip";

        [TestMethod]
        public void TestMultiThreadedBocchan()
        {
            new Tokenizer(DicFileName).IsSameMultiThreadedTokenizedStream(5, 10, "Input\\UniDic\\bocchan-unidic-features.txt", "Input\\UniDic\\bocchan.txt");
        }

        [TestMethod]
        public void TestMultiThreadedUserDictionary()
        {
            new Tokenizer.Builder(DicFileName).SetUserDictionary("Dict\\userdict.txt").Build()
                .IsSameMultiThreadedTokenizedStream(5, 250, "Input\\UniDic\\jawikisentences-unidic-features.txt", "Input\\UniDic\\jawikisentences.txt");
        }
    }
}
