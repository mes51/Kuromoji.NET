using Kuromoji.NET.Dict;
using Kuromoji.NET.Trie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public class ViterbiBuilder
    {
        DoubleArrayTrie DoubleArrayTrie { get; }

        TokenInfoDictionary Dictionary { get; }

        UnknownDictionary UnknownDictionary { get; }

        UserDictionary UserDictionary { get; }

        CharacterDefinitions CharacterDefinitions { get; }

        bool UseUserDictionary { get; }

        bool SearchMode { get; }

        public ViterbiBuilder(DoubleArrayTrie doubleArrayTrie, TokenInfoDictionary dictionary, UnknownDictionary unknownDictionary, UserDictionary userDictionary, TokenizerMode mode)
        {
            DoubleArrayTrie = doubleArrayTrie;
            Dictionary = dictionary;
            UnknownDictionary = unknownDictionary;
            UserDictionary = userDictionary;

            UseUserDictionary = userDictionary != null;

            SearchMode = mode == TokenizerMode.Search || mode == TokenizerMode.Extended;

            CharacterDefinitions = UnknownDictionary.CharacterDefinition;
        }

        /// <summary>
        /// Build lattice from input text
        /// </summary>
        /// <param name="text">source text for the lattice</param>
        /// <returns>built lattice, not null</returns>
        public ViterbiLattice Build(string text)
        {
            var textLength = text.Length;
            var lattice = new ViterbiLattice(textLength + 2);

            lattice.AddBos();

            var unknownWordEndIndex = -1; // index of the last character of unknown word

            for (var startIndex = 0; startIndex < textLength; startIndex++)
            {
                // If no token ends where current token starts, skip this index
                if (lattice.TokenEndsWhereCurrentTokenStarts(startIndex))
                {
                    var suffix = text.Substring(startIndex);
                    var found = ProcessIndex(lattice, startIndex, suffix);

                    // In the case of normal mode, it doesn't process unknown word greedily.
                    if (SearchMode || unknownWordEndIndex <= startIndex)
                    {

                        int[] categories = CharacterDefinitions.LookupCategories(suffix[0]);

                        for (int i = 0; i < categories.Length; i++)
                        {
                            int category = categories[i];
                            unknownWordEndIndex = ProcessUnknownWord(category, i, lattice, unknownWordEndIndex, startIndex, suffix, found);
                        }
                    }
                }
            }

            if (UseUserDictionary)
            {
                ProcessUserDictionary(text, lattice);
            }

            lattice.AddEos();

            return lattice;
        }

        bool ProcessIndex(ViterbiLattice lattice, int startIndex, string suffix)
        {
            var found = false;

            for (var endIndex = 1; endIndex < suffix.Length + 1; endIndex++)
            {
                var prefix = suffix.Substring(0, endIndex);
                var result = DoubleArrayTrie.Lookup(prefix);

                if (result > 0)
                {
                    found = true; // Don't produce unknown word starting from this index
                    foreach (var wordId in Dictionary.LookupWordIds(result))
                    {
                        var node = new ViterbiNode(wordId, prefix, Dictionary, startIndex, ViterbiNode.NodeType.Known);
                        lattice.AddNode(node, startIndex + 1, startIndex + 1 + endIndex);
                    }
                }
                else if (result < 0)
                {
                    // If result is less than zero, continue to next position
                    break;
                }
            }

            return found;
        }

        int ProcessUnknownWord(int category, int i, ViterbiLattice lattice, int unknownWordEndIndex, int startIndex, string suffix, bool found)
        {
            var unknownWordLength = 0;
            var definition = CharacterDefinitions.LookupDefinition(category);

            if (definition[CharacterDefinitions.Invoke] == 1 || found == false)
            {
                if (definition[CharacterDefinitions.Group] == 0)
                {
                    unknownWordLength = 1;
                }
                else
                {
                    unknownWordLength = 1;
                    for (var j = 1; j < suffix.Length; j++)
                    {
                        var c = suffix[j];

                        var categories = CharacterDefinitions.LookupCategories(c);

                        if (categories == null)
                        {
                            break;
                        }

                        if (i < categories.Length && category == categories[i])
                        {
                            unknownWordLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (unknownWordLength > 0)
            {
                var unkWord = suffix.Substring(0, unknownWordLength);
                var wordIds = UnknownDictionary.LookupWordIds(category); // characters in input text are supposed to be the same

                foreach (var wordId in wordIds)
                {
                    var node = new ViterbiNode(wordId, unkWord, UnknownDictionary, startIndex, ViterbiNode.NodeType.Unknown);
                    lattice.AddNode(node, startIndex + 1, startIndex + 1 + unknownWordLength);
                }
                unknownWordEndIndex = startIndex + unknownWordLength;
            }

            return unknownWordEndIndex;
        }

        /// <summary>
        /// Find token(s) in input text and set found token(s) in arrays as normal tokens
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lattice"></param>
        void ProcessUserDictionary(string text, ViterbiLattice lattice)
        {
            var matches = UserDictionary.FindUserDictionaryMatches(text);

            foreach (var match in matches)
            {
                var wordId = match.WordId;
                var index = match.MatchStartIndex;
                var length = match.MatchLength;

                var word = text.Substring(index, length);

                var node = new ViterbiNode(wordId, word, UserDictionary, index, ViterbiNode.NodeType.User);
                var nodeStartIndex = index + 1;
                var nodeEndIndex = nodeStartIndex + length;

                lattice.AddNode(node, nodeStartIndex, nodeEndIndex);

                if (IsLatticeBrokenBefore(nodeStartIndex, lattice))
                {
                    RepairBrokenLatticeBefore(lattice, index);
                }

                if (IsLatticeBrokenAfter(nodeStartIndex + length, lattice))
                {
                    RepairBrokenLatticeAfter(lattice, nodeEndIndex);
                }
            }
        }

        /// <summary>
        /// Checks whether there exists any node in the lattice that connects to the newly inserted entry on the left side
        /// (before the new entry).
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <param name="lattice"></param>
        /// <returns>whether the lattice has a node that ends at nodeIndex</returns>
        bool IsLatticeBrokenBefore(int nodeIndex, ViterbiLattice lattice)
        {
            var nodeEndIndices = lattice.EndIndexArr;

            return nodeEndIndices[nodeIndex] == null;
        }

        /// <summary>
        /// Checks whether there exists any node in the lattice that connects to the newly inserted entry on the right side
        /// (after the new entry).
        /// </summary>
        /// <param name="endIndex"></param>
        /// <param name="lattice"></param>
        /// <returns>whether the lattice has a node that starts at endIndex</returns>
        bool IsLatticeBrokenAfter(int endIndex, ViterbiLattice lattice)
        {
            var nodeStartIndices = lattice.StartIndexArr;

            return nodeStartIndices[endIndex] == null;
        }

        /// <summary>
        /// Tries to repair the lattice by creating and adding an additional Viterbi node to the LEFT of the newly
        /// inserted user dictionary entry by using the substring of the node in the lattice that overlaps the least
        /// </summary>
        /// <param name="lattice"></param>
        /// <param name="index"></param>
        void RepairBrokenLatticeBefore(ViterbiLattice lattice, int index)
        {
            var nodeStartIndices = lattice.StartIndexArr;

            for (var startIndex = index; startIndex > 0; startIndex--)
            {
                if (nodeStartIndices[startIndex] != null)
                {
                    var glueBase = FindGlueNodeCandidate(index, nodeStartIndices[startIndex], startIndex);
                    if (glueBase != null)
                    {
                        var length = index + 1 - startIndex;
                        var surface = glueBase.Surface.Substring(0, length);
                        var glueNode = MakeGlueNode(startIndex, glueBase, surface);
                        lattice.AddNode(glueNode, startIndex, startIndex + glueNode.Surface.Length);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Tries to repair the lattice by creating and adding an additional Viterbi node to the RIGHT of the newly
        /// inserted user dictionary entry by using the substring of the node in the lattice that overlaps the least
        /// </summary>
        /// <param name="lattice"></param>
        /// <param name="nodeEndIndex"></param>
        void RepairBrokenLatticeAfter(ViterbiLattice lattice, int nodeEndIndex)
        {
            var nodeEndIndices = lattice.EndIndexArr;

            for (var endIndex = nodeEndIndex + 1; endIndex < nodeEndIndices.Length; endIndex++)
            {
                if (nodeEndIndices[endIndex] != null)
                {
                    ViterbiNode glueBase = FindGlueNodeCandidate(nodeEndIndex, nodeEndIndices[endIndex], endIndex);
                    if (glueBase != null)
                    {
                        var delta = endIndex - nodeEndIndex;
                        var glueBaseSurface = glueBase.Surface;
                        var surface = glueBaseSurface.Substring(glueBaseSurface.Length - delta);
                        var glueNode = MakeGlueNode(nodeEndIndex, glueBase, surface);
                        lattice.AddNode(glueNode, nodeEndIndex, nodeEndIndex + glueNode.Surface.Length);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Tries to locate a candidate for a "glue" node that repairs the broken lattice by looking at all nodes at the
        /// current index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="latticeNodes"></param>
        /// <param name="startIndex"></param>
        /// <returns>new ViterbiNode that can be inserted to glue the graph if such a node exists, else null</returns>
        ViterbiNode FindGlueNodeCandidate(int index, ViterbiNode[] latticeNodes, int startIndex)
        {
            var candidates = new List<ViterbiNode>();

            foreach (var viterbiNode in latticeNodes)
            {
                if (viterbiNode != null)
                {
                    candidates.Add(viterbiNode);
                }
            }
            if (candidates.Count != 0)
            {
                ViterbiNode glueBase = null;
                var length = index + 1 - startIndex;
                foreach (var candidate in candidates)
                {
                    if (IsAcceptableCandidate(length, glueBase, candidate))
                    {
                        glueBase = candidate;
                    }
                }
                if (glueBase != null)
                {
                    return glueBase;
                }
            }
            return null;
        }

        /// <summary>
        /// Check whether a candidate for a glue node is acceptable.
        /// The candidate should be as short as possible, but long enough to overlap with the inserted user entry
        /// </summary>
        /// <param name="targetLength"></param>
        /// <param name="glueBase"></param>
        /// <param name="candidate"></param>
        /// <returns></returns>
        bool IsAcceptableCandidate(int targetLength, ViterbiNode glueBase, ViterbiNode candidate)
        {
            return (glueBase == null || candidate.Surface.Length < glueBase.Surface.Length) &&
                candidate.Surface.Length >= targetLength;
        }

        /// <summary>
        /// Create a glue node to be inserted based on ViterbiNode already in the lattice.
        /// The new node takes the same parameters as the node it is based on, but the word is truncated to match the
        /// hole in the lattice caused by the new user entry
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="glueBase"></param>
        /// <param name="surface"></param>
        /// <returns>new ViterbiNode to be inserted as glue into the lattice</returns>
        ViterbiNode MakeGlueNode(int startIndex, ViterbiNode glueBase, String surface)
        {
            return new ViterbiNode(
                glueBase.WordId,
                surface,
                glueBase.LeftId,
                glueBase.RightId,
                glueBase.WordCost,
                startIndex,
                ViterbiNode.NodeType.Inserted
            );
        }
    }
}
