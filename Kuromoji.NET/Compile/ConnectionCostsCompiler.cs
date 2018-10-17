using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;

namespace Kuromoji.NET.Compile
{
    public class ConnectionCostsCompiler : ICompiler, IDisposable
    {
        const int ShortBytes = sizeof(short);

        public int Cardinality { get; private set; }

        int BufferSize { get; set; }

        public short[] Costs { get; private set; }

        Stream Output { get; }

        bool LeaveOpen { get; }

        bool Disposed { get; set; }

        public ConnectionCostsCompiler(Stream output) : this(output, false) { }

        public ConnectionCostsCompiler(Stream output, bool leaveOpen)
        {
            Output = output;
            LeaveOpen = leaveOpen;
        }

        public void ReadCosts(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                var line = reader.ReadLine().TrimEnd();
                var cardinalities = Regexs.CustomSegmentationSpliter.Split(line);

                var forwardSize = int.Parse(cardinalities[0]);
                var backwardSize = int.Parse(cardinalities[1]);

                Cardinality = backwardSize;
                BufferSize = forwardSize * backwardSize;
                Costs = new short[BufferSize];

                while ((line = reader.ReadLine()) != null)
                {
                    var fields = Regexs.CustomSegmentationSpliter.Split(line);

                    var forwardId = short.Parse(fields[0]);
                    var backwardId = short.Parse(fields[1]);
                    var cost = short.Parse(fields[2]);

                    PutCost(forwardId, backwardId, cost);
                }
            }
        }

        public void PutCost(short forwardId, short backwardId, short cost)
        {
            Costs[backwardId + forwardId * Cardinality] = cost;
        }

        public void Compile()
        {
            Output.Write(Cardinality);
            Output.Write(BufferSize * ShortBytes);
            Output.Write(Costs);
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (!LeaveOpen)
                {
                    Output.Dispose();
                }
                Disposed = true;
            }
        }
    }
}
