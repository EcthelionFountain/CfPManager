using HtmlAgilityPack;
using Microsoft.VisualBasic;
using System.Data.Common;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Scraper
{
    public interface IPageReader
    {
        Task<Cfp> ScrapeAsync(string url);
        bool IsValidUrl(string url);
    }
    public abstract class BaseReader: IPageReader
    {
        protected BaseReader()
        {
            
        }
        protected static readonly HttpClient client = new HttpClient();
        public async Task<Cfp> ScrapeAsync(string url)
        {
            var html = await client.GetStringAsync(url);
            var doc = new HtmlDocument();
            string id = GenerateId(url);
            doc.LoadHtml(html);

            return ReadPage(doc, id);
        }

        public virtual bool IsValidUrl(string url) => true;
        protected abstract Cfp ReadPage(HtmlDocument website, string id);

        protected string Clean(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return HttpUtility.HtmlDecode(input).Replace("\n", "").Replace("\r", "").Trim();
        }

        protected string GenerateId(string url)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(url.ToLower().Trim());
            byte[] hashBytes = MD5.HashData(inputBytes);

            return Convert.ToHexString(hashBytes);
        }

    }
    public class AVLdigitalReader: BaseReader
    {
        public override bool IsValidUrl(string url) 
            => url.Contains("avldigital.de");

        protected override Cfp ReadPage(HtmlDocument website, string id)
        {
            var headerNode = website.DocumentNode.SelectSingleNode("//h2[@id='content_heading-0']");
            string header = (headerNode != null) ? Clean(headerNode.InnerText): "No Title given";

            var deadlineNode = website.DocumentNode.SelectSingleNode("//span[p/b[contains(text(), 'Deadline Abstract')]]/p[2]");
            DateTime deadline = DateTime.MaxValue;
            if (deadlineNode != null)
            {
                string rawDate = deadlineNode.InnerText.Trim(); // Results in "01.02.2026"
                
                // Now convert it to a proper DateTime for your record
                if (DateTime.TryParse(rawDate, out deadline))
                {
                    Console.WriteLine($"Found deadline: {deadline:yyyy-MM-dd}");
                }
            }
            string description = "No description found.";

            // Target the specific class used for the body content
            var descNode = website.DocumentNode.SelectSingleNode("//div[contains(@class, 'field--name-body')]");

            if (descNode != null)
            {
                // Clean() will handle the &ldquo; and &ouml; entities automatically
                description = Clean(descNode.InnerText);
            }

            string contactEmail = "N/A";

            // Target the specific email field container
            var emailNode = website.DocumentNode.SelectSingleNode("//div[contains(@class, 'field--name-field-announcement-email')]//div[@class='field__item']");

            if (emailNode != null)
            {
                contactEmail = emailNode.InnerText.Trim();
            }

            string location = "None";

            var linkNode = website.DocumentNode.SelectSingleNode("//div[h4/b[contains(., 'Links')]]/a");
            string externalLink = linkNode?.GetAttributeValue("href", "") ?? "";

            var topicNodes = website.DocumentNode.SelectNodes("//div[contains(@class, 'flex-wrap')]//a");

            List<string> topics = topicNodes != null 
                ? topicNodes.Select(n => n.InnerText.Trim().TrimEnd(',')).ToList() 
                : new List<string>();
            
            DateTime creationDate = DateTime.Now; // Default

            var dateContainer = website.DocumentNode.SelectSingleNode("//div[contains(., 'Veröffentlicht am:')]");

            if (dateContainer != null)
            {
                // Get the full text: "Beitrag von: ... Veröffentlicht am: 19.12.2025 Letzte Änderung..."
                string fullText = dateContainer.InnerText;
                
                // Split the string to get the part after the label
                string[] parts = fullText.Split("Veröffentlicht am:");
                if (parts.Length > 1)
                {
                    // parts[1] starts with " 19.12.2025..."
                    // We take the first 10 characters (the date length)
                    string rawDate = parts[1].Trim().Substring(0, 10);
                    
                    DateTime.TryParse(rawDate, out creationDate);
                }
            }

            Cfp OutputFile = new Cfp(
                CfpID: id,
                Title: header,
                Deadline: deadline,
                SourceURL: externalLink,
                Location: location,
                Topics: topics,
                Descirption:description,
                CreationDate: creationDate,
                Contact: contactEmail                
            );

            return OutputFile;
        }
    }

    public class GenericReader: BaseReader
    {
        protected override Cfp ReadPage(HtmlDocument website, string id)
        {
            return null;
        }
    }

    public class ScraperFactory
    {
        private readonly List<IPageReader> _readers = [
            new AVLdigitalReader(),
            new GenericReader()
        ];

        public IPageReader GetReader(string url)
        {
            return _readers.FirstOrDefault(r => r.IsValidUrl(url))
                ?? throw new Exception("Now scraper for this site!");
        }
    }
}