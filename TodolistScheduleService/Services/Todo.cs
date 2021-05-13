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
    public class Todo : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        SchedulerMultiple _scheduler;
        private readonly HubConnection _connection;
        List<MailingDto> _mailingDtos = null;
        public Todo(ILogger<Worker> logger, HubConnection connection)
        {
            _logger = logger;
            _connection = connection;

        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _scheduler = new SchedulerMultiple();
            await _scheduler.StartAllJob();
            Console.WriteLine("StartAllJob");
            _connection.Closed += async (error) =>
            {
                Console.WriteLine("Dut ket noi: Danh sach mail con lai la: " + _mailingDtos.Count);

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
            _connection.Reconnected += async (error) =>
            {
                Console.WriteLine("Ket Noi lai: Danh sach mail con lai la: " + _mailingDtos.Count);
                await ScheduleJob();
            };
            // Ket noi thanh cong den hub

            // Hoi thoi gian gui mail

            // Lang nghe tg gui mail khoi tao schedule 
            _connection.On<List<MailingDto>>("ReceiveMailing", (data) =>
           {
               _mailingDtos = data;
               Console.WriteLine("Len lich gui mail: " + _mailingDtos.Count);
           });
            _connection.On<List<MailingDto>>("RescheduleJob", async (data) =>
            {
                _mailingDtos = data;
                Console.WriteLine("Cap nhat lai lich gui mail: " + _mailingDtos.Count);
                await RescheduleAllJob();
            });
            // Loop is here to wait until the server is running
            while (true)
            {

                try
                {
                    await _connection.StartAsync();
                    if (_mailingDtos == null)
                    {
                        await _connection.InvokeAsync("Mailing");
                        Console.WriteLine("Chua co danh sach gui mail. Hoi danh sach gui mail.");
                    }
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
            await Console.Out.WriteLineAsync($"Hub: {_connection.State}");

            if (_mailingDtos != null)
            {
                await ScheduleJob();
            }

        }

        async Task ScheduleJob()
        {
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
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                maps.Add(jobMap.Report, jobMap.Report);
                            }
                            await _scheduler.StartDaily(time.Hour, time.Minute, maps);
                            Console.WriteLine($"StartDaily: The report will send at {time.Hour}:{time.Minute}");
                        }
                        else if (frequencycItem.Key == DMR_API.Constants.FrequencyOption.Weekly)
                        {
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                maps.Add(jobMap.Report, jobMap.Report);
                            }
                          
                            await _scheduler.StartWeekly(time.DayOfWeek, time.Hour, time.Minute, maps);
                        }
                        else if (frequencycItem.Key == DMR_API.Constants.FrequencyOption.Monthly)
                        {
                            await _scheduler.StartMonthly(time.Hour, time.Minute);
                        }
                    }
                }
                Console.WriteLine(_mailingDtos.Count);
            }
        }
        async Task RescheduleAllJob()
        {
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
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                maps.Add(jobMap.Report, jobMap.Report);
                            }
                            await _scheduler.UpdateDailyTrigger(time.Hour, time.Minute, maps);
                            Console.WriteLine($"Cap nhat lai daily: {time.Hour}:{time.Minute}");
                        }
                        else if (frequencycItem.Key == DMR_API.Constants.FrequencyOption.Weekly)
                        {
                            IDictionary<string, object> maps = new Dictionary<string, object>();
                            foreach (var jobMap in frequencycItem.ToList())
                            {
                                maps.Add(jobMap.Report, jobMap.Report);
                            }
                            await _scheduler.UpdateWeeklyTrigger(time.DayOfWeek, time.Hour, time.Minute, maps);
                            Console.WriteLine($"Cap nhat lai weekly: {time.Hour}:{time.Minute}");
                        }
                        else if (frequencycItem.Key == DMR_API.Constants.FrequencyOption.Monthly)
                        {
                            await _scheduler.UpdateMonthlyTrigger(time.Hour, time.Minute);
                            Console.WriteLine($"Cap nhat lai Monthly: {time.Hour}:{time.Minute}");

                        }
                    }
                }
            }
        }
    }
}
