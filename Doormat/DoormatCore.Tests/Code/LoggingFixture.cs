using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace DoormatCore.Tests.Code
{
    public class LoggingFixture
    {
        public ILogger Logger { get; }

        public LoggingFixture()
        {
            // Set up a service collection
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
                builder.AddConsole()
                       .SetMinimumLevel(LogLevel.Debug));  // Configure as needed

            // Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Get the logger from the service provider
            Logger = serviceProvider.GetRequiredService<ILogger<LoggingFixture>>();
        }
    }
}
