using Kuromoji.NET.Dict;
using Kuromoji.NET.Trie;
using Kuromoji.NET.Util;
using Kuromoji.NET.Viterbi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET
{
    public enum TokenizerMode
    {
        Normal,
        Search,
        Extended
    }

    public class TokenizerBase<T> where T : TokenBase
    {
        ViterbiBuilder ViterbiBuilder { get; set; }

        ViterbiSearcher ViterbiSearcher { get; set; }

        ViterbiFormatter ViterbiFormatter { get; set; }

        bool Split { get; set; }

        TokenInfoDictionary TokenInfoDictionary { get; set; }

        UnknownDictionary UnknownDictionary { get; set; }

        UserDictionary UserDictionary { get; set; }

        InsertedDictionary InsertedDictionary { get; set; }

        protected ITokenFactory<T> TokenFactory { get; set; }

        protected Dictionary<ViterbiNode.NodeType, IDictionary> DictionaryMap { get; } = new Dictionary<ViterbiNode.NodeType, IDictionary>();

        protected void Configure(BuilderBase builder)
        {
            builder.LoadDictionaries();

            TokenFactory = builder.TokenFactory;

            TokenInfoDictionary = builder.TokenInfoDictionary;
            UnknownDictionary = builder.UnknownDictionary;
            UserDictionary = builder.UserDictionary;
            InsertedDictionary = builder.InsertedDictionary;

            ViterbiBuilder = new ViterbiBuilder(builder.DoubleArrayTrie, TokenInfoDictionary, UnknownDictionary, UserDictionary, builder.Mode);
            ViterbiSearcher = new ViterbiSearcher(builder.Mode, builder.ConnectionCosts, UnknownDictionary, builder.Penalties);
            ViterbiFormatter = new ViterbiFormatter(builder.ConnectionCosts);
            Split = builder.Split;

            InitDictionaryMap();
        }

        public virtual List<T> Tokenize(string text)
        {
            return CreateTokenList(text);
        }

        public List<List<T>> MultiTokenize(string text, int maxCount, int costSlack)
        {
            return CreateMultiTokenList(text, maxCount, costSlack);
        }

        public List<List<T>> MultiTokenizeNBest(string text, int n)
        {
            return MultiTokenize(text, n, int.MaxValue);
        }

        public List<List<T>> MultiTokenizeBySlack(string text, int costSlack)
        {
            return MultiTokenize(text, int.MaxValue, costSlack);
        }

        /// <summary>
        /// Tokenizes the provided text and outputs the corresponding Viterbi lattice and the Viterbi path to the provided output stream
        /// 
        /// The output is written in <a href="https://en.wikipedia.org/wiki/DOT_(graph_description_language)">DOT</a> format.
        /// 
        /// This method is not thread safe
        /// </summary>
        /// <param name="output">output stream to write to</param>
        /// <param name="text">text to tokenize</param>
        public void DebugTokenize(Stream output, string text)
        {
            var lattice = ViterbiBuilder.Build(text);
            var bestPath = ViterbiSearcher.Search(lattice);

            using (var writer = new StreamWriter(output, Encoding.UTF8, 1024, true))
            {
                writer.Write(ViterbiFormatter.Format(lattice, bestPath));
            }
        }

        /// <summary>
        /// Writes the Viterbi lattice for the provided text to an output stream
        /// 
        /// The output is written in <a href="https://en.wikipedia.org/wiki/DOT_(graph_description_language)">DOT</a> format.
        /// 
        /// This method is not thread safe
        /// </summary>
        /// <param name="output">output stream to write to</param>
        /// <param name="text">text to create lattice for</param>
        public void DebugLattice(Stream output, string text)
        {
            var lattice = ViterbiBuilder.Build(text);

            using (var writer = new StreamWriter(output, Encoding.UTF8, 1024, true))
            {
                writer.Write(ViterbiFormatter.Format(lattice));
            }
        }

        /// <summary>
        /// Tokenizes the provided text and returns a list of tokens with various feature information
        /// 
        /// This method is thread safe
        /// </summary>
        /// <param name="text">text to tokenize</param>
        /// <returns>list of Token, not null</returns>
        protected List<T> CreateTokenList(string text)
        {
            if (!Split)
            {
                return CreateTokenList(0, text);
            }

            var splitPositions = GetSplitPositions(text);

            if (splitPositions.Count == 0)
            {
                return CreateTokenList(0, text);
            }

            var result = new List<T>();

            var offset = 0;

            foreach (var position in splitPositions)
            {
                result.AddRange(CreateTokenList(offset, text.Substring(offset, position + 1 - offset)));
                offset = position + 1;
            }

            if (offset < text.Length)
            {
                result.AddRange(CreateTokenList(offset, text.Substring(offset)));
            }

            return result;
        }

        /// <summary>
        /// Tokenizes the provided text and returns up to maxCount lists of tokens with various feature information.
        /// Each list corresponds to a possible tokenization with cost at most OPT + costSlack, where OPT is the optimal solution.
        /// 
        /// This method is thread safe
        /// </summary>
        /// <param name="text">text to tokenize</param>
        /// <param name="maxCount">maximum number of different tokenizations</param>
        /// <param name="costSlack">maximum cost slack of a tokenization</param>
        /// <returns>list of Token, not null</returns>
        protected List<List<T>> CreateMultiTokenList(string text, int maxCount, int costSlack)
        {
            if (!Split)
            {
                return ConvertMultiSearchResultToList(CreateMultiSearchResult(text, maxCount, costSlack));
            }

            var splitPositions = GetSplitPositions(text);

            if (splitPositions.Count == 0)
            {
                return ConvertMultiSearchResultToList(CreateMultiSearchResult(text, maxCount, costSlack));
            }

            var results = new List<MultiSearchResult>();
            var offset = 0;

            foreach (var position in splitPositions)
            {
                results.Add(CreateMultiSearchResult(text.Substring(offset, position + 1 - offset), maxCount, costSlack));
                offset = position + 1;
            }

            if (offset < text.Length)
            {
                results.Add(CreateMultiSearchResult(text.Substring(offset), maxCount, costSlack));
            }

            var merger = new MultiSearchMerger(maxCount, costSlack);
            var mergedResult = merger.Merge(results);

            return ConvertMultiSearchResultToList(mergedResult);
        }

        void InitDictionaryMap()
        {
            DictionaryMap.Add(ViterbiNode.NodeType.Known, TokenInfoDictionary);
            DictionaryMap.Add(ViterbiNode.NodeType.Unknown, UnknownDictionary);
            DictionaryMap.Add(ViterbiNode.NodeType.User, UserDictionary);
            DictionaryMap.Add(ViterbiNode.NodeType.Inserted, InsertedDictionary);
        }

        /// <summary>
        /// Tokenize input sentence. Up to maxCount different paths of cost at most OPT + costSlack are returned ordered in ascending order by cost, where OPT is the optimal solution.
        /// </summary>
        /// <param name="text">sentence to tokenize</param>
        /// <param name="maxCount">maximum number of paths</param>
        /// <param name="costSlack">maximum cost slack of a path</param>
        /// <returns>instance of MultiSearchResult containing the tokenizations</returns>
        MultiSearchResult CreateMultiSearchResult(string text, int maxCount, int costSlack)
        {
            var lattice = ViterbiBuilder.Build(text);
            return ViterbiSearcher.SearchMultiple(lattice, maxCount, costSlack);
        }

        List<List<T>> ConvertMultiSearchResultToList(MultiSearchResult multiSearchResult)
        {
            var result = new List<List<T>>();

            var paths = multiSearchResult.TokenizedResults;

            foreach (var path in paths)
            {
                var tokens = new List<T>();

                foreach (var node in path)
                {
                    var wordId = node.WordId;
                    if (node.Type == ViterbiNode.NodeType.Known && wordId == -1)
                    {
                        // Do not include BOS/EOS
                        continue;
                    }

                    var token = TokenFactory.CreateToken(
                        wordId,
                        node.Surface,
                        node.Type,
                        node.StartIndex,
                        DictionaryMap[node.Type]
                    );
                    tokens.Add(token);
                }
                result.Add(tokens);
            }

            return result;
        }

        /// <summary>
        /// Tokenize input sentence.
        /// </summary>
        /// <param name="offset">offset of sentence in original input text</param>
        /// <param name="text">sentence to tokenize</param>
        /// <returns>list of Token</returns>
        List<T> CreateTokenList(int offset, string text)
        {
            var result = new List<T>();

            var lattice = ViterbiBuilder.Build(text);
            var bestPath = ViterbiSearcher.Search(lattice);

            foreach (var node in bestPath)
            {
                var wordId = node.WordId;
                if (node.Type == ViterbiNode.NodeType.Known && wordId == -1)
                {
                    // Do not include BOS/EOS
                    continue;
                }

                var token = TokenFactory.CreateToken(
                    wordId,
                    node.Surface,
                    node.Type,
                    offset + node.StartIndex,
                    DictionaryMap[node.Type]
                );
                result.Add(token);
            }

            return result;
        }

        /// <summary>
        /// Split input text at 句読点, which is 。 and 、
        /// </summary>
        /// <param name="text"></param>
        /// <returns>list of split position</returns>
        List<int> GetSplitPositions(string text)
        {
            var splitPositions = new List<int>();
            int position;
            var currentPosition = 0;

            while (true)
            {
                var indexOfMaru = text.IndexOf("。", currentPosition);
                var indexOfTen = text.IndexOf("、", currentPosition);

                if (indexOfMaru < 0 || indexOfTen < 0)
                {
                    position = Math.Max(indexOfMaru, indexOfTen);
                }
                else
                {
                    position = Math.Min(indexOfMaru, indexOfTen);
                }

                if (position >= 0)
                {
                    splitPositions.Add(position);
                    currentPosition = position + 1;
                }
                else
                {
                    break;
                }
            }

            return splitPositions;
        }

        public abstract class BuilderBase
        {
            protected internal DoubleArrayTrie DoubleArrayTrie { get; set; }

            protected internal ConnectionCosts ConnectionCosts { get; set; }

            protected internal TokenInfoDictionary TokenInfoDictionary { get; set; }

            protected internal UnknownDictionary UnknownDictionary { get; set; }

            protected internal CharacterDefinitions CharacterDefinitions { get; set; }

            protected internal InsertedDictionary InsertedDictionary { get; set; }

            protected internal UserDictionary UserDictionary { get; set; }

            protected internal TokenizerMode Mode { get; set; }

            protected internal bool Split { get; set; } = true;

            protected internal List<int> Penalties { get; set; } = new List<int>();

            protected internal int TotalFeatures { get; set; } = -1;

            protected internal int ReadingFeature { get; set; } = -1;

            protected internal int PartOfSpeechFeature { get; set; } = -1;

            protected internal IResourceResolver Resolver { get; set; }

            protected internal ITokenFactory<T> TokenFactory { get; set; }

            protected internal string DictionaryFilePath { get; }

            public BuilderBase(string filePath)
            {
                DictionaryFilePath = filePath;
            }

            /// <summary>
            /// Creates a Tokenizer instance defined by this Builder
            /// </summary>
            /// <returns>Tokenizer instance</returns>
            public abstract TokenizerBase<T> Build();

            /// <summary>
            /// Sets an optional user dictionary as an input stream
            /// 
            /// The inpuut stream provided is not closed by this method
            /// </summary>
            /// <param name="input">user dictionary as an input stream</param>
            /// <returns>this builder</returns>
            public BuilderBase SetUserDictionary(Stream input)
            {
                UserDictionary = new UserDictionary(input, TotalFeatures, ReadingFeature, PartOfSpeechFeature);
                return this;
            }

            /// <summary>
            /// Sets an optional user dictionary filename
            /// </summary>
            /// <param name="fileName">user dictionary filename</param>
            /// <returns>this builder</returns>
            public BuilderBase SetUserDictionary(string fileName)
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    return SetUserDictionary(fs);
                }
            }

            protected internal virtual void LoadDictionaries()
            {
                DoubleArrayTrie = DoubleArrayTrie.NewInstance(Resolver);
                ConnectionCosts = ConnectionCosts.NewInstance(Resolver);
                TokenInfoDictionary = TokenInfoDictionary.NewInstance(Resolver);
                CharacterDefinitions = CharacterDefinitions.NewInstance(Resolver);
                UnknownDictionary = UnknownDictionary.NewInstance(Resolver, CharacterDefinitions, TotalFeatures);
                InsertedDictionary = new InsertedDictionary(TotalFeatures);
            }
        }
    }
}
