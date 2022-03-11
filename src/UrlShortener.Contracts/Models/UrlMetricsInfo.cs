namespace UrlShortener.Contracts.Models
{
    /// <summary>
    /// Class that represents metrics information about a short url.
    /// </summary>
    public class UrlMetricsInfo
    {
        /// <summary>
        /// Gets or sets the count of times that a url has been retrieved.
        /// </summary>
        public ulong RetrievalCount { get; set; }
    }
}