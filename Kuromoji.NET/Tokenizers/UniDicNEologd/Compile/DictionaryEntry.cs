using Kuromoji.NET.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Tokenizers.UniDicNEologd.Compile
{
    public class DictionaryEntry : DictionaryEntryBase
    {
        public const int IndexPartOfSpeechLevel1 = 4;
        public const int IndexPartOfSpeechLevel2 = 5;
        public const int IndexPartOfSpeechLevel3 = 6;
        public const int IndexPartOfSpeechLevel4 = 7;
        public const int IndexConjugationType = 8;
        public const int IndexConjugationForm = 9;
        public const int IndexLemmaReadingForm = 10;
        public const int IndexLemma = 11;
        public const int IndexWrittenForm = 12;
        public const int IndexPronunciation = 13;
        public const int IndexWrittenBaseForm = 14;
        public const int IndexPronunciationBaseForm = 15;
        public const int IndexLanguageType = 16;
        public const int IndexInitialSoundAlterationType = 17;
        public const int IndexInitialSoundAlterationForm = 18;
        public const int IndexFinalSoundAlterationType = 19;
        public const int IndexFinalSoundAlterationForm = 20;
        public const int IndexTotalFeatures = 17;
        public const int IndexReadingFeature = 7;
        public const int IndexPartOfSpeechFeature = 0;


        public string PartOfSpeechLevel1 { get; }
        public string PartOfSpeechLevel2 { get; }
        public string PartOfSpeechLevel3 { get; }
        public string PartOfSpeechLevel4 { get; }
        public string ConjugationType { get; }
        public string ConjugationForm { get; }
        public string LemmaReadingForm { get; }
        public string Lemma { get; }
        public string WrittenForm { get; }
        public string Pronunciation { get; }
        public string WrittenBaseForm { get; }
        public string PronunciationBaseForm { get; }
        public string LanguageType { get; }
        public string InitialSoundAlterationType { get; }
        public string InitialSoundAlterationForm { get; }
        public string FinalSoundAlterationType { get; }
        public string FinalSoundAlterationForm { get; }
        public string TotalFeatures { get; }
        public string ReadingFeature { get; }

        public DictionaryEntry(string[] fields)
            : base(
                  fields[(int)DictionaryField.Surface],
                  short.Parse(fields[(int)DictionaryField.LeftId]),
                  short.Parse(fields[(int)DictionaryField.RightId]),
                  int.Parse(fields[(int)DictionaryField.WordCost])
              )
        {
            PartOfSpeechLevel1 = fields[IndexPartOfSpeechLevel1];
            PartOfSpeechLevel2 = fields[IndexPartOfSpeechLevel2];
            PartOfSpeechLevel3 = fields[IndexPartOfSpeechLevel3];
            PartOfSpeechLevel4 = fields[IndexPartOfSpeechLevel4];
            ConjugationType = fields[IndexConjugationType];
            ConjugationForm = fields[IndexConjugationForm];
            LemmaReadingForm = fields[IndexLemmaReadingForm];
            Lemma = fields[IndexLemma];
            WrittenForm = fields[IndexWrittenForm];
            Pronunciation = fields[IndexPronunciation];
            WrittenBaseForm = fields[IndexWrittenBaseForm];
            PronunciationBaseForm = fields[IndexPronunciationBaseForm];
            LanguageType = fields[IndexLanguageType];
            InitialSoundAlterationType = fields[IndexInitialSoundAlterationType];
            InitialSoundAlterationForm = fields[IndexInitialSoundAlterationForm];
            FinalSoundAlterationType = fields[IndexFinalSoundAlterationType];
            FinalSoundAlterationForm = fields[IndexFinalSoundAlterationForm];
            TotalFeatures = fields[IndexTotalFeatures];
            ReadingFeature = fields[IndexReadingFeature];
        }
    }
}
