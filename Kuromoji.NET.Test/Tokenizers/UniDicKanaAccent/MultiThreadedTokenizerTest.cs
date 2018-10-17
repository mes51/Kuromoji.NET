using System;
using Kuromoji.NET.Tokenizers.UniDicKanaAccent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Tokenizers.UniDicKanaAccent
{
    [TestClass]
    [DeploymentItem("Resources\\UniDicKanaAccent\\unidic-kanaaccent.zip", "Dict")]
    [DeploymentItem("Resources\\userdict.txt", "Dict")]
    [DeploymentItem("Resources\\UniDicKanaAccent\\bocchan-unidic-kanaaccent-features.txt", "Input\\UniDicKanaAcent")]
    [DeploymentItem("Resources\\UniDicKanaAccent\\bocchan.txt", "Input\\UniDicKanaAcent")]
    [DeploymentItem("Resources\\UniDicKanaAccent\\jawikisentences-unidic-kanaaccent-features.txt", "Input\\UniDicKanaAcent")]
    [DeploymentItem("Resources\\UniDicKanaAccent\\jawikisentences.txt", "Input\\UniDicKanaAcent")]
    public class MultiThreadedTokenizerTest
    {
        const string DicFileName = @"Dict\unidic-kanaaccent.zip";

        [TestMethod]
        public void TestMultiThreadedBocchan()
        {
            new Tokenizer(DicFileName).IsSameMultiThreadedTokenizedStream(5, 10, "Input\\UniDicKanaAcent\\bocchan-unidic-kanaaccent-features.txt", "Input\\UniDicKanaAcent\\bocchan.txt");
        }

        [TestMethod]
        public void TestMultiThreadedUserDictionary()
        {
            new Tokenizer.Builder(DicFileName).SetUserDictionary("Dict\\userdict.txt").Build()
                .IsSameMultiThreadedTokenizedStream(5, 250, "Input\\UniDicKanaAcent\\jawikisentences-unidic-kanaaccent-features.txt", "Input\\UniDicKanaAcent\\jawikisentences.txt");
        }
    }
}
