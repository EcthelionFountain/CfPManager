using HtmlAgilityPack;
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
            doc.LoadHtml(html);

            return ReadPage(doc);
        }

        public virtual bool IsValidUrl(string url) => true;
        protected abstract Cfp ReadPage(HtmlDocument website);

        protected string Clean(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return HttpUtility.HtmlDecode(input).Replace("\n", "").Replace("\r", "").Trim();
        }

    }
    public class AVLdigitalReader: BaseReader
    {
        public override bool IsValidUrl(string url) 
            => url.Contains("avldigital.de");

        protected override Cfp ReadPage(HtmlDocument website)
        {
            return null;
        }
    }

    public class GenericReader: BaseReader
    {
        protected override Cfp ReadPage(HtmlDocument website)
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