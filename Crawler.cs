using HtmlAgilityPack;

namespace Scraper;

public interface IDiscoveryArchive
{
    bool IsAlreadyProcessed(string url, DateTime lastModified);

    DateTime GetLastScrapeDate(string siteName);
}

public abstract class Crawler
{
    
}

public class AVLdigitalCrawler(IDiscoveryArchive archive)
{
    private readonly HttpClient _client = new();
    ///Plan: Crawl overview page
    /// Build a Queue of URLs that need to be scraped
    /// then give that Queue to the scraper
    /// 
    /// Extra steps: save Queue to a Database Table (in the same database as the CfP data will be!)
    
    public async Task<Queue<string>> GetURLS(string baseURL)
    {
        var urlQueue = new Queue<string>();

        int offset = 0;
        bool keepCrawling = true;

        while(keepCrawling)
        {
            
            string separator = baseURL.Contains("?") ? "&": "?";
            string currentUrl = $"{baseURL}{separator}start={offset}";

            try{

                string html = await _client.GetStringAsync(baseURL);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var cards = doc.DocumentNode.SelectNodes("//h4[@class='card-title break-words']");

                if (cards == null) return urlQueue;

                foreach (var card in cards)
                {
                    var linkNode = card.SelectSingleNode(".//a");
                    if (linkNode == null) continue;

                    string relativeUrl = linkNode.GetAttributeValue("href", "");

                    string fullUrl = "https://avldigital.de/de/vernetzen/fachinformationen/"+relativeUrl;

                    var dateSpan = card.ParentNode.SelectSingleNode(".//span[contains(., 'Veröffentlicht am:')]/following-sibling::span[@class='avl_grey']");

                    DateTime publishedDate = DateTime.MinValue;
                    if (dateSpan != null)
                    {
                        DateTime.TryParse(dateSpan.InnerText.Trim(), out publishedDate);
                    }

                    // 4. THE STOP CONDITION
                    // If the archive tells us we've already processed this specific URL at this date,
                    // and assuming the list is sorted by date, we can stop crawling.
                    if (archive.IsAlreadyProcessed(fullUrl, publishedDate))
                    {
                        Console.WriteLine("Reached already processed records. Stopping discovery.");
                        keepCrawling = false;
                        break; 
                    }

                    urlQueue.Enqueue(fullUrl);

                    // Don't overtax the server.
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to crawl offset {offset}: {ex.Message}");
                break;
            }

            offset += 10;
        }

        return urlQueue;
        
    }

}