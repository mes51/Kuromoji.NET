using Kuromoji.NET.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuromoji.NET.Extentions;
using Kuromoji.NET.IO;

namespace Kuromoji.NET.Dict
{
    public class ConnectionCosts
    {
        public const string ConnectionCostsFileName = "connectionCosts.bin";

        int Size { get; }

        short[] Costs { get; }

        public int this[int forwardId, int backwardId]
        {
            get
            {
                return Costs[backwardId + forwardId * Size];
            }
        }

        public ConnectionCosts(int size, short[] costs)
        {
            Size = size;
            Costs = costs;
        }

        public static ConnectionCosts NewInstance(IResourceResolver resolver)
        {
            using (var resource = resolver.Resolve(ConnectionCostsFileName))
            {
                return Read(resource);
            }
        }

        static ConnectionCosts Read(Stream input)
        {
            var size = input.ReadInt32();
            var length = input.ReadInt32();

            var costs = new short[length];
            input.ReadShortArray(costs);

            return new ConnectionCosts(size, costs);
        }
    }
}
