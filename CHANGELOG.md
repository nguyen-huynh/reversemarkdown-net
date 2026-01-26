# Changelog

Tất cả các thay đổi quan trọng của project này sẽ được ghi chép trong file này.

Format dựa trên [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
và project này tuân theo [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [5.1.0] - 2026-01-26

### Added
- Manual Test console application (`ReverseMarkdown.ManualTest`)
  - Auto-scan và convert HTML files từ folder `input/` sang `output/`
  - Error handling và progress reporting
  - Target .NET Framework 4.8
- Support cho .NET Framework 4.8 multi-targeting
- `System.Memory` package dependency cho .NET Framework 4.8 để support Span<T>
- BUILD.md với hướng dẫn build chi tiết
- Table Complex Handling với config option `TableComplexHandling`
  - Support preserve HTML table khi có colspan/rowspan
  - 3 modes: ConvertToMarkdown, PreserveHtmlWhenComplex, AlwaysPreserveHtml
  - Cell content vẫn được convert sang Markdown (bold, italic, links...)

### Changed
- Multi-targeting strategy: loại bỏ .NET 8.0 và .NET 9.0, giữ lại .NET 10.0 và thêm .NET Framework 4.8
- Target frameworks từ `net8.0;net9.0;net10.0` sang `net10.0;net48`
- Repository URLs cập nhật từ mysticmind sang nguyen-huynh fork

## [5.0.0] - 2024

Baseline version từ upstream [mysticmind/reversemarkdown-net](https://github.com/mysticmind/reversemarkdown-net).

### Breaking Changes
- Thay đổi behavior mặc định của table handling
- API changes cho config options

### Features
- Support 30+ HTML tags
- Nested lists và tables
- GitHub Flavored Markdown (GFM)
- Slack Flavored Markdown
- Base64 image handling với 3 modes
- Performance optimizations (O(1) ancestor lookup)
- Unknown tag handling với 4 strategies

---

[5.1.0]: https://github.com/nguyen-huynh/reversemarkdown-net/compare/v5.0.0...v5.1.0
[5.0.0]: https://github.com/mysticmind/reversemarkdown-net/releases/tag/v5.0.0
