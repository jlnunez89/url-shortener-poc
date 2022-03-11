using CommandLine;

namespace UrlShortener.ConsoleApp.Commands
{
    [Verb("delete", HelpText = "Deletes a short url.")]
	internal class DeleteCommand : ICommand
	{
		[Option('i', "identifier", Required = true, HelpText = "The identifier for the short url to delete.")]
		public string? ShortId { get; set; }

		public void Execute()
		{
			Console.WriteLine($"Executing delete command. Id: {this.ShortId}.");
		}
	}
}
