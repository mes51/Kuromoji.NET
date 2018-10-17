using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Test.TestSuites
{
    /// <summary>
    /// StringGenerator generates random strings composed of characters. What these characters
    /// are and their distribution depends on a subclass.
    /// </summary>
    abstract class StringGenerator
    {
        /// <summary>
        /// An alias for OfCodeUnitsLength
        /// </summary>
        /// <param name="r"></param>
        /// <param name="minCodeUnits">Minimum number of code units (inclusive).</param>
        /// <param name="maxCodeUnits">Maximum number of code units (inclusive).</param>
        /// <returns>Returns a string of variable length between <code>minCodeUnits</code> (inclusive)
        /// and <code>maxCodeUnits</code> (inclusive) length. Code units are essentially
        /// an equivalent of <code>char</code> type, see string class for
        /// explanation.  </returns>
        public string OfStringLength(Random r, int minCodeUnits, int maxCodeUnits)
        {
            return OfCodeUnitsLength(r, minCodeUnits, maxCodeUnits);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="minCodeUnits">Minimum number of code units (inclusive).</param>
        /// <param name="maxCodeUnits">Maximum number of code units (inclusive).</param>
        /// <returns>Returns a string of variable length between <code>minCodeUnits</code> (inclusive)
        /// and <code>maxCodeUnits</code> (inclusive) length. Code units are essentially
        /// an equivalent of <code>char</code> type, see string class for
        /// explanation.  </returns>
        public abstract string OfCodeUnitsLength(Random r, int minCodeUnits, int maxCodeUnits);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="minCodePoints">Minimum number of code points (inclusive).</param>
        /// <param name="maxCodePoints">Maximum number of code points (inclusive).</param>
        /// <returns>Returns a string of variable length between <code>minCodePoints</code> (inclusive)
        /// and <code>maxCodePoints</code> (inclusive) length. Code points are full unicodeGenerator
        /// codepoints or an equivalent of <code>int</code> type, see string class for
        /// explanation. The returned {@link String#length()} may exceed <code>maxCodeUnits</code>
        /// because certain code points may be encoded as surrogate pairs.</returns>
        public abstract string OfCodePointsLength(Random r, int minCodePoints, int maxCodePoints);
    }
}
