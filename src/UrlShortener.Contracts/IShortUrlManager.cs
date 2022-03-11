using UrlShortener.Contracts.Enumerations;
using UrlShortener.Contracts.Models;

namespace UrlShortener.Contracts
{
    /// <summary>
    /// Interface for services that manage short urls.
    /// </summary>
    public interface IShortUrlManager
    {
        /// <summary>
        /// Attempt to create a new short url.
        /// </summary>
        /// <param name="targetUrl">The long url that the short url being created will target.</param>
        /// <param name="desiredShortUrlId">Optional. The desired identifier for the short url being created.</param>
        /// <returns>
        /// A tuple of <see cref="ResultCode.Success"/> and retrieved url info if successfully created and a tuple 
        /// of a different <see cref="ResultCode"/> denoting the reason why and null url info if unsuccessful.
        /// if it could not be created.
        /// </returns>
        (ResultCode, UrlInfo?) CreateShortUrl(string targetUrl, string desiredShortUrlId = "");

        /// <summary>
        /// Attempts to retrieve the short url information given its identifier.
        /// </summary>
        /// <param name="shortUrlId">The identifier for the short url being retrieved.</param>
        /// <returns>
        /// A tuple of <see cref="ResultCode.Success"/> and retrieved url info if successful and a tuple 
        /// of a different <see cref="ResultCode"/> denoting the reason why and null url info if unsuccessful.
        /// </returns>
        (ResultCode, UrlInfo?) GetShortUrl(string shortUrlId);

        /// <summary>
        /// Attempts to update the short url information given its identifier.
        /// </summary>
        /// <param name="shortUrlId">The identifier for the short url being updated.</param>
        /// <param name="targetUrl">The long url that the short url being updated will target.</param>
        /// <returns>
        /// A tuple of <see cref="ResultCode.Success"/> and retrieved url info if successfully updated and a tuple 
        /// of a different <see cref="ResultCode"/> denoting the reason why and null url info if unsuccessful.
        /// </returns>
        (ResultCode, UrlInfo?) UpdateShortUrl(string shortUrlId, string targetUrl);

        /// <summary>
        /// Attempts to delete a short url.
        /// </summary>
        /// <param name="shortUrlId">The identifier for the short url being deleted.</param>
        /// <returns>
        /// <see cref="ResultCode.Success"/> if the short url is successfully deleted, and another <see cref="ResultCode"/> 
        /// if it could not be deleted.
        /// </returns>
        ResultCode DeleteShortUrl(string shortUrlId);
    }
}