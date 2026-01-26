# Plan: Hỗ trợ Table HTML với colspan/rowspan

## 📋 Tổng Quan

### Vấn đề
- Markdown không hỗ trợ `colspan` và `rowspan` trong table
- Khi convert HTML table có cấu trúc phức tạp sang Markdown → mất thông tin, hiển thị sai
- AI khó đọc hiểu các table Markdown khi cấu trúc gốc có colspan/rowspan

### Giải pháp
- Thêm config option để preserve table dưới dạng HTML khi detect colspan/rowspan
- Clean các attributes không cần thiết (style, class, id...), chỉ giữ colspan và rowspan
- Cell content vẫn được convert sang Markdown (bold, italic, links...)
- Table đơn giản vẫn convert sang Markdown như hiện tại

## 🎯 Implementation Plan

### Step 1: Thêm Config Option

**File**: `src/ReverseMarkdown/Config.cs`

**Công việc**:
1. Thêm enum mới cho table handling:
```csharp
/// <summary>
/// Defines how to handle complex HTML tables (with colspan/rowspan)
/// </summary>
public enum TableComplexHandlingOption
{
    /// <summary>
    /// Convert to Markdown table (may lose colspan/rowspan structure)
    /// </summary>
    ConvertToMarkdown,
    
    /// <summary>
    /// Preserve as cleaned HTML when colspan/rowspan detected
    /// </summary>
    PreserveHtmlWhenComplex,
    
    /// <summary>
    /// Always preserve table as cleaned HTML
    /// </summary>
    AlwaysPreserveHtml
}
```

2. Thêm property với default value:
```csharp
/// <summary>
/// Gets or sets how to handle complex HTML tables.
/// Default is ConvertToMarkdown for backward compatibility.
/// </summary>
public TableComplexHandlingOption TableComplexHandling { get; set; } = 
    TableComplexHandlingOption.ConvertToMarkdown;
```

**Lý do thiết kế**:
- `ConvertToMarkdown` (default): Giữ nguyên behavior hiện tại, backward compatible
- `PreserveHtmlWhenComplex`: Chỉ preserve khi cần (có colspan/rowspan), optimize output
- `AlwaysPreserveHtml`: Force preserve, hữu ích khi cần table HTML trong Markdown

---

### Step 2: Thêm Helper Methods trong Table Converter

**File**: `src/ReverseMarkdown/Converters/Table.cs`

#### 2.1. Method detect complex structure
```csharp
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
```

#### 2.2. Method clean table HTML
```csharp
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
```

**Lưu ý**:
- Dùng `TreatChildrenAsString()` từ base class để convert cell content
- Điều này đảm bảo **bold**, *italic*, [links] vẫn được convert sang Markdown
- Output HTML được compact (không có newlines thừa)

---

### Step 3: Modify Table.Convert() Logic

**File**: `src/ReverseMarkdown/Converters/Table.cs`

**Vị trí**: Đầu method `Convert()`, ngay sau check SlackFlavored

```csharp
public override void Convert(TextWriter writer, HtmlNode node)
{
    if (Converter.Config.SlackFlavored)
    {
        throw new SlackUnsupportedTagException(node.Name);
    }

    // NEW: Check complex table handling
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

    // Existing logic cho nested tables
    if (Context.AncestorsAny("table"))
    {
        writer.Write(node.OuterHtml.CompactHtmlForMarkdown());
        return;
    }

    // ... phần còn lại giữ nguyên
}
```

**Lý do đặt ở đầu**:
- Detect và handle complex table càng sớm càng tốt
- Tránh làm thay đổi DOM (như remove caption) khi cần preserve HTML
- Nested table check vẫn cần giữ vì có thể kết hợp với config mới

---

### Step 4: Viết Test Cases

**File**: `src/ReverseMarkdown.Test/ConverterTests.cs`

#### Test 1: Table với colspan - PreserveHtmlWhenComplex
```csharp
[Fact]
public Task WhenTable_HasColSpan_AndPreserveHtmlWhenComplex_ThenOutputCleanHtml()
{
    var html = @"
        <table style='border: 1px solid' class='table-class' id='my-table'>
            <thead>
                <tr>
                    <th colspan='2' style='text-align: center'>Header</th>
                    <th>Col3</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td><strong>Bold</strong></td>
                    <td><em>Italic</em></td>
                    <td>Normal</td>
                </tr>
            </tbody>
        </table>";
    
    var config = new Config
    {
        TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex
    };
    
    return CheckConversion(html, config);
}
```

**Expected output** (`.verified.md`):
```html
<table><thead><tr><th colspan="2">Header</th><th>Col3</th></tr></thead><tbody><tr><td>**Bold**</td><td>*Italic*</td><td>Normal</td></tr></tbody></table>
```

#### Test 2: Table với rowspan
```csharp
[Fact]
public Task WhenTable_HasRowSpan_AndPreserveHtmlWhenComplex_ThenOutputCleanHtml()
{
    var html = @"
        <table>
            <tr>
                <td rowspan='2'>Merged</td>
                <td>Cell 1</td>
            </tr>
            <tr>
                <td>Cell 2</td>
            </tr>
        </table>";
    
    var config = new Config
    {
        TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex
    };
    
    return CheckConversion(html, config);
}
```

