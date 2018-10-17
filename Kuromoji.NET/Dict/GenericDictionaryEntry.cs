using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public class GenericDictionaryEntry : DictionaryEntryBase
    {
        public string[] PartOfSpeechFeatures { get; }

        public string[] OtherFeatures { get; }

        public GenericDictionaryEntry(Builder builder) : base(builder.Surface, builder.LeftId, builder.RightId, builder.WordCost)
        {
            PartOfSpeechFeatures = builder.PartOfSpeechFeatures;
            OtherFeatures = builder.OtherFeatures;
        }

        public class Builder
        {
            public string Surface { get; private set; }

            public short LeftId { get; private set; }

            public short RightId { get; private set; }

            public short WordCost { get; private set; }

            public string[] PartOfSpeechFeatures { get; private set; }

            public string[] OtherFeatures { get; private set; }

            public Builder SetSurface(string surface)
            {
                Surface = surface;
                return this;
            }

            public Builder SetLeftId(short leftId)
            {
                LeftId = leftId;
                return this;
            }

            public Builder SetRightId(short rightId)
            {
                RightId = rightId;
                return this;
            }

            public Builder SetWordCost(short wordCost)
            {
                WordCost = wordCost;
                return this;
            }

            public Builder SetPartOfSpeech(IEnumerable<string> pos)
            {
                PartOfSpeechFeatures = pos.ToArray();
                return this;
            }

            public Builder SetFeatures(IEnumerable<string> features)
            {
                OtherFeatures = features.ToArray();
                return this;
            }

            public GenericDictionaryEntry Build()
            {
                return new GenericDictionaryEntry(this);
            }
        }
    }
}
