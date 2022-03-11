using CommandLine;
using Microsoft.Extensions.Hosting;
using UrlShortener.ConsoleApp.Commands;

namespace UrlShortener.ConsoleApp
{
    internal class CommandParserService : IHostedService
    {
        private Stream stream;
        private Action<ICommand> parsedCommandHandler;
        private Action<IEnumerable<Error>> parsingErrorHandler;

        public CommandParserService(Stream openStandardInput, Action<ICommand> handleParsedCommand, Action<IEnumerable<Error>> handleParseError)
        {
            this.stream = openStandardInput;
            this.parsedCommandHandler = handleParsedCommand;
            this.parsingErrorHandler = handleParseError;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var streamReader = new StreamReader(this.stream);

            Task.Run(
                async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Read from our stream and issue events when there is a new line.
                        try
                        {
                            var lineRead = await streamReader.ReadLineAsync();
                            var args = lineRead?.Split(' ');

                            Parser.Default.ParseArguments<CreateCommand, DeleteCommand, GetCommand>(args)
                               .WithParsed(this.parsedCommandHandler)
                               .WithNotParsed(this.parsingErrorHandler);
                        }
                        catch (Exception)
                        {
                        }
                    }
                },
                cancellationToken);

            // return this to allow other IHostedService-s to start.
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}