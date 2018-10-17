using Kuromoji.NET.Dict;
using Kuromoji.NET.Util;
using Kuromoji.NET.Viterbi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Tokenizers.UniDicNEologd.Compile;

namespace Kuromoji.NET.Tokenizers.UniDicNEologd
{
    public class Tokenizer : TokenizerBase<Token>
    {
        public Tokenizer(string dictionaryFilePath) : this(new Builder(dictionaryFilePath)) { }

        public Tokenizer(Builder builder)
        {
            Configure(builder);
        }

        /// <summary>
        /// Tokenizes the provided text and returns a list of tokens with various feature information
        /// 
        /// This method is thread safe
        /// </summary>
        /// <param name="text">text to tokenize</param>
        /// <returns>list of Token, not null</returns>
        public override List<Token> Tokenize(string text)
        {
            return CreateTokenList(text);
        }

        public class Builder : BuilderBase
        {
            public Builder(string filePath) : base(filePath)
            {
                TotalFeatures = DictionaryEntry.IndexTotalFeatures;
                ReadingFeature = DictionaryEntry.IndexReadingFeature;
                PartOfSpeechFeature = DictionaryEntry.IndexPartOfSpeechFeature;

                Resolver = new ZipResourceResolver(DictionaryFilePath);

                TokenFactory = new UniDicNEologdTokenFactory();
            }

            public override TokenizerBase<Token> Build()
            {
                return new Tokenizer(this);
            }
        }

        class UniDicNEologdTokenFactory : ITokenFactory<Token>
        {
            public Token CreateToken(int wordId, string surface, ViterbiNode.NodeType type, int position, IDictionary dictionary)
            {
                return new Token(wordId, surface, type, position, dictionary);
            }
        }
    }
}
