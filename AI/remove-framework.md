# Plan: Multi-Targeting .NET 10 + .NET Framework 4.8 - ReverseMarkdown.NET

## 🎯 Mục Tiêu
- ❌ **Loại bỏ**: .NET 8.0, .NET 9.0 (các phiên bản không còn cần thiết)
- ✅ **Giữ lại**: .NET 10.0 (latest, modern features, performance)
- ✅ **Thêm mới**: .NET Framework 4.8 (compatibility với legacy systems)

## 📋 Các Bước Thực Hiện

### 1. Cập Nhật ReverseMarkdown.csproj (Thư viện chính)
**File**: `src/ReverseMarkdown/ReverseMarkdown.csproj`

**Thay đổi**:
```xml
<!-- Trước -->
<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>

<!-- Sau -->
<TargetFrameworks>net10.0;net48</TargetFrameworks>
```

**Lưu ý**:
- GIỮ `<TargetFrameworks>` (số nhiều) - multi-targeting
- Thứ tự: net10.0 trước (preferred), net48 sau
- Kiểm tra `HtmlAgilityPack` v1.12.4 compatibility với cả 2 frameworks
- `LangVersion>preview` có thể GIỮ - modern compiler sẽ handle differences
- Có thể cần conditional `LangVersion` nếu gặp vấn đề

---

### 2. Cập Nhật ReverseMarkdown.Test.csproj (Test project)
**File**: `src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj`

**Thay đổi**:
```xml
<!-- Trước -->
<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>

<!-- Sau -->
<TargetFrameworks>net10.0;net48</TargetFrameworks>
```

**Lưu ý**:
- GIỮ `<TargetFrameworks>` - multi-targeting
- Test packages hiện đại thường support cả 2 frameworks
- Run tests trên CẢ HAI frameworks sau khi thay đổi
- `<IsTestProject>true</IsTestProject>` có thể giữ - không vấn đề với multi-targeting

---

### 3. Cập Nhật ReverseMarkdown.Benchmark.csproj (Benchmark project)
**File**: `src/ReverseMarkdown.Benchmark/ReverseMarkdown.Benchmark.csproj`

**Thay đổi Option 1** (Recommended - Multi-target để so sánh performance):
```xml
<!-- Trước -->
<TargetFramework>net9.0</TargetFramework>

<!-- Sau -->
<TargetFrameworks>net10.0;net48</TargetFrameworks>
```

**Thay đổi Option 2** (Nếu chỉ muốn benchmark .NET 10):
```xml
<TargetFramework>net10.0</TargetFramework>
```

**Lưu ý**:
- BenchmarkDotNet có thể chạy benchmarks trên nhiều frameworks
- `<ImplicitUsings>` có thể GIỮ với conditional:
  ```xml
  <ImplicitUsings Condition="'$(TargetFramework)' == 'net10.0'">enable</ImplicitUsings>
  ```
- Multi-target cho phép so sánh performance giữa .NET 10 vs .NET Framework 4.8

---

## 🔍 Kiểm Tra Dependencies

### HtmlAgilityPack v1.12.4
- ✅ Verify compatibility với .NET Framework 4.8
- ✅ Kiểm tra trên NuGet package page
- ⚠️ Nếu không tương thích, cần upgrade/downgrade version
- 💡 HtmlAgilityPack thường support .NET Framework 4.x rất tốt

### Test Packages
- `Microsoft.NET.Test.Sdk` v18.0.1 → ✅ Thường support multi-targeting
- `xunit` v2.9.3 → ✅ Support cả .NET 10 và .NET Framework 4.8
- `xunit.runner.visualstudio` v3.1.5 → ✅ Support multi-targeting
- `Verify.Xunit` v31.9.3 → ⚠️ Verify, có thể cần version cũ hơn cho net48
- `coverlet.collector` v6.0.4 → ✅ Thường OK với multi-targeting

**Lưu ý**: Multi-targeting giúp packages tự động chọn version phù hợp cho mỗi framework.

### Benchmark Packages
- `BenchmarkDotNet` v0.15.8 → ✅ Excellent multi-framework support
- BenchmarkDotNet được thiết kế để benchmark trên nhiều frameworks khác nhau

---

## ⚠️ Rủi Ro & Giải Pháp (Multi-Targeting Strategy)

### Rủi Ro 1: Language Features Differences
**Vấn đề**: Modern C# features có thể không work trên .NET Framework 4.8

**Giải pháp - Conditional LangVersion** (nếu cần):
```xml
<PropertyGroup>
  <LangVersion>preview</LangVersion>
  <!-- Hoặc conditional -->
  <LangVersion Condition="'$(TargetFramework)' == 'net10.0'">preview</LangVersion>
  <LangVersion Condition="'$(TargetFramework)' == 'net48'">latest</LangVersion>
</PropertyGroup>
```

**Code với Conditional Compilation** (nếu gặp incompatibilities):
```csharp
#if NET10_0
    // Modern C# features - chỉ cho .NET 10
    public required string Name { get; init; }
#else
    // Classic syntax cho .NET Framework 4.8
    public string Name { get; set; } = null!;
#endif
```

