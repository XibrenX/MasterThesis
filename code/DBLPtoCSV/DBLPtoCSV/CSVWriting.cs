namespace DBLPtoCSV
{
    class CSVWriting : IDisposable
    {
        public static long SplitSize => long.Parse(ReadConfig.Config["NEO4J_MAX_CSV_LINES"]);

        public long BatchCount { get; private set; } = 0;
        public long TotalCount { get; private set; } = 0;

        public string Header { get; }
        private int fieldCount;

        public string Prefix { get; }

        public StreamWriter? Writer { get; private set; }

        public CSVWriting(string prefix, params string[] headers)
        {
            Header = string.Join(',',headers);
            fieldCount = headers.Length;
            Prefix = prefix;
            InitCSV();
        }

        public void WriteCSVLine(params string?[] fields)
        {
            if (fields.Length != fieldCount)
                throw new ArgumentException("fields length is not equal to header");

            Writer!.WriteLine(string.Join(',', fields.Select(f => Escape(f))));
            TotalCount += 1;
            if (TotalCount % SplitSize == 0)
            {
                BatchCount += 1;
                InitCSV();

                Console.WriteLine($"Starting new batch for {Prefix}");
            }
        }

        private string Escape(string? field)
        {
            if (field is not null && (field.Contains(',') || field.Contains('\\') || field.Contains('"')))
            {
                return $"\"{field.Replace("\"", "\"\"").Replace("\\", "\\\\")}\"";
            }
            else
            {
                return field;
            }
        }

        private void InitCSV()
        {
            Writer?.Close();
            Writer = new StreamWriter(Path.Combine(ReadConfig.Config["NEO4J_IMPORT"], $"{ReadConfig.Config["DBLP_TO_CSV_RUNID"]}_{Prefix}_{BatchCount}.csv"));
            Writer.WriteLine(Header);
        }

        public string ShortProgress() => $"{Prefix}: {BatchCount}-{TotalCount % SplitSize} ({TotalCount})";

        public string LongProgress() => $"{Prefix}:\nTotal: {TotalCount}\nBatch: {BatchCount}\nLine: {TotalCount % SplitSize}";

        public void Flush()
        {
            Writer?.Flush();
        }

        public void Close()
        { 
            Flush();
            Writer?.Close();
            Writer = null;
        }

        public void Dispose() => ((IDisposable?)Writer)?.Dispose();
    }
}