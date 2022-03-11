namespace UrlShortener.Contracts.Models
{
    /// <summary>
    /// Class that represents information of a short url.
    /// </summary>
    public class UrlInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlInfo"/> class.
        /// </summary>
        /// <param name="shortUrlId">The identifier for the short url.</param>
        /// <param name="targetUrl">The target url that the short url will reference.</param>
        /// <exception cref="ArgumentNullException">When either of <paramref name="shortUrlId"/> or
        /// <paramref name="targetUrl"/> have invalid values.</exception>
        public UrlInfo(string shortUrlId, string targetUrl)
        {
            this.Identifier = shortUrlId ?? throw new ArgumentNullException(nameof(shortUrlId));
            this.TargetUrl = targetUrl ?? throw new ArgumentNullException(nameof(targetUrl));
            this.Metrics = new UrlMetricsInfo() { RetrievalCount = 0 };
        }

        /// <summary>
        /// Gets or sets the identifier for the short url.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the actual, long url that this short url points to.
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// Gets or sets the metrics associated with this url.
        /// </summary>
        public UrlMetricsInfo Metrics { get; set; }

        public override string ToString()
        {
            return $"\tIdentifier: {this.Identifier}, TargetUrl: {this.TargetUrl}. Metrics: [RetrievalCount: {this.Metrics.RetrievalCount}]";
        }
    }
}
