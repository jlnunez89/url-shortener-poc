using CommandLine;

namespace UrlShortener.ConsoleApp.Commands
{
    [Verb("create", HelpText = "Creates a short url.")]
	internal class CreateCommand : ICommand
	{
		[Option('t', "target-url", Required = true, HelpText = "The target url for the short url being created.")]
        public string? TargetUrl { get; set; }

		[Option('i', "desired-id", Required = false, HelpText = "A desired id for the short url being created.")]
		public string? DesiredShortId { get; set; }

		public void Execute()
		{
			Console.WriteLine($"Executing create command. TargetUrl: {this.TargetUrl}, DesiredId: {this.DesiredShortId ?? "not supplied"}.");
		}
	}
}
