using ReverseMarkdown;
using System;

namespace SecurityExample
{
    /// <summary>
    /// Demonstrates secure configuration for ReverseMarkdown when processing untrusted HTML
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ReverseMarkdown Security Configuration Examples ===\n");

            // Example 1: UNSAFE - Default configuration
            UnsafeExample();

            Console.WriteLine("\n" + new string('-', 60) + "\n");

            // Example 2: SAFE - Recommended configuration
            SafeExample();

            Console.WriteLine("\n" + new string('-', 60) + "\n");

            // Example 3: Google Docs specific
            GoogleDocsExample();

            Console.ReadKey();
        }

        static void UnsafeExample()
        {
            Console.WriteLine("❌ UNSAFE - Default Configuration");
            Console.WriteLine("(Allows ALL URL schemes including javascript:)\n");

            var converter = new Converter(); // DANGEROUS!

            string maliciousHtml = @"
                <p>Safe paragraph</p>
                <a href='javascript:alert(document.cookie)'>Click for XSS</a>
                <a href='http://safe.com'>Safe Link</a>
            ";

            string result = converter.Convert(maliciousHtml);
            
            Console.WriteLine("Input HTML:");
            Console.WriteLine(maliciousHtml);
            Console.WriteLine("\nOutput Markdown:");
            Console.WriteLine(result);
            Console.WriteLine("\n⚠️ NOTICE: javascript: protocol is preserved in output!");
        }

        static void SafeExample()
        {
            Console.WriteLine("✅ SAFE - Recommended Secure Configuration\n");

            var config = new Config
            {
                // CRITICAL: Whitelist only safe schemes
                WhitelistUriSchemes = 
                {
                    "http",
                    "https",
                    "mailto",
                    "tel"
                },

                // Remove HTML comments (may contain malicious content)
                RemoveComments = true,

                // Handle unknown tags safely
                UnknownTags = Config.UnknownTagsOption.Bypass
            };

            var converter = new Converter(config);

            string maliciousHtml = @"
                <p>Safe paragraph</p>
                <a href='javascript:alert(document.cookie)'>Click for XSS</a>
                <a href='http://safe.com'>Safe Link</a>
                <a href='tel:+1234567890'>Call us</a>
            ";

            string result = converter.Convert(maliciousHtml);

            Console.WriteLine("Input HTML:");
            Console.WriteLine(maliciousHtml);
            Console.WriteLine("\nOutput Markdown:");
            Console.WriteLine(result);
            Console.WriteLine("\n✅ SECURE: javascript: link is stripped, only text remains!");
        }

        static void GoogleDocsExample()
        {
            Console.WriteLine("📄 Google Docs Export - Secure Configuration\n");

            var config = new Config
            {
                // Google Docs only uses http/https
                WhitelistUriSchemes = { "http", "https" },
                
                // Google Docs may have comments
                RemoveComments = true,
                
                // Better table formatting
                GithubFlavored = true,
                
                // Handle Google Docs specific tags
                UnknownTags = Config.UnknownTagsOption.Bypass
            };

            var converter = new Converter(config);

            string googleDocsHtml = @"
                <div style='font-family:Arial'>
                    <h1>Document Title</h1>
                    <p>This is a <strong>bold</strong> paragraph.</p>
                    <!-- Google Docs metadata -->
                    <a href='https://google.com'>External Link</a>
                    <script>
                        // Potential tracking script
                        console.log('tracking');
                    </script>
                </div>
            ";

            string result = converter.Convert(googleDocsHtml);

            Console.WriteLine("Input HTML (Google Docs export):");
            Console.WriteLine(googleDocsHtml);
            Console.WriteLine("\nOutput Markdown:");
            Console.WriteLine(result);
            Console.WriteLine("\n✅ Clean output: scripts dropped, comments removed, safe links preserved");
        }
    }
}
