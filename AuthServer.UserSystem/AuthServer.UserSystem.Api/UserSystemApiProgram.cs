using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AuthServer.UserSystem.Api
{
    public class UserSystemApiProgram
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
                       config.AddJsonFile(".\\Settings\\app-settings.json", optional: true, reloadOnChange: true)  
                             .AddJsonFile(".\\Settings\\rabbitmq-settings.json", optional: false, reloadOnChange: false)
                             .AddJsonFile(".\\Settings\\mongodb-settings.json", optional: false, reloadOnChange: false);

                       config.AddEnvironmentVariables();

                       if (args != null)
                       {
                           config.AddCommandLine(args);
                       }
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
