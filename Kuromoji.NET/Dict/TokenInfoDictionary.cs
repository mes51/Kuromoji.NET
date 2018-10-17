using Kuromoji.NET.Buffer;
using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public class TokenInfoDictionary : IDictionary
    {
        public const string TokenInfoDictionaryFileName = "tokenInfoDictionary.bin";

        public const string FeatureMapFileName = "tokenInfoFeaturesMap.bin";

        public const string PosMapFileName = "tokenInfoPartOfSpeechMap.bin";

        public const string TargetMapFileName = "tokenInfoTargetMap.bin";

        const int LeftId = 0;

        const int RightId = 1;

        const int WordCost = 2;

        const int TokenInfoOffset = 3;

        const string FeatureSeparator = ",";

        TokenInfoBuffer TokenInfoBuffer { get; }

        StringValueMapBuffer PosValues { get; }

        StringValueMapBuffer StringValues { get; }

        WordIdMap WordIdMap { get; }

        private TokenInfoDictionary(IResourceResolver resolver)
        {
            using (var tokenInfoBufferResource = resolver.Resolve(TokenInfoDictionaryFileName))
            using (var stringValueResource = resolver.Resolve(FeatureMapFileName))
            using (var posValueResource = resolver.Resolve(PosMapFileName))
            using (var wordIdMapResource = resolver.Resolve(TargetMapFileName))
            {
                TokenInfoBuffer = new TokenInfoBuffer(tokenInfoBufferResource);
                StringValues = new StringValueMapBuffer(stringValueResource);
                PosValues = new StringValueMapBuffer(posValueResource);
                WordIdMap = new WordIdMap(wordIdMapResource);
            }
        }

        public int[] LookupWordIds(int sourceId)
        {
            return WordIdMap.LookUp(sourceId);
        }

        public string GetAllFeatures(int wordId)
        {
            return string.Join(FeatureSeparator, GetAllFeaturesArray(wordId).Select(DictionaryEntryLineParser.Escape));
        }

        public string[] GetAllFeaturesArray(int wordId)
        {
            var bufferEntry = TokenInfoBuffer.LookupEntry(wordId);

            var posLength = bufferEntry.PosInfos.Length;
            var featureLength = bufferEntry.FeatureInfos.Length;

            var partOfSpeechAsShorts = false;

            if (posLength == 0)
            {
                posLength = bufferEntry.TokenInfos.Length - TokenInfoOffset;
                partOfSpeechAsShorts = true;
            }

            var result = new string[posLength + featureLength];

            if (partOfSpeechAsShorts)
            {
                for (var i = 0; i < posLength; i++)
                {
                    var feature = bufferEntry.TokenInfos[i + TokenInfoOffset];
                    result[i] = PosValues[feature];
                }
            }
            else
            {
                for (var i = 0; i < posLength; i++)
                {
                    var feature = bufferEntry.PosInfos[i] & 0xff;
                    result[i] = PosValues[feature];
                }
            }

            for (var i = 0; i < featureLength; i++)
            {
                var feature = bufferEntry.FeatureInfos[i];
                var s = StringValues[feature];
                result[i + posLength] = s;
            }

            return result;
        }

        public string GetFeature(int wordId, params int[] fields)
        {
            if (fields.Length == 1)
            {
                return ExtractSingleFeature(wordId, fields[0]);
            }
            else
            {
                return ExtractMultipleFeatures(wordId, fields);
            }
        }

        public int GetLeftId(int wordId)
        {
            return TokenInfoBuffer.LookupTokenInfo(wordId, LeftId);
        }

        public int GetRightId(int wordId)
        {
            return TokenInfoBuffer.LookupTokenInfo(wordId, RightId);
        }

        public int GetWordCost(int wordId)
        {
            return TokenInfoBuffer.LookupTokenInfo(wordId, WordCost);
        }

        string ExtractSingleFeature(int wordId, int field)
        {
            if (TokenInfoBuffer.isPartOfSpeechFeature(field))
            {
                var featureId = TokenInfoBuffer.LookupPartOfSpeechFeature(wordId, field);
                return PosValues[featureId];
            }
            else
            {
                var featureId = TokenInfoBuffer.LookupFeature(wordId, field);
                return StringValues[featureId];
            }
        }

        string ExtractMultipleFeatures(int wordId, int[] fields)
        {
            if (fields.Length == 0)
            {
                return GetAllFeatures(wordId);
            }

            if (fields.Length == 1)
            {
                return ExtractSingleFeature(wordId, fields[0]);
            }

            var allFeatures = GetAllFeaturesArray(wordId);

            return string.Join(FeatureSeparator, fields.Select(i => allFeatures[i]).Select(DictionaryEntryLineParser.Escape));
        }

        public static TokenInfoDictionary NewInstance(IResourceResolver resolver)
        {
            return new TokenInfoDictionary(resolver);
        }
    }
}
