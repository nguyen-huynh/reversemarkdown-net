# ReverseMarkdown Introduction

[![Build status](https://github.com/mysticmind/reversemarkdown-net/actions/workflows/ci.yaml/badge.svg)](https://github.com/mysticmind/reversemarkdown-net/actions/workflows/ci.yaml) [![NuGet Version](https://badgen.net/nuget/v/reversemarkdown)](https://www.nuget.org/packages/ReverseMarkdown/)

ReverseMarkdown is an HTML to Markdown converter library written in C#. Conversion is very reliable since it uses the HTML Agility Pack (HAP) library to parse the HTML DOM.

**📦 This Fork:** This is a fork of [mysticmind/reversemarkdown-net](https://github.com/mysticmind/reversemarkdown-net) with the following improvements:
- ✅ .NET Framework 4.8 Support
- ✅ Table Complex Handling (preserve HTML when colspan/rowspan present)
- ✅ Manual Test Console Application

📚 **Documentation:** [Build Instructions](BUILD.md) | [Changelog](CHANGELOG.md)

## Usage

Install the package from NuGet using `Install-Package ReverseMarkdown` or clone the repository and build it yourself.

<!-- snippet: Usage -->
<a id='snippet-Usage'></a>
```cs
var converter = new ReverseMarkdown.Converter();

string html = "This a sample <strong>paragraph</strong> from <a href=\"http://test.com\">my site</a>";

string result = converter.Convert(html);
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.cs#L12-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-Usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Will produce the result:

<!-- snippet: Snippets.Usage.verified.txt -->
<a id='snippet-Snippets.Usage.verified.txt'></a>
```txt
This a sample **paragraph** from [my site](http://test.com)
```
<sup><a href='/src/ReverseMarkdown.Test/Snippets.Usage.verified.txt#L1-L1' title='Snippet source file'>snippet source</a> | <a href='#snippet-Snippets.Usage.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Conversion can also be customized:

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

## Configuration Options

### 📋 Quick Reference Table

| Config | Type | Default | Description |
|--------|------|---------|-------------|
| `GithubFlavored` | `bool` | `false` | GitHub-style Markdown for br, pre, table |
| `SlackFlavored` | `bool` | `false` | Slack-style Markdown (`*` bold, `_` italic, `~` strike) |
| `UnknownTags` | `enum` | `PassThrough` | Handle unknown tags (PassThrough/Drop/Bypass/Raise) |
| `TableComplexHandling` | `enum` | `ConvertToMarkdown` | Handle tables with colspan/rowspan ⭐ **NEW** |
| `SmartHrefHandling` | `bool` | `false` | Optimize output for links with matching text and href |
| `Base64Images` | `enum` | `Include` | Handle base64 images (Include/Skip/SaveToFile) |
| `RemoveComments` | `bool` | `false` | Remove HTML comments |
| `CleanupUnnecessarySpaces` | `bool` | `true` | Clean up unnecessary whitespace |
| `ListBulletChar` | `char` | `-` | Bullet character (ignored if SlackFlavored=true) |
| `DefaultCodeBlockLanguage` | `string` | `null` | Default language for code blocks |

### 📖 Configuration Details

#### Markdown Flavors

**`GithubFlavored`** (`bool`, default: `false`)
- Enable GitHub-style Markdown for `br`, `pre`, `table`, and task lists
- Example: Tables are always converted to GFM format when possible

**`SlackFlavored`** (`bool`, default: `false`)
- Slack formatting: `*bold*`, `_italic_`, `~strike~`, `•` bullets
- Overrides `ListBulletChar` when enabled

#### Table Handling ⭐ **NEW**

**`TableComplexHandling`** (`enum`, default: `ConvertToMarkdown`)

Handle complex HTML tables (with `colspan`/`rowspan` - not natively supported in Markdown):

```csharp
// Option 1: ConvertToMarkdown (default - backward compatible)
// Always convert to Markdown, lose colspan/rowspan structure
var config = new Config 
{ 
    TableComplexHandling = Config.TableComplexHandlingOption.ConvertToMarkdown 
};

// Option 2: PreserveHtmlWhenComplex (recommended) ⭐
// Automatically preserve HTML ONLY WHEN colspan/rowspan detected
var config = new Config 
{ 
    TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex 
};

// Option 3: AlwaysPreserveHtml
// ALWAYS keep table as HTML (compact, cleaned)
var config = new Config 
{ 
    TableComplexHandling = Config.TableComplexHandlingOption.AlwaysPreserveHtml 
};
```

**Note:**
- When preserving HTML, cell content is still converted to Markdown (bold, italic, links...)
- HTML output is cleaned (only keep colspan/rowspan, remove styles)
- Use `PreserveHtmlWhenComplex` for best results

**`TableWithoutHeaderRowHandling`** (`enum`, default: `Default`)
- `Default`: First row becomes header
- `EmptyRow`: Add empty row as header

**`TableHeaderColumnSpanHandling`** (`bool`, default: `true`)
- Handle column spans in header row

#### Links & Images

**`SmartHrefHandling`** (`bool`, default: `false`)

Optimize output for links:
- `false`: Always output `[text](url)` even when text = url
- `true`: Optimize:
  - If text = url → output just `url`
  - `tel:123` with text "123" → output just `123`
  - `mailto:a@b.c` with text "a@b.c" → output just `a@b.c`
  - `http://example.com` with text "example.com" → output `http://example.com`

**`WhitelistUriSchemes`** (`HashSet<string>`)
- Specify allowed schemes for `<a>` and `<img>` (e.g., "http", "https", "ftp")
- Default: allow all
- Other schemes will be ignored

**`Base64Images`** (`enum`, default: `Include`)

Handle base64 images (data URI):
- `Include`: Keep base64 string in markdown as-is
- `Skip`: Skip entirely
- `SaveToFile`: Save file and reference path (requires `Base64ImageSaveDirectory`)

**`Base64ImageSaveDirectory`** (`string`)
- Directory to save images when `Base64Images = SaveToFile`

**`Base64ImageFileNameGenerator`** (`Func<int, string, string>`)
- Custom filename for saved images
- Takes: `(index, mimeType)` → Returns: filename (without extension)
- Default: `image_0`, `image_1`, ...

#### Unknown Tags & Pass-Through

**`UnknownTags`** (`enum`, default: `PassThrough`)

Handle unsupported HTML tags:
- `PassThrough`: Keep tag + content in output as-is
- `Drop`: Remove tag and content
- `Bypass`: Remove tag but convert content
- `Raise`: Throw `UnknownTagException`

**`PassThroughTags`** (`HashSet<string>`)
- List of tags to keep as HTML (don't convert)
- Example: `config.PassThroughTags.Add("svg");`

#### Formatting & Cleanup

**`RemoveComments`** (`bool`, default: `false`)
- Remove HTML comments (`<!-- ... -->`)

**`CleanupUnnecessarySpaces`** (`bool`, default: `true`)
- Clean up unnecessary whitespace in output

**`SuppressDivNewlines`** (`bool`, default: `false`)
- Remove newlines from `<div>` tags

**`ListBulletChar`** (`char`, default: `-`)
- Character for unordered list bullets
- Overridden by `SlackFlavored` (uses `•`)

**`DefaultCodeBlockLanguage`** (`string`, default: `null`)
- Default language for fenced code blocks (GFM)
- Used when no language class hint is present

### Base64 Image Handling Examples

ReverseMarkdown provides flexible options for handling base64-encoded images (inline data URIs) during HTML to Markdown conversion.

**Include Base64 Images (Default)**

By default, base64-encoded images are included in the markdown output as-is:

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

**Skip Base64 Images**

To skip base64-encoded images entirely:

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

**Save Base64 Images to Disk**

To extract and save base64-encoded images to disk:

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

**Custom Filename Generator**

You can provide a custom filename generator for saved images:

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

**Supported Image Formats:**
- PNG (`image/png`)
- JPEG (`image/jpeg`, `image/jpg`)
- GIF (`image/gif`)
- BMP (`image/bmp`)
- TIFF (`image/tiff`)
- WebP (`image/webp`)
- SVG (`image/svg+xml`)

## Features

* Supports all established HTML tags like h1, h2, h3, h4, h5, h6, p, em, strong, i, b, blockquote, code, img, a, hr, li, ol, ul, table, tr, th, td, br
* Supports nested lists
* Supports GitHub Flavored Markdown conversion for br, pre, tasklists, and tables. Use `var config = new ReverseMarkdown.Config(githubFlavoured:true);`. By default, tables will always be converted to GitHub flavored markdown regardless of this flag
* Supports Slack Flavored Markdown conversion. Use `var config = new ReverseMarkdown.Config { SlackFlavored = true };`
* Performance improvements with optimized text writer approach and O(1) ancestor lookup
* Supports nested tables (converted to HTML inside markdown)
* Supports table captions (rendered as paragraphs above tables)
* Handles base64-encoded images with options to include as-is, skip, or save to disk

## Breaking Changes

### v5.0.0

**Configuration Changes:**
* `WhitelistUriSchemes` - Changed from `string[]` to `HashSet<string>` (read-only property). Use `.Add()` method to add schemes instead of array assignment
* `PassThroughTags` - Changed from `string[]` to `HashSet<string>`

**API Changes:**
* `IConverter` interface signature changed from `string Convert(HtmlNode node)` to `void Convert(TextWriter writer, HtmlNode node)`. If you have custom converters, you'll need to update them to write to TextWriter instead of returning strings

**Target Framework Changes:**

* Dropped support for older and EOL .NET versions. Only actively supported .NET versions are targeted, i.e., .NET 8, .NET 9, and .NET 10.

### v2.0.0

* `UnknownTags` configuration has been changed to enumeration type

## Acknowledgements

The initial inspiration for this library came from the Ruby-based Html to Markdown converter [xijo/reverse_markdown](https://github.com/xijo/reverse_markdown).

## Copyright

Copyright © Babu Annamalai

## License

ReverseMarkdown is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to the [License file](https://github.com/mysticmind/reversemarkdown-net/blob/master/LICENSE) for more information.
