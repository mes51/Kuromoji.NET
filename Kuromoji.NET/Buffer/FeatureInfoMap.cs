using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;

namespace Kuromoji.NET.Buffer
{
    public class FeatureInfoMap
    {
        Dictionary<string, int> FeatureMap { get; } = new Dictionary<string, int>();

        public int EntryCount { get; private set; }

        public List<int> MapFeatures(string[] features)
        {
            var posFeatureIds = new List<int>();
            foreach (var feature in features)
            {
                if (FeatureMap.ContainsKey(feature))
                {
                    posFeatureIds.Add(FeatureMap[feature]);
                }
                else
                {
                    FeatureMap.Add(feature, EntryCount);
                    posFeatureIds.Add(EntryCount);
                    EntryCount++;
                }
            }
            return posFeatureIds;
        }

        public SortedDictionary<int, string> Invert()
        {
            var features = new SortedDictionary<int, string>();

            foreach (var (key, value) in FeatureMap)
            {
                features.Add(value, key);
            }

            return features;
        }

        public override string ToString()
        {
            return $"FeatureInfoMap{{featureMap={FeatureMap}, maxValue={EntryCount}}}";
        }
    }
}
