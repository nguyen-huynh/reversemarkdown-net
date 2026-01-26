using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;


namespace ReverseMarkdown.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [RPlotExporter]
    [MemoryDiagnoser]
    public class CompareBenchmark
    {
#if NET10_0
        private string _html = null!;
        private Converter _converter = null!;
#else
        private string _html;
        private Converter _converter;
#endif

        [GlobalSetup]
        public void Setup()
        {
            //_html = FileHelper.ReadFile("Files/1000-paragraphs.html");
            _html = FileHelper.ReadFile("Files/10k-paragraphs.html");
            _converter = new Converter(new Config());
        }

        [Benchmark]
        public string ReverseMarkdown()
        {
            var result = _converter.Convert(_html);
            return result;
        }
    }
}
