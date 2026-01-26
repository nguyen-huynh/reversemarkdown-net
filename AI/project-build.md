# Plan: Build & Release ReverseMarkdown .NET Framework 4.8

## Mục Tiêu
- Build DLL package cho .NET Framework 4.8 để reference vào project chính
- Tạo documentation cho build/release process
- Track changes với CHANGELOG từ version 5.0.0 (upstream) đến 5.1.0 (fork với improvements)

## Tình Trạng Hiện Tại
- ✅ Project đã được cấu hình multi-targeting: `net10.0;net48`
- ✅ NuGet metadata đã có trong ReverseMarkdown.csproj
- ✅ Version hiện tại: `5.0.0`
- ❌ Chưa có CHANGELOG.md
- ❌ Chưa có hướng dẫn build/release
- ❌ Repository URLs vẫn trỏ về mysticmind (cần update sang nguyen-huynh)

## Các Thay Đổi Mới (Minor Release 5.1.0)

### 1. Manual Test Application ([manual-test.md](manual-test.md))
- Tạo console app `ReverseMarkdown.ManualTest` với target net48
- Auto-scan và convert HTML files từ folder `input/` → `output/`
- Error handling và progress reporting

### 2. Multi-Targeting Strategy ([remove-framework.md](remove-framework.md))
- Loại bỏ .NET 8.0 và .NET 9.0
- Giữ lại .NET 10.0 và thêm .NET Framework 4.8
- Thêm `System.Memory` package cho net48 support

### 3. Table HTML Complex Handling ([table-html.md](table-html.md))
- **Trạng thái**: Planning/Design only (chưa implement)
- Hỗ trợ preserve HTML table khi có colspan/rowspan
- Config option `TableComplexHandling` với 3 modes

---

## Implementation Plan

### ✅ Task 1: Build DLL cho .NET Framework 4.8
**CLI Commands:**
```powershell
# Build configuration Release cho tất cả target frameworks
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release

# Build chỉ cho net48 specific
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release -f net48

# Output DLL location:
# src/ReverseMarkdown/bin/Release/net48/ReverseMarkdown.dll
```

**Để reference DLL vào project khác:**
1. Copy DLL từ `bin/Release/net48/`
2. Trong Visual Studio: Add Reference → Browse → chọn DLL
3. Hoặc edit `.csproj`:
   ```xml
   <ItemGroup>
     <Reference Include="ReverseMarkdown">
       <HintPath>path\to\ReverseMarkdown.dll</HintPath>
     </Reference>
   </ItemGroup>
   ```

### ✅ Task 2: Tạo CHANGELOG.md
**Location:** Root directory `CHANGELOG.md`

**Format:** Keep a Changelog (https://keepachangelog.com/)

**Nội dung:**
- `[5.0.0] - 2024-XX-XX` - Baseline từ upstream mysticmind/reversemarkdown-net
- `[5.1.0] - 2026-01-26` - Minor release với:
  - Added: Manual Test console application
  - Changed: Multi-targeting strategy (net10.0 + net48)
  - Added: System.Memory dependency for net48
  - Planned: Table complex handling (not implemented yet)

### ✅ Task 3: Bump Version 5.0.0 → 5.1.0
**File:** `src/ReverseMarkdown/ReverseMarkdown.csproj`

**Change:**
```xml
<VersionPrefix>5.1.0</VersionPrefix>
```

### ✅ Task 4: Tạo BUILD.md
**Location:** Root directory `BUILD.md`

**Sections:**
1. **Prerequisites**: .NET SDK, .NET Framework 4.8 Developer Pack
2. **Build Commands**: dotnet build, dotnet pack
3. **Output Locations**: bin/Release/netXX/
4. **Testing**: dotnet test
5. **Local Reference**: Cách add DLL reference vào project
6. **NuGet Package**: dotnet pack và publish commands (optional)

### ✅ Task 5: Update Repository Metadata
**File:** `src/ReverseMarkdown/ReverseMarkdown.csproj`

**Changes:**
```xml
<!-- Old -->
<PackageProjectUrl>https://github.com/mysticmind/reversemarkdown-net</PackageProjectUrl>
<RepositoryUrl>https://github.com/mysticmind/reversemarkdown-net.git</RepositoryUrl>

<!-- New -->
<PackageProjectUrl>https://github.com/nguyen-huynh/reversemarkdown-net</PackageProjectUrl>
<RepositoryUrl>https://github.com/nguyen-huynh/reversemarkdown-net.git</RepositoryUrl>
```

**Optional:** Update `Authors` field to include fork maintainer

### ✅ Task 6: Update README.md (Optional)
**Additions:**
- Badge cho fork repository
- Link đến BUILD.md
- Link đến CHANGELOG.md
- Mention about .NET Framework 4.8 support

---

## Quick Start Commands

```powershell
# 1. Build DLL
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release -f net48

# 2. Find output
# Location: src\ReverseMarkdown\bin\Release\net48\ReverseMarkdown.dll

# 3. Copy to your project (optional)
Copy-Item "src\ReverseMarkdown\bin\Release\net48\ReverseMarkdown.dll" "C:\YourProject\libs\"

# 4. Run tests (verify build)
dotnet test src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj

# 5. Create NuGet package (optional)
dotnet pack src/ReverseMarkdown/ReverseMarkdown.csproj -c Release
# Output: src\ReverseMarkdown\bin\Release\ReverseMarkdown.5.1.0.nupkg
```

---

## Release Strategy

### Option A: Local DLL Reference (Simple)
- Chỉ build DLL và copy vào project
- Không cần publish NuGet
- Phù hợp cho internal projects

### Option B: NuGet Package (Advanced)
- Build `.nupkg` file với `dotnet pack`
- Publish lên:
  - NuGet.org (public) - cần API key
  - Private NuGet feed (Azure Artifacts, MyGet, GitHub Packages)
  - Local folder feed

---

## Checklist

- [ ] Build DLL thành công cho net48
- [ ] Tạo CHANGELOG.md với version 5.0.0 và 5.1.0
- [ ] Bump version lên 5.1.0 trong .csproj
- [ ] Tạo BUILD.md với hướng dẫn chi tiết
- [ ] Update repository URLs trong .csproj
- [ ] (Optional) Update README.md với links
- [ ] (Optional) Test reference DLL trong project khác
- [ ] (Optional) Create NuGet package

---

## Notes

- **Table HTML feature** từ table-html.md vẫn là planning only → có thể đề cập trong CHANGELOG dưới "Planned" hoặc skip cho đến khi implement
- **Version numbering**: 5.1.0 là minor release phù hợp vì có backward-compatible additions
- **Dependencies**: Nhớ copy `HtmlAgilityPack.dll` và `System.Memory.dll` nếu reference manually
- **.NET Framework 4.8 Developer Pack**: Cần cài đặt nếu chưa có (https://dotnet.microsoft.com/download/dotnet-framework/net48)