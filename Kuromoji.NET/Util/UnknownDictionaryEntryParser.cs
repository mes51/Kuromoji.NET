using Kuromoji.NET.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public static class UnknownDictionaryEntryParser
    {
        public static GenericDictionaryEntry Parse(string entry)
        {
            var fields = DictionaryEntryLineParser.ParseLine(entry);

            var surface = fields[0];
            var leftId = short.Parse(fields[1]);
            var rightId = short.Parse(fields[2]);
            var wordCost = short.Parse(fields[3]);

            var pos = new List<string>();
            pos.AddRange(fields.Skip(4).Take(6));

            var features = new List<string>();
            features.AddRange(fields.Skip(10));

            return new GenericDictionaryEntry.Builder()
                .SetSurface(surface)
                .SetLeftId(leftId)
                .SetRightId(rightId)
                .SetWordCost(wordCost)
                .SetPartOfSpeech(pos)
                .SetFeatures(features)
                .Build();
        }
    }
}
