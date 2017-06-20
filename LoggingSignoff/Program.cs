using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LoggingSignoff
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                //.WriteTo.ColoredConsole()
                .WriteTo.ColoredConsole(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Fatal)
                .CreateLogger();

            BuildWebHost(args).Run();
        }

        public static int Counter { get; set; }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(b =>
                {
                    var providers = b.Services.Where(d => d.ServiceType == typeof(ILoggerProvider)).ToArray();

                    foreach (var provider in providers)
                    {
                        Console.WriteLine($"Found {provider.ImplementationType}");
                        //b.Services.Remove(provider);
                    }

                    b.AddFilter((name, level) =>
                    {
                        if (!name.StartsWith("LoggingSignoff"))
                        {
                            return true;
                        }

                        return level >= LogLevel.Debug;
                    });
                    
                    b.AddProvider(new Serilog.Extensions.Logging.SerilogLoggerProvider());

                    b.AddConsole(options => options.IncludeScopes = Counter++ % 2 == 0);
                    b.Services.Configure<LoggerFilterOptions>(o => { });
                })
                .UseStartup<Startup>()
                .Build();
        }
    }
}
