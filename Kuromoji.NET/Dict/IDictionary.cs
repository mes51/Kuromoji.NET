using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Dict
{
    public interface IDictionary
    {
        /// <summary>
        /// Gets the left id of the specified word
        /// </summary>
        /// <param name="wordId">word id to get left id cost for</param>
        /// <returns>left id cost</returns>
        int GetLeftId(int wordId);

        /// <summary>
        /// Gets the right id of the specified word
        /// </summary>
        /// <param name="wordId">word id to get right id cost for</param>
        /// <returns>right id cost</returns>
        int GetRightId(int wordId);

        /// <summary>
        /// Gets the word cost of the specified word
        /// </summary>
        /// <param name="wordId">word id to get word cost for</param>
        /// <returns>word cost</returns>
        int GetWordCost(int wordId);

        /// <summary>
        /// Gets all features of the specified word id
        /// </summary>
        /// <param name="wordId">word id to get features for</param>
        /// <returns>All features as a string</returns>
        string GetAllFeatures(int wordId);

        /// <summary>
        /// Gets all features of the specified word id as a String array
        /// </summary>
        /// <param name="wordId">word id to get features for</param>
        /// <returns>Array with all features</returns>
        string[] GetAllFeaturesArray(int wordId);

        /// <summary>
        /// Gets one or more specific features of a token
        /// 
        /// This is an expert API
        /// </summary>
        /// <param name="wordId">word id to get features for</param>
        /// <param name="fields">array of feature ids. If this array is empty, all features are returned</param>
        /// <returns>Array with specified features</returns>
        string GetFeature(int wordId, params int[] fields);
    }
}
