
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UrlShortener.ConsoleApp.Commands;
using UrlShortener.Contracts;
using UrlShortener.Plugin.InMemoryDb.Extensions;

namespace UrlShortener.ConsoleApp
{
    /// <summary>
    /// Class that represents a program that runs the url shortener proof-of-concept service.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// Gets or sets a reference to the short url manager in use for this PoC.
        /// </summary>
        private static IShortUrlManager ShortUrlManager { get; set; }

        /// <summary>
        /// The cancellation token source for the entire application.
        /// </summary>
        private static readonly CancellationTokenSource MasterCancellationTokenSource = new();

        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">The arguments for this program.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var serverHost = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices(ConfigureServices)
                // TODO: enable logging (Serilog?) and inject it in the composition root, and consume the ILogger/ILoggerFactory interfaces within the services.
                .Build();

            // hack to make the manager available to the commands handler.
            ShortUrlManager = serverHost.Services.GetService<IShortUrlManager>() ?? throw new InvalidOperationException($"No {nameof(IShortUrlManager)} was registered.");

            // Just to provide some visual feedback on the console...
            
            Console.WriteLine("==============================");
            Console.WriteLine(" Short-Url manager PoC");
            Console.WriteLine("==============================");
            Console.WriteLine();
            Console.WriteLine("Type 'create' 'delete' or 'get' to get stated. You can also type '--help' to display available commands.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            await serverHost.RunAsync(Program.MasterCancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Composition root, where services are configured and added into the service collection, often depending on the configuration set.
        /// </summary>
        /// <param name="hostingContext">The hosting context.</param>
        /// <param name="services">The services collection.</param>
        private static void ConfigureServices(HostBuilderContext hostingContext, IServiceCollection services)
        {
            // We inject flavors of the interfaces in this PoC:

            // 1) The ShortUrl manager 
            services.AddShortUrlManager(hostingContext.Configuration);

            // 2) The In-memory repository.
            services.AddInMemoryShortUrlRepository();

            // Add this service which helps us read from the command line and interpret commands.
            services.AddHostedService((svcs) => new CommandParserService(Console.OpenStandardInput(), HandleParsedCommand, HandleParseError));
        }

        private static void HandleParsedCommand(ICommand parsedCommand)
        {
            switch (parsedCommand.GetType().Name)
            {
                case nameof(CreateCommand):
                    if (parsedCommand is CreateCommand createCommand)
                    {
                        var (resultCode, urlInfo) = ShortUrlManager.CreateShortUrl(createCommand.TargetUrl, createCommand.DesiredShortId);

                        Console.Write(resultCode);

                        if (urlInfo != null)
                        {
                            Console.WriteLine(urlInfo);
                        }

                        Console.WriteLine(Environment.NewLine);
                    }
                    break;
                case nameof(DeleteCommand):
                    if (parsedCommand is DeleteCommand deleteCommand)
                    {
                        var resultCode = ShortUrlManager.DeleteShortUrl(deleteCommand.ShortId);

                        Console.Write(resultCode);
                        Console.WriteLine(Environment.NewLine);
                    }
                    break;
                case nameof(GetCommand):
                    if (parsedCommand is GetCommand getCommand)
                    {
                        var (resultCode, urlInfo) = ShortUrlManager.GetShortUrl(getCommand.ShortId);

                        Console.Write(resultCode);
                        
                        if (urlInfo != null)
                        {
                            Console.WriteLine(urlInfo);
                        }

                        Console.WriteLine(Environment.NewLine);
                    }
                    break;
                default:
                    throw new NotSupportedException($"A command of type {parsedCommand.GetType().Name} is not supported.");
            }
        }

        static void HandleParseError(IEnumerable<Error> errors)
        {
            // TODO: handle parsing errors
        }
    }
}
