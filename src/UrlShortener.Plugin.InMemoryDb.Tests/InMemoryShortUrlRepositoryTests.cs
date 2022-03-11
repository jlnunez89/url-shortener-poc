using Microsoft.VisualStudio.TestTools.UnitTesting;
using UrlShortener.Contracts.Models;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
namespace UrlShortener.Plugin.InMemoryDb.Tests
{
    [TestClass]
    public class ShortUrlManagerTests
    {
        [TestMethod]
        public void TestInitialization()
        {
            _ = new InMemoryShortUrlRepository();
        }

        [TestMethod]
        public void TestAdd()
        {
            const string AnyString = "jell-o";

            var urlToAdd = new UrlInfo(AnyString, AnyString);

            var repo = new InMemoryShortUrlRepository();

            // Adding any record for the first time should be fine.
            Assert.IsTrue(repo.AddShortUrl(urlToAdd));

            // Attempting to add it again (same id) should return false instead.
            Assert.IsFalse(repo.AddShortUrl(urlToAdd));
        }

        [TestMethod]
        public void TestRemove()
        {
            const string AnyString = "jell-o";

            var urlToAdd = new UrlInfo(AnyString, AnyString);

            var repo = new InMemoryShortUrlRepository();

            // Attempting to remove an inexistent record should return false.
            Assert.IsFalse(repo.RemoveShortUrl(AnyString));

            // Adding any record for the first time should be fine.
            Assert.IsTrue(repo.AddShortUrl(urlToAdd));

            // Attempting to remove it again should return true.
            Assert.IsTrue(repo.RemoveShortUrl(urlToAdd.Identifier));

            // Attempting to remove an inexistent record should return false.
            Assert.IsFalse(repo.RemoveShortUrl(urlToAdd.Identifier));
        }

        [TestMethod]
        public void TestGet()
        {
            const string AnyString = "jell-o";

            // We expect a 0 because it's not the domain of the storage layer to increment this,
            // but rather the service's.
            const ulong ExpectedRetrievalCountMetricValue = 0;

            var urlToAdd = new UrlInfo(AnyString, AnyString);

            var repo = new InMemoryShortUrlRepository();

            // Adding any record for the first time should be fine.
            Assert.IsTrue(repo.AddShortUrl(urlToAdd));

            // Try getting it back now.
            var (urlRetrieved, ok) = repo.GetShortUrl(AnyString);

            Assert.IsTrue(ok);
            Assert.IsNotNull(urlRetrieved);
            Assert.AreEqual(urlToAdd, urlRetrieved);
            Assert.IsNotNull(urlRetrieved.Metrics);
            Assert.AreEqual(ExpectedRetrievalCountMetricValue, urlRetrieved.Metrics.RetrievalCount);
        }
    }
}
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type