### Rủi Ro 2: API Differences
**Vấn đề**: Một số APIs không có trong .NET Framework 4.8

**Giải pháp - Conditional Compilation**:
```csharp
#if NET10_0_OR_GREATER
    // Modern APIs
    ReadOnlySpan<char> span = text.AsSpan();
#else
    // .NET Framework fallback
    string result = text.Substring(0, length);
#endif
```

**Polyfill Packages** (nếu cần):
```xml
<!-- Thêm System.Memory cho net48 để có Span<T> support -->
<PackageReference Include="System.Memory" Version="4.5.5" Condition="'$(TargetFramework)' == 'net48'" />
<PackageReference Include="System.ValueTuple" Version="4.5.0" Condition="'$(TargetFramework)' == 'net48'" />
```

### Rủi Ro 3: Build & Tooling
**Vấn đề**: Cần .NET Framework Developer Pack cho net48

**Giải pháp**:
- Đảm bảo máy build có .NET Framework 4.8 Developer Pack installed
- Visual Studio: Install ".NET Framework 4.8 targeting pack"
- CI/CD: Sử dụng windows-latest image (có sẵn .NET Framework)

### Rủi Ro 4: Nullable Reference Types
**Vấn đề**: `<Nullable>enable</Nullable>` - C# 8 feature

**Giải pháp**: 
```xml
<!-- Option 1: Keep globally (modern compiler supports) -->
<Nullable>enable</Nullable>

<!-- Option 2: Conditional -->
<Nullable Condition="'$(TargetFramework)' == 'net10.0'">enable</Nullable>
```

Compiler hiện đại có thể compile nullable annotations cho cả net48, chỉ là runtime support khác nhau.
# Hoặc dùng MSBuild nếu dotnet build gặp vấn đề
msbuild src/ReverseMarkdown.sln /p:Configuration=Release

# Run tests
dotnet test src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj

# Run benchmarks
dotnet run --project src/ReverseMarkdown.Benchmark/ReverseMarkdown.Benchmark.csproj -c Release
```

### 2. Verify Output
- Kiểm tra build output:
  - `bin/Debug/net10.0/` và `bin/Release/net10.0/`
  - `bin/Debug/net48/` và `bin/Release/net48/`
- Verify cả 2 DLLs được tạo ra: `ReverseMarkdown.dll` cho mỗi framework
- Check dependencies trong mỗi output folder
- Verify assemblies (dùng ILSpy):
  - net10.0 version targets .NET 10
  - net48 version targets .NET Framework 4.8

### 3. Update Documentation
- **README.md**: Cập nhật supported frameworks (.NET 10 và .NET Framework 4.8)
- **Release Notes**: Document changes (removed .NET 8/9, added .NET Framework 4.8)
- **.github/workflows**: Update CI/CD pipeline để build cả 2 frameworks
- **NuGet package metadata**: Verify package chứa cả `lib/net10.0/` và `lib/net48/`

### 1. Build & Test
```powershell
# Build solution
dotnet build src/ReverseMarkdown.sln

# Run tests cho tất cả frameworks
dotnet test src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj

