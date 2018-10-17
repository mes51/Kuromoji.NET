using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public static class ScriptUtils
    {
        /// <summary>
        /// Predicate denoting if input is all katakana characters
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>true if input is all katakana characters</returns>
        public static bool IsKatakana(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!IsFullWidthKatakana(str[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Full-width katakana predicate
        /// </summary>
        /// <param name="c">character to test</param>
        /// <returns>true if and only if c is a full-width katakana character</returns>
        public static bool IsFullWidthKatakana(char c)
        {
            // ゠ から ヿまで
            return '\u30A0' <= c && c <= '\u30FF';
        }

        /// <summary>
        /// Predicate denoting if input is all hiragana characters
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>true if input is all hiragana characters</returns>
        public static bool IsHiragana(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!IsHiragana(str[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Hiragana predicate
        /// </summary>
        /// <param name="c">character to test</param>
        /// <returns>true if and only if c is a hiragana character</returns>
        public static bool IsHiragana(char c)
        {
            return ('\u3041' <= c && c <= '\u3096') || ('\u3099' <= c && c <= '\u309F');
        }
    }
}
