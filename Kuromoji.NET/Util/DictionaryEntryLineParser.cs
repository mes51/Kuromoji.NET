using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public static class DictionaryEntryLineParser
    {
        const char Quote = '"';

        const char Comma = ',';

        const string QuoteEscaped = "\"\"";

        /// <summary>
        /// Parse CSV line
        /// </summary>
        /// <param name="text">line to parse</param>
        /// <returns>String array of parsed valued, null</returns>
        public static string[] ParseLine(string line)
        {
            var insideQuote = false;
            var result = new List<string>();
            var builder = new StringBuilder();
            var quoteCount = 0;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == Quote)
                {
                    insideQuote = !insideQuote;
                    quoteCount++;
                }

                if (c == Comma && !insideQuote)
                {
                    var value = Unescape(builder.ToString());
                    result.Add(value);
                    builder.Clear();
                    continue;
                }

                builder.Append(c);
            }

            result.Add(builder.ToString());

            if (quoteCount % 2 != 0)
            {
                throw new ArgumentException("Unmatched quote in entry: " + line);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Unescape input for CSV
        /// </summary>
        /// <param name="text">text to be unescaped</param>
        /// <returns>unescaped value, not null</returns>
        public static string Unescape(string text)
        {
            var builder = new StringBuilder();
            var foundQuote = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (i == 0 && c == Quote || i == text.Length - 1 && c == Quote)
                {
                    continue;
                }

                if (c == Quote)
                {
                    if (foundQuote)
                    {
                        builder.Append(Quote);
                        foundQuote = false;
                    }
                    else
                    {
                        foundQuote = true;
                    }
                }
                else
                {
                    foundQuote = false;
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Escape input for CSV
        /// </summary>
        /// <param name="text">text to be escaped</param>
        /// <returns>escaped value, not null</returns>
        public static string Escape(string text)
        {
            var hasQuote = text.IndexOf(Quote) >= 0;
            var hasComma = text.IndexOf(Comma) >= 0;

            if (!(hasQuote || hasComma))
            {
                return text;
            }

            var builder = new StringBuilder();

            if (hasQuote)
            {
                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];

                    if (c == Quote)
                    {
                        builder.Append(QuoteEscaped);
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
            }
            else
            {
                builder.Append(text);
            }

            if (hasComma)
            {
                builder.Insert(0, Quote);
                builder.Append(Quote);
            }

            return builder.ToString();
        }
    }
}
