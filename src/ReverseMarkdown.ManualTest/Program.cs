using System;
using System.IO;
using ReverseMarkdown;

namespace ReverseMarkdown.ManualTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup console
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            PrintHeader();

            // Get base directory and setup paths
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var inputDir = Path.Combine(baseDir, "input");
            var outputDir = Path.Combine(baseDir, "output");

            // Ensure directories exist
            if (!Directory.Exists(inputDir))
            {
                Directory.CreateDirectory(inputDir);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Đã tạo folder 'input/'. Vui lòng thêm HTML files vào folder này và chạy lại.");
                Console.ResetColor();
                Console.WriteLine("\nNhấn phím bất kỳ để thoát...");
                Console.ReadKey();
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Configure converter
            var config = new Config
            {
                GithubFlavored = true,
                SmartHrefHandling = true,
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                RemoveComments = false,
                TableComplexHandling = Config.TableComplexHandlingOption.PreserveHtmlWhenComplex
            };

            var converter = new Converter(config);

            // Get all HTML files
            var htmlFiles = Directory.GetFiles(inputDir, "*.html");
            
            if (htmlFiles.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Không tìm thấy file HTML nào trong folder 'input/'.");
                Console.ResetColor();
                Console.WriteLine("\nNhấn phím bất kỳ để thoát...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Đang convert {htmlFiles.Length} file(s) HTML từ 'input/' sang 'output/'...\n");

            // Convert files
            int successCount = 0;
            int failCount = 0;

            for (int i = 0; i < htmlFiles.Length; i++)
            {
                var htmlFile = htmlFiles[i];
                var fileName = Path.GetFileName(htmlFile);
                var outputFileName = Path.GetFileNameWithoutExtension(htmlFile) + ".md";
                var outputPath = Path.Combine(outputDir, outputFileName);

                Console.Write($"[{i + 1}/{htmlFiles.Length}] {fileName} → {outputFileName} ... ");

                try
                {
                    // Read HTML content
                    var htmlContent = File.ReadAllText(htmlFile);

                    // Convert to Markdown
                    var markdown = converter.Convert(htmlContent);

                    // Write to output file
                    File.WriteAllText(outputPath, markdown);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK");
                    Console.ResetColor();
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.ResetColor();
                    failCount++;
                }
            }

            PrintFooter(successCount, failCount, htmlFiles.Length);

            Console.WriteLine("\nNhấn phím bất kỳ để thoát...");
            Console.ReadKey();
        }

        static void PrintHeader()
        {
            Console.WriteLine("========================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ReverseMarkdown Manual Test Tool");
            Console.ResetColor();
            Console.WriteLine("========================================");
            Console.WriteLine();
        }

        static void PrintFooter(int successCount, int failCount, int total)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Hoàn Thành Conversion!");
            Console.ResetColor();
            Console.WriteLine("========================================");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Thành công: {successCount}/{total} file(s)");
            Console.ResetColor();
            
            if (failCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Thất bại: {failCount}/{total} file(s)");
                Console.ResetColor();
            }
            
            Console.WriteLine("========================================");
        }
    }
}