# Run benchmarks
dotnet run --project src/ReverseMarkdown.Benchmark/ReverseMarkdown.Benchmark.csproj -c Release
```

### 2. Verify Output
- Kiểm tra build output cho cả net10.0 và net48
- Verify DLL được tạo ra đúng frameworks
- Check dependencies trong output folders

### 3. Update Documentation
- **README.md**: Cập nhật supported frameworks section
- **Release Notes**: Document breaking changes nếu có
- **.github/workflows**: Update CI/CD pipeline (nếu có)

---

## 🎯 Checklist
- [ ] Check all package dependencies support .NET Framework 4.8
- [ ] Review code cho modern C# features không tương thích
- [ ] Đảm bảo máy dev có .NET Framework 4.8 Developer Pack

### Implementation - Project Files
- [ ] Update `src/ReverseMarkdown/ReverseMarkdown.csproj`:
  - [ ] `<TargetFramework>net48</TargetFramework>` (số ít)
  - [ ] Adjust `<LangVersion>` về 7.3 hoặc latest
  - [ ] Review `<Nullable>enable</Nullable>` - có thể cần remove
  - [ ] Check PackageReferences compatibility
- [ ] Update `src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj`:
  - [ ] `<TargetFramework>net48</TargetFramework>`
  - [ ] Downgrade test packages nếu cần
  - [ ] Review `<IsTestProject>true</IsTestProject>`
- [ ] Update `src/ReverseMarkdown.Benchmark/ReverseMarkdown.Benchmark.csproj`:
  - [ ] `<TargetFramework>net48</TargetFramework>`
  - [ ] Remove `<ImplicitUsings>enable</ImplicitUsings>`
  - [ ] Check BenchmarkDotNet compatibility

### Implementation - Code Changes
- [ ] Review và fix modern C# syntax:
  - [ ] Pattern matching nâng cao
  - [ ] Init-only properties → normal properties
  - [ ] Record types →
- **Release date**: April 18, 2019
- **End of support**: Follow Windows lifecycle (still supported)
- **C# version**: 
  - Officially: C# 7.3
  - Với compiler mới: Có thể dùng features cao hơn (C# 8, 9, 10) nếu không phụ thuộc runtime APIs
- **Target Framework Moniker (TFM)**: `net48`
- **Runtime**: CLR 4.0
- **BCL**: Full .NET Framework Base Class Library

### C# 7.3 Features (Safe)
✅ Tuples with named elements  
✅ Out variables  
✅ Pattern matching (basic)  
✅ Local functions  
✅ More expression-bodied members  
✅ Ref locals and returns  
✅ Discards  
✅ Binary literals and digit separators  

### Features Cần Tránh (C# 8+)
❌ Nullable reference types annotations (có thể compile nhưng không có runtime support)  
❌ Async streams (`IAsyncEnumerable<T>`)  
❌ Ranges and indices (`..`, `^`)  
❌ Default interface implementations  
❌ Pattern matching enhancements (C# 8+)  
❌ Switch expressions  
## 🎓 Lưu Ý Quan Trọng (Multi-Targeting Strategy)

### Tại Sao Multi-Targeting?
✅ **Best of Both Worlds**:
- .NET 10: Modern features, best performance, cross-platform
- .NET Framework 4.8: Legacy system compatibility, corporate environments

✅ **Một NuGet Package cho tất cả**:
- Consumers tự động nhận version phù hợp
- Không cần maintain 2 packages riêng

✅ **Minimal Code Changes**:
- Giữ được modern C# code cho .NET 10
- Chỉ thêm conditional compilation khi cần thiết

### Cách NuGet Package Hoạt Động

Khi user install package:
```powershell
Install-Package ReverseMarkdown
```

NuGet tự động chọn:
- **Project .NET 10** → Dùng `lib/net10.0/ReverseMarkdown.dll`
- **Project .NET Framework 4.8** → Dùng `lib/net48/ReverseMarkdown.dll`
- **Project .NET 6/7/8** → Fallback về `lib/net10.0/` (compatible)
- **Project .NET Framework 4.6-4.7** → Fallback về `lib/net48/` (compatible)

### Performance Considerations

- .NET 10 version sẽ nhanh hơn đáng kể (JIT improvements, Span<T>, etc.)
- .NET Framework 4.8 version có performance ổn định, proven
- Có thể so sánh với BenchmarkDotNet nếu benchmark project multi-target

### Maintenance Strategy

```csharp
// Keep modern code for both frameworks
public void Process(string text)
{
    // Works on both frameworks
    var result = text.Trim();
    
#if NET10_0_OR_GREATER
    // Only for .NET 10 - optimize with Span<T>
    ReadOnlySpan<char> span = text.AsSpan().Trim();
#endif
}
```

---

*Plan được tạo: January 26, 2026*  
*Plan được cập nhật: January 26, 2026*  
*Status: ⏳ Ready for Implementation*  
*Strategy: Multi-Targeting (.NET 10 + .NET Framework 4.8)*  
*Estimated Time: 2-4 hours*
❌ Top-level statements (C# 9)  
❌ File-scoped namespaces (C# 10)  

---

## 🚀 Timeline Ước Tính (Multi-Targeting)

- **Step 1-3 (Update .csproj files)**: 10-15 phút
- **Initial build test**: 5 phút
- **Fix compilation errors** (nếu có): 30 phút - 1 giờ
- **Add conditional compilation** (nếu cần): 30 phút - 1 giờ
- **Add polyfill packages** (nếu cần): 15 phút
- **Full testing on both frameworks**: 30 phút
- **Documentation update**: 30 phút
- **NuGet package verification**: 15 phút

**Tổng cộng**: ~2-4 giờ

**Rủi ro thấp**: Multi-targeting cho phép giữ code hiện tại, chỉ cần handle differences giữa frameworks khi gặp errors.

---

## 📚 Reference

### .NET Framework 4.8 Support
- Release date: April 18, 2019
- End of support: Follow Windows lifecycle
- C# version: Up to C# 7.3 officially, but newer features may work with compiler
- Target Framework Moniker (TFM): `net48`

### .NET 10
- Latest version
- Full C# 13+ support
- Modern APIs and performance improvements
- TFM: `net10.0`

---

## 🚀 Timeline Ước Tính

- **Step 1-3 (Update .csproj files)**: 15 phút
- **Dependencies verification**: 30 phút
- **Build & initial testing**: 30 phút
- **Fix compatibility issues (if any)**: 1-2 giờ
- **Full testing & verification**: 1 giờ
- **Documentation update**: 30 phút

**Tổng cộng**: ~3-4 giờ (bao gồm buffer cho issues không lường trước)

---

*Plan được tạo: January 26, 2026*  
*Plan được cập nhật: January 26, 2026*  
*Status: ⏳ Ready for Implementation*  
*Strategy: Multi-Targeting (.NET 10 + .NET Framework 4.8)*  
*Estimated Time: 2-4 hours*