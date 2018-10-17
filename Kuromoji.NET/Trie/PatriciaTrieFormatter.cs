using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Trie
{
    public static class PatriciaTrieFormatter
    {
        const string FontName = "Helvetica";

        /// <summary>
        /// Format trie
        /// </summary>
        /// <param name="trie">trie to format</param>
        /// <returns>formatted trie, not null</returns>
        public static string Format<T>(PatriciaTrie<T> trie)
        {
            return Format(trie, true);
        }

        /// <summary>
        /// Format trie
        /// </summary>
        /// <param name="trie">trie to format</param>
        /// <param name="formatBitString">true if the bits for this key should be included in the node</param>
        /// <returns>formatted trie, not null</returns>
        public static string Format<T>(PatriciaTrie<T> trie, bool formatBitString)
        {
            var builder = new StringBuilder();

            builder.Append(FormatHeader());
            builder.Append(FormatNode(trie.Root.Left, -1, trie.KeyMapper, formatBitString));
            builder.Append(FormatTrailer());

            return builder.ToString();
        }

        public static void Format<T>(PatriciaTrie<T> trie, string fileName)
        {
            Format(trie, true, fileName);
        }

        public static void Format<T>(PatriciaTrie<T> trie, bool formatBitString, string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(fs, Encoding.Unicode))
            {
                writer.Write(Format(trie, formatBitString));
            }
        }

        /// <summary>
        /// Format header
        /// </summary>
        /// <returns>formatted header, not null</returns>
        static string FormatHeader()
        {
            return "digraph patricia {\n" +
                "nodesep=1.5;" +
                "node [ style=\"filled\" fillcolor=\"#e8e8f0\" shape=\"Mrecord\" fontname=\"" + FontName + "\" ]\n";
        }

        /// <summary>
        /// Format trailer
        /// </summary>
        /// <returns>formatted trailer</returns>
        static string FormatTrailer()
        {
            return "}";
        }

        /// <summary>
        /// Formats nodes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">node to format</param>
        /// <param name="bit">bit for this node</param>
        /// <param name="keyMapper">keymapper to map keys to bits</param>
        /// <param name="formatBitString">true if the bits for this key should be included in the node</param>
        /// <returns>formatted node, not null</returns>
        static string FormatNode<T>(PatriciaTrie<T>.PatriciaNode node, int bit, PatriciaTrie<T>.IKeyMapper<string> keyMapper, bool formatBitString)
        {
            if (node.Bit <= bit)
            {
                return "";
            }
            else
            {
                var buffer = new StringBuilder();
                buffer.AppendLine($"\"{GetNodeId(node)}\" [ label={FormatNodeLabel(node, keyMapper, formatBitString)} ]");

                buffer.Append(FormatPointer(node, node.Left, "l", "sw"));
                buffer.Append(FormatPointer(node, node.Right, "r", "se"));

                buffer.Append(FormatNode(node.Left, node.Bit, keyMapper, formatBitString));
                buffer.Append(FormatNode(node.Right, node.Bit, keyMapper, formatBitString));

                return buffer.ToString();
            }
        }

        /// <summary>
        /// Formats a link between two nodes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from">from node</param>
        /// <param name="to">to node</param>
        /// <param name="label">label for this link</param>
        /// <param name="tailport">tail port to use when formatting (dot-specific, "sw" or "se)</param>
        /// <returns>formatted link, not null</returns>
        static string FormatPointer<T>(PatriciaTrie<T>.PatriciaNode from, PatriciaTrie<T>.PatriciaNode to, string label, string tailport)
        {
            return $"{GetNodeId(from)} -> {GetNodeId(to)} [ label=\"{label}\" tailport=\"{tailport}\" fontcolor=\"#666666\"  ]\n";
        }

        /// <summary>
        /// Format node label
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">node to format</param>
        /// <param name="keyMapper">keymapper to map keys to bits</param>
        /// <param name="formatBitString">true if the bits for this key should be included in the node</param>
        /// <returns>formatted formatted node, not null</returns>
        static string FormatNodeLabel<T>(PatriciaTrie<T>.PatriciaNode node, PatriciaTrie<T>.IKeyMapper<string> keyMapper, bool formatBitString)
        {
            var builder = new StringBuilder();
            builder.Append("<<table border=\"0\" cellborder=\"0\">");

            // Key
            builder.Append("<tr><td>");
            builder.Append("key: <font color=\"#00a000\">");
            builder.Append(GetNodeLabel(node));
            builder.Append("</font> </td></tr>");

            // Critical bit
            builder.Append("<tr><td>");
            builder.Append("bit: <font color=\"blue\">");
            builder.Append(node.Bit);
            builder.Append("</font> </td></tr>");

            // Bit string
            if (formatBitString)
            {
                builder.Append("<tr><td>");
                builder.Append("bitString: <font color=\"blue\">");
                var bitString = keyMapper.ToBitString(node.Key);
                int c = node.Bit + node.Bit / 4;
                builder.Append(bitString.Substring(0, c));
                builder.Append("<font color=\"red\">");
                builder.Append(bitString[c]);
                builder.Append("</font>");
                builder.Append(bitString.Substring(c + 1));
                builder.Append("</font> </td></tr>");
            }

            // Value
            builder.Append("<tr><td>");
            builder.Append("value: <font color=\"#00a0a0\">");
            builder.Append(node.Value);
            builder.Append("</font> </td></tr>");

            builder.Append("</table>>");

            return builder.ToString();
        }

        /// <summary>
        /// Get node label
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns>label, not null</returns>
        static string GetNodeLabel<T>(PatriciaTrie<T>.PatriciaNode node)
        {
            return node.Key;
        }

        /// <summary>
        /// Get node id used to distinguish nodes internally
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns>node id, not null</returns>
        static string GetNodeId<T>(PatriciaTrie<T>.PatriciaNode node)
        {
            if (node == null)
            {
                return "null";
            }
            else
            {
                return node.Key;
            }
        }
    }
}
