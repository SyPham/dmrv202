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
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        Scheduler _scheduler;
        Scheduler _schedulerWeekly;
        Scheduler _schedulerMonthly;
        private readonly HubConnection _connection;
        List<MailingDto> _mailingDtos = null;
        public Worker(ILogger<Worker> logger, HubConnection connection)
        {
            _logger = logger;
            _connection = connection;

            _scheduler = new Scheduler();
            _schedulerWeekly = new Scheduler();
            _schedulerMonthly = new Scheduler();

        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection.Closed += async (error) =>
            {
                while (true)
                {
                    try
                    {
                        await _connection.StartAsync();

                        await Console.Out.WriteLineAsync($"Hub: {_connection.State}");
                        break;
                    }
                    catch
                    {
                        await Task.Delay(1000);
                    }
                }
            };
            // Ket noi thanh cong den hub

            // Hoi thoi gian gui mail

            // Lang nghe tg gui mail khoi tao schedule
            _connection.On<List<MailingDto>>("ReceiveMailing", async (data) =>
            {
                _mailingDtos = data;
                if (_mailingDtos != null)
                {
                    var group = _mailingDtos.GroupBy(x => new { x.Report, x.Frequency }).ToList();

                    
                    foreach (var item in group)
                    {
                        var groupyByFrequency = item.GroupBy(x => x.Frequency).ToList();
                        var mailList = item.SelectMany(x => x.UserList.Select(x => x.Email)).ToList();
                        var time = item.First().TimeSend;
                        foreach (var frequencycItem in groupyByFrequency)
                        {
                            if (frequencycItem.Key == DMR_API.Constants.FrequencyOption.Daily)
                            {
                                await _scheduler.Start(time.Hour, time.Minute);
                                Console.WriteLine($"The report will send at {time.Hour}:{time.Minute}");
                            } else if (frequencycItem.Key == DMR_API.Constants.FrequencyOption.Weekly)
                            {

                            }
                            else if (frequencycItem.Key == DMR_API.Constants.FrequencyOption.Monthly)
                            {

                            }
                        }
                  

                    }
                }
                Console.WriteLine(data);
            });
            // Loop is here to wait until the server is running
            while (true)
            {

                try
                {
                    await _connection.StartAsync();
                    await _connection.InvokeAsync("Mailing");
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
            await Console.Out.WriteLineAsync($"Hub: {_connection.State}");

            //_scheduler = new Scheduler();
            //// Thuc thi luc 8:50
            //await _scheduler.Start(17, 30);
            
        }
    }
}
