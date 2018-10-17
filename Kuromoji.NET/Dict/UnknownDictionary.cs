using Kuromoji.NET.IO;
using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public class UnknownDictionary : IDictionary
    {
        public const string UnknownDictionaryFileName = "unknownDictionary.bin";

        const string DefaultFeature = "*";

        const string FeatureSeparator = ",";

        int[][] Entries { get; }

        int[][] Costs { get; }

        string[][] Features { get; }

        int TotalFeatures { get; }

        public CharacterDefinitions CharacterDefinition { get; }

        public UnknownDictionary(CharacterDefinitions characterDefinition, int[][] entries, int[][] costs, string[][] features, int totalFeatures)
        {
            CharacterDefinition = characterDefinition;
            Entries = entries;
            Costs = costs;
            Features = features;
            TotalFeatures = totalFeatures;
        }

        public UnknownDictionary(CharacterDefinitions characterDefinition, int[][] entries, int[][] costs, string[][] features)
            : this(characterDefinition, entries, costs, features, features.Length) { }

        public int[] LookupWordIds(int categoryId)
        {
            return Entries[categoryId];
        }

        public string GetAllFeatures(int wordId)
        {
            return string.Join(FeatureSeparator, GetAllFeaturesArray(wordId));
        }

        public string[] GetAllFeaturesArray(int wordId)
        {
            if (TotalFeatures == Features.Length)
            {
                return Features[wordId];
            }

            var allFeatures = Enumerable.Repeat(DefaultFeature, TotalFeatures).ToArray();
            var basicFeatures = Features[wordId];
            Array.Copy(basicFeatures, allFeatures, basicFeatures.Length);

            return allFeatures;
        }

        public string GetFeature(int wordId, params int[] fields)
        {
            var allFeautres = GetAllFeaturesArray(wordId);

            return string.Join(FeatureSeparator, fields.Select(i => allFeautres[i]));
        }

        public int GetLeftId(int wordId)
        {
            return Costs[wordId][0];
        }

        public int GetRightId(int wordId)
        {
            return Costs[wordId][1];
        }

        public int GetWordCost(int wordId)
        {
            return Costs[wordId][2];
        }

        public static UnknownDictionary NewInstance(IResourceResolver resolver, CharacterDefinitions characterDefinitions, int totalFeatures)
        {
            using (var unkDefInput = resolver.Resolve(UnknownDictionaryFileName))
            {
                var costs = IntArrayIO.ReadArray2D(unkDefInput);
                var references = IntArrayIO.ReadArray2D(unkDefInput);
                var features = StringArrayIO.ReadArray2D(unkDefInput);

                return new UnknownDictionary(
                    characterDefinitions,
                    references,
                    costs,
                    features,
                    totalFeatures
                );
            }
        }
    }
}
