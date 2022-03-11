using UrlShortener.Contracts.Models;

namespace UrlShortener.Contracts
{
    /// <summary>
    /// Interface for repositories of short urls.
    /// </summary>
    public interface IShortUrlRepository
    {
        /// <summary>
        /// Adds a new short url to the repository.
        /// </summary>
        /// <param name="urlInfo">The short url's information to add to the repository.</param>
        /// <returns>True if the short url is successfully added, and false otherwise.</returns>
        bool AddShortUrl(UrlInfo urlInfo);

        /// <summary>
        /// Gets information about a short url in the repository.
        /// </summary>
        /// <param name="shortUrlId">The identifier of the short url to look for.</param>
        /// <returns>A tuple with the <see cref="UrlInfo?"/> with a True value, if one was found,
        /// and null with a False value, if none was found.</returns>
        (UrlInfo?, bool) GetShortUrl(string shortUrlId);

        /// <summary>
        /// Removes a short url in the repository, if one exists.
        /// </summary>
        /// <param name="shortUrlId">The identifier of the short url to look for and delete.</param>
        /// <returns>True if a short url was found and successfully deleted, and false otherwise.</returns>
        bool RemoveShortUrl(string shortUrlId);
    }
}