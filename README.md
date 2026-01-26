# Giới Thiệu ReverseMarkdown

[![Build status](https://github.com/mysticmind/reversemarkdown-net/actions/workflows/ci.yaml/badge.svg)](https://github.com/mysticmind/reversemarkdown-net/actions/workflows/ci.yaml) [![NuGet Version](https://badgen.net/nuget/v/reversemarkdown)](https://www.nuget.org/packages/ReverseMarkdown/)

ReverseMarkdown là thư viện chuyển đổi HTML sang Markdown được viết bằng C#. Chuyển đổi rất đáng tin cậy vì sử dụng thư viện HtmlAgilityPack (HAP) để duyệt HTML DOM.

Nếu bạn đã sử dụng và được hưởng lợi từ thư viện này, hãy thoải mái tài trợ cho tôi!<br>
<a href="https://github.com/sponsors/mysticmind" target="_blank"><img height="30" style="border:0px;height:36px;" src="https://img.shields.io/static/v1?label=GitHub Sponsor&message=%E2%9D%A4&logo=GitHub" border="0" alt="GitHub Sponsor" /></a>

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

* `DefaultCodeBlockLanguage` - Tùy chọn để đặt ngôn ngữ mặc định cho khối code theo kiểu Github markdown nếu không có các đánh dấu ngôn ngữ dựa trên class
* `GithubFlavored` - Markdown theo kiểu Github cho br, pre và table. Mặc định là false
* `SlackFlavored` - Định dạng markdown theo kiểu Slack. Khi bật, sử dụng `*` cho in đậm, `_` cho in nghiêng, `~` cho gạch ngang, và `•` cho dấu đầu dòng danh sách. Mặc định là false
* `CleanupUnnecessarySpaces` - Dọn dẹp khoảng trắng không cần thiết trong đầu ra. Mặc định là true
* `SuppressDivNewlines` - Loại bỏ dòng mới có tiền tố từ thẻ `div`. Mặc định là false
* `ListBulletChar` - Cho phép bạn thay đổi ký tự dấu đầu dòng. Giá trị mặc định là `-`. Một số hệ thống yêu cầu ký tự dấu đầu dòng là `*` thay vì `-`, cấu hình này cho phép bạn thay đổi nó. Lưu ý: Tùy chọn này bị bỏ qua khi `SlackFlavored` được bật
* `RemoveComments` - Loại bỏ thẻ comment cùng với văn bản. Mặc định là false
* `SmartHrefHandling` - Cách xử lý thuộc tính href của thẻ `<a>`
  * `false` - Xuất `[{name}]({href}{title})` ngay cả khi name và href giống nhau. Đây là tùy chọn mặc định.
  * `true` - Nếu name và href bằng nhau, chỉ xuất `name`. Lưu ý rằng nếu Uri không được định dạng tốt theo [`Uri.IsWellFormedUriString`](https://docs.microsoft.com/en-us/dotnet/api/system.uri.iswellformeduristring) (ví dụ: chuỗi không được escape đúng như `http://example.com/path/file name.docx`) thì cú pháp markdown vẫn sẽ được sử dụng.

    Nếu `href` chứa giao thức `http/https`, và `name` thì không nhưng giống nhau, chỉ xuất `href`

    Nếu là scheme `tel:` hoặc `mailto:`, nhưng sau đó giống với name, chỉ xuất `name`.
* `UnknownTags` - xử lý các thẻ không xác định.
  * `UnknownTagsOption.PassThrough` - Bao gồm hoàn toàn thẻ không xác định vào kết quả. Tức là, thẻ cùng với văn bản sẽ được giữ lại trong đầu ra. Đây là mặc định
  * `UnknownTagsOption.Drop` - Loại bỏ thẻ không xác định và nội dung của nó
  * `UnknownTagsOption.Bypass` - Bỏ qua thẻ không xác định nhưng cố gắng chuyển đổi nội dung của nó
  * `UnknownTagsOption.Raise` - Đưa ra lỗi để cho bạn biết
* `PassThroughTags` - Truyền danh sách các thẻ để truyền qua nguyên trạng mà không xử lý gì.
* `WhitelistUriSchemes` - Chỉ định các scheme nào (không có dấu hai chấm ở cuối) được phép cho thẻ `<a>` và `<img>`. Những cái khác sẽ bị bỏ qua (xuất văn bản hoặc không có gì). Mặc định cho phép mọi thứ.

  Nếu cung cấp `string.Empty` và khi không thể xác định schema `href` hoặc `src` - đưa vào whitelist

  Schema được xác định bởi class `Uri`, ngoại trừ khi url bắt đầu bằng `/` (file schema) và `//` (http schema)
* `TableWithoutHeaderRowHandling` - xử lý bảng không có hàng tiêu đề
  * `TableWithoutHeaderRowHandlingOption.Default` - Hàng đầu tiên sẽ được sử dụng làm hàng tiêu đề (mặc định)
  * `TableWithoutHeaderRowHandlingOption.EmptyRow` - Một hàng trống sẽ được thêm vào làm hàng tiêu đề
* `TableHeaderColumnSpanHandling` - Đặt cờ này để xử lý hoặc xử lý cột tiêu đề bảng với column spans. Mặc định là true
* `Base64Images` - Kiểm soát cách xử lý hình ảnh được mã hóa base64 (URI dữ liệu nội tuyến) trong quá trình chuyển đổi
  * `Base64ImageHandling.Include` - Bao gồm hình ảnh được mã hóa base64 trong đầu ra markdown nguyên trạng (hành vi mặc định)
  * `Base64ImageHandling.Skip` - Bỏ qua/bỏ qua hoàn toàn hình ảnh được mã hóa base64
  * `Base64ImageHandling.SaveToFile` - Lưu hình ảnh được mã hóa base64 vào đĩa và tham chiếu đường dẫn tệp đã lưu trong markdown. Yêu cầu đặt `Base64ImageSaveDirectory`
* `Base64ImageSaveDirectory` - Khi `Base64Images` được đặt thành `SaveToFile`, chỉ định đường dẫn thư mục nơi hình ảnh sẽ được lưu
* `Base64ImageFileNameGenerator` - Khi `Base64Images` được đặt thành `SaveToFile`, hàm này tạo tên tệp cho mỗi hình ảnh được lưu. Hàm nhận chỉ mục hình ảnh (int) và kiểu MIME (string), và phải trả về tên tệp không có phần mở rộng. Nếu không được chỉ định, hình ảnh sẽ được đặt tên là `image_0`, `image_1`, v.v.

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
