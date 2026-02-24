# Security Configuration Guide

## ⚠️ Security Notice

Khi xử lý HTML từ **untrusted sources** (Google Docs exports, user input, web scraping), bạn PHẢI configure whitelist để tránh XSS attacks.

## 🔒 Recommended Secure Configuration

```csharp
var config = new Config
{
    // Chỉ cho phép safe URL schemes
    WhitelistUriSchemes = 
    {
        "http",
        "https",
        "mailto",
        "tel"
    },
    
    // Remove HTML comments (có thể chứa malicious code)
    RemoveComments = true,
    
    // Optional: Handle unknown tags safely
    UnknownTags = UnknownTagsOption.Bypass  // hoặc Drop
};

var converter = new Converter(config);
string markdown = converter.Convert(untrustedHtml);
```

## 🚨 Security Risks Nếu Không Config

### ❌ Default Configuration (UNSAFE)
```csharp
// DANGEROUS: Cho phép TẤT CẢ URL schemes
var converter = new Converter();
```

**Các attack vectors:**

1. **JavaScript Protocol Injection**
   ```html
   <a href="javascript:alert(document.cookie)">Click me</a>
   ```
   → Output: `[Click me](javascript:alert(document.cookie))`

2. **Data URI XSS**
   ```html
   <img src="data:text/html,<script>alert(1)</script>">
   ```

3. **Event Handlers** (trong unknown tags)
   ```html
   <button onclick="malicious()">Button</button>
   ```

## ✅ Safe Configuration Examples

### For Google Docs Export
```csharp
var config = new Config
{
    WhitelistUriSchemes = { "http", "https" },
    RemoveComments = true,
    GithubFlavored = true  // for better formatting
};
```

### For Email Content
```csharp
var config = new Config
{
    WhitelistUriSchemes = { "http", "https", "mailto" },
    RemoveComments = true,
    UnknownTags = UnknownTagsOption.Drop  // drop all unknown tags
};
```

### For Restricted Internal Use
```csharp
var config = new Config
{
    WhitelistUriSchemes = { "http", "https", "tel", "mailto", "ftp" },
    RemoveComments = true,
    UnknownTags = UnknownTagsOption.Bypass
};
```

## 🔧 Advanced: Custom Protocol Support

Nếu cần custom schemes:

```csharp
var config = new Config
{
    WhitelistUriSchemes = 
    {
        "http",
        "https",
        "custom",      // your custom protocol
        "myapp"        // deep linking
    },
    RemoveComments = true
};
```

## 🧪 Verify Your Configuration

```csharp
// Test với malicious input
var testHtml = @"
    <a href='javascript:alert(1)'>JS Link</a>
    <a href='http://safe.com'>Safe Link</a>
";

var result = converter.Convert(testHtml);

// Expected output:
// JS Link  (chỉ text, không có link)
// [Safe Link](http://safe.com)
```

## 📚 Further Reading

- [OWASP XSS Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [Content Security Policy (CSP)](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
