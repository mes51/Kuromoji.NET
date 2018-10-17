using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kuromoji.NET.Extentions;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;

namespace Kuromoji.NET.Test
{
    public static class TestUtils
    {
        static readonly Regex Tab = new Regex("\\t", RegexOptions.Compiled);

        public static void IsSameTokenSurfaces<T>(this List<T> actualTokens, string[] expectedSurfaces) where T : TokenBase
        {
            actualTokens.Select(t => t.Surface).SequenceEqual(expectedSurfaces).IsTrue();
        }

        public static void IsCanTokenizeStream<T>(this TokenizerBase<T> tokenizer, Stream untokenizedInput) where T : TokenBase
        {
            foreach (var untokenizedLine in untokenizedInput.ReadLines(Encoding.UTF8))
            {
                tokenizer.IsCanTokenizeString(untokenizedLine);
            }
        }

        public static void IsCanTokenizeString<T>(this TokenizerBase<T> tokenizer, string input) where T : TokenBase
        {
            var tokens = tokenizer.Tokenize(input);

            if (input.Length > 0)
            {
                tokens.Any().IsTrue();
            }
            else
            {
                tokens.Any().IsFalse();
            }
        }

        public static void IsCanMultiTokenizeString<T>(this TokenizerBase<T> tokenizer, string input, int maxCount, int costSlack) where T : TokenBase
        {
            var tokens = tokenizer.MultiTokenize(input, maxCount, costSlack);

            if (input.Length > 0)
            {
                tokens.Any().IsTrue();
            }
            else
            {
                tokens.Any().IsFalse();
            }
        }

        public static void IsSameTokenizedStream<T>(this TokenizerBase<T> tokenizer, string tokenizedInputResource, string untokenizedInputResource) where T : TokenBase
        {
            using (var tokenizedInput = GetResource(tokenizedInputResource))
            using (var untokenizedInput = GetResource(untokenizedInputResource))
            {
                tokenizer.IsSameTokenizedStream(tokenizedInput, untokenizedInput);
            }
        }

        public static void IsSameTokenizedStream<T>(this TokenizerBase<T> tokenizer, Stream tokenizedInput, Stream untokenizedInput) where T : TokenBase
        {
            var tokenizedLines = new Queue<string>(tokenizedInput.ReadLines(Encoding.UTF8));

            foreach (var untokenizedLine in untokenizedInput.ReadLines(Encoding.UTF8))
            {
                var tokens = tokenizer.Tokenize(untokenizedLine);

                foreach (var token in tokens)
                {
                    var tokenLine = tokenizedLines.Dequeue();

                    // TODO: Verify if this tab handling is correct...
                    var parts = Tab.Split(tokenLine, 2);
                    var surface = parts[0];
                    var features = parts[1];

                    token.Surface.Is(surface);
                    token.GetAllFeatures().Is(features);
                }
            }
        }

        public static void IsSameMultiThreadedTokenizedStream<T>(this TokenizerBase<T> tokenizer, int numThreads, int perThreadRuns, string tokenizedInputResource, string untokenizedInputResource) where T : TokenBase
        {
            var threads = new Thread[numThreads];
            var waitHandles = new WaitHandle[numThreads];

            for (var i = 0; i < numThreads; i++)
            {
                var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                var thread = new Thread(() =>
                {
                    using (var tokenizedInput = GetResource(tokenizedInputResource))
                    using (var untokenizedInput = GetResource(untokenizedInputResource))
                    {
                        tokenizer.IsSameTokenizedStream(tokenizedInput, untokenizedInput);
                    }
                    handle.Set();
                });
                thread.Start();

                threads[i] = thread;
                waitHandles[i] = handle;
            }

            WaitHandle.WaitAll(waitHandles);
        }

        public static void IsSameTokenFeatureLengths<T>(this TokenizerBase<T> tokenizer, string text) where T : TokenBase
        {
            var tokens = tokenizer.Tokenize(text);

            tokens.Select(t => t.GetAllFeaturesArray().Length).Distinct().Count().Is(1);
        }

        static Stream GetResource(string fileName)
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
