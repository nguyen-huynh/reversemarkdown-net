# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [5.1.0] - 2026-01-26

### Added
- Manual Test console application (`ReverseMarkdown.ManualTest`)
  - Auto-scan and convert HTML files from `input/` folder to `output/`
  - Error handling and progress reporting
  - Target .NET Framework 4.8
- Support for .NET Framework 4.8 multi-targeting
- `System.Memory` package dependency for .NET Framework 4.8 to support Span<T>
- BUILD.md with detailed build instructions
- XML documentation file generation (`.xml`) for IntelliSense support and API documentation
- Table Complex Handling with config option `TableComplexHandling`
  - Support preserve HTML table when colspan/rowspan present
  - 3 modes: ConvertToMarkdown, PreserveHtmlWhenComplex, AlwaysPreserveHtml
  - Cell content is still converted to Markdown (bold, italic, links...)

### Changed
- Multi-targeting strategy: removed .NET 8.0 and .NET 9.0, kept .NET 10.0 and added .NET Framework 4.8
- Target frameworks from `net8.0;net9.0;net10.0` to `net10.0;net48`
- Repository URLs updated from mysticmind to nguyen-huynh fork

## [5.0.0] - 2024

Baseline version from upstream [mysticmind/reversemarkdown-net](https://github.com/mysticmind/reversemarkdown-net).

### Breaking Changes
- Changed default behavior of table handling
- API changes for config options

### Features
- Support 30+ HTML tags
- Nested lists and tables
- GitHub Flavored Markdown (GFM)
- Slack Flavored Markdown
- Base64 image handling with 3 modes
- Performance optimizations (O(1) ancestor lookup)
- Unknown tag handling with 4 strategies

---

[5.1.0]: https://github.com/nguyen-huynh/reversemarkdown-net/compare/v5.0.0...v5.1.0
[5.0.0]: https://github.com/mysticmind/reversemarkdown-net/releases/tag/v5.0.0
