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
using Quartz;
using TodolistScheduleService.Jobs;
using TodolistScheduleService.Schedulers;

namespace TodolistScheduleService.Services
{
    public class ReloadTodo : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        SchedulerBase<ReloadTodoJob> _scheduler;
        SchedulerBase<ReloadDispatchJob> _schedulerDispatchJob;
        public ReloadTodo(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _scheduler = new SchedulerBase<ReloadTodoJob>();
            // Thuc thi luc 12:50

            await _scheduler.Start(IntervalUnit.Hour, 6, 00);
            _schedulerDispatchJob = new SchedulerBase<ReloadDispatchJob>();
            // Thuc thi luc 8:50
            var startAt = TimeSpan.FromHours(6);
            var endAt = TimeSpan.FromHours(23);
            var repeatMins = 30;
            await _schedulerDispatchJob.Start(repeatMins, startAt, endAt);

            //_schedulerSendMailJob  = new SchedulerBase<SendMailJob>();
            //await _schedulerSendMailJob.Start(17, 30);
            Console.WriteLine($"Client ID: Start ReloadTodo#############################################################");

        }
    }
}
