using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown.Helpers;


namespace ReverseMarkdown.Converters {
    public class Table : ConverterBase {
        public Table(Converter converter) : base(converter)
        {
            Converter.Register("table", this);
        }

        public override void Convert(TextWriter writer, HtmlNode node)
        {
            if (Converter.Config.SlackFlavored) {
                throw new SlackUnsupportedTagException(node.Name);
            }

            // Check complex table handling - preserve as HTML if needed
            var tableHandling = Converter.Config.TableComplexHandling;
            
            if (tableHandling != Config.TableComplexHandlingOption.ConvertToMarkdown)
            {
                bool shouldPreserveHtml = false;
                
                if (tableHandling == Config.TableComplexHandlingOption.AlwaysPreserveHtml)
                {
                    shouldPreserveHtml = true;
                }
                else if (tableHandling == Config.TableComplexHandlingOption.PreserveHtmlWhenComplex)
                {
                    shouldPreserveHtml = HasComplexStructure(node);
                }
                
                if (shouldPreserveHtml)
                {
                    writer.WriteLine();
                    writer.Write(BuildCleanTableHtml(node));
                    writer.WriteLine();
                    return; // Early return, không xử lý markdown
                }
            }

            // Tables inside tables are not supported as markdown, so leave as HTML
            if (Context.AncestorsAny("table")) {
                // Compact the nested table HTML to prevent breaking the markdown table
                writer.Write(node.OuterHtml.CompactHtmlForMarkdown());
                return;
            }
            
            var captionNode = node.SelectSingleNode("caption");
            var captionText = captionNode?.InnerText?.Trim();
            captionNode?.Remove();

            // if table does not have a header row , add empty header row if set in config
            var useEmptyRowForHeader = (
                this.Converter.Config.TableWithoutHeaderRowHandling == Config.TableWithoutHeaderRowHandlingOption.EmptyRow
            );

            var emptyHeaderRow = HasNoTableHeaderRow(node) && useEmptyRowForHeader
                ? EmptyHeader(node)
                : string.Empty;
            
            // add caption text as a paragraph above table
            if (captionText != string.Empty)
            {
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine(captionText);
            }

            writer.WriteLine();
            writer.WriteLine();

            writer.Write(emptyHeaderRow);
            TreatChildren(writer, node);
            writer.WriteLine();
        }

        private static bool HasNoTableHeaderRow(HtmlNode node)
        {
            var thNode = node.SelectNodes("//th")?.FirstOrDefault();
            return thNode == null;
        }

        private static string EmptyHeader(HtmlNode node)
        {
            var firstRow = node.SelectNodes("//tr")?.FirstOrDefault();

            if (firstRow == null) {
                return string.Empty;
            }

            var colCount = firstRow.ChildNodes.Count(n => n.Name.Contains("td") || n.Name.Contains("th"));

            var headerRowItems = new List<string>();
            var underlineRowItems = new List<string>();

            for (var i = 0; i < colCount; i++) {
                headerRowItems.Add("<!---->");
                underlineRowItems.Add("---");
            }

            var headerRow = $"| {string.Join(" | ", headerRowItems)} |{Environment.NewLine}";
            var underlineRow = $"| {string.Join(" | ", underlineRowItems)} |{Environment.NewLine}";

            return headerRow + underlineRow;
        }

        /// <summary>
        /// Kiểm tra xem table có chứa colspan hoặc rowspan không
        /// </summary>
        private bool HasComplexStructure(HtmlNode tableNode)
        {
            // Tìm tất cả td/th có colspan hoặc rowspan
            var complexCells = tableNode.SelectNodes(
                ".//td[@colspan] | .//td[@rowspan] | .//th[@colspan] | .//th[@rowspan]"
            );
            
            return complexCells != null && complexCells.Count > 0;
        }

        /// <summary>
        /// Clean table HTML, chỉ giữ colspan và rowspan attributes
        /// Cell content được convert sang Markdown
        /// </summary>
        private string BuildCleanTableHtml(HtmlNode tableNode)
        {
            var result = new StringBuilder();
            result.Append("<table>");
            
            // Process caption nếu có
            var captionNode = tableNode.SelectSingleNode("caption");
            if (captionNode != null)
            {
                result.Append("<caption>");
                result.Append(captionNode.InnerText.Trim());
                result.Append("</caption>");
            }
            
            // Process table children
            foreach (var child in tableNode.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                {
                    ProcessTableSection(child, result);
                }
            }
            
            result.Append("</table>");
            return result.ToString();
        }

        private void ProcessTableSection(HtmlNode node, StringBuilder result)
        {
            var tagName = node.Name.ToLower();
            
            // Skip caption (đã xử lý ở trên)
            if (tagName == "caption") return;
            
            // Xử lý thead, tbody, tfoot
            if (tagName is "thead" or "tbody" or "tfoot")
            {
                result.Append($"<{tagName}>");
                foreach (var child in node.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element)
                    {
                        ProcessTableRow(child, result);
                    }
                }
                result.Append($"</{tagName}>");
            }
            // Xử lý tr trực tiếp trong table
            else if (tagName == "tr")
            {
                ProcessTableRow(node, result);
            }
        }

        private void ProcessTableRow(HtmlNode rowNode, StringBuilder result)
        {
            if (rowNode.Name.ToLower() != "tr") return;
            
            result.Append("<tr>");
            
            foreach (var cell in rowNode.ChildNodes)
            {
                if (cell.NodeType == HtmlNodeType.Element && 
                    cell.Name.ToLower() is "td" or "th")
                {
                    ProcessTableCell(cell, result);
                }
            }
            
            result.Append("</tr>");
        }

        private void ProcessTableCell(HtmlNode cellNode, StringBuilder result)
        {
            var tagName = cellNode.Name.ToLower();
            
            result.Append($"<{tagName}");
            
            // Chỉ giữ colspan và rowspan (nếu khác 1)
            var colspan = cellNode.GetAttributeValue("colspan", "");
            var rowspan = cellNode.GetAttributeValue("rowspan", "");
            
            if (!string.IsNullOrEmpty(colspan) && colspan != "1")
            {
                result.Append($" colspan=\"{colspan}\"");
            }
            if (!string.IsNullOrEmpty(rowspan) && rowspan != "1")
            {
                result.Append($" rowspan=\"{rowspan}\"");
            }
            
            result.Append(">");
            
            // Convert cell content bằng ReverseMarkdown
            // Điều này giữ được markdown formatting (bold, italic, links...)
            var cellContent = TreatChildrenAsString(cellNode).Trim();
            result.Append(cellContent);
            
            result.Append($"</{tagName}>");
        }
    }
}
