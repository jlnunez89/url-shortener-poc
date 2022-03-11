using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TestingUtils;
using UrlShortener.Contracts;
using UrlShortener.Contracts.Enumerations;
using UrlShortener.Contracts.Models;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
namespace UrlShortener.Tests
{
    [TestClass]
    public class ShortUrlManagerTests
    {
        [TestMethod]
        public void TestInitialization()
        {
            var mockRepository = Mock.Of<IShortUrlRepository>();
            var mockOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 1, minLen: 1); 

            // initializing with null arguments must throw.
            ExceptionAssert.Throws<ArgumentNullException>(() => new ShortUrlManager(null, null), "Value cannot be null. (Parameter 'shortUrlRepository')");
            ExceptionAssert.Throws<ArgumentNullException>(() => new ShortUrlManager(mockRepository, null), "Value cannot be null. (Parameter 'options')");

            // initializing with non-null arguments must not throw.
            _ = new ShortUrlManager(mockRepository, mockOptions);
        }

        [TestMethod]
        public void TestOptionsValidation()
        {
            var mockRepository = Mock.Of<IShortUrlRepository>();
            var allValidOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 1, minLen: 1);

            var invalidMaxAttemptsOptions = this.GenerateOptions(maxAttempts: 0, maxLen: 1, minLen: 1);

            ExceptionAssert.Throws<ArgumentException>(() => new ShortUrlManager(mockRepository, invalidMaxAttemptsOptions), $"The specified value must be a positive integer. (Parameter '{nameof(ShortUrlManagerOptions.MaximumCreationAttempts)}')");

