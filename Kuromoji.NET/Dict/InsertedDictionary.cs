using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public class InsertedDictionary : IDictionary
    {
        const string DefaultFeature = "*";

        const string FeatureSeparator = ",";

        string[] FeaturesArray { get; }

        string FeaturesString { get; }

        public InsertedDictionary(int features)
        {
            FeaturesArray = Enumerable.Repeat(DefaultFeature, features).ToArray();
            FeaturesString = string.Join(FeatureSeparator, FeaturesArray);
        }

        public string GetAllFeatures(int wordId) => FeaturesString;

        public string[] GetAllFeaturesArray(int wordId) => FeaturesArray;

        public string GetFeature(int wordId, params int[] fields)
        {
            return string.Join(FeatureSeparator, fields.Select(i => FeaturesArray[i]));
        }

        public int GetLeftId(int wordId) => 0;

        public int GetRightId(int wordId) => 0;

        public int GetWordCost(int wordId) => 0;
    }
}
