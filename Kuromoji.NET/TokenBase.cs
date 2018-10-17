using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Dict;
using Kuromoji.NET.Viterbi;

namespace Kuromoji.NET
{
    public abstract class TokenBase
    {
        const int MetaDataSize = 4;

        /// <summary>
        /// Gets the surface form of this token (表層形)
        /// </summary>
        public string Surface { get; }

        /// <summary>
        /// Gets the position/start index where this token is found in the input text
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Predicate indicating whether this token is known (contained in the standard dictionary)
        /// </summary>
        public bool IsKnown => Type == ViterbiNode.NodeType.Known;

        /// <summary>
        /// Predicate indicating whether this token is included is from the user dictionary
        /// 
        /// If a token is contained both in the user dictionary and standard dictionary, this method will return true
        /// </summary>
        public bool IsUser => Type == ViterbiNode.NodeType.User;

        ViterbiNode.NodeType Type { get; }

        IDictionary Dictionary { get; }

        int WordId { get; }

        public TokenBase(int wordId, string surface, ViterbiNode.NodeType type, int position, IDictionary dictionary)
        {
            WordId = wordId;
            Surface = surface;
            Type = type;
            Position = position;
            Dictionary = dictionary;
        }

        /// <summary>
        /// Gets all features for this token as a comma-separated String
        /// </summary>
        /// <returns>token features, not null</returns>
        public string GetAllFeatures()
        {
            return Dictionary.GetAllFeatures(WordId);
        }

        /// <summary>
        /// Gets all features for this token as a String array
        /// </summary>
        /// <returns>token feature array, not null</returns>
        public string[] GetAllFeaturesArray()
        {
            return Dictionary.GetAllFeaturesArray(WordId);
        }

        /// <summary>
        /// Gets a numbered feature for this token
        /// </summary>
        /// <param name="feature">feature number</param>
        /// <returns>token feature, not null</returns>
        protected string GetFeature(int feature)
        {
            return Dictionary.GetFeature(WordId, feature - MetaDataSize);
        }

        public override string ToString()
        {
            return $"Token{{surface='{Surface}', position={Position}, type={Type}, dictionary={Dictionary}, wordId={WordId}}}";
        }
    }
}