            var invalidMaxLengthOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 0, minLen: 1);

            ExceptionAssert.Throws<ArgumentException>(() => new ShortUrlManager(mockRepository, invalidMaxLengthOptions), $"The specified value must be a positive integer. (Parameter '{nameof(ShortUrlManagerOptions.UrlIdMaximumLength)}')");

            var invalidMinLengthOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 1, minLen: 0);

            ExceptionAssert.Throws<ArgumentException>(() => new ShortUrlManager(mockRepository, invalidMinLengthOptions), $"The specified value must be a positive integer. (Parameter '{nameof(ShortUrlManagerOptions.UrlIdMinimumLength)}')");

            var minGreaterThanMaxLengthOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 1, minLen: 2);

            ExceptionAssert.Throws<ArgumentException>(() => new ShortUrlManager(mockRepository, minGreaterThanMaxLengthOptions), $"The minimum value must less than or equal to the maximum value. (Parameter '{nameof(ShortUrlManagerOptions.UrlIdMinimumLength)}')");
        }

        [TestMethod]
        public void TestCreateShortUrl_WitInvalidTargetUrl_Throws()
        {
            const string DesiredShortUrl = "potato";
            const string TargetUrl = "this is not a valid url";
            const ResultCode ExpectedResultCode = ResultCode.InvalidTargetUrl;

            var aDictionary = new Dictionary<string, UrlInfo>();
            var mockRepo = this.GenerateGoodRepoMock(aDictionary);
            var mockOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 6, minLen: 1);

            var urlManager = new ShortUrlManager(mockRepo, mockOptions);

            // While this shouldn't throw, it should return an invalid url code.
            var (resultCode, urlInfo) = urlManager.CreateShortUrl(TargetUrl, DesiredShortUrl);

            Assert.AreEqual(ExpectedResultCode, resultCode, $"Result code does not match: expected {ExpectedResultCode} but got: {resultCode}.");
            Assert.IsFalse(aDictionary.ContainsKey(DesiredShortUrl), $"Expected dictionary not to contain record with key {DesiredShortUrl} but it didn't.");
        }

        [TestMethod]
        public void TestCreateShortUrl_WithDesiredId_Passes()
        {
            const string DesiredShortUrl = "potato";
            const string TargetUrl = "https://example.com";
            const ResultCode ExpectedResultCode = ResultCode.Success;
            const int ExpectedDictionaryCount = 1;

            var aDictionary = new Dictionary<string, UrlInfo>();
            var mockRepo = this.GenerateGoodRepoMock(aDictionary);
            var mockOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 6, minLen: 1);

            var urlManager = new ShortUrlManager(mockRepo, mockOptions);

            // this should not throw.
            var (resultCode, urlInfo) = urlManager.CreateShortUrl(TargetUrl, DesiredShortUrl);

            Assert.AreEqual(ExpectedResultCode, resultCode, $"Result code does not match: expected {ExpectedResultCode} but got: {resultCode}.");
            Assert.AreEqual(ExpectedDictionaryCount, aDictionary.Count, $"Expected count of elements in dictionary does not match: expected {ExpectedDictionaryCount} but got: {aDictionary.Count}");
            Assert.IsTrue(aDictionary.ContainsKey(DesiredShortUrl), $"Expected dictionary to contain record with key {DesiredShortUrl} but it didn't.");

            var record = aDictionary[DesiredShortUrl];

            Assert.AreEqual(TargetUrl, record.TargetUrl, $"Expected {nameof(record.TargetUrl)} in record to be {TargetUrl} but it is {record.TargetUrl}.");
            Assert.IsFalse(string.IsNullOrEmpty(record.Identifier), $"Expected {nameof(record.Identifier)} in record not to be empty.");
            Assert.IsNotNull(record.Metrics, $"Expected {nameof(record.Metrics)} in record not to be null.");
        }

        [TestMethod]
        public void TestCreateShortUrl_WithRandomId_Passes()
        {
            const string TargetUrl = "https://example.com";
            const ResultCode ExpectedResultCode = ResultCode.Success;
            const int ExpectedDictionaryCount = 1;

            var aDictionary = new Dictionary<string, UrlInfo>();
            var mockRepo = this.GenerateGoodRepoMock(aDictionary);
            var mockOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 6, minLen: 1);

            var urlManager = new ShortUrlManager(mockRepo, mockOptions);

            // this should not throw.
            var (resultCode, urlInfo) = urlManager.CreateShortUrl(TargetUrl);

            Assert.AreEqual(ExpectedResultCode, resultCode, $"Result code does not match: expected {ExpectedResultCode} but got: {resultCode}.");
            Assert.AreEqual(ExpectedDictionaryCount, aDictionary.Count, $"Expected count of elements in dictionary does not match: expected {ExpectedDictionaryCount} but got: {aDictionary.Count}");

            var (shortUrlId, urlStored) = aDictionary.First();

            // The generated Id must conform to the options' max and min lengths.
            Assert.IsTrue(shortUrlId.Length >= mockOptions.Value.UrlIdMinimumLength && shortUrlId.Length <= mockOptions.Value.UrlIdMaximumLength,
                $"Expected the id's length to be between the minium and maximum url length specified in options, but got {shortUrlId.Length}");

            Assert.AreEqual(TargetUrl, urlStored.TargetUrl, $"Expected {nameof(urlStored.TargetUrl)} in record to be {TargetUrl} but it is {urlStored.TargetUrl}.");
            Assert.IsFalse(string.IsNullOrEmpty(urlStored.Identifier), $"Expected {nameof(urlStored.Identifier)} in record not to be empty.");
            Assert.IsNotNull(urlStored.Metrics, $"Expected {nameof(urlStored.Metrics)} in record not to be null.");
        }

        [TestMethod]
        public void TestDeleteShortUrl()
        {
            const string ShortUrlId = "adroit";
            const string TargetUrl = "https://example.com";
            const ResultCode ExpectedResultCode = ResultCode.Success;
            const int ExpectedDictionaryCount = 0;

            // Seed the dictionary with a record.
            var aDictionary = new Dictionary<string, UrlInfo>()
            {
                { ShortUrlId, new UrlInfo(ShortUrlId, TargetUrl) }
            };

            var mockRepo = this.GenerateGoodRepoMock(aDictionary);
            var mockOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 6, minLen: 1);

            var urlManager = new ShortUrlManager(mockRepo, mockOptions);

            // this should not throw.
            var resultCode = urlManager.DeleteShortUrl(ShortUrlId);

            Assert.AreEqual(ExpectedResultCode, resultCode, $"Result code does not match: expected {ExpectedResultCode} but got: {resultCode}.");
            Assert.AreEqual(ExpectedDictionaryCount, aDictionary.Count, $"Expected count of elements in dictionary does not match: expected {ExpectedDictionaryCount} but got: {aDictionary.Count}");
        }

        [TestMethod]
        public void TestGetShortUrl()
        {
            const string ShortUrlId = "hippo";
            const string NonExistentShortUrlId = "rhyno";
            const string TargetUrl = "https://example.com";
            const ResultCode ExpectedResultCode = ResultCode.Success;
            const int ExpectedDictionaryCount = 1;
            const ulong ExpectedRetrievalCountMetricValue = 1;

            // Seed the dictionary with a record.
            var aDictionary = new Dictionary<string, UrlInfo>()
            {
                { ShortUrlId, new UrlInfo(ShortUrlId, TargetUrl) }
            };

            var mockRepo = this.GenerateGoodRepoMock(aDictionary);
            var mockOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 6, minLen: 1);

            var urlManager = new ShortUrlManager(mockRepo, mockOptions);

            // this should not throw.
            var (resultCode, urlInfo) = urlManager.GetShortUrl(ShortUrlId);

            Assert.AreEqual(ExpectedResultCode, resultCode, $"Result code does not match: expected {ExpectedResultCode} but got: {resultCode}.");
            Assert.IsNotNull(urlInfo, $"Expected {nameof(urlInfo)} not to be null.");

            Assert.AreEqual(ExpectedDictionaryCount, aDictionary.Count, $"Expected count of elements in dictionary does not match: expected {ExpectedDictionaryCount} but got: {aDictionary.Count}");
            Assert.IsTrue(aDictionary.ContainsKey(ShortUrlId), $"Expected dictionary to contain record with key {ShortUrlId} but it didn't.");
            
            Assert.AreEqual(TargetUrl, urlInfo.TargetUrl, $"Expected {nameof(urlInfo.TargetUrl)} in record to be {TargetUrl} but it is {urlInfo.TargetUrl}.");
            Assert.IsFalse(string.IsNullOrEmpty(urlInfo.Identifier), $"Expected {nameof(urlInfo.Identifier)} in record not to be empty.");
            Assert.IsNotNull(urlInfo.Metrics, $"Expected {nameof(urlInfo.Metrics)} in record not to be null.");

            Assert.AreEqual(ExpectedRetrievalCountMetricValue, urlInfo.Metrics.RetrievalCount, $"Expected retrieval count metric value to be {ExpectedRetrievalCountMetricValue} but got: {urlInfo.Metrics.RetrievalCount}.");

            // Now just test with something we know doesn't exist.
            (resultCode, urlInfo) = urlManager.GetShortUrl(NonExistentShortUrlId);

            Assert.AreEqual(ResultCode.NotFound, resultCode, $"Result code does not match: expected {ResultCode.NotFound} but got: {resultCode}.");
            Assert.IsNull(urlInfo, "Expected returned url info to be null.");
        }

        [TestMethod]
        public void TestUpdateShortUrlThrows()
        {
            const string AnyTargetId = "potato";
            const string AnyUrlId = "123456";

            var mockRepository = Mock.Of<IShortUrlRepository>();
            var mockOptions = this.GenerateOptions(maxAttempts: 1, maxLen: 1, minLen: 1); 

            var shortUrlManager = new ShortUrlManager(mockRepository, mockOptions);

            ExceptionAssert.Throws<NotSupportedException>(() => shortUrlManager.UpdateShortUrl(AnyUrlId, AnyTargetId), "This operation is not supported in this manager.");
        }

        private IShortUrlRepository GenerateGoodRepoMock(Dictionary<string, UrlInfo> dictionary)
        {
            var mockRepository = new Mock<IShortUrlRepository>();

            // support adding a url.
            mockRepository.Setup(repo => repo.AddShortUrl(It.IsAny<UrlInfo>())).Returns<UrlInfo>((urlInfo) =>
            {
                dictionary.Add(urlInfo.Identifier, urlInfo);

                return true;
            });

            // support deleting a url.
            mockRepository.Setup(repo => repo.RemoveShortUrl(It.IsAny<string>())).Returns<string>((urlId) =>
            {
                return dictionary.Remove(urlId);
            });

            // support getting the url.
            mockRepository.Setup(repo => repo.GetShortUrl(It.IsAny<string>())).Returns<string>((urlId) =>
            {
                return dictionary.TryGetValue(urlId, out UrlInfo? urlInfo) ? (urlInfo, true) : (null, false);
            });

            return mockRepository.Object;
        }

        private IOptions<ShortUrlManagerOptions> GenerateOptions(int maxAttempts, int maxLen, int minLen)
        {
            return Options.Create(new ShortUrlManagerOptions()
            {
                MaximumCreationAttempts = maxAttempts,
                UrlIdMinimumLength = minLen,
                UrlIdMaximumLength = maxLen,
            });
        }
    }
}
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type