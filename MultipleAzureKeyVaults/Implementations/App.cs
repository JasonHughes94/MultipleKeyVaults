namespace MultipleAzureKeyVaults.Implementations
{
    using System.Text.Json;
    using Interfaces;
    using Microsoft.Extensions.Options;
    using Options;

    public class App : IApp
    {
        private readonly ApplicationOptions _options;

        public App(IOptions<ApplicationOptions> options)
        {
            _options = options.Value;
        }

        public Task<bool> Run()
        {
            Console.WriteLine(JsonSerializer.Serialize(_options));
            return Task.FromResult(true);
        }
    }
}