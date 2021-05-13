using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace SignalRSelfHost.Helpers
{
    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var webHostService = new CustomWebHostService.CustomWebHostService(host);
            ServiceBase.Run(webHostService);
        }
    }
}
