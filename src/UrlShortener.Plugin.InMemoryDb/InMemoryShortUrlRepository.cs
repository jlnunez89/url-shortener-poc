using UrlShortener.Contracts;
using UrlShortener.Contracts.Models;

namespace UrlShortener.Plugin.InMemoryDb
{
    /// <summary>
    /// Class that implements a short-url repository by simulating a Db with a dictionary in memory.
    /// This is a thread-safe implementation.
    /// </summary>
    public class InMemoryShortUrlRepository : IShortUrlRepository
    {
        /// <summary>
        /// Dictionary to hold the mapping of short url identifiers to the actual models.
        /// </summary>
        private readonly Dictionary<string, UrlInfo> shortUrlMap;

        /// <summary>
        /// Object used to lock on the <see cref="shortUrlMap"/>.
        /// </summary>
        private readonly object shortUrlMapLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryShortUrlRepository"/> class.
        /// </summary>
        public InMemoryShortUrlRepository()
        {
            // We could have used a Concurrent dictionary here but I wanted to showcase 
            // the use of locks to make this class thread-safe.
            this.shortUrlMap = new Dictionary<string, UrlInfo>();
            this.shortUrlMapLock = new object();
        }

        /// <summary>
        /// Adds a new short url to the repository.
        /// </summary>
        /// <param name="urlInfo">The short url's information to add to the repository.</param>
        /// <returns>True if the short url is successfully added, and false otherwise.</returns>
        public bool AddShortUrl(UrlInfo urlInfo)
        {
            lock (this.shortUrlMapLock)
            {
                if (this.shortUrlMap.ContainsKey(urlInfo.Identifier))
                {
                    return false;
                }

                this.shortUrlMap[urlInfo.Identifier] = urlInfo;

                return true;
            }
        }

        /// <summary>
        /// Gets information about a short url in the repository.
        /// </summary>
        /// <param name="shortUrlId">The identifier of the short url to look for.</param>
        /// <returns>A tuple with the <see cref="UrlInfo?"/> with a True value, if one was found,
        /// and null with a False value, if none was found.</returns>
        public (UrlInfo?, bool) GetShortUrl(string shortUrlId)
        {
            lock (this.shortUrlMapLock)
            {
                if (this.shortUrlMap.TryGetValue(shortUrlId, out UrlInfo? urlInfo))
                {
                    return (urlInfo, true);
                }

                return (null, false);
            }
        }

        /// <summary>
        /// Removes a short url in the repository, if one exists.
        /// </summary>
        /// <param name="shortUrlId">The identifier of the short url to look for and delete.</param>
        /// <returns>True if a short url was found and successfully deleted, and false otherwise.</returns>
        public bool RemoveShortUrl(string shortUrlId)
        {
            lock (this.shortUrlMapLock)
            {
                return this.shortUrlMap.Remove(shortUrlId);
            }
        }
    }
}
