using System;
using System.IO;
using System.Text;
using Kuromoji.NET.Tokenizers.UniDicKanaAccent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Tokenizers.UniDicKanaAccent
{
    [TestClass]
    [DeploymentItem("Resources\\UniDicKanaAccent\\unidic-kanaaccent.zip", "Dict")]
    [DeploymentItem("Resources\\UniDicKanaAccent\\bocchan-unidic-kanaaccent-features.txt", "Input\\UniDicKanaAcent")]
    [DeploymentItem("Resources\\UniDicKanaAccent\\bocchan.txt", "Input\\UniDicKanaAcent")]
    public class TokenizerTest
    {
        const string DicFileName = @"Dict\unidic-kanaaccent.zip";

        static Tokenizer Tokenizer { get; set; }

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            Tokenizer = new Tokenizer(DicFileName);
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            Tokenizer = null;
        }

        [TestMethod]
        public void TestSimpleMultiTokenization()
        {
            var input = "スペースステーションに行きます。うたがわしい。";

            using (var fs = new System.IO.FileStream("lattice.text", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
            {
                Tokenizer.DebugLattice(fs, input);
            }

            var tokenLists = Tokenizer.MultiTokenize(input, 20, 100000);

            tokenLists.Count.Is(20);

            foreach (var tokens in tokenLists)
            {
                var sb = new StringBuilder();
                foreach (var token in tokens)
                {
                    sb.Append(token.Surface);
                }
                sb.ToString().Is(input);
            }

            var surfaces = new string[] { "スペース", "ステーション", "に", "行き", "ます", "。", "うたがわしい", "。" };
            tokenLists[0].IsSameTokenSurfaces(surfaces);
        }

        [TestMethod]
        public void TestMultiNoOverflow()
        {
            var input = "バスできた。";
            var tokenLists = Tokenizer.MultiTokenizeBySlack(input, int.MaxValue);
            tokenLists.Count.IsNot(0);
        }

        [TestMethod]
        public void TestMultiEmptyString()
        {
            var input = "";
            var tokenLists = Tokenizer.MultiTokenize(input, 10, int.MaxValue);
            tokenLists.Count.Is(1);
        }

        [TestMethod]
        public void TestFirstEntryCornerCase()
        {
            var tokens = Tokenizer.Tokenize("¡");
            var expectedFeatures = "補助記号,一般,*,*,*,*,,¡,¡,,¡,,記号,*,*,*,*,,,,,*,*,*,*,*";

            tokens[0].GetAllFeatures().Is(expectedFeatures);
        }

        [TestMethod]
        public void TestLastEntryCornerCase()
        {
            var tokens = Tokenizer.Tokenize("ヴィ");
            var expectedFeatures = "記号,一般,*,*,*,*,ヴィ,ヴィ,ヴィ,ヴィ,ヴィ,ヴィ,記号,*,*,*,*,ヴィ,ヴィ,ヴィ,ヴィ,*,*,1,*,*";

            tokens.Count.Is(1);
            tokens[0].GetAllFeatures().Is(expectedFeatures);
        }

        [TestMethod]
        public void TestExtendedUnidic()
        {
            var tokens = Tokenizer.Tokenize("日本語の形態素解析は面白い");

            var expectedSurfaces = new string[] { "日本", "語", "の", "形態", "素", "解析", "は", "面白い" };

            var expectedFeatures = new string[] {
                "名詞,固有名詞,地名,国,*,*,ニッポン,日本,日本,ニッポン,日本,ニッポン,固,*,*,*,*,ニッポン,ニッポン,ニッポン,ニッポン,*,*,3,*,*",
                "名詞,普通名詞,一般,*,*,*,ゴ,語,語,ゴ,語,ゴ,漢,*,*,*,*,ゴ,ゴ,ゴ,ゴ,*,*,1,C3,*",
                "助詞,格助詞,*,*,*,*,ノ,の,の,ノ,の,ノ,和,*,*,*,*,ノ,ノ,ノ,ノ,*,*,*,名詞%F1,*",
                "名詞,普通名詞,一般,*,*,*,ケイタイ,形態,形態,ケータイ,形態,ケータイ,漢,*,*,*,*,ケイタイ,ケイタイ,ケイタイ,ケイタイ,*,*,0,C2,*",
                "接尾辞,名詞的,一般,*,*,*,ソ,素,素,ソ,素,ソ,漢,*,*,*,*,ソ,ソ,ソ,ソ,*,*,*,C3,*",
                "名詞,普通名詞,サ変可能,*,*,*,カイセキ,解析,解析,カイセキ,解析,カイセキ,漢,*,*,*,*,カイセキ,カイセキ,カイセキ,カイセキ,*,*,0,C2,*",
                "助詞,係助詞,*,*,*,*,ハ,は,は,ワ,は,ワ,和,*,*,*,*,ハ,ハ,ハ,ハ,*,*,*,\"動詞%F2@0,名詞%F1,形容詞%F2@-1\",*",
                "形容詞,一般,*,*,形容詞,終止形-一般,オモシロイ,面白い,面白い,オモシロイ,面白い,オモシロイ,和,*,*,*,*,オモシロイ,オモシロイ,オモシロイ,オモシロイ,*,*,4,C1,*"
            };

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].Surface.Is(expectedSurfaces[i]);
                tokens[i].GetAllFeatures().Is(expectedFeatures[i]);
            }
        }

        [TestMethod]
        public void TestUnknownWord()
        {
            var tokens = Tokenizer.Tokenize("Google");
            var expectedFeatures = "名詞,普通名詞,一般,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*,*";

            tokens[0].GetAllFeatures().Is(expectedFeatures);
        }

        [TestMethod]
        public void TestUserDictionary()
        {
            var entries = "北斗の拳,北斗の拳,ホクトノケン,カスタム名詞";
            var tokenizer = BuildTokenizerWithUserDictionary(entries);

            var tokens = tokenizer.Tokenize("北斗の拳は非常に面白かった。");
            var expectedSurface = "北斗の拳";
            var expectedFeatures = "カスタム名詞,*,*,*,*,*,*,*,*,*,*,*,*,ホクトノケン,*,*,*,*,*,*,*,*,*,*,*,*";

            tokens[0].Surface.Is(expectedSurface);
            tokens[0].GetAllFeatures().Is(expectedFeatures);
        }

        [TestMethod]
        public void TestKansaiInternationalAirport()
        {
            var tokens = Tokenizer.Tokenize("関西国際空港");

            var expectedSurfaces = new string[] { "関西", "国際", "空港" };
            var expectedFeatures = new string[] {
                "名詞,固有名詞,地名,一般,*,*,カンサイ,カンサイ,関西,カンサイ,関西,カンサイ,固,*,*,*,*,カンサイ,カンサイ,カンサイ,カンサイ,*,*,1,*,*",
                "名詞,普通名詞,一般,*,*,*,コクサイ,国際,国際,コクサイ,国際,コクサイ,漢,*,*,*,*,コクサイ,コクサイ,コクサイ,コクサイ,*,*,0,C2,*",
                "名詞,普通名詞,一般,*,*,*,クウコウ,空港,空港,クーコー,空港,クーコー,漢,*,*,*,*,クウコウ,クウコウ,クウコウ,クウコウ,*,*,0,C2,*"
            };

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].Surface.Is(expectedSurfaces[i]);
                tokens[i].GetAllFeatures().Is(expectedFeatures[i]);
            }
        }

        [TestMethod]
        public void TestPosLevels()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var posLevel1 = new string[] { "接頭辞", "名詞", "助詞", "動詞", "助動詞" };
            var posLevel2 = new string[] { "*", "普通名詞", "格助詞", "一般", "*" };
            var posLevel3 = new string[] { "*", "一般", "*", "*", "*" };
            var posLevel4 = new string[] { "*", "*", "*", "*", "*" };

            tokens.Count.Is(posLevel1.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].PartOfSpeechLevel1.Is(posLevel1[i]);
                tokens[i].PartOfSpeechLevel2.Is(posLevel2[i]);
                tokens[i].PartOfSpeechLevel3.Is(posLevel3[i]);
                tokens[i].PartOfSpeechLevel4.Is(posLevel4[i]);
            }
        }

        [TestMethod]
        public void TestConjugationTypeAndForm()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedConjugationForms = new string[] { "*", "*", "*", "連用形-一般", "終止形-一般" };
            var expectedConjugationTypes = new string[] { "*", "*", "*", "下一段-バ行", "助動詞-タイ" };

            tokens.Count.Is(expectedConjugationForms.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].ConjugationForm.Is(expectedConjugationForms[i]);
                tokens[i].ConjugationType.Is(expectedConjugationTypes[i]);
            }
        }

        [TestMethod]
        public void TestLemmasAndLemmaReadings()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedReadingForms = new string[] { "オ", "スシ", "ガ", "タベル", "タイ" };
            var expectedLemmas = new string[] { "御", "寿司", "が", "食べる", "たい" };

            tokens.Count.Is(expectedLemmas.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].LemmaReadingForm.Is(expectedReadingForms[i]);
                tokens[i].Lemma.Is(expectedLemmas[i]);
            }
        }

        [TestMethod]
        public void TestWrittenFormsAndWrittenBaseForms()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedWrittenForms = new string[] { "お", "寿司", "が", "食べ", "たい" };
            var expectedWrittenBaseForms = new string[] { "お", "寿司", "が", "食べる", "たい" };

            tokens.Count.Is(expectedWrittenForms.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].WrittenForm.Is(expectedWrittenForms[i]);
                tokens[i].WrittenBaseForm.Is(expectedWrittenBaseForms[i]);
            }
        }

        [TestMethod]
        public void TestPronunciationAndPronunciationBaseForms()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedPronunciations = new string[] { "オ", "スシ", "ガ", "タベ", "タイ" };
            var expectedPronunciationBaseForms = new string[] { "オ", "スシ", "ガ", "タベル", "タイ" };

            tokens.Count.Is(expectedPronunciations.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].Pronunciation.Is(expectedPronunciations[i]);
                tokens[i].PronunciationBaseForm.Is(expectedPronunciationBaseForms[i]);
            }
        }

        [TestMethod]
        public void TestLanguageType()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedLanguageType = "和";

            foreach (var token in tokens)
            {
                token.LanguageType.Is(expectedLanguageType);
            }
        }

        [TestMethod]
        public void TestInitialSoundAlterationTypesAndForms()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedInitialSoundAlterationTypes = new string[] { "*", "ス濁", "*", "*", "*" };
            var expectedInitialSoundAlterationForms = new string[] { "*", "基本形", "*", "*", "*" };

            tokens.Count.Is(expectedInitialSoundAlterationTypes.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].InitialSoundAlterationType.Is(expectedInitialSoundAlterationTypes[i]);
                tokens[i].InitialSoundAlterationForm.Is(expectedInitialSoundAlterationForms[i]);
            }
        }

        [TestMethod]
        public void TestFinalSoundAlterationTypesAndForms()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedFinalSoundAlterationTypes = new string[] { "促添", "*", "*", "*", "*" };
            var expectedFinalSoundAlterationForms = new string[] { "基本形", "*", "*", "*", "*" };

            tokens.Count.Is(expectedFinalSoundAlterationTypes.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].FinalSoundAlterationType.Is(expectedFinalSoundAlterationTypes[i]);
                tokens[i].FinalSoundAlterationForm.Is(expectedFinalSoundAlterationForms[i]);
            }
        }

        [TestMethod]
        public void TestKanaAndKanaBaseAndFormAndFormBase()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            var expectedKana = new string[] { "オ", "スシ", "ガ", "タベ", "タイ" };
            var expectedKanaBase = new string[] { "オ", "スシ", "ガ", "タベル", "タイ" };
            var expectedForm = new string[] { "オ", "スシ", "ガ", "タベ", "タイ" };
            var expectedFormBase = new string[] { "オ", "スシ", "ガ", "タベル", "タイ" };

            tokens.Count.Is(expectedKana.Length);

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].Kana.Is(expectedKana[i]);
                tokens[i].KanaBase.Is(expectedKanaBase[i]);
                tokens[i].Form.Is(expectedForm[i]);
                tokens[i].FormBase.Is(expectedFormBase[i]);
            }
        }

        [TestMethod]
        public void TestConnectionTypes()
        {
            var tokens = Tokenizer.Tokenize("お寿司が食べたい");

            // Todo: Should have a more interesting test sample here
            var expectedInitialConnectionTypes = new string[] { "*", "*", "*", "*", "*" };
            var expectedFinalConnectionTypes = new string[] { "*", "*", "*", "*", "*" };

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].InitialConnectionType.Is(expectedInitialConnectionTypes[i]);
                tokens[i].FinalConnectionType.Is(expectedFinalConnectionTypes[i]);
            }
        }

        [TestMethod]
        public void AccentTypes()
        {
            var tokens = Tokenizer.Tokenize("お寿司を造ろう");

            var expectedAccentTypes = new string[] { "*", "1,2", "*", "2" };
            var expectedAccentConnectionTypes = new string[] { "P2", "C3", "動詞%F2@0,名詞%F1,形容詞%F2@-1", "C1" };
            var expectedAccentModificationTypes = new string[] { "*", "*", "*", "M1@1" };

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].AccentType.Is(expectedAccentTypes[i]);
                tokens[i].AccentConnectionType.Is(expectedAccentConnectionTypes[i]);
                tokens[i].AccentModificationType.Is(expectedAccentModificationTypes[i]);
            }
        }

        [TestMethod]
        public void TestFeatureLengths()
        {
            var userDictionary = "gsf,gsf,ジーエスーエフ,カスタム名詞\n";

            BuildTokenizerWithUserDictionary(userDictionary)
                .IsSameTokenFeatureLengths("ahgsfdajhgsfdこの丘はアクロポリスと呼ばれている。");
        }

        [TestMethod]
        public void TestNewBocchan()
        {
            Tokenizer.IsSameTokenizedStream("Input\\UniDicKanaAcent\\bocchan-unidic-kanaaccent-features.txt", "Input\\UniDicKanaAcent\\bocchan.txt");
        }

        [TestMethod]
        public void TestQuotedFeature()
        {
            var tokens = Tokenizer.Tokenize("合い方");

            tokens.Count.Is(1);

            var token = tokens[0];
            token.GetAllFeatures().Is("名詞,普通名詞,一般,*,*,*,アイカタ,合方,合い方,アイカタ,合い方,アイカタ,和,*,*,*,*,アイカタ,アイカタ,アイカタ,アイカタ,*,*,\"0,4\",C2,*");
            token.AccentType.Is("0,4");
        }

        [TestMethod]
        public void TestPunctuation()
        {
            CommonCornerCasesTest.TestPunctuation(new Tokenizer(DicFileName));
        }

        Tokenizer BuildTokenizerWithUserDictionary(string userDictionaryEntry)
        {
            using (var ms = GetUserDictionaryFromString(userDictionaryEntry))
            {
                return new Tokenizer.Builder(DicFileName).SetUserDictionary(GetUserDictionaryFromString(userDictionaryEntry)).Build() as Tokenizer;
            }
        }

        Stream GetUserDictionaryFromString(string userDictionaryEntry)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(userDictionaryEntry));
        }
    }
}
