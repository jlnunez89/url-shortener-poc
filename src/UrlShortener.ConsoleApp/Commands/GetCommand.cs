using CommandLine;

namespace UrlShortener.ConsoleApp.Commands
{
    [Verb("get", HelpText = "Gets a short url's info.")]
	internal class GetCommand : ICommand
	{
		[Option('i', "identifier", Required = true, HelpText = "The identifier for the short url to get.")]
		public string ShortId { get; set; }

		public void Execute()
		{
			Console.WriteLine($"Executing get command. Id: {this.ShortId}.");
		}
	}
}
