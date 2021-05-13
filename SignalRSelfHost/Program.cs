using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignalRSelfHost.Helpers;

namespace SignalRSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            var builder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray());

            var host = builder.Build();

            if (isService)
            {
                // To run the app without the CustomWebHostService change the
                // next line to host.RunAsService();
                host.RunAsCustomService();
            }
            else
            {
                host.Run();
            }

        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
       WebHost.CreateDefaultBuilder(args)
            .UseUrls("http://localhost:5001")
           .ConfigureLogging((hostingContext, logging) =>
           {
               logging.AddEventLog();
           })
            
           .ConfigureAppConfiguration((context, config) =>
           {
               // Configure the app here.
           })
           .UseStartup<Startup>();

       
        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)

        //    .ConfigureLogging(loggerFactory => {
        //        var path = Directory.GetCurrentDirectory();
        //        loggerFactory.AddFile($"{path}\\Logs\\Log.txt");
        //    })
        //    .UseWindowsService()
        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            IConfiguration configuration = hostContext.Configuration;
        //            AppSettings options = configuration.GetSection("AppSettings").Get<AppSettings>();
        //            services.AddSingleton(options);
        //            services.AddSignalR();
        //            services.AddLogging();
        //        })
        //    .ConfigureWebHostDefaults(webBuilder =>
        //    {
        //        webBuilder.UseUrls("http://localhost:5001");
        //    })
        //    .UseWindowsService()
        //    ;
    }
    
}
