using Kuromoji.NET.Buffer;
using Kuromoji.NET.Dict;
using Kuromoji.NET.Extentions;
using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Compile
{
    public abstract class TokenInfoDictionaryCompilerBase<T> : ICompiler where T : DictionaryEntryBase
    {
        protected List<BufferEntry> BufferEntries { get; } = new List<BufferEntry>();

        protected FeatureInfoMap PosInfo { get; } = new FeatureInfoMap();

        protected FeatureInfoMap OtherInfo { get; } = new FeatureInfoMap();

        protected WordIdMapCompiler WordIdsCompiler { get; } = new WordIdMapCompiler();

        protected List<GenericDictionaryEntry> DictionaryEntries { get; set; }

        Encoding Encoding { get; }

        public List<string> Surfaces { get; } = new List<string>();

        public TokenInfoDictionaryCompilerBase(Encoding encoding)
        {
            Encoding = encoding;
        }

        public void AnalyzeTokenInfo(Stream input)
        {
            foreach (var line in input.ReadLines(Encoding))
            {
                var entry = Parse(line);
                var dictionaryEntry = MakeGenericDictionaryEntry(entry);
                PosInfo.MapFeatures(dictionaryEntry.PartOfSpeechFeatures);
            }
        }

        public void ReadTokenInfo(Stream input)
        {
            var entryCount = PosInfo.EntryCount;
            foreach (var line in input.ReadLines(Encoding))
            {
                var entry = Parse(line);
                var dictionaryEntry = MakeGenericDictionaryEntry(entry);

                var leftId = dictionaryEntry.LeftId;
                var rightId = dictionaryEntry.RightId;
                var wordCost = dictionaryEntry.WordCost;

                var allPosFeatures = dictionaryEntry.PartOfSpeechFeatures;

                var posFeatureIds = PosInfo.MapFeatures(allPosFeatures);

                var featureList = dictionaryEntry.OtherFeatures;
                var otherFeatureIds = OtherInfo.MapFeatures(featureList);

                var bufferEntry = new BufferEntry();
                bufferEntry.TokenInfo.Add(leftId);
                bufferEntry.TokenInfo.Add(rightId);
                bufferEntry.TokenInfo.Add(wordCost);

                if (EntriesFitInAByte(entryCount))
                {
                    var posFeatureIdBytes = CreatePosFeatureIds(posFeatureIds);
                    bufferEntry.PosInfo.AddRange(posFeatureIdBytes);
                }
                else
                {
                    foreach (var posFeatureId in posFeatureIds)
                    {
                        bufferEntry.TokenInfo.Add((short)posFeatureId);
                    }
                }

                bufferEntry.Features.AddRange(otherFeatureIds);

                BufferEntries.Add(bufferEntry);
                Surfaces.Add(dictionaryEntry.Surface);

                if (DictionaryEntries != null)
                {
                    DictionaryEntries.Add(dictionaryEntry);
                }
            }
        }

        public Stream CombinedSequentialFileInputStream(string dir)
        {
            var files = GetCsvFiles(dir);

            return new SequenceStream(files.Select(f => new FileStream(f, FileMode.Open, FileAccess.Read)));
        }

        public List<string> GetCsvFiles(string dir)
        {
            var files = Directory.GetFiles(dir, "*.csv").OrderBy(s => s);
            return files.ToList();
        }

        public void AddMapping(int sourceId, int wordId)
        {
            WordIdsCompiler.AddMapping(sourceId, wordId);
        }

        public void Write(string directoryName)
        {
            WriteDictionary(Path.Combine(directoryName, TokenInfoDictionary.TokenInfoDictionaryFileName));
            WriteMap(Path.Combine(directoryName, TokenInfoDictionary.PosMapFileName), PosInfo);
            WriteMap(Path.Combine(directoryName, TokenInfoDictionary.FeatureMapFileName), OtherInfo);
            WriteWordIds(Path.Combine(directoryName, TokenInfoDictionary.TargetMapFileName));
        }

        protected void WriteMap(string fileName, FeatureInfoMap map)
        {
            var features = map.Invert();

            var mapBuffer = new StringValueMapBuffer(features);
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                mapBuffer.Write(fs);
            }
        }

        protected void WriteDictionary(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                var tokenInfoBufferCompiler = new TokenInfoBufferCompiler(fs, BufferEntries);
                tokenInfoBufferCompiler.Compile();
            }
        }

        protected void WriteWordIds(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                WordIdsCompiler.Write(fs);
            }
        }

        protected abstract GenericDictionaryEntry MakeGenericDictionaryEntry(T entry);

        protected abstract T Parse(string line);

        bool EntriesFitInAByte(int entryCount)
        {
            return entryCount <= 0xff;
        }

        List<byte> CreatePosFeatureIds(List<int> posFeatureIds)
        {
            var posFeatureIdBytes = new List<byte>();
            foreach (var posFeatureId in posFeatureIds)
            {
                posFeatureIdBytes.Add((byte)posFeatureId);
            }
            return posFeatureIdBytes;
        }

        public virtual void Compile()
        {
            // TODO: Should call this method instead of write()
        }

        /*
         * skip implement
         * 
        @Deprecated
        public WordIdMap getWordIdMap() throws IOException {
            File file = File.createTempFile("kuromoji-wordid-", ".bin");
            file.deleteOnExit();

            OutputStream output = new BufferedOutputStream(new FileOutputStream(file));
            wordIdsCompiler.write(output);
            output.close();

            InputStream input = new BufferedInputStream(new FileInputStream(file));
            WordIdMap wordIds = new WordIdMap(input);
            input.close();

            return wordIds;
        }

        @Deprecated
        public List<BufferEntry> getBufferEntries() {
            return bufferEntries;
        }

        @Deprecated
        public List<GenericDictionaryEntry> getDictionaryEntries() {
            return dictionaryEntries;
        }

        @Deprecated
        public void setDictionaryEntries(List<GenericDictionaryEntry> dictionaryEntries) {
            this.dictionaryEntries = dictionaryEntries;
        }
         */
    }
}
