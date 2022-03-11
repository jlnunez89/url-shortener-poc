using Microsoft.Extensions.Configuration;
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
        /// Additionally, registers the options related to the concrete implementations added, such as:
        ///     <see cref="ShortUrlManagerOptions"/>.
        /// </summary>
        /// <param name="services">The services collection to inject the instance to.</param>
        public static void AddShortUrlManager(this IServiceCollection services, IConfiguration config)
        {
            // configure options for the short url manager.
            services.Configure<ShortUrlManagerOptions>(config.GetSection(nameof(ShortUrlManagerOptions)));

            services.AddSingleton<IShortUrlManager, ShortUrlManager>();
        }
    }
}
