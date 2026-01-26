# Plan: Tạo Project Manual Test cho ReverseMarkdown

## Mục Tiêu
Tạo một .NET Framework 4.8 console application để manual test thư viện ReverseMarkdown bằng cách quét và convert các file HTML từ folder input thành Markdown.

## Các Bước Thực Hiện

### 1. Tạo Project Structure
- Tạo project `ReverseMarkdown.ManualTest` trong folder `src/`
- Target framework: `net48`
- OutputType: `Exe` (Console Application)
- Reference đến project `ReverseMarkdown`

### 2. Implement Program Logic
Tạo `Program.cs` với các chức năng:
- Quét tất cả file HTML trong folder `input/`
- Đọc nội dung từng file HTML
- Sử dụng `Converter.Convert()` để convert HTML sang Markdown
- Ghi kết quả vào folder `output/` với tên file tương ứng (đổi extension thành `.md`)
- Hiển thị progress và kết quả conversion trên console

### 3. Setup Folders
- Tạo folder `input/` trong project để chứa HTML files cần test
- Tạo folder `output/` để chứa kết quả Markdown (hoặc tạo tự động khi chạy)
- Thêm vài sample HTML files vào `input/` để test

### 4. Update Solution
- Cập nhật `ReverseMarkdown.sln` để include project mới
- Đảm bảo build order đúng (ReverseMarkdown build trước ManualTest)

### 5. Configuration Options
Hardcode config mặc định:
```csharp
var config = new Config
{
    GithubFlavored = true,
    SmartHrefHandling = true,
    UnknownTags = UnknownTagsOption.PassThrough,
    RemoveComments = false
};
```

## Implementation Details

### File Naming Convention
- Input: `example.html` → Output: `example.md`
- Giữ nguyên tên file, chỉ đổi extension

### Error Handling
- Log lỗi ra console với màu đỏ
- Skip file lỗi và tiếp tục convert các file khác
- Hiển thị tổng kết: số file thành công / tổng số file

### Console Output Format
```
========================================
ReverseMarkdown Manual Test Tool
========================================

Converting HTML files from 'input/' to 'output/'...

[1/5] example1.html → example1.md ... OK
[2/5] example2.html → example2.md ... OK
[3/5] broken.html → broken.md ... ERROR: Invalid HTML
[4/5] table.html → table.md ... OK
[5/5] complex.html → complex.md ... OK

========================================
Conversion Complete!
Success: 4/5 files
Failed: 1/5 files
========================================

Press any key to exit...
```

## Technical Stack
- **Framework**: .NET Framework 4.8
- **Dependencies**: 
  - ReverseMarkdown (Project Reference)
  - HtmlAgilityPack (inherited from ReverseMarkdown)
  - System.Memory (inherited from ReverseMarkdown cho net48)

## Next Steps
Sau khi implement xong, người dùng sẽ:
1. Thêm các HTML files cần test vào folder `input/`
2. Chạy console app
3. Kiểm tra kết quả trong folder `output/`
4. So sánh và đánh giá chất lượng conversion