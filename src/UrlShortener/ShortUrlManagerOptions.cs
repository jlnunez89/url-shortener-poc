namespace UrlShortener
{
    /// <summary>
    /// Class that represents options for the <see cref="ShortUrlManager"/>.
    /// </summary>
    public class ShortUrlManagerOptions
    {
        /// <summary>
        /// Gets or sets the maximum valid length for a short url identifier.
        /// </summary>
        public int UrlIdMaximumLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum valid length for a short url identifier.
        /// </summary>
        public int UrlIdMinimumLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum attempts when attempting to create a short url.
        /// </summary>
        public int MaximumCreationAttempts { get; set; }
    }
}