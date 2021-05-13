using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DMR_API.DTO;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TodolistScheduleService.Schedulers;

namespace TodolistScheduleService.Services
{
    public class ReloadDispatch : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        SchedulerDispatch _scheduler;
        public ReloadDispatch(ILogger<Worker> logger)
        {
            _logger = logger;
        }
      
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_scheduler = new SchedulerDispatch();
            //// Thuc thi luc 8:50
            //await _scheduler.Start(1, 6, 23);
            Console.WriteLine($"Client ID: Start ReloadDispatch#############################################################");
            return Task.CompletedTask;
        }
    }
}
