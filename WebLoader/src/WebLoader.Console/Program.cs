using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebLoader.ConsoleApp
{
    internal class Program
    {
        public IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, args);

            // Application application = new Application(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var app = serviceProvider.GetService<Application>();

            //// For async
            //Task.Run(() => app.Run()).Wait(); // Exceptions thrown here will be lost! Catch them all at Run()
            //                                  // Otherwise use sync as in: app.Run();  

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, string[] args)
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            services.AddSingleton(loggerFactory); // Add first my already configured instance
            services.AddLogging(); // Allow ILogger<T>

            var configuration = GetConfiguration(args);
            services.AddSingleton(configuration);

            // Support typed Options
            services.AddOptions();
            services.Configure<WebSettings>(configuration.GetSection("WebSettings"));

            services.AddTransient<Application>();
        }

        private static IConfigurationRoot GetConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"WebSettings.json", true).AddEnvironmentVariables()
                .Build();
        }
    }
}