# Giới Thiệu ReverseMarkdown

[![Build status](https://github.com/mysticmind/reversemarkdown-net/actions/workflows/ci.yaml/badge.svg)](https://github.com/mysticmind/reversemarkdown-net/actions/workflows/ci.yaml) [![NuGet Version](https://badgen.net/nuget/v/reversemarkdown)](https://www.nuget.org/packages/ReverseMarkdown/)

ReverseMarkdown là thư viện chuyển đổi HTML sang Markdown được viết bằng C#. Chuyển đổi rất đáng tin cậy vì sử dụng thư viện HtmlAgilityPack (HAP) để duyệt HTML DOM.

**📦 Fork này:** Đây là fork từ [mysticmind/reversemarkdown-net](https://github.com/mysticmind/reversemarkdown-net) với các cải tiến:
- ✅ Hỗ trợ .NET Framework 4.8
- ✅ Table Complex Handling (preserve HTML khi có colspan/rowspan)
- ✅ Manual Test Console Application

📚 **Tài liệu:** [Build Instructions](BUILD.md) | [Changelog](CHANGELOG.md)

## Cách Sử Dụng

Cài đặt gói từ NuGet bằng lệnh `Install-Package ReverseMarkdown` hoặc clone kho lưu trữ và tự build.

<!-- snippet: Usage -->
<a id='snippet-Usage'></a>
```cs
var converter = new ReverseMarkdown.Converter();

string html = "This a sample <strong>paragraph</strong> from <a href=\"http://test.com\">my site</a>";

string result = converter.Convert(html);
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.cs#L12-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-Usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Sẽ cho kết quả:

<!-- snippet: Snippets.Usage.verified.txt -->
<a id='snippet-Snippets.Usage.verified.txt'></a>
```txt
This a sample **paragraph** from [my site](http://test.com)
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.Usage.verified.txt#L1-L1' title='Snippet source file'>snippet source</a> | <a href='#snippet-Snippets.Usage.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Chuyển đổi cũng có thể được tùy chỉnh:

<!-- snippet: UsageWithConfig -->
<a id='snippet-UsageWithConfig'></a>
```cs
var config = new ReverseMarkdown.Config
{
    // Include the unknown tag completely in the result (default as well)
    UnknownTags = Config.UnknownTagsOption.PassThrough,
    // generate GitHub flavoured markdown, supported for BR, PRE and table tags
    GithubFlavored = true,
    // will ignore all comments
    RemoveComments = true,
    // remove markdown output for links where appropriate
    SmartHrefHandling = true
};

var converter = new ReverseMarkdown.Converter(config);
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.cs#L28-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-UsageWithConfig' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Tùy Chọn Cấu Hình

### 📋 Bảng Tổng Hợp Nhanh

| Config | Loại | Mặc định | Mô tả |
|--------|------|----------|-------|
| `GithubFlavored` | `bool` | `false` | Markdown theo kiểu GitHub cho br, pre, table |
| `SlackFlavored` | `bool` | `false` | Markdown theo kiểu Slack (`*` bold, `_` italic, `~` strike) |
| `UnknownTags` | `enum` | `PassThrough` | Xử lý các thẻ không xác định (PassThrough/Drop/Bypass/Raise) |
| `TableComplexHandling` | `enum` | `ConvertToMarkdown` | Xử lý table có colspan/rowspan ⭐ **MỚI** |
| `SmartHrefHandling` | `bool` | `false` | Tối ưu output cho links trùng text và href |
| `Base64Images` | `enum` | `Include` | Xử lý hình ảnh base64 (Include/Skip/SaveToFile) |
| `RemoveComments` | `bool` | `false` | Loại bỏ HTML comments |
| `CleanupUnnecessarySpaces` | `bool` | `true` | Dọn dẹp khoảng trắng thừa |
| `ListBulletChar` | `char` | `-` | Ký tự dấu đầu dòng (bỏ qua nếu SlackFlavored=true) |
| `DefaultCodeBlockLanguage` | `string` | `null` | Ngôn ngữ mặc định cho code block |

### 📖 Chi Tiết Cấu Hình

#### Markdown Flavors

**`GithubFlavored`** (`bool`, mặc định: `false`)
- Bật Markdown theo kiểu GitHub cho `br`, `pre`, `table`, và task lists
- Ví dụ: Table luôn được convert sang GFM format khi có thể

**`SlackFlavored`** (`bool`, mặc định: `false`)
- Định dạng Slack: `*bold*`, `_italic_`, `~strike~`, `•` bullets
- Ghi đè `ListBulletChar` khi bật

#### Table Handling ⭐ **MỚI**

**`TableComplexHandling`** (`enum`, mặc định: `ConvertToMarkdown`)

Xử lý bảng HTML phức tạp (có `colspan`/`rowspan` - Markdown không hỗ trợ native):

```csharp
// Option 1: ConvertToMarkdown (mặc định - backward compatible)
// Luôn convert sang Markdown, mất cấu trúc colspan/rowspan
var config = new Config 
{ 
    TableComplexHandling = Config.TableComplexHandlingOption.ConvertToMarkdown 
};

// Option 2: PreserveHtmlWhenComplex (khuyến nghị) ⭐
// Tự động preserve HTML CHỈ KHI phát hiện colspan/rowspan
var config = new Config 
{ 
    TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex 
};

// Option 3: AlwaysPreserveHtml
// LUÔN giữ table dưới dạng HTML (compact, cleaned)
var config = new Config 
{ 
    TableComplexHandling = Config.TableComplexHandlingOption.AlwaysPreserveHtml 
};
```

**Lưu ý:**
- Khi preserve HTML, cell content vẫn được convert sang Markdown (bold, italic, links...)
- HTML output được làm sạch (chỉ giữ colspan/rowspan, loại bỏ styles)
- Sử dụng `PreserveHtmlWhenComplex` cho kết quả tốt nhất

**`TableWithoutHeaderRowHandling`** (`enum`, mặc định: `Default`)
- `Default`: Hàng đầu tiên làm header
- `EmptyRow`: Thêm hàng trống làm header

**`TableHeaderColumnSpanHandling`** (`bool`, mặc định: `true`)
- Xử lý column spans trong header row

#### Links & Images

**`SmartHrefHandling`** (`bool`, mặc định: `false`)

Tối ưu output cho links:
- `false`: Luôn xuất `[text](url)` ngay cả khi text = url
- `true`: Tối ưu hóa:
  - Nếu text = url → chỉ xuất `url`
  - `tel:123` với text "123" → chỉ xuất `123`
  - `mailto:a@b.c` với text "a@b.c" → chỉ xuất `a@b.c`
  - `http://example.com` với text "example.com" → xuất `http://example.com`

**`WhitelistUriSchemes`** (`HashSet<string>`)
- Chỉ định schemes được phép cho `<a>` và `<img>` (vd: "http", "https", "ftp")
- Mặc định: cho phép tất cả
- Schemes khác sẽ bị bỏ qua

**`Base64Images`** (`enum`, mặc định: `Include`)

Xử lý hình ảnh base64 (data URI):
- `Include`: Giữ nguyên base64 string trong markdown
- `Skip`: Bỏ qua hoàn toàn
- `SaveToFile`: Lưu file và reference đường dẫn (cần `Base64ImageSaveDirectory`)

**`Base64ImageSaveDirectory`** (`string`)
- Thư mục lưu ảnh khi `Base64Images = SaveToFile`

**`Base64ImageFileNameGenerator`** (`Func<int, string, string>`)
- Custom tên file cho ảnh được lưu
- Nhận: `(index, mimeType)` → Trả về: tên file (không có extension)
- Mặc định: `image_0`, `image_1`, ...

#### Unknown Tags & Pass-Through

**`UnknownTags`** (`enum`, mặc định: `PassThrough`)

Xử lý các thẻ HTML không được hỗ trợ:
- `PassThrough`: Giữ nguyên thẻ + content trong output
- `Drop`: Loại bỏ thẻ và content
- `Bypass`: Bỏ thẻ nhưng convert content
- `Raise`: Throw exception `UnknownTagException`

**`PassThroughTags`** (`HashSet<string>`)
- Danh sách thẻ để giữ nguyên HTML (không convert)
- Ví dụ: `config.PassThroughTags.Add("svg");`

#### Formatting & Cleanup

**`RemoveComments`** (`bool`, mặc định: `false`)
- Loại bỏ HTML comments (`<!-- ... -->`)

**`CleanupUnnecessarySpaces`** (`bool`, mặc định: `true`)
- Dọn dẹp khoảng trắng thừa trong output

**`SuppressDivNewlines`** (`bool`, mặc định: `false`)
- Loại bỏ newline từ thẻ `<div>`

**`ListBulletChar`** (`char`, mặc định: `-`)
- Ký tự cho unordered list bullets
- Bị override bởi `SlackFlavored` (dùng `•`)

**`DefaultCodeBlockLanguage`** (`string`, mặc định: `null`)
- Ngôn ngữ mặc định cho fenced code blocks (GFM)
- Sử dụng khi không có class language hint

### Ví Dụ Xử Lý Hình Ảnh Base64

ReverseMarkdown cung cấp các tùy chọn linh hoạt để xử lý hình ảnh được mã hóa base64 (URI dữ liệu nội tuyến) trong quá trình chuyển đổi HTML sang Markdown.

**Bao Gồm Hình Ảnh Base64 (Mặc định)**

Theo mặc định, hình ảnh được mã hóa base64 được bao gồm trong đầu ra markdown nguyên trạng:

<!-- snippet: Base64ImageInclude -->
<a id='snippet-Base64ImageInclude'></a>
```cs
var converter = new ReverseMarkdown.Converter();
string html = "<img src=\"data:image/png;base64,iVBORw0KGg...\" alt=\"Sample Image\"/>";
string result = converter.Convert(html);
// Output: ![Sample Image](data:image/png;base64,iVBORw0KGg...)
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.cs#L50-L57' title='Snippet source file'>snippet source</a> | <a href='#snippet-Base64ImageInclude' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

**Bỏ Qua Hình Ảnh Base64**

Để bỏ qua hoàn toàn hình ảnh được mã hóa base64:

<!-- snippet: Base64ImageSkip -->
<a id='snippet-Base64ImageSkip'></a>
```cs
var config = new ReverseMarkdown.Config
{
    Base64Images = Config.Base64ImageHandling.Skip
};
var converter = new ReverseMarkdown.Converter(config);
string html = "<img src=\"data:image/png;base64,iVBORw0KGg...\" alt=\"Sample Image\"/>";
string result = converter.Convert(html);
// Output: (empty - image is skipped)
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.cs#L63-L74' title='Snippet source file'>snippet source</a> | <a href='#snippet-Base64ImageSkip' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

**Lưu Hình Ảnh Base64 Vào Đĩa**

Để trích xuất và lưu hình ảnh được mã hóa base64 vào đĩa:

<!-- snippet: Base64ImageSaveToFile -->
<a id='snippet-Base64ImageSaveToFile'></a>
```cs
var config = new ReverseMarkdown.Config
{
    Base64Images = Config.Base64ImageHandling.SaveToFile,
    Base64ImageSaveDirectory = "/path/to/images"
};
var converter = new ReverseMarkdown.Converter(config);
string html = "<img src=\"data:image/png;base64,iVBORw0KGg...\" alt=\"Sample Image\"/>";
string result = converter.Convert(html);
// Output: ![Sample Image](/path/to/images/image_0.png)
// Image file saved to: /path/to/images/image_0.png
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.cs#L80-L93' title='Snippet source file'>snippet source</a> | <a href='#snippet-Base64ImageSaveToFile' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

**Tùy Chỉnh Bộ Tạo Tên Tệp**

Bạn có thể cung cấp bộ tạo tên tệp tùy chỉnh cho hình ảnh được lưu:

<!-- snippet: Base64ImageCustomFilename -->
<a id='snippet-Base64ImageCustomFilename'></a>
```cs
var config = new ReverseMarkdown.Config
{
    Base64Images = Config.Base64ImageHandling.SaveToFile,
    Base64ImageSaveDirectory = "/path/to/images",
    Base64ImageFileNameGenerator = (index, mimeType) => 
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"converted_{timestamp}_{index}";
    }
};
var converter = new ReverseMarkdown.Converter(config);
// Images will be saved as: converted_20260108_143022_0.png, converted_20260108_143022_1.jpg, etc.
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.cs#L99-L114' title='Snippet source file'>snippet source</a> | <a href='#snippet-Base64ImageCustomFilename' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

**Định Dạng Hình Ảnh Được Hỗ Trợ:**
- PNG (`image/png`)
- JPEG (`image/jpeg`, `image/jpg`)
- GIF (`image/gif`)
- BMP (`image/bmp`)
- TIFF (`image/tiff`)
- WebP (`image/webp`)
- SVG (`image/svg+xml`)

## Tính Năng

* Hỗ trợ tất cả các thẻ html đã được thiết lập như h1, h2, h3, h4, h5, h6, p, em, strong, i, b, blockquote, code, img, a, hr, li, ol, ul, table, tr, th, td, br
* Hỗ trợ danh sách lồng nhau
* Hỗ trợ chuyển đổi Github Flavoured Markdown cho br, pre, tasklists và table. Sử dụng `var config = new ReverseMarkdown.Config(githubFlavoured:true);`. Theo mặc định, bảng sẽ luôn được chuyển đổi sang Github flavored markdown bất kể cờ này
* Hỗ trợ chuyển đổi Slack Flavoured Markdown. Sử dụng `var config = new ReverseMarkdown.Config { SlackFlavored = true };`
* Cải thiện hiệu suất với phương pháp text writer được tối ưu hóa và tra cứu tổ tiên O(1)
* Hỗ trợ bảng lồng nhau (được chuyển đổi thành HTML bên trong markdown)
* Hỗ trợ chú thích bảng (được hiển thị dưới dạng đoạn văn phía trên bảng)
* Xử lý hình ảnh được mã hóa base64 với các tùy chọn bao gồm nguyên trạng, bỏ qua hoặc lưu vào đĩa

## Thay Đổi Phá Vỡ

### v5.0.0

**Thay Đổi Cấu Hình:**
* `WhitelistUriSchemes` - Đã thay đổi từ `string[]` sang `HashSet<string>` (thuộc tính chỉ đọc). Sử dụng phương thức `.Add()` để thêm schemes thay vì gán mảng
* `PassThroughTags` - Đã thay đổi từ `string[]` sang `HashSet<string>`

**Thay Đổi API:**
* Chữ ký giao diện `IConverter` đã thay đổi từ `string Convert(HtmlNode node)` sang `void Convert(TextWriter writer, HtmlNode node)`. Nếu bạn có các converter tùy chỉnh, bạn sẽ cần cập nhật chúng để ghi vào TextWriter thay vì trả về chuỗi

**Thay Đổi Target Framework:**

* Đã loại bỏ hỗ trợ cho các phiên bản .NET cũ và đã hết vòng đời. Chỉ các phiên bản .NET được hỗ trợ tích cực mới được nhắm mục tiêu, tức là .NET 8, .NET 9 và .NET 10.

### v2.0.0

* Cấu hình `UnknownTags` đã được thay đổi thành kiểu enumeration

## Lời Cảm Ơn

Ý tưởng triển khai ban đầu của thư viện này từ bộ chuyển đổi Html sang Markdown dựa trên Ruby [xijo/reverse_markdown](https://github.com/xijo/reverse_markdown).

## Bản Quyền

Copyright © Babu Annamalai

## Giấy Phép

ReverseMarkdown được cấp phép theo [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Tham khảo [tệp License](https://github.com/mysticmind/reversemarkdown-net/blob/master/LICENSE) để biết thêm thông tin.
