using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Contracts;

namespace UrlShortener.Plugin.InMemoryDb.Extensions
{
    /// <summary>
    /// Static class that provides convenient extension methods that allow to inject the components
    /// contained in this library.
    /// </summary>
    public static class CompositionRootExtensions
    {
        /// <summary>
        /// Adds a singleton instance of the <see cref="InMemoryShortUrlRepository"/> class to the
        /// services collection.
        /// </summary>
        /// <param name="services">The services collection to inject the instance to.</param>
        public static void AddInMemoryShortUrlRepository(this IServiceCollection services)
        {
            services.AddSingleton<IShortUrlRepository, InMemoryShortUrlRepository>();
        }
    }
}
