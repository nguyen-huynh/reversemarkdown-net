# Build Instructions

Guide for building the ReverseMarkdown .NET library to create DLLs for local reference.

## Prerequisites

### Required
- [.NET SDK](https://dotnet.microsoft.com/download) (10.0 or higher)
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)

### Optional
- Visual Studio 2022 (or higher) to build from IDE
- Git to clone repository

## Verify Prerequisites

```powershell
# Check .NET SDK version
dotnet --version

# Check if .NET Framework Developer Pack is installed
# If installed, you'll see net48 in the list
dotnet --list-sdks
```

## Build Commands

### 1. Build All Target Frameworks

Build for both `net10.0` and `net48`:

```powershell
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release
```

### 2. Build Only .NET Framework 4.8

If you only need the DLL for .NET Framework 4.8:

```powershell
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release -f net48
```

### 3. Build From Solution

Build the entire solution (including test projects):

```powershell
dotnet build src/ReverseMarkdown.sln -c Release
```

## Output Locations

After building, DLLs will be created at:

```
src/ReverseMarkdown/bin/Release/
├── net10.0/
│   ├── ReverseMarkdown.dll
│   ├── ReverseMarkdown.pdb
│   ├── ReverseMarkdown.xml          (XML documentation)
│   └── HtmlAgilityPack.dll
└── net48/
    ├── ReverseMarkdown.dll
    ├── ReverseMarkdown.pdb
    ├── ReverseMarkdown.xml          (XML documentation)
    ├── HtmlAgilityPack.dll
    └── System.Memory.dll
```

**Note:** The `.xml` file contains XML documentation for IntelliSense support in Visual Studio and API documentation generation.

## Testing

### Run All Tests

```powershell
dotnet test src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj
```

### Run Tests With Coverage

```powershell
dotnet test src/ReverseMarkdown.Test/ReverseMarkdown.Test.csproj --collect:"XPlat Code Coverage"
```

## Using the DLL in Your Project

### Option 1: Direct DLL Reference (Recommended for Local Development)

1. **Copy DLL and dependencies:**

```powershell
# Create libs folder in your project
New-Item -ItemType Directory -Force -Path "C:\YourProject\libs"

# Copy DLL and XML documentation from build output
Copy-Item "src\ReverseMarkdown\bin\Release\net48\*.dll" "C:\YourProject\libs\"
Copy-Item "src\ReverseMarkdown\bin\Release\net48\ReverseMarkdown.xml" "C:\YourProject\libs\"
```

**Note:** Include the `.xml` file to enable IntelliSense in Visual Studio.

2. **Add reference in Visual Studio:**
   - Right-click project → Add → Reference
   - Browse → Select `ReverseMarkdown.dll`

3. **Or edit `.csproj` manually:**

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

If you're developing and want to debug into the source code:

```xml
<ItemGroup>
  <ProjectReference Include="..\path\to\reversemarkdown-net\src\ReverseMarkdown\ReverseMarkdown.csproj" />
</ItemGroup>
```

## Clean Build

To remove all build artifacts and rebuild from scratch:

```powershell
# Clean
dotnet clean src/ReverseMarkdown/ReverseMarkdown.csproj

# Rebuild
dotnet build src/ReverseMarkdown/ReverseMarkdown.csproj -c Release
```

## Troubleshooting

### Error: "The target framework 'net48' is not supported"

**Solution:** Install .NET Framework 4.8 Developer Pack from https://dotnet.microsoft.com/download/dotnet-framework/net48

### Error: "HtmlAgilityPack could not be found"

**Solution:** Restore NuGet packages:

```powershell
dotnet restore src/ReverseMarkdown/ReverseMarkdown.csproj
```

### Error: "System.Memory could not be found"

**Solution:** This package is automatically restored for net48. Run:

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

After successful build:

1. ✅ Check DLL at `bin/Release/net48/`
2. ✅ Copy DLL and dependencies to your project
3. ✅ Add reference in Visual Studio or .csproj
4. ✅ Import namespace: `using ReverseMarkdown;`
5. ✅ Use it: `var converter = new Converter();`

## Manual Test Application

This project includes a console application for manual testing:

```powershell
# Build manual test app
dotnet build src/ReverseMarkdown.ManualTest/ReverseMarkdown.ManualTest.csproj -c Release

# Run manual test
cd src/ReverseMarkdown.ManualTest/bin/Release/net48
.\ReverseMarkdown.ManualTest.exe

# Place HTML files in input/ folder, results will be in output/ folder
```

## Documentation

- [README.md](README.md) - Overview and usage examples
- [CHANGELOG.md](CHANGELOG.md) - Version history and changes
- [AI/project-build.md](AI/project-build.md) - Detailed build plan

## Support

If you encounter build issues:
1. Check [Troubleshooting](#troubleshooting) section
2. Verify all prerequisites are installed
3. Create an issue on GitHub repository
