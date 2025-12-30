
namespace Scraper
{

    public record Cfp(
        string CfpID,
        string Title,
        string SourceURL,
        string Location,
        List<string> Topics,
        string Descirption,
        DateTime CreationDate
    );
}