using Kuromoji.NET.Trie;
using Kuromoji.NET.Util;
using Kuromoji.NET.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public class UserDictionary : IDictionary
    {
        const int SimpleUserDictFields = 4;

        const int WordCostBase = -100000;

        const int MinimumWordCost = int.MinValue / 2;

        const int LeftId = 5;

        const int RightId = 5;

        const string DefaultFeature = "*";

        const string FeatureSeparator = ",";

        List<UserDictionaryEntry> Entries { get; } = new List<UserDictionaryEntry>();

        int ReadingFeature { get; }

        int PartOfSpeechFeature { get; }

        int TotalFeatures { get; }

        /// <summary>
        /// The word id below is the word id for the source string
        /// surface string => [ word id, 1st token length, 2nd token length, ... , nth token length
        /// </summary>
        PatriciaTrie<int[]> Surfaces { get; } = new PatriciaTrie<int[]>();

        public UserDictionary(Stream input, int totalFeatures, int readingFeature, int partOfSpeechFeature)
        {
            TotalFeatures = totalFeatures;
            ReadingFeature = readingFeature;
            PartOfSpeechFeature = partOfSpeechFeature;
            Read(input);
        }

        public List<UserDictionaryMatch> FindUserDictionaryMatches(string text)
        {
            var matchInfos = new List<UserDictionaryMatch>();
            var startIndex = 0;

            while (startIndex < text.Length)
            {
                var matchLength = 0;
                var endIndex = 0;

                while (CurrentInputContainsPotentialMatch(text, startIndex, endIndex))
                {
                    var matchCandidate = text.Substring(startIndex, endIndex);
                    if (Surfaces.ContainsKey(matchCandidate))
                    {
                        matchLength = endIndex;
                    }

                    endIndex++;
                }

                if (matchLength > 0)
                {
                    var match = text.Substring(startIndex, matchLength);
                    if (Surfaces.TryGetValue(match, out int[] details) && details != null)
                    {
                        matchInfos.AddRange(MakeMatchDetails(startIndex, details));
                    }
                }

                startIndex++;
            }

            return matchInfos;
        }

        public string GetAllFeatures(int wordId)
        {
            return Entries[wordId].GetAllFeatures();
        }

        public string[] GetAllFeaturesArray(int wordId)
        {
            return Entries[wordId].Features;
        }

        public string GetFeature(int wordId, params int[] fields)
        {
            return Entries[wordId].GetFeature(fields);
        }

        public int GetLeftId(int wordId)
        {
            return Entries[wordId].LeftId;
        }

        public int GetRightId(int wordId)
        {
            return Entries[wordId].RightId;
        }

        public int GetWordCost(int wordId)
        {
            return Entries[wordId].WordCost;
        }

        bool CurrentInputContainsPotentialMatch(string text, int startIndex, int endIndex)
        {
            return startIndex + endIndex <= text.Length && Surfaces.ContainsKeyPrefix(text.Substring(startIndex, endIndex));
        }

        List<UserDictionaryMatch> MakeMatchDetails(int matchStartIndex, int[] details)
        {
            var matchDetails = new List<UserDictionaryMatch>(details.Length - 1);

            var wordId = details[0];
            var startIndex = 0;

            for (var i = 1; i < details.Length; i++)
            {
                var matchLength = details[i];

                matchDetails.Add(new UserDictionaryMatch(wordId, matchStartIndex + startIndex, matchLength));

                startIndex += matchLength;
                wordId++;
            }

            return matchDetails;
        }

        void Read(Stream input)
        {
            foreach (var l in input.ReadLines(Encoding.UTF8))
            {
                // Remove comments and trim leading and trailing whitespace
                var line = Regexs.Comment.Replace(l, "");
                line = line.Trim();

                if (!string.IsNullOrEmpty(line))
                {
                    AddEntry(line);
                }
            }
        }

        void AddEntry(string entry)
        {
            var values = DictionaryEntryLineParser.ParseLine(entry);

            if (values.Length == SimpleUserDictFields)
            {
                AddSimpleEntry(values);
            }
            else if (values.Length == TotalFeatures + 4) // 4 = surface, left id, right id, word cost
            {
                AddFullEntry(values);
            }
            else
            {
                throw new ArgumentException("Illegal user dictionary entry " + entry);
            }
        }

        void AddFullEntry(string[] values)
        {
            var surface = values[0];
            var costs = new int[]
            {
                int.Parse(values[1]),
                int.Parse(values[2]),
                int.Parse(values[3])
            };

            var features = values.Skip(4).ToArray();

            var entry = new UserDictionaryEntry(surface, costs, features);

            var wordIdAndLengths = new int[1 + 1]; // Surface and a single length - the length of surface
            wordIdAndLengths[0] = Entries.Count;
            wordIdAndLengths[1] = surface.Length;

            Entries.Add(entry);

            Surfaces.Add(surface, wordIdAndLengths);
        }

        void AddSimpleEntry(string[] values)
        {
            var surface = values[0];
            var segmentationValue = values[1];
            var readingsValue = values[2];
            var partOfSpeech = values[3];

            var segmentation = new string[] { segmentationValue };
            var readings = new string[] { readingsValue };

            if (IsCustomSegmentation(surface, segmentationValue))
            {
                segmentation = Regexs.CustomSegmentationSpliter.Split(segmentationValue);
                readings = Regexs.CustomSegmentationSpliter.Split(readingsValue);
            }

            if (segmentation.Length != readings.Length)
            {
                throw new ArgumentException("User dictionary entry not properly formatted: " + string.Join(",", values));
            }

            // { wordId, 1st token length, 2nd token length, ... , nth token length
            var wordIdAndLengths = new int[segmentation.Length + 1];

            var wordId = Entries.Count;
            wordIdAndLengths[0] = wordId;

            for (var i = 0; i < segmentation.Length; i++)
            {
                wordIdAndLengths[i + 1] = segmentation[i].Length;
                var features = MakeSimpleFeatures(partOfSpeech, readings[i]);
                var costs = MakeCosts(surface.Length);

                var entry = new UserDictionaryEntry(segmentation[i], costs, features);
                Entries.Add(entry);
            }

            Surfaces.Add(surface, wordIdAndLengths);
        }

        int[] MakeCosts(int length)
        {
            var wordCost = Math.Max(WordCostBase * length, MinimumWordCost);

            return new int[] { LeftId, RightId, wordCost };
        }

        string[] MakeSimpleFeatures(string partOfSpeech, string reading)
        {
            var features = EmptyFeatureArray();
            features[PartOfSpeechFeature] = partOfSpeech;
            features[ReadingFeature] = reading;
            return features;
        }

        string[] EmptyFeatureArray()
        {
            return Enumerable.Repeat(DefaultFeature, TotalFeatures).ToArray();
        }

        bool IsCustomSegmentation(string surface, string segmentation)
        {
            return surface != segmentation;
        }

        public class UserDictionaryMatch
        {
            public int WordId { get; }

            public int MatchStartIndex { get; }

            public int MatchLength { get; }

            public UserDictionaryMatch(int wordId, int matchStartIndex, int matchLength)
            {
                WordId = wordId;
                MatchStartIndex = matchStartIndex;
                MatchLength = matchLength;
            }

            public override string ToString()
            {
                return $"UserDictionaryMatch{{wordId={WordId}, matchStartIndex={MatchStartIndex}, matchLength={MatchLength}}}";
            }
        }

        class UserDictionaryEntry
        {
            public string Surface { get; }

            public int[] Costs { get; }

            public string[] Features { get; }

            public int LeftId => Costs[0];

            public int RightId => Costs[1];

            public int WordCost => Costs[2];

            public UserDictionaryEntry(string surface, int[] costs, string[] features)
            {
                Surface = surface;
                Costs = costs;
                Features = features;
            }

            public string GetAllFeatures()
            {
                return string.Join(FeatureSeparator, Features);
            }

            public string GetFeature(params int[] fields)
            {
                return string.Join(FeatureSeparator, fields.Select(i => Features[i]));
            }

            public override string ToString()
            {
                return $"{Surface},{LeftId},{RightId},{WordCost},{ GetAllFeatures() }";
            }
        }
    }
}