#### Test 3: Table đơn giản với PreserveHtmlWhenComplex → vẫn convert Markdown
```csharp
[Fact]
public Task WhenTable_NoComplexStructure_AndPreserveHtmlWhenComplex_ThenConvertToMarkdown()
{
    var html = @"
        <table>
            <tr>
                <th>Col1</th>
                <th>Col2</th>
            </tr>
            <tr>
                <td>A</td>
                <td>B</td>
            </tr>
        </table>";
    
    var config = new Config
    {
        TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex
    };
    
    return CheckConversion(html, config);
}
```

**Expected output**: Markdown table như hiện tại

#### Test 4: AlwaysPreserveHtml với simple table
```csharp
[Fact]
public Task WhenTable_SimpleTable_AndAlwaysPreserveHtml_ThenOutputCleanHtml()
{
    var html = @"
        <table style='width: 100%'>
            <tr>
                <td>A</td>
                <td>B</td>
            </tr>
        </table>";
    
    var config = new Config
    {
        TableComplexHandling = Config.TableComplexHandlingOption.AlwaysPreserveHtml
    };
    
    return CheckConversion(html, config);
}
```

#### Test 5: Table với caption
```csharp
[Fact]
public Task WhenTable_HasCaption_AndPreserveHtml_ThenIncludeCaption()
{
    var html = @"
        <table>
            <caption>Table Caption</caption>
            <tr>
                <th colspan='2'>Header</th>
            </tr>
        </table>";
    
    var config = new Config
    {
        TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex
    };
    
    return CheckConversion(html, config);
}
```

#### Test 6: Cell content với nested HTML
```csharp
[Fact]
public Task WhenTable_CellHasNestedHtml_AndPreserveHtml_ThenConvertCellContent()
{
    var html = @"
        <table>
            <tr>
                <td colspan='2'>
                    <p>Paragraph with <strong>bold</strong> and <a href='url'>link</a></p>
                </td>
            </tr>
        </table>";
    
    var config = new Config
    {
        TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex
    };
    
    return CheckConversion(html, config);
}
```

---

## 🔍 Technical Considerations

### 1. Cell Content Processing
**Quyết định**: Convert cell content sang Markdown bằng `TreatChildrenAsString()`

**Lý do**:
- Giữ được formatting như **bold**, *italic*, [links]
- Output dễ đọc hơn khi embed trong HTML
- AI có thể đọc hiểu tốt hơn

**Alternative**: Giữ nguyên HTML bằng `InnerHtml`
- Ưu: Giữ nguyên 100% original
- Nhược: Output phức tạp, khó đọc

### 2. HTML Compaction
**Quyết định**: Compact HTML (remove newlines, collapse whitespace)

**Lý do**:
- Giống pattern hiện tại cho nested tables
- Output gọn gàng, không tốn space trong Markdown
- Dễ dàng copy/paste

**Implementation**: Có thể dùng `CompactHtmlForMarkdown()` extension method đã có

### 3. SlackFlavored Mode
**Quyết định**: Giữ nguyên check và throw exception

**Lý do**:
- Slack không hỗ trợ HTML tables
- Không thay đổi behavior hiện tại
- User phải tự quyết định: tắt SlackFlavored hoặc không dùng tables

### 4. Nested Tables
**Quyết định**: Nested table check vẫn có hiệu lực

**Flow**:
1. Check SlackFlavored → throw nếu true
2. Check TableComplexHandling → preserve HTML nếu cần
3. Check nested table → compact HTML theo logic cũ
4. Process markdown table

**Lý do**: Các checks không conflict nhau, vẫn cover edge cases

### 5. Attributes to Keep
**Quyết định**: Chỉ giữ `colspan` và `rowspan`, bỏ tất cả còn lại

**Attributes bị loại bỏ**:
- `style`, `class`, `id`
- `width`, `height`, `align`, `valign`
- `bgcolor`, `border`
- Custom data attributes (`data-*`)

**Lưu ý**: Không giữ `colspan="1"` và `rowspan="1"` (default values)

### 6. Empty Cells
**Quyết định**: Output `<td></td>` cho empty cells

**Không cần**: Thêm `&nbsp;` hay placeholder
**Lý do**: HTML spec cho phép empty cells, browsers render OK

---

## ✅ Acceptance Criteria

1. ✓ Config option mới với 3 modes hoạt động đúng
2. ✓ Detect colspan/rowspan chính xác (XPath query)
3. ✓ Clean attributes - chỉ giữ colspan/rowspan
4. ✓ Cell content được convert sang Markdown
5. ✓ Table đơn giản với PreserveHtmlWhenComplex vẫn ra Markdown
6. ✓ Backward compatible - default behavior không đổi
7. ✓ Test coverage đầy đủ cho tất cả scenarios
8. ✓ Verified snapshots cho expected output

---

## 📝 Summary

**Tính năng**: Preserve HTML table khi có colspan/rowspan, clean attributes không cần thiết

**API mới**:
```csharp
var config = new Config
{
    TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex
};
```

**Behavior**:
- `ConvertToMarkdown` (default): Như hiện tại, backward compatible
- `PreserveHtmlWhenComplex`: Chỉ preserve khi có colspan/rowspan
- `AlwaysPreserveHtml`: Luôn preserve table HTML

**Output quality**:
- Clean HTML: chỉ colspan/rowspan
- Cell content: Markdown formatted
- Compact: không whitespace thừa

**Files cần sửa**:
1. `Config.cs` - Add enum + property
2. `Converters/Table.cs` - Add logic + helper methods
3. `ConverterTests.cs` - Add 6+ test cases

**Backward compatibility**: ✓ Guaranteed (default behavior không đổi)