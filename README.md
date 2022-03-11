# How to use:
1) Clone or download repo.
1) Build solution:
    ```
    dotnet build src/url-shortener-poc.sln
    ```
1) Change to build dir:

    ```
    cd src/UrlShortener.ConsoleApp/bin/Debug/net6.0/
    ```
1) Run the console app:
   ```
   dotnet UrlShortener.ConsoleApp.dll
   ```
1) Use the `create` `delete` or `get` commands.
   
> Ctrl+C to interrupt (exit) execution.

# TODOs:
- Implement logging (using dependency injection too).
- Improve error reporting (e.g. it tells you that you are giving it an invalid Id but it doesn't say why).