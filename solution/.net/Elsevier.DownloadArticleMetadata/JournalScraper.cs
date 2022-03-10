using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Elsevier.DownloadArticleMetadata
{
    class JournalScraper : IDisposable
    {
        private IWebDriver _driver;
        private string _outputDirectory;
        private Process _torProcess;
        private string _torBrowserPath;

        public JournalScraper(string outputDirectory, string torBrowserPath)
        {
            _outputDirectory = outputDirectory;
            _torBrowserPath = torBrowserPath;
        }

        public void RefreshBrowser()
        {
            Dispose();
            Console.WriteLine($"Initializing webdriver");
            _driver = CreateNewDriver();
            string ip = GetCurrentPublicIp(_driver);
            Console.WriteLine($"Executing from public ip: {ip}");
        }

        public void GetJournalData(long id, string title)
        {
            List<string> articles = ProcessJournal(title);
            int i = 1;
            foreach (string articleLink in articles)
            {
                string articleInformation = GetArticleInformation(articleLink);
                if (!Directory.Exists(Path.Combine(_outputDirectory, title)))
                {
                    Directory.CreateDirectory(Path.Combine(_outputDirectory, title));
                }
                using StreamWriter sw = new StreamWriter(Path.Combine(_outputDirectory, title, $"{i}.json"));
                sw.WriteLine(articleInformation);
                sw.Flush();
                sw.Close();
                i++;
            }
        }

        private static string GetArticleInformation(string link)
        {
            string url = $"https://sciencedirect.com/{link}";
            Console.WriteLine($"Reading article at: {url}");
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var content = doc.DocumentNode.SelectSingleNode("//script[@type = 'application/json' and @data-iso-key = '_0']").InnerText;
            return content;
        }

        private List<string> ProcessJournal(string journalName)
        {
            Console.WriteLine($"Processing: {journalName}");

            List<string> articleSubLinks = new List<string>();
            int offset = 0;
            int show = 100;

            while (true)
            {
                var url = $"https://www.sciencedirect.com/search?pub={journalName.Replace("-", "%20")}&show={show}&sortBy=date&offset={offset}&articleTypes=FLA";
                _driver.Navigate().GoToUrl(url);
                //_driver.Manage().Timeouts().PageLoad = new TimeSpan(0, 0, 10);
                Thread.Sleep(5000);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(_driver.PageSource);
                var articles = doc.DocumentNode.SelectNodes("//a[contains(@class, 'result-list-title-link')]");

                if (articles is null || articles.Count == 0)
                {
                    Console.WriteLine("Got no results, so I assume I got everything.");
                    break;
                }
                Console.WriteLine($"Found {articles.Count} article(s)");
                foreach (var a in articles)
                {
                    string link = a.GetAttributeValue("href", string.Empty);
                    articleSubLinks.Add(link);
                }
                offset = offset + show;
            }
            return articleSubLinks;
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing...");
            if (_driver != null)
                _driver.Close();
            if (_torProcess != null)
                _torProcess.Kill();
        }

        private IWebDriver CreateNewDriver()
        {
            Console.WriteLine("Create new webdriver");
            IWebDriver driver;

            _torProcess = new Process();
            _torProcess.StartInfo.FileName = _torBrowserPath;
            _torProcess.StartInfo.Arguments = "-n";
            _torProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            _torProcess.Start();

            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("network.proxy.type", 1);
            profile.SetPreference("network.proxy.socks", "127.0.0.1");
            profile.SetPreference("network.proxy.socks_port", 9150);

            CodePagesEncodingProvider.Instance.GetEncoding(437);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            FirefoxOptions options = new FirefoxOptions();
            options.BrowserExecutableLocation = _torBrowserPath;
            options.Profile = profile;
            driver = new FirefoxDriver(options);
            driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 30);
            driver.Manage().Timeouts().PageLoad = new TimeSpan(0, 0, 30);

            return driver;
        }

        private string GetCurrentPublicIp(IWebDriver driver)
        {
            string url = "https://www.whatsmyip.org/";
            driver.Navigate().GoToUrl(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(driver.PageSource);
            string ip = doc.DocumentNode.SelectSingleNode("//*[@id='ip']").InnerText;
            return ip;
        }
    }
}
