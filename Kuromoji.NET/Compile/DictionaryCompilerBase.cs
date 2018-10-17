using Kuromoji.NET.Dict;
using Kuromoji.NET.Trie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Compile
{
    public abstract class DictionaryCompilerBase<T> where T : DictionaryEntryBase
    {
        public void Build(string inputDirname, string outputDirname, Encoding encoding, bool compactTries)
        {
            Directory.CreateDirectory(outputDirname);
            BuildTokenInfoDictionary(inputDirname, outputDirname, encoding, compactTries);
            BuildUnknownWordDictionary(inputDirname, outputDirname, encoding);
            BuildConnectionCosts(inputDirname, outputDirname);
        }

        protected void BuildUnknownWordDictionary(string inputDirname, string outputDirname, Encoding encoding)
        {
            ProgressLog.Begin("compiling unknown word dict");

            using (var charDefOutput = new FileStream(Path.Combine(outputDirname, CharacterDefinitions.CharacterDefinitionsFileName), FileMode.Create, FileAccess.ReadWrite))
            using (var charDefInput = new FileStream(Path.Combine(inputDirname, "char.def"), FileMode.Open, FileAccess.Read))
            using (var unkDicOutput = new FileStream(Path.Combine(outputDirname, UnknownDictionary.UnknownDictionaryFileName), FileMode.Create, FileAccess.ReadWrite))
            using (var unkDicInput = new FileStream(Path.Combine(inputDirname, "unk.def"), FileMode.Open, FileAccess.Read))
            {
                var charDefCompiler = new CharacterDefinitionsCompiler(charDefOutput);
                charDefCompiler.ReadCharacterDefinition(charDefInput, encoding);
                charDefCompiler.Compile();

                var unkDicCompiler = new UnknownDictionaryCompiler(unkDicOutput, charDefCompiler.MakeCharacterCategoryMap());
                unkDicCompiler.ReadUnknownDefinition(unkDicInput, encoding);
                unkDicCompiler.Compile();
            }

            ProgressLog.End();
        }

        protected abstract TokenInfoDictionaryCompilerBase<T> GetTokenInfoDictionaryCompiler(Encoding encoding);

        protected virtual void Build(string[] args)
        {
            var inputDirname = args[0];
            var outputDirname = args[1];
            var inputEncoding = args[2];
            var compactTries = args.Length > 3 ? false : args[3].ToLower() == "true";

            ProgressLog.Println("dictionary compiler");
            ProgressLog.Println("");
            ProgressLog.Println("input directory: " + inputDirname);
            ProgressLog.Println("output directory: " + outputDirname);
            ProgressLog.Println("input encoding: " + inputEncoding);
            ProgressLog.Println("compact tries: " + compactTries);
            ProgressLog.Println("");

            Build(inputDirname, outputDirname, Encoding.GetEncoding(inputEncoding), compactTries);
        }

        void BuildTokenInfoDictionary(string inputDirname, string outputDirname, Encoding encoding, bool compactTries)
        {
            ProgressLog.Begin("compiling tokeninfo dict");

            var tokenInfoCompiler = GetTokenInfoDictionaryCompiler(encoding);

            ProgressLog.Println("analyzing dictionary features");
            using (var stream = tokenInfoCompiler.CombinedSequentialFileInputStream(inputDirname))
            {
                tokenInfoCompiler.AnalyzeTokenInfo(stream);
            }

            ProgressLog.Println("reading tokeninfo");
            using (var stream = tokenInfoCompiler.CombinedSequentialFileInputStream(inputDirname))
            {
                tokenInfoCompiler.ReadTokenInfo(stream);
            }
            
            tokenInfoCompiler.Compile();

            var surfaces = tokenInfoCompiler.Surfaces;

            ProgressLog.Begin("compiling double array trie");
            using (var fs = new FileStream(Path.Combine(outputDirname, DoubleArrayTrie.DoubleArrayTrieFileName), FileMode.Create, FileAccess.ReadWrite))
            {
                var trie = DoubleArrayTrieCompiler.Build(surfaces, compactTries);
                trie.Write(fs);
            }

            ProgressLog.Println("validating saved double array trie");
            DoubleArrayTrie daTrie;
            using (var fs = new FileStream(Path.Combine(outputDirname, DoubleArrayTrie.DoubleArrayTrieFileName), FileMode.Open, FileAccess.Read))
            {
                daTrie = DoubleArrayTrie.Read(fs);
                foreach (var surface in surfaces)
                {
                    if (daTrie.Lookup(surface) < 0)
                    {
                        ProgressLog.Println("failed to look up [" + surface + "]");
                    }
                }
            }

            ProgressLog.End();

            ProgressLog.Begin("processing target map");

            for (var i = 0; i < surfaces.Count; i++)
            {
                int id = daTrie.Lookup(surfaces[i]);
                tokenInfoCompiler.AddMapping(id, i);
            }

            tokenInfoCompiler.Write(outputDirname); // TODO: Should be refactored -Christian

            ProgressLog.End();

            ProgressLog.End();
        }

        void BuildConnectionCosts(string inputDirname, string outputDirname)
        {
            ProgressLog.Begin("compiling connection costs");

            using (var output = new FileStream(Path.Combine(outputDirname, ConnectionCosts.ConnectionCostsFileName), FileMode.Create, FileAccess.ReadWrite))
            using (var matrix = new FileStream(Path.Combine(inputDirname, "matrix.def"), FileMode.Open, FileAccess.Read))
            {
                var connectionCostsCompiler = new ConnectionCostsCompiler(output);
                connectionCostsCompiler.ReadCosts(matrix);
                connectionCostsCompiler.Compile();
            }

            ProgressLog.End();
        }
    }
}
