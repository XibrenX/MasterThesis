namespace Cycles.Old
{
    public static class ReadConfig
    {
        private static Dictionary<string, string>? _config;

        public static Dictionary<string, string> Config
        {
            get
            {
                if (_config is not null)
                    return _config;

                var config = new Dictionary<string, string>();

                string configFile;
                if (!Environment.CurrentDirectory.Contains(@"\code\"))
                {
                    throw new Exception("Cannot find config file");
                }
                configFile = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf(@"\code\")) + @"\code\config";

                using (var sr = new StreamReader(configFile))
                {
                    while (!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var split = line.Split('=');
                            config[split[0].Trim()] = split[1].Trim();
                        }
                    }
                }

                _config = config;

                return _config;
            }
        }
    }
}
