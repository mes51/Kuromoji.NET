using System;
using System.IO;
using System.Text;
using Kuromoji.NET.Tokenizers.UniDicNEologd;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kuromoji.NET.Test.Tokenizers.UniDicNEologd
{
    [TestClass]
    [DeploymentItem("Resources\\UniDicNEologd\\unidic-neologd.zip", "Dict")]
    [DeploymentItem("Resources\\UniDicNEologd\\bocchan-unidic-neologd-features.txt", "Input\\UniDicNEologd")]
    [DeploymentItem("Resources\\UniDicNEologd\\bocchan.txt", "Input\\UniDicNEologd")]
    public class TokenizerTest
    {
        const string DicFileName = @"Dict\unidic-neologd.zip";

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

            var surfaces = new string[] { "スペースステーション", "に", "行き", "ます", "。", "うたがわしい", "。" };
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
            var tokens = Tokenizer.Tokenize("ヴィ");
            var expectedFeatures = "記号,一般,*,*,*,*,ヴィ,ヴィ,ヴィ,ヴィ,ヴィ,ヴィ,記号,*,*,*,*";

            tokens[0].GetAllFeatures().Is(expectedFeatures);
        }

        [TestMethod]
        public void TestKansaiInternationalAirport()
        {
            var tokens = Tokenizer.Tokenize("関西国際空港");
            var expectedSurfaces = new string[] { "関西国際空港" };
            var expectedFeatures = new string[] { "名詞,固有名詞,一般,*,*,*,カンサイコクサイクウコウ,関西国際空港,関西国際空港,カンサイコクサイクウコー,関西国際空港,カンサイコクサイクウコー,固,*,*,*,*" };

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].Surface.Is(expectedSurfaces[i]);
                tokens[i].GetAllFeatures().Is(expectedFeatures[i]);
            }
        }

        [TestMethod]
        public void TestNucleus()
        {
            var tokens = Tokenizer.Tokenize("核");

            var expectedSurface = "核";
            var expectedFeatures = "名詞,固有名詞,人名,姓,*,*,サネ,核,核,サネ,核,サネ,固,*,*,*,*";

            tokens[0].Surface.Is(expectedSurface);
            tokens[0].GetAllFeatures().Is(expectedFeatures);
        }

        [TestMethod]
        public void TestEmoji()
        {
            var tokens = Tokenizer.Tokenize("Σ（゜□゜）");
            var expectedFeatures = "補助記号,ＡＡ,顔文字,*,*,*,,Σ（゜□゜）,Σ（゜□゜）,,Σ（゜□゜）,,記号,*,*,*,*";

            tokens[0].GetAllFeatures().Is(expectedFeatures);
        }

        [TestMethod]
        public void TestKakeyo()
        {
            var tokens = Tokenizer.Tokenize("掛けよう");
            var expectedFeatures = "動詞,非自立可能,*,*,下一段-カ行,意志推量形,カケル,掛ける,掛けよう,カケヨー,掛ける,カケル,和,カ濁,基本形,*,*";

            tokens[0].GetAllFeatures().Is(expectedFeatures);
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

            for (var i = 0; i < tokens.Count; i++)
            {
                tokens[i].LanguageType.Is(expectedLanguageType);
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
        public void TestFeatureLengths()
        {
            var userDictionary = "gsf,gsf,ジーエスーエフ,カスタム名詞\n";

            BuildTokenizerWithUserDictionary(userDictionary)
                .IsSameTokenFeatureLengths("ahgsfdajhgsfdこの丘はアクロポリスと呼ばれている。");
        }

        [TestMethod]
        public void TestNewBocchan()
        {
            Tokenizer.IsSameTokenizedStream("Input\\UniDicNEologd\\bocchan-unidic-neologd-features.txt", "Input\\UniDicNEologd\\bocchan.txt");
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
