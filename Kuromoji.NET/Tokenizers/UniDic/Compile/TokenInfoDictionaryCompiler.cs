using Kuromoji.NET.Compile;
using Kuromoji.NET.Dict;
using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Tokenizers.UniDic.Compile
{
    public class TokenInfoDictionaryCompiler : TokenInfoDictionaryCompilerBase<DictionaryEntry>
    {
        public TokenInfoDictionaryCompiler(Encoding encoding) : base(encoding) { }

        protected override GenericDictionaryEntry MakeGenericDictionaryEntry(DictionaryEntry entry)
        {
            var pos = MakePartOfSpeechFeatures(entry);
            var features = MakeOtherFeatures(entry);

            return new GenericDictionaryEntry.Builder()
                .SetSurface(entry.Surface)
                .SetLeftId(entry.LeftId)
                .SetRightId(entry.RightId)
                .SetWordCost(entry.WordCost)
                .SetPartOfSpeech(pos)
                .SetFeatures(features)
                .Build();
        }

        protected override DictionaryEntry Parse(string line)
        {
            return new DictionaryEntry(DictionaryEntryLineParser.ParseLine(line));
        }

        public List<string> MakePartOfSpeechFeatures(DictionaryEntry entry)
        {
            return new List<string>
            {
                entry.PartOfSpeechLevel1,
                entry.PartOfSpeechLevel2,
                entry.PartOfSpeechLevel3,
                entry.PartOfSpeechLevel4,
                entry.ConjugationType,
                entry.ConjugationForm
            };
        }

        public List<string> MakeOtherFeatures(DictionaryEntry entry)
        {
            return new List<string>
            {
                entry.LemmaReadingForm,
                entry.Lemma,
                entry.WrittenForm,
                entry.Pronunciation,
                entry.WrittenBaseForm,
                entry.PronunciationBaseForm,
                entry.LanguageType,
                entry.InitialSoundAlterationType,
                entry.InitialSoundAlterationForm,
                entry.FinalSoundAlterationType,
                entry.FinalSoundAlterationForm
            };
        }
    }
}
