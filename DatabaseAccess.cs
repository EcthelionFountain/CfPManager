namespace Scraper;

using Microsoft.Data.Sqlite;

public class SqliteArchive(string connectionString) : IDiscoveryArchive
{
    public bool IsAlreadyProcessed(string url, DateTime lastModified)
    {
        using var conn = new SqliteConnection(connectionString);
        conn.Open(); // Standard SQL connections must be opened manually
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM ScrapeQueue WHERE Url = @url AND LastUpdate = @date)";
        cmd.Parameters.AddWithValue("@url", url);
        cmd.Parameters.AddWithValue("@date", lastModified.ToString("yyyy-MM-dd HH:mm:ss"));

        // ExecuteScalar returns an object, so we convert it to long (SQLite's boolean)
        var result = cmd.ExecuteScalar();
        return Convert.ToInt64(result) == 1;
    }
    public DateTime GetLastScrapeDate(string siteName)
    {
        return DateTime.Now;
    }
}