# 📊 TỔNG QUAN DỰ ÁN REVERSEMARKDOWN.NET

## 🎯 Mục Đích
**ReverseMarkdown.NET** là thư viện C# chuyển đổi HTML sang Markdown, sử dụng HtmlAgilityPack để parse DOM. Hỗ trợ GitHub Flavored Markdown (GFM) và Slack Flavored Markdown với khả năng tùy chỉnh cao.

---

## 🏗️ KIẾN TRÚC HỆ THỐNG

### Các Class Chính

| Class | Vai Trò |
|-------|---------|
| **Converter.cs** | Điều phối chính, quản lý registry của 30+ converters |
| **Config.cs** | 20+ tùy chọn cấu hình (GFM, Slack, base64 images, etc.) |
| **ConverterContext.cs** | Track ancestors với O(1) lookup để xử lý nested structures |
| **Cleaner.cs** | Pre/post processing (normalize spaces, clean HTML) |
| **ImageUtils.cs** | Xử lý base64 images, MIME type mapping |

### 30 Converters Chuyên Biệt

Trong thư mục **Converters/**:

**Text Formatting:**
- **Strong** - `<strong>`, `<b>` → `**text**`
- **Em** - `<em>`, `<i>` → `*text*`
- **S** - `<s>`, `<del>`, `<strike>` → `~~text~~`
- **Code** - `<code>` → `` `code` ``
- **Sup** - `<sup>` → `<sup>text</sup>`

**Block Elements:**
- **P** - `<p>` với xử lý indentation thông minh
- **H** - `<h1>`-`<h6>` → `# Heading`
- **Blockquote** - `<blockquote>` → `> text`
- **Pre** - `<pre>` → fenced code blocks
- **Div** - `<div>` với SuppressDivNewlines option
- **Aside** - `<aside>` tag
- **Hr** - `<hr>` → `* * *`

**Lists:**
- **Ol** - `<ol>` ordered list
- **Li** - `<li>` với GFM checkbox support
- **Dl**, **Dt**, **Dd** - Definition lists

**Tables:**
- **Table** - `<table>` với caption và nested table support
- **Tr** - Table rows với auto underline cho header
- **Td** - `<td>`, `<th>` với colspan support

**Media & Links:**
- **A** - `<a>` với SmartHrefHandling
- **Img** - `<img>` với base64 image handling

**Special:**
- **PassThrough** - Giữ nguyên HTML
- **ByPass** - Bỏ tag, giữ children
- **Drop** - Bỏ hoàn toàn
- **Ignore** - Ignore các tag không cần thiết
- **Br** - Line breaks
- **Text** - Text node processing

---

## 🔄 FLOW XỬ LÝ CHI TIẾT

### Quy Trình Chuyển Đổi Tổng Thể

```
HTML Input
    ↓
1. Pre-processing (Cleaner.PreTidy)
    ├── Normalize spaces (non-breaking → normal)
    ├── Clean tag borders (remove \n\t between tags)
    └── Remove comments (if configured)
    ↓
2. Parse HTML (HtmlAgilityPack)
    ├── Load HTML into HtmlDocument
    ├── Get DocumentNode
    └── Extract body node if exists
    ↓
3. Recursive Conversion (ConvertNode)
    ├── Lookup converter for tag
    ├── Enter context (track ancestors)
    ├── Converter.Convert(writer, node)
    ├── Process children recursively
    └── Leave context
    ↓
4. Post-processing
    ├── Cleanup multiple newlines
    ├── Slack tidy (if SlackFlavored)
    └── Trim unnecessary spaces (if configured)
    ↓
Markdown Output
```

### Converter Lookup Logic

```
Node tag name
    ↓
Is in PassThroughTags? → YES → PassThroughTagsConverter (Keep as HTML)
    ↓ NO
In Converters registry? → YES → Registered Converter
    ↓ NO
UnknownTags config?
    ├── PassThrough → Keep as HTML
    ├── Drop → Remove all
    ├── Bypass → Keep children only
    └── Raise → Throw UnknownTagException
```

### Ví Dụ Conversion

**Input HTML:**
```html
<p>This is <strong>bold</strong> and <em>italic</em> with <a href="http://example.com">link</a>.</p>
```

**Processing Steps:**
1. **P Converter** - Xử lý `<p>`, viết newline nếu cần
2. **Text Converter** - "This is "
3. **Strong Converter** - `**bold**` với whitespace guard
4. **Text Converter** - " and "
5. **Em Converter** - `*italic*` với whitespace guard
6. **Text Converter** - " with "
7. **A Converter** - `[link](http://example.com)`
8. **Text Converter** - "."

**Output Markdown:**
```markdown
This is **bold** and *italic* with [link](http://example.com).
```

---

## ⚙️ CẤU HÌNH QUAN TRỌNG

### GitHub vs Slack Flavored

| Feature | GitHub Flavored | Slack Flavored |
|---------|-----------------|----------------|
| Bold | `**text**` | `*text*` |
| Italic | `*text*` | `_text_` |
| Line break | `  \n` (2 spaces) | `\n` |
| Code blocks | ` ```lang ` | Fenced |
| Bullet | `-` | `•` |
| Tables | ✅ Supported | ❌ Exception |

### Base64 Image Handling

```csharp
// Option 1: Giữ nguyên trong markdown (mặc định)
Config.Base64Images = Base64ImageHandling.Include;
// → ![alt](data:image/png;base64,iVBORw0KG...)

// Option 2: Bỏ qua hoàn toàn
Config.Base64Images = Base64ImageHandling.Skip;
// → (no image)

// Option 3: Lưu ra file
Config.Base64Images = Base64ImageHandling.SaveToFile;
Config.Base64ImageSaveDirectory = "images/";
Config.Base64ImageFileNameGenerator = (index, mime) => $"image_{index}";
// → ![alt](images/image_1.png)
```

### Unknown Tags Handling

```csharp
// PassThrough: Giữ nguyên HTML
UnknownTags = UnknownTagsOption.PassThrough;
// <unknown>content</unknown>

// Drop: Bỏ cả tag và content
UnknownTags = UnknownTagsOption.Drop;
// (nothing)

// Bypass: Chỉ bỏ tag, giữ content
UnknownTags = UnknownTagsOption.Bypass;
// content

// Raise: Throw exception
UnknownTags = UnknownTagsOption.Raise;
// UnknownTagException
```

### SmartHrefHandling

Khi bật `Config.SmartHrefHandling = true`:
- `<a href="http://example.com">http://example.com</a>` → `http://example.com`
- `<a href="mailto:test@example.com">test@example.com</a>` → `test@example.com`
- `<a href="http://example.com">example.com</a>` → `http://example.com`

### Các Tùy Chọn Khác

```csharp
Config.RemoveComments = true;              // Bỏ HTML comments
Config.SuppressDivNewlines = false;        // Kiểm soát newlines trong div
Config.ListBulletChar = '-';               // Ký tự bullet cho unordered list
Config.TableWithoutHeaderRowHandling = ...; // Xử lý table không có header
Config.WhitelistUriSchemes = ["http", "https", "mailto"];  // URI scheme whitelist
Config.PassThroughTags = ["svg", "math"];  // Tags giữ nguyên HTML
```

---

## 🎨 DESIGN PATTERNS & ARCHITECTURE

### 1. Strategy Pattern
Mỗi converter là một strategy độc lập implement `IConverter` interface. `Converter` class hoạt động như context, chọn strategy phù hợp dựa trên HTML tag.

### 2. Registration Pattern
Converters tự đăng ký trong constructor:
```csharp
public Strong(Converter converter) : base(converter) {
    Converter.Register("strong", this);
    Converter.Register("b", this);
}
```

### 3. Visitor Pattern
Recursive tree traversal với `ConvertNodes()` giống visitor pattern, xử lý từng node trong DOM tree.

### 4. Builder Pattern
Sử dụng `StringBuilder` để build markdown output incrementally, tối ưu hóa performance.

### 5. Template Method Pattern
`ConverterBase` cung cấp template methods như `ConvertChildren()`, `TreatChildren()`, `IndentationFor()` mà các converter con sử dụng.

### 6. Context Pattern
`ConverterContext` track state khi traverse DOM tree, cho phép converters biết context (nested lists, tables, etc.)

---

## 🚀 PERFORMANCE OPTIMIZATION

### Memory Management
- **StringBuilder pooling**: Sử dụng capacity estimation để giảm allocation
- **O(1) ancestor lookup**: Dictionary-based tracking thay vì recursive search
- **Lazy evaluation**: Chỉ parse khi cần thiết

### String Operations
- **StringExtensions**: Specialized methods cho string manipulation
- **StringUtils**: Efficient URI parsing, markdown escaping
- **Inline optimizations**: Avoid unnecessary string allocations

### DOM Processing
- **HtmlAgilityPack**: Efficient HTML parsing
- **Single-pass conversion**: Recursive traversal một lần qua DOM tree
- **Context caching**: Reuse ancestor information

---

## 📚 HELPERS & UTILITIES

### Cleaner.cs
- `PreTidy()`: Chuẩn hóa HTML trước khi xử lý
  - Normalize spaces
  - Clean tag borders
  - Remove comments
- `PostTidy()`: Cleanup markdown output
- `SlackTidy()`: Xử lý đặc biệt cho Slack markdown

### ImageUtils.cs
- `IsDataUri()`: Kiểm tra data URI hợp lệ
- `SaveDataUriToFile()`: Lưu base64 image ra file
- `GetExtensionFromMimeType()`: Map MIME type sang extension

### StringExtensions.cs
- `TrimNewLines()`: Trim và remove line endings
- `EnumerateLines()`: Iterator để đọc từng dòng
- `NormalizeNewlines()`: Chuẩn hóa newlines
- `CompactHtml()`: Compact HTML cho nested tables

### StringUtils.cs
- `GetScheme()`: Extract URI scheme với xử lý đặc biệt
- `EscapeMarkdownCharsInLinkText()`: Escape ký tự đặc biệt
- `ParseStyle()`: Parse inline CSS styles
- `UnescapeHtml()`: HTML entity decoding

---

## 🧪 TESTING

### Test Structure
- **ConverterTests.cs**: Main test suite với 100+ test cases
- **Verified tests**: Sử dụng snapshot testing với `.verified.md` files
- **Bug regression tests**: Mỗi bug fix có dedicated test case

### Test Coverage Areas
- ✅ Basic text formatting (bold, italic, strikethrough)
- ✅ Complex nested structures
- ✅ Tables với các edge cases
- ✅ Lists (ordered, unordered, nested, checkboxes)
- ✅ Links và images (including base64)
- ✅ Code blocks (inline và fenced)
- ✅ Unknown tags handling
- ✅ GitHub/Slack flavored differences
- ✅ Whitespace handling edge cases

### Benchmark
- **ReverseMarkdown.Benchmark**: Performance benchmarking project
- Sử dụng BenchmarkDotNet
- So sánh performance với các version khác nhau

---

## 📝 USE CASES

### 1. Content Migration
Chuyển đổi nội dung từ CMS/websites sang Markdown:
```csharp
var converter = new Converter();
var markdown = converter.Convert(htmlContent);
```

### 2. Documentation Generation
Tạo markdown documentation từ HTML exports:
```csharp
var config = new Config {
    GithubFlavored = true,
    RemoveComments = true
};
var converter = new Converter(config);
```

### 3. Email-to-Markdown
Chuyển đổi HTML emails:
```csharp
var config = new Config {
    SmartHrefHandling = true,
    Base64Images = Base64ImageHandling.SaveToFile,
    Base64ImageSaveDirectory = "email-images/"
};
```

### 4. Web Scraping
Scrape web content với markdown output:
```csharp
var config = new Config {
    UnknownTags = UnknownTagsOption.Bypass,
    WhitelistUriSchemes = new[] { "http", "https" }
};
```

### 5. Technical Writing Automation
Tự động hóa việc tạo technical documentation:
```csharp
var config = new Config {
    GithubFlavored = true,
    TableWithoutHeaderRowHandling = TableWithoutHeaderRowHandlingOption.EmptyRow
};
```

---

## 🔧 CUSTOMIZATION & EXTENSION

### Tạo Custom Converter

```csharp
public class CustomTag : ConverterBase
{
    public CustomTag(Converter converter) : base(converter)
    {
        Converter.Register("customtag", this);
    }

    public override string Convert(StringBuilder writer, HtmlNode node)
    {
        // Custom conversion logic
        writer.Append("<!-- custom -->");
        ConvertChildren(writer, node);
        writer.Append("<!-- /custom -->");
        return writer.ToString();
    }
}

// Sử dụng
var converter = new Converter();
new CustomTag(converter); // Auto-register
var markdown = converter.Convert(html);
```

### Override Existing Converter

```csharp
// Tạo converter mới với behavior khác
public class MyStrongConverter : ConverterBase
{
    public MyStrongConverter(Converter converter) : base(converter)
    {
        // Override existing registration
        Converter.Register("strong", this);
    }

    public override string Convert(StringBuilder writer, HtmlNode node)
    {
        // Custom bold conversion
        writer.Append("__");
        ConvertChildren(writer, node);
        writer.Append("__");
        return writer.ToString();
    }
}
```

---

## 🎯 KEY FEATURES

### ✅ Robust HTML Parsing
- Sử dụng HtmlAgilityPack - battle-tested HTML parser
- Xử lý malformed HTML
- Support cho HTML entities

### ✅ Smart Whitespace Handling
- Preserve meaningful whitespace
- Clean up unnecessary newlines
- Proper indentation cho nested structures

### ✅ Nested Structures Support
- Tables trong tables
- Lists trong lists
- Blockquotes trong blockquotes
- Proper context tracking

### ✅ Extensible Architecture
- Dễ dàng thêm converter mới
- Override existing converters
- Custom configuration options

### ✅ Multiple Markdown Flavors
- Standard Markdown
- GitHub Flavored Markdown (GFM)
- Slack Flavored Markdown
- Customizable output format

### ✅ Advanced Features
- Base64 image extraction và storage
- SmartHrefHandling cho links
- URI scheme whitelisting
- Table without header handling
- Unknown tags handling strategies

---

## 🐛 NOTABLE BUG FIXES

Dựa trên test files, các bugs đã được fix:

- **Bug #255**: Table newline character issue
- **Bug #294**: Table bug với row superfluous newlines
- **Bug #391**: Anchor tag unnecessarily indented
- **Bug #393**: Regression với varying newlines
- **Bug #400**: Missing span space với italics
- **Bug #403**: Unexpected behaviour khi table body rows với TH cells

Mỗi bug đều có dedicated test case để đảm bảo không regression.

---

## 📦 PROJECT STRUCTURE

```
src/
├── ReverseMarkdown/              # Main library
│   ├── Converter.cs              # Core converter
│   ├── Config.cs                 # Configuration
│   ├── ConverterContext.cs       # Context tracking
│   ├── Cleaner.cs                # Pre/post processing
│   ├── ImageUtils.cs             # Image handling
│   ├── Converters/               # 30+ converters
│   │   ├── A.cs, Blockquote.cs, Code.cs, ...
│   └── Helpers/                  # Utility helpers
│       ├── StringExtensions.cs
│       ├── StringUtils.cs
│       └── ...
├── ReverseMarkdown.Test/         # Test suite
│   ├── ConverterTests.cs         # Main tests
│   └── *.verified.md             # Snapshot tests
└── ReverseMarkdown.Benchmark/    # Performance benchmarks
    ├── CompareBenchmark.cs
    └── Files/                    # Test files
```

---

## 🚀 GETTING STARTED

### Basic Usage

```csharp
using ReverseMarkdown;

// Simple conversion
var converter = new Converter();
var markdown = converter.Convert("<p>Hello <strong>world</strong>!</p>");
// Output: "Hello **world**!"
```

### With Configuration

```csharp
var config = new Config
{
    GithubFlavored = true,
    SmartHrefHandling = true,
    RemoveComments = true,
    UnknownTags = UnknownTagsOption.Bypass
};

var converter = new Converter(config);
var markdown = converter.Convert(htmlContent);
```

---

## 🎓 KẾT LUẬN

**ReverseMarkdown.NET** là một thư viện được thiết kế xuất sắc với:

### Điểm Mạnh
✅ **Kiến trúc mở rộng**: Dễ dàng thêm converter mới  
✅ **Performance cao**: Optimized memory allocation, O(1) lookups  
✅ **Tính linh hoạt**: 20+ configuration options  
✅ **Standards support**: GitHub/Slack Flavored Markdown  
✅ **Robust handling**: Smart whitespace, nested structures, edge cases  
✅ **Test coverage**: Extensive test suite với verified snapshots  
✅ **Well-documented**: Clear code structure với meaningful names  

### Best Practices
- Sử dụng configuration phù hợp với use case
- Test với edge cases (nested structures, malformed HTML)
- Custom converters khi cần behavior đặc biệt
- Performance testing với large documents

### Future Enhancements (Suggestions)
- StringBuilder pooling để optimize memory hơn nữa
- Async API cho large document processing
- Streaming API cho memory efficiency
- Plugin system cho converters
- More markdown flavors (CommonMark, etc.)

---

*Tài liệu này được tạo ngày 26/01/2026*
*Dự án: ReverseMarkdown.NET - Folk từ https://github.com/mysticmind/reversemarkdown-net*
