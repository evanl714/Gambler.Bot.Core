using Gambler.Bot.Core.Sites;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.Core.Tests.Code
{
    public class baseSiteFixture
    {
        public BaseSite site { get; set; }
        protected readonly ILogger logger;

        public baseSiteFixture()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
                builder.AddConsole()
                       .SetMinimumLevel(LogLevel.Debug));  // Configure as needed

            // Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Get the logger from the service provider
            logger = serviceProvider.GetRequiredService<ILogger<baseSiteFixture>>();
        }
    }
}
