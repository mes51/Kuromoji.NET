using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public abstract class DictionaryEntryBase
    {
        public string Surface { get; }

        public short LeftId { get; }

        public short RightId { get; }

        public short WordCost { get; }

        public DictionaryEntryBase(string surface, short leftId, short rightId, int wordCost)
        {
            Surface = surface;
            LeftId = leftId;
            RightId = rightId;
            // TODO: Temporary work-around for UniDic NEologd to deal with costs outside the short value range
            WordCost = (short)Math.Max(Math.Min(wordCost, short.MaxValue), short.MinValue);
        }
    }
}
