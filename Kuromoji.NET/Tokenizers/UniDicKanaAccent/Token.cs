using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Dict;
using Kuromoji.NET.Tokenizers.UniDicKanaAccent.Compile;
using Kuromoji.NET.Viterbi;

namespace Kuromoji.NET.Tokenizers.UniDicKanaAccent
{
    public class Token : TokenBase
    {
        /// <summary>
        /// Gets the 1st level part-of-speech tag for this token (品詞細分類1)
        /// </summary>
        public string PartOfSpeechLevel1 => GetFeature(DictionaryEntry.IndexPartOfSpeechLevel1);

        /// <summary>
        /// Gets the 1st level part-of-speech tag for this token (品詞細分類2)
        /// </summary>
        public string PartOfSpeechLevel2 => GetFeature(DictionaryEntry.IndexPartOfSpeechLevel2);

        /// <summary>
        /// Gets the 1st level part-of-speech tag for this token (品詞細分類3)
        /// </summary>
        public string PartOfSpeechLevel3 => GetFeature(DictionaryEntry.IndexPartOfSpeechLevel3);

        /// <summary>
        /// Gets the 1st level part-of-speech tag for this token (品詞細分類4)
        /// </summary>
        public string PartOfSpeechLevel4 => GetFeature(DictionaryEntry.IndexPartOfSpeechLevel4);

        /// <summary>
        /// Gets the conjugation form for this token (活用形), if applicable
        /// 
        /// If this token does not have a conjugation form, return *
        /// </summary>
        public string ConjugationForm => GetFeature(DictionaryEntry.IndexConjugationForm);

        /// <summary>
        /// Gets the conjugation type for this token (活用型), if applicable
        /// 
        /// If this token does not have a conjugation type, return *
        /// </summary>
        public string ConjugationType => GetFeature(DictionaryEntry.IndexConjugationType);

        /// <summary>
        /// Return the lemma reading form for this token (語彙素読み)
        /// </summary>
        public string LemmaReadingForm => GetFeature(DictionaryEntry.IndexLemmaReadingForm);

        /// <summary>
        /// Gets the lemma for this token (語彙素表記)
        /// </summary>
        public string Lemma => GetFeature(DictionaryEntry.IndexLemma);

        /// <summary>
        /// Gets the pronunciation for this token (発音)
        /// </summary>
        public string Pronunciation => GetFeature(DictionaryEntry.IndexPronunciation);

        /// <summary>
        /// Gets the pronunciation base form for this token (発音形基本形)
        /// </summary>
        public string PronunciationBaseForm => GetFeature(DictionaryEntry.IndexPronunciationBaseForm);

        /// <summary>
        /// Gets the written form for this token (書字形)
        /// </summary>
        public string WrittenForm => GetFeature(DictionaryEntry.IndexWrittenForm);

        /// <summary>
        /// Gets the written base form of this token (書字形出現形)
        /// </summary>
        public string WrittenBaseForm => GetFeature(DictionaryEntry.IndexWrittenBaseForm);

        /// <summary>
        /// Returns the language type of this token (語種)
        /// </summary>
        public string LanguageType => GetFeature(DictionaryEntry.IndexLanguageType);

        /// <summary>
        /// Returns the initial sound alteration type for the token (語頭変化型)
        /// </summary>
        public string InitialSoundAlterationType => GetFeature(DictionaryEntry.IndexInitialSoundAlterationType);

        /// <summary>
        /// Returns the initial sound alteration form for the token (語頭変化形)
        /// </summary>
        public string InitialSoundAlterationForm => GetFeature(DictionaryEntry.IndexInitialSoundAlterationForm);

        /// <summary>
        /// Returns the final sound alteration type for the token (語末変化型)
        /// </summary>
        public string FinalSoundAlterationType => GetFeature(DictionaryEntry.IndexFinalSoundAlterationType);

        /// <summary>
        /// Returns the final sound alteration form for the token (語末変化形)
        /// </summary>
        public string FinalSoundAlterationForm => GetFeature(DictionaryEntry.IndexFinalSoundAlterationForm);

        /// <summary>
        /// Return the kana version of this token (仮名形出現形)
        /// </summary>
        public string Kana => GetFeature(DictionaryEntry.IndexKana);

        /// <summary>
        /// Return the kana base form of this token (仮名形基本形)
        /// </summary>
        public string KanaBase => GetFeature(DictionaryEntry.IndexKanaBase);

        /// <summary>
        /// Gets the form of this token (語末変化形)
        /// </summary>
        public string Form => GetFeature(DictionaryEntry.IndexForm);

        /// <summary>
        /// Gets the form base of this token (語形基本形)
        /// </summary>
        public string FormBase => GetFeature(DictionaryEntry.IndexFormBase);

        /// <summary>
        /// Gets the initial connection type of this token (語頭変化結合形)
        /// </summary>
        public string InitialConnectionType => GetFeature(DictionaryEntry.IndexInitialConnectionType);

        /// <summary>
        /// Gets the final connection type of this token (語末変化結合形)
        /// </summary>
        public string FinalConnectionType => GetFeature(DictionaryEntry.IndexFinalConnectionType);

        /// <summary>
        /// Gets the accent type of this token (アクセント型)
        /// </summary>
        public string AccentType => GetFeature(DictionaryEntry.IndexAccentType);

        /// <summary>
        /// Gets the accent connection type of this token (アクセント結合型)
        /// </summary>
        public string AccentConnectionType => GetFeature(DictionaryEntry.IndexAccentConnectionType);

        /// <summary>
        /// Return the accent modification type of this token (アクセント修飾型)
        /// </summary>
        public string AccentModificationType => GetFeature(DictionaryEntry.IndexAccentModificationType);

        public Token(int wordId, string surface, ViterbiNode.NodeType type, int position, IDictionary dictionary)
            : base(wordId, surface, type, position, dictionary) { }
    }
}
