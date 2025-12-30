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
    ///Plan: Crawl overview page
    /// Build a Queue of URLs that need to be scraped
    /// then give that Queue to the scraper
    /// 
    /// Extra steps: save Queue to a Database Table (in the same database as the CfP data will be!)
    
    public Queue<string> GetURLS(string baseURL)
    {
        return null;
    }

}