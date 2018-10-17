using ICSharpCode.SharpZipLib.Tar;
using Kuromoji.NET.Compile;
using Kuromoji.NET.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.DicBuilder.Builder
{
    enum DicCompressType
    {
        Zip,
        Tar
    }

    abstract class BuilderBase<T> where T : DictionaryEntryBase
    {
        protected abstract string DirectoryName { get; }

        protected abstract string OutputDicFileName { get; }

        protected abstract Encoding DicEncoding { get; }

        public string DicUrl { get; set; }

        string DownloadDir => Path.Combine("Downloads", DirectoryName);

        string DownloadFilePath => Path.Combine(DownloadDir, "dic");

        string ExtractDir => Path.Combine("Extract", DirectoryName);

        string CompileOutputDir => Path.Combine("Compiled", DirectoryName);

        string OutputDir => Path.Combine("Output", DirectoryName);

        string OutputFileName => Path.Combine(OutputDir, OutputDicFileName);

        DicCompressType CompressType => DicUrl.EndsWith(".zip") ? DicCompressType.Zip : DicCompressType.Tar;

        public void Build(bool compactTrie)
        {
            CleanUp();

            Directory.CreateDirectory(DownloadDir);
            Directory.CreateDirectory(ExtractDir);
            Directory.CreateDirectory(CompileOutputDir);
            Directory.CreateDirectory(OutputDir);

            if (!File.Exists(DownloadFilePath))
            {
                using (var cli = new WebClient())
                {
                    Console.Write("Dictionary downloading... ");
                    cli.DownloadFile(DicUrl, DownloadFilePath);
                    Console.WriteLine("Complete.");
                }
            }

            Console.Write("Extracting... ");

            switch (CompressType)
            {
                case DicCompressType.Zip:
                    ZipFile.ExtractToDirectory(DownloadFilePath, ExtractDir);
                    break;
                case DicCompressType.Tar:
                    using (var fs = new FileStream(DownloadFilePath, FileMode.Open, FileAccess.Read))
                    using (var gzip = new GZipStream(fs, CompressionMode.Decompress))
                    using (var tar = TarArchive.CreateInputTarArchive(gzip))
                    {
                        tar.ExtractContents(ExtractDir);
                    }
                    break;
            }

            Console.WriteLine("Complete.");

            var srcDir = Directory.GetDirectories(ExtractDir).FirstOrDefault();
            if (srcDir == null)
            {
                srcDir = ExtractDir;
            }
            
            CreateCompiler().Build(srcDir, CompileOutputDir, DicEncoding, compactTrie);

            Console.Write("Compress compiled dic... ");
            using (var zip = ZipFile.Open(OutputFileName, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.GetFiles(CompileOutputDir))
                {
                    zip.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }
            Console.WriteLine("Complete.");

            Console.Write("Clean up dir... ");
            CleanUp();
            Console.WriteLine("Complete.");
        }

        protected abstract DictionaryCompilerBase<T> CreateCompiler();

        void CleanUp()
        {
            if (Directory.Exists(CompileOutputDir))
            {
                Directory.Delete(CompileOutputDir, true);
            }
            if (Directory.Exists(ExtractDir))
            {
                Directory.Delete(ExtractDir, true);
            }
        }
    }
}
