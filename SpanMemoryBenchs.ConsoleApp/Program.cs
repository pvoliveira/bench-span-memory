namespace SpanMemoryBenchs.ConsoleApp
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;

    [MemoryDiagnoser]
    public class SpanBench
    {
        private string _textSample = "new text to test span type\nnew line to test split\ntrying to replicate allocation of strings";

        public SpanBench()
        {
            
        }

        [Benchmark]
        public void processString()
        {
            DoSomething(_textSample);
        }

        [Benchmark]
        public void processStringSpan()
        {
            DoSomething(_textSample.AsSpan());
        }

        // split lines and process each one
        public void DoSomething(string text)
        {
            const char lb = '\n';

            var lines = text.Split(lb);
            foreach (var l in lines)
            {
                countLineWords(l);
            }
        }

        public int countLineWords(string line)
        {
            const char lb = '\n';

            // count words
            return line.Split(lb, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public void DoSomething(ReadOnlySpan<char> text)
        {
            const char lb = '\n';
            
            // if does'nt exists linebreak, count the words and returns
            if (text.IndexOf(lb) < 0)
            {
                countLineWords(text);
                return;
            }

            int lastLb = 0;

            do
            {
                int idx = text.Slice(lastLb).IndexOf(lb) + lastLb;
                countLineWords(text.Slice(lastLb, (idx - lastLb)));
                lastLb = idx + 1;

                if (text.Slice(lastLb).IndexOf(lb) < 0 
                    && lastLb < (text.Length - 1))
                {
                    countLineWords(text.Slice(lastLb));
                }

            }
            while (text.Slice(lastLb).IndexOf(lb) >= 0);
        }

        public int countLineWords(ReadOnlySpan<char> line)
        {
            const char space = ' ';
            
            // count words
            int lastSpace = 0;
            int words = 0;

            // one word
            if (line.Slice(lastSpace).IndexOf(space) < 0 
                && (line.Length - 1) > 0)
            {
                return 1;
            }
            
            
            do
            {
                if (lastSpace >= line.Length)
                {
                    break;
                }

                int idx = line.Slice(lastSpace).IndexOf(space) + lastSpace;
                
                if (idx >= 0)
                {
                    if ((idx - lastSpace) >= 1)
                    {
                        words++;
                    }

                    lastSpace = idx + 1;
                }

                // verify if exists more chars
                if (line.Slice(lastSpace).IndexOf(space) < 0
                    && line.Slice(lastSpace).Length > 0)
                {
                    words++;
                }
            }
            while (line.Slice(lastSpace).IndexOf(space) >= 0);

            return words;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            // var test = new SpanBench();
            // test.DoSomething("test counting\nlines".AsSpan());

            var summary = BenchmarkRunner.Run<SpanBench>();
        }
    }
}
