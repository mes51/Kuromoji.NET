using Kuromoji.NET.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Viterbi
{
    public class ViterbiFormatter
    {
        const string BOSLabel = "BOS";

        const string EOSLabel = "EOS";

        const string FontName = "Helvetica";

        ConnectionCosts Costs { get; }

        Dictionary<string, ViterbiNode> NodeMap { get; } = new Dictionary<string, ViterbiNode>();

        Dictionary<string, string> BestPathMap { get; } = new Dictionary<string, string>();

        bool FoundBOS { get; set; }

        public ViterbiFormatter(ConnectionCosts costs)
        {
            Costs = costs;
        }

        public string Format(ViterbiLattice lattice)
        {
            return Format(lattice, null);
        }

        public string Format(ViterbiLattice lattice, List<ViterbiNode> bestPath)
        {
            InitBestPathMap(bestPath);

            var builder = new StringBuilder();

            builder.Append(FormatHeader());
            builder.Append(FormatNodes(lattice));
            builder.Append(FormatTrailer());

            return builder.ToString();
        }

        void InitBestPathMap(List<ViterbiNode> bestPath)
        {
            BestPathMap.Clear();

            if (bestPath == null)
            {
                return;
            }

            for (var i = 0; i < bestPath.Count; i++)
            {
                var from = bestPath[i];
                var to = bestPath[i + 1];

                var fromId = GetNodeId(from);
                var toId = GetNodeId(to);

                BestPathMap.Add(fromId, toId);
            }
        }

        string FormatNodes(ViterbiLattice lattice)
        {
            var startsArray = lattice.StartIndexArr;
            var endsArray = lattice.EndIndexArr;
            NodeMap.Clear();
            FoundBOS = false;

            var builder = new StringBuilder();
            for (var i = 1; i < endsArray.Length; i++)
            {
                if (endsArray[i] == null || startsArray[i] == null)
                {
                    continue;
                }

                for (var j = 0; j < endsArray[i].Length; j++)
                {
                    var from = endsArray[i][j];
                    if (from == null)
                    {
                        continue;
                    }

                    builder.Append(FormatNodeIfNew(from));
                    for (var k = 0; k < startsArray[i].Length; k++)
                    {
                        var to = startsArray[i][k];
                        if (to == null)
                        {
                            break;
                        }
                        builder.Append(FormatNodeIfNew(to));
                        builder.Append(FormatEdge(from, to));
                    }
                }
            }

            return builder.ToString();
        }

        string FormatNodeIfNew(ViterbiNode node)
        {
            var nodeId = GetNodeId(node);
            if (!NodeMap.ContainsKey(nodeId))
            {
                NodeMap.Add(node.GetHashCode().ToString(), node);
                return FormatNode(node);
            }
            else
            {
                return "";
            }
        }

        string FormatEdge(ViterbiNode from, ViterbiNode to)
        {
            if (BestPathMap.ContainsKey(GetNodeId(from)) && BestPathMap[GetNodeId(from)] == GetNodeId(to))
            {
                return FormatEdge(from, to, "color=\"#40e050\" fontcolor=\"#40a050\" penwidth=3 fontsize=20 ");

            }
            else
            {
                return FormatEdge(from, to, "");
            }
        }

        string FormatEdge(ViterbiNode from, ViterbiNode to, String attributes)
        {
            var builder = new StringBuilder();

            builder.Append(GetNodeId(from));
            builder.Append(" -> ");
            builder.Append(GetNodeId(to));
            builder.Append(" [ ");
            builder.Append("label=\"");
            builder.Append(GetCost(from, to));
            builder.Append("\"");
            builder.Append(" ");
            builder.Append(attributes);
            builder.Append(" ");
            builder.Append(" ]");
            builder.Append("\n");

            return builder.ToString();
        }

        string FormatNode(ViterbiNode node)
        {
            var builder = new StringBuilder();

            builder.Append("\"");
            builder.Append(GetNodeId(node));
            builder.Append("\"");
            builder.Append(" [ ");
            builder.Append("label=");
            builder.Append(FormatNodeLabel(node));
            if (node.Type == ViterbiNode.NodeType.User)
            {
                builder.Append(" fillcolor=\"#e8f8e8\"");
            }
            else if (node.Type == ViterbiNode.NodeType.Unknown)
            {
                builder.Append(" fillcolor=\"#f8e8f8\"");
            }
            else if (node.Type == ViterbiNode.NodeType.Inserted)
            {
                builder.Append(" fillcolor=\"#ffe8e8\"");
            }
            builder.Append(" ]");

            return builder.ToString();
        }

        string FormatNodeLabel(ViterbiNode node)
        {
            var builder = new StringBuilder();

            builder.Append("<<table border=\"0\" cellborder=\"0\">");
            builder.Append("<tr><td>");
            builder.Append(GetNodeLabel(node));
            builder.Append("</td></tr>");
            builder.Append("<tr><td>");
            builder.Append("<font color=\"blue\">");
            builder.Append(node.WordCost);
            builder.Append("</font>");
            builder.Append("</td></tr>");
            builder.Append("</table>>");

            return builder.ToString();
        }

        string GetNodeId(ViterbiNode node)
        {
            return node.GetHashCode().ToString();
        }

        string GetNodeLabel(ViterbiNode node)
        {
            if (node.Type == ViterbiNode.NodeType.Known && node.WordId == 0)
            {
                if (FoundBOS)
                {
                    return EOSLabel;
                }
                else
                {
                    this.FoundBOS = true;
                    return BOSLabel;
                }
            }
            else
            {
                return node.Surface;
            }
        }

        int GetCost(ViterbiNode from, ViterbiNode to)
        {
            return Costs[from.LeftId, to.RightId];
        }

        static string FormatHeader()
        {
            var builder = new StringBuilder();

            builder.Append("digraph viterbi {\n");
            builder.Append("graph [ fontsize=30 labelloc=\"t\" label=\"\" splines=true overlap=false rankdir = \"LR\" ];\n");
            builder.Append("# A2 paper size\n");
            builder.Append("size = \"34.4,16.5\";\n");
            builder.Append("# try to fill paper\n");
            builder.Append("ratio = fill;\n");
            builder.Append("edge [ fontname=\"" + FontName + "\" fontcolor=\"red\" color=\"#606060\" ]\n");
            builder.Append("node [ style=\"filled\" fillcolor=\"#e8e8f0\" shape=\"Mrecord\" fontname=\"" + FontName + "\" ]\n");

            return builder.ToString();
        }

        static string FormatTrailer() => "}";
    }
}
