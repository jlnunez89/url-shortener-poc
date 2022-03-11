using Microsoft.Extensions.Options;
using UrlShortener.Contracts;
using UrlShortener.Contracts.Enumerations;
using UrlShortener.Contracts.Models;

namespace UrlShortener
{
    /// <summary>
    /// Class that implements a simple short url manager.
    /// </summary>
    public class ShortUrlManager : IShortUrlManager
    {
        /// <summary>
        /// The short urls repository (storage) that this manager proxies to.
        /// </summary>
        private readonly IShortUrlRepository urlRepository;

        /// <summary>
        /// The options configured for this manager.
        /// </summary>
        private readonly ShortUrlManagerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortUrlManager"/> class.
        /// </summary>
        /// <param name="shortUrlRepository">A reference to a short url repository instance to proxy with for storage.</param>
        public ShortUrlManager(IShortUrlRepository shortUrlRepository, IOptions<ShortUrlManagerOptions> options)
        {
            this.urlRepository = shortUrlRepository ?? throw new ArgumentNullException(nameof(shortUrlRepository));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // validate options
            if (this.options.UrlIdMaximumLength <= 0)
            {
                throw new ArgumentException("The specified value must be a positive integer.", nameof(this.options.UrlIdMaximumLength));
            }

            if (this.options.UrlIdMinimumLength <= 0)
            {
                throw new ArgumentException("The specified value must be a positive integer.", nameof(this.options.UrlIdMinimumLength));
            }

            if (this.options.UrlIdMinimumLength > this.options.UrlIdMaximumLength)
            {
                throw new ArgumentException("The minimum value must less than or equal to the maximum value.", nameof(this.options.UrlIdMinimumLength));
            }

            if (this.options.MaximumCreationAttempts <= 0)
            {
                throw new ArgumentException("The specified value must be a positive integer.", nameof(this.options.MaximumCreationAttempts));
            }
        }

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
        public (ResultCode, UrlInfo?) CreateShortUrl(string targetUrl, string desiredShortUrlId = "")
        {
            if (string.IsNullOrEmpty(targetUrl) || !Uri.TryCreate(targetUrl, UriKind.Absolute, out _))
            {
                return (ResultCode.InvalidTargetUrl, null);
            }

            // They have a specific id in mind, so we only try that one.
            if (!string.IsNullOrEmpty(desiredShortUrlId))
            {
                // Validate the picked Url.
                if (desiredShortUrlId.Length < options.UrlIdMinimumLength ||
                    desiredShortUrlId.Length > options.UrlIdMaximumLength)
                {
                    return (ResultCode.InvalidUrlIdentifier, null);
                }

                var urlModel = new UrlInfo(desiredShortUrlId, targetUrl);

                if (this.urlRepository.AddShortUrl(urlModel))
                {
                    return (ResultCode.Success, urlModel);
                }

                return (ResultCode.AlreadyInUse, null);
            }

            if (this.AttemptToAddRandomized(targetUrl) is UrlInfo createdUrlInfo)
            {
                return (ResultCode.Success, createdUrlInfo);
            }

            return (ResultCode.UnableToCreateAfterMaxAttempts, null);
        }

        /// <summary>
        /// Attempts to delete a short url.
        /// </summary>
        /// <param name="shortUrlId">The identifier for the short url being deleted.</param>
        /// <returns>
        /// <see cref="ResultCode.Success"/> if the short url is successfully deleted, and another <see cref="ResultCode"/> 
        /// if it could not be deleted.
        /// </returns>
        public ResultCode DeleteShortUrl(string shortUrlId)
        {
            if (this.urlRepository.RemoveShortUrl(shortUrlId))
            {
                return ResultCode.Success;
            }

            return ResultCode.NotFound;
        }

        /// <summary>
        /// Attempts to retrieve the short url information given its identifier.
        /// </summary>
        /// <param name="shortUrlId">The identifier for the short url being retrieved.</param>
        /// <returns>
        /// A tuple of <see cref="ResultCode.Success"/> and retrieved url info if successful and a tuple 
        /// of a different <see cref="ResultCode"/> denoting the reason why a null url info if unsuccessful.
        /// </returns>
        public (ResultCode, UrlInfo?) GetShortUrl(string shortUrlId)
        {
            var (shortUrl, ok) = this.urlRepository.GetShortUrl(shortUrlId);

            if (ok)
            {
                if (shortUrl == null)
                {
                    throw new ApplicationException($"Expected to a non-null url info when getting from the url repo returned ok.");
                }

                shortUrl.Metrics.RetrievalCount++;

                return (ResultCode.Success, shortUrl);
            }

            return (ResultCode.NotFound, null);
        }

        /// <summary>
        /// Attempts to update the short url information given its identifier.
        /// </summary>
        /// <param name="shortUrlId">The identifier for the short url being updated.</param>
        /// <param name="targetUrl">The long url that the short url being updated will target.</param>
        /// <returns>
        /// A <see cref="NotSupportedException"/> always, because this is not supported.
        /// </returns>
        public (ResultCode, UrlInfo?) UpdateShortUrl(string shortUrlId, string targetUrl)
        {
            throw new NotSupportedException("This operation is not supported in this manager.");
        }

        /// <summary>
        /// Attempts to add a short url to the repository up to the maximum attempts configured in the options.
        /// </summary>
        /// <param name="targetUrl">The target url for the short url being added.</param>
        /// <returns>An instance of <see cref="UrlInfo"/> if the short url was successfully created, and null otherwise.</returns>
        private UrlInfo? AttemptToAddRandomized(string targetUrl)
        {
            var rng = new Random(DateTimeOffset.Now.Millisecond);

            // Generate a new short url identifier and try up to the configured amount of times:
            for (int i = 0; i < options.MaximumCreationAttempts; i++)
            {
                // 1) We pick a length between the minimum and maximum lengths configured.
                var randomIdLen = rng.Next(options.UrlIdMinimumLength, options.UrlIdMaximumLength + 1);

                // 2) We generate an identifier out of a Guid and truncate to the chosen length.
                var attemptWithId = Guid.NewGuid().ToString("N")[..randomIdLen];

                // 3) We try to create it.
                var urlModel = new UrlInfo(attemptWithId, targetUrl);

                if (this.urlRepository.AddShortUrl(urlModel))
                {
                    return urlModel;
                }
            }

            return null;
        }
    }
}