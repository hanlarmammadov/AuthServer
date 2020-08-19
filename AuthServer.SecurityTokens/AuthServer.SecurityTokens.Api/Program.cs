using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace AuthServer.SecurityTokens.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildCustomHost(args).Run();
        }

        public static IWebHost BuildCustomHost(string[] args)
        {
            WebHostBuilder builder = new WebHostBuilder();

            builder.UseKestrel()
                   .UseStartup<Startup>()
                   .UseContentRoot(Directory.GetCurrentDirectory())
                   .ConfigureAppConfiguration((hostingContext, config) =>
                   {
                       var env = hostingContext.HostingEnvironment;
                       config.AddJsonFile(".\\Settings\\jwt-settings.json", optional: false, reloadOnChange: false)
                             .AddJsonFile(".\\Settings\\redis-settings.json", optional: false, reloadOnChange: false)
                             .AddJsonFile(".\\Settings\\rabbitmq-settings.json", optional: false, reloadOnChange: false)
                             .AddJsonFile(".\\Settings\\mongodb-settings.json", optional: false, reloadOnChange: false);

                       config.AddEnvironmentVariables();

                       if (args != null)
                       {
                           config.AddCommandLine(args);
                       }
                   })
                   .ConfigureLogging((hostingContext, logging) =>
                   {
                       logging.SetMinimumLevel(LogLevel.Information);
                       logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                       logging.AddConsole((ConsoleLoggerOptions opt) =>
                       {
                           opt.IncludeScopes = true;
                       });
                       logging.AddDebug();
                   })
                   .UseIISIntegration()
                   .UseDefaultServiceProvider((context, options) =>
                   {
                       options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                   });

            return builder.Build();
        }
    }
}
