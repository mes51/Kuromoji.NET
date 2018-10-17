using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public static class Regexs
    {
        /// <summary>
        /// "\\s*#.*$"
        /// </summary>
        public static readonly Regex Comment = new Regex("\\s*#.*$", RegexOptions.Compiled);

        /// <summary>
        /// "\\s+"
        /// </summary>
        public static readonly Regex CustomSegmentationSpliter = new Regex("\\s+", RegexOptions.Compiled);
    }
}
