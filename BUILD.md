# Build Instructions

Hướng dẫn build ReverseMarkdown .NET library để tạo DLL cho local reference.

## Prerequisites

### Required
- [.NET SDK](https://dotnet.microsoft.com/download) (10.0 hoặc cao hơn)
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)

### Optional
- Visual Studio 2022 (hoặc cao hơn) để build từ IDE
- Git để clone repository

## Verify Prerequisites

```powershell
# Kiểm tra .NET SDK version
dotnet --version

# Kiểm tra .NET Framework Developer Pack đã cài
# Nếu có, sẽ thấy net48 trong danh sách
dotnet --list-sdks
```

## Build Commands

### 1. Build Tất Cả Target Frameworks

Build cho cả `net10.0` và `net48`:

```powershell
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release
```

### 2. Build Chỉ .NET Framework 4.8

Nếu chỉ cần DLL cho .NET Framework 4.8:

```powershell
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release -f net48
```

### 3. Build Từ Solution

Build toàn bộ solution (bao gồm test projects):

```powershell
dotnet build src/ReverseMarkdown.sln -c Release
```

## Output Locations

Sau khi build, DLL sẽ được tạo tại:

```
src/ReverseMarkdown/bin/Release/
├── net10.0/
│   ├── ReverseMarkdown.dll
│   ├── ReverseMarkdown.pdb
│   └── HtmlAgilityPack.dll
└── net48/
    ├── ReverseMarkdown.dll
    ├── ReverseMarkdown.pdb
    ├── HtmlAgilityPack.dll
    └── System.Memory.dll
```

## Testing

### Run All Tests

```powershell
dotnet test src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj
```

### Run Tests Với Coverage

```powershell
dotnet test src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj --collect:"XPlat Code Coverage"
```

## Using the DLL in Your Project

### Option 1: Direct DLL Reference (Recommended for Local Development)

1. **Copy DLL và dependencies:**

```powershell
# Tạo folder libs trong project của bạn
New-Item -ItemType Directory -Force -Path "C:\YourProject\libs"

# Copy DLL từ build output
Copy-Item "src\ReverseMarkdown\bin\Release\net48\*.dll" "C:\YourProject\libs\"
```

2. **Add reference trong Visual Studio:**
   - Right-click project → Add → Reference
   - Browse → Chọn `ReverseMarkdown.dll`

3. **Hoặc edit `.csproj` manually:**

```xml
<ItemGroup>
  <Reference Include="ReverseMarkdown">
    <HintPath>libs\ReverseMarkdown.dll</HintPath>
  </Reference>
  <Reference Include="HtmlAgilityPack">
    <HintPath>libs\HtmlAgilityPack.dll</HintPath>
  </Reference>
  <Reference Include="System.Memory">
    <HintPath>libs\System.Memory.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Option 2: Project Reference (For Development)

Nếu bạn đang phát triển và muốn debug vào source code:

```xml
<ItemGroup>
  <ProjectReference Include="..\path\to\reversemarkdown-net\src\ReverseMarkdown\ReverseMarkdown.csproj" />
</ItemGroup>
```

## Clean Build

Để xóa tất cả build artifacts và build lại từ đầu:

```powershell
# Clean
dotnet clean src/ReverseMarkdown/ReverseMarkdown.csproj

# Rebuild
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release
```

## Troubleshooting

### Error: "The target framework 'net48' is not supported"

**Giải pháp:** Cài đặt .NET Framework 4.8 Developer Pack từ https://dotnet.microsoft.com/download/dotnet-framework/net48

### Error: "HtmlAgilityPack could not be found"

**Giải pháp:** Restore NuGet packages:

```powershell
dotnet restore src/ReverseMarkdown/ReverseMarkdown.csproj
```

### Error: "System.Memory could not be found"

**Giải pháp:** Package này tự động được restore cho net48. Chạy:

```powershell
dotnet restore src/ReverseMarkdown/ReverseMarkdown.csproj
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release -f net48
```

## Advanced Builds

### Debug Build

```powershell
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Debug -f net48
```

### Verbose Output

```powershell
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release -v detailed
```

### Parallel Build

```powershell
dotnet build src/ReverseMarkdown.sln -c Release -m
```

## Quick Reference

| Command | Description |
|---------|-------------|
| `dotnet restore` | Restore NuGet packages |
| `dotnet build -c Release` | Build Release configuration |
| `dotnet build -c Debug` | Build Debug configuration |
| `dotnet build -f net48` | Build specific framework |
| `dotnet clean` | Clean build outputs |
| `dotnet test` | Run unit tests |

## Next Steps

Sau khi build thành công:

1. ✅ Kiểm tra DLL tại `bin/Release/net48/`
2. ✅ Copy DLL và dependencies vào project của bạn
3. ✅ Add reference trong Visual Studio hoặc .csproj
4. ✅ Import namespace: `using ReverseMarkdown;`
5. ✅ Sử dụng: `var converter = new Converter();`

## Manual Test Application

Project này bao gồm một console application để test thủ công:

```powershell
# Build manual test app
dotnet build src/ReverseMarkdown.ManualTest/ReverseMarkdown.ManualTest.csproj -c Release

# Run manual test
cd src/ReverseMarkdown.ManualTest/bin/Release/net48
.\ReverseMarkdown.ManualTest.exe

# Đặt HTML files vào folder input/, kết quả sẽ ra folder output/
```

## Documentation

- [README.md](README.md) - Tổng quan và usage examples
- [CHANGELOG.md](CHANGELOG.md) - Version history và changes
- [AI/project-build.md](AI/project-build.md) - Detailed build plan

## Support

Nếu gặp vấn đề khi build, vui lòng:
1. Check [Troubleshooting](#troubleshooting) section
2. Verify prerequisites đã đủ
3. Create issue trên GitHub repository
