using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultipleAzureKeyVaults.Implementations;
using MultipleAzureKeyVaults.Interfaces;
using MultipleAzureKeyVaults.Options;
using Serilog;

Console.WriteLine("Hello, World!");

var host = AppStartup();

using var serviceScope = host.Services.CreateScope();
var provider = serviceScope.ServiceProvider;

var app = provider.GetRequiredService<IApp>();
var appResult = await app.Run();
return appResult ? 0 : 1;

static IHost AppStartup()
{
    var builder = new ConfigurationBuilder();
    var configurationRoot = ConfigSetup(builder);

    Log.Logger = new LoggerConfiguration().ReadFrom
        .Configuration(builder.Build())
        .Enrich.FromLogContext()
        .CreateLogger();

    var host = Host.CreateDefaultBuilder()
        .ConfigureServices(
            (context, services) =>
            {
                ConfigureServices(services, configurationRoot);
            }
        )
        .UseSerilog()
        .Build();

    return host;
}


static IConfigurationRoot ConfigSetup(ConfigurationBuilder configurationBuilder)
{
    configurationBuilder
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false)
        .AddUserSecrets("b90e64ff-4614-4e8a-b9a3-a04ac4ea662c");

    return configurationBuilder.Build();
}

static void ConfigureServices(
    IServiceCollection serviceCollection,
    IConfigurationRoot configurationRoot
)
{
    serviceCollection.Configure<ApplicationOptions>(configurationRoot);
    var applicationOptions = configurationRoot.Get<ApplicationOptions>();

    var keyVaultOneUrl = "https://" + applicationOptions.KeyVaultOne.KeyVaultName + ".vault.azure.net";
    var keyVaultTwoUrl = "https://" + applicationOptions.KeyVaultOne.KeyVaultName + ".vault.azure.net";

    var keyVaultOneClient = new SecretClient(new Uri(keyVaultOneUrl), new DefaultAzureCredential());
    var keyVaultTwoClient = new SecretClient(new Uri(keyVaultTwoUrl), new DefaultAzureCredential());

    var keyVaultOneSecret = keyVaultOneClient.GetSecretAsync(applicationOptions.SecretName).Result;
    var keyVaultTwoSecret = keyVaultTwoClient.GetSecretAsync(applicationOptions.SecretName).Result;

    serviceCollection.PostConfigureAll<ApplicationOptions>(options =>
    {
        options.KeyVaultOne.TestString = keyVaultOneSecret.Value.Value;
        options.KeyVaultTwo.TestString = keyVaultTwoSecret.Value.Value;
    });

    serviceCollection.AddTransient<IApp, App>();
}