using Kuromoji.NET.Compile;
using Kuromoji.NET.Dict;
using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Tokenizers.UniDicKanaAccent.Compile
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

        public List<string> MakePartOfSpeechFeatures(DictionaryEntry entry)
        {
            var posFeatures = new List<string>();

            posFeatures.Add(entry.PartOfSpeechLevel1);
            posFeatures.Add(entry.PartOfSpeechLevel2);
            posFeatures.Add(entry.PartOfSpeechLevel3);
            posFeatures.Add(entry.PartOfSpeechLevel4);

            posFeatures.Add(entry.ConjugationType);
            posFeatures.Add(entry.ConjugationForm);

            return posFeatures;
        }

        public List<string> MakeOtherFeatures(DictionaryEntry entry)
        {
            var otherFeatures = new List<string>();

            otherFeatures.Add(entry.LemmaReadingForm);
            otherFeatures.Add(entry.Lemma);

            otherFeatures.Add(entry.WrittenForm);
            otherFeatures.Add(entry.Pronunciation);
            otherFeatures.Add(entry.WrittenBaseForm);
            otherFeatures.Add(entry.PronunciationBaseForm);

            otherFeatures.Add(entry.LanguageType);
            otherFeatures.Add(entry.InitialSoundAlterationType);
            otherFeatures.Add(entry.InitialSoundAlterationForm);
            otherFeatures.Add(entry.FinalSoundAlterationType);
            otherFeatures.Add(entry.FinalSoundAlterationForm);

            otherFeatures.Add(entry.Kana);
            otherFeatures.Add(entry.KanaBase);
            otherFeatures.Add(entry.Form);
            otherFeatures.Add(entry.FormBase);
            otherFeatures.Add(entry.InitialConnectionType);
            otherFeatures.Add(entry.FinalConnectionType);

            otherFeatures.Add(entry.AccentType);
            otherFeatures.Add(entry.AccentConnectionType);
            otherFeatures.Add(entry.AccentModificationType);

            return otherFeatures;
        }

        protected override DictionaryEntry Parse(string line)
        {
            var fields = DictionaryEntryLineParser.ParseLine(line);
            return new DictionaryEntry(fields);
        }
    }
}
