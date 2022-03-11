namespace UrlShortener.Contracts.Enumerations
{
    /// <summary>
    /// Enumeration of the possible result codes for the url manager.
    /// </summary>
    public enum ResultCode
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Success,

        /// <summary>
        /// The target url supplied is invalid.
        /// </summary>
        InvalidTargetUrl,

        /// <summary>
        /// The manually-picked short url identifier is invalid.
        /// </summary>
        InvalidUrlIdentifier,

        /// <summary>
        /// The desired short url is already in use.
        /// </summary>
        AlreadyInUse,

        /// <summary>
        /// The short url could not be found in the storage layer, so the operation cannot be performed.
        /// </summary>
        NotFound,

        /// <summary>
        /// The creation of a short url (with a randomized id) failed after attempting the maximum amount of tries. 
        /// </summary>
        UnableToCreateAfterMaxAttempts,
    }
}