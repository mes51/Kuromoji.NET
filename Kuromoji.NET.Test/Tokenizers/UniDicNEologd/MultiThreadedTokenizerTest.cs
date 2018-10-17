using System;
using Kuromoji.NET.Tokenizers.UniDicNEologd;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Tokenizers.UniDicNEologd
{
    [TestClass]
    [DeploymentItem("Resources\\userdict.txt", "Dict")]
    [DeploymentItem("Resources\\UniDicNEologd\\unidic-neologd.zip", "Dict")]
    [DeploymentItem("Resources\\UniDicNEologd\\bocchan-unidic-neologd-features.txt", "Input\\UniDicNEologd")]
    [DeploymentItem("Resources\\UniDicNEologd\\bocchan.txt", "Input\\UniDicNEologd")]
    [DeploymentItem("Resources\\UniDicNEologd\\jawikisentences-unidic-neologd-features.txt", "Input\\UniDicNEologd")]
    [DeploymentItem("Resources\\UniDicNEologd\\jawikisentences.txt", "Input\\UniDicNEologd")]
    public class MultiThreadedTokenizerTest
    {
        const string DicFileName = @"Dict\unidic-neologd.zip";

        [TestMethod]
        public void TestMultiThreadedBocchan()
        {
            new Tokenizer(DicFileName).IsSameMultiThreadedTokenizedStream(5, 10, "Input\\UniDicNEologd\\bocchan-unidic-neologd-features.txt", "Input\\UniDicNEologd\\bocchan.txt");
        }

        [TestMethod]
        public void TestMultiThreadedUserDictionary()
        {
            new Tokenizer.Builder(DicFileName).SetUserDictionary("Dict\\userdict.txt").Build()
                .IsSameMultiThreadedTokenizedStream(5, 250, "Input\\UniDicNEologd\\jawikisentences-unidic-neologd-features.txt", "Input\\UniDicNEologd\\jawikisentences.txt");
        }
    }
}
