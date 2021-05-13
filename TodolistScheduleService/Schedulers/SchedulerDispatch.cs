using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;
using TodolistScheduleService.Jobs;

namespace TodolistScheduleService.Schedulers
{

    public class SchedulerDispatch
    {
        IScheduler _scheduler;
        IJobDetail _job;
        ITrigger _trigger;

        public async Task Start(int repeatMinute, int startHourAt , int endHourAt)
        {
            var ct = DateTime.Now.ToLocalTime();
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<ReloadDispatchJob>().Build();
            var st = DateTime.Now.Date.Add(new TimeSpan(startHourAt, 0, 0));
            var end = DateTimeOffset.Now.Date.Add(new TimeSpan(endHourAt, 0, 0));
            Console.WriteLine(st);

            _trigger = TriggerBuilder.Create()
                        .StartAt(st)
                        .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(repeatMinute))
                        .EndAt(end)
                        .Build();
            await _scheduler.ScheduleJob(_job, _trigger);
        }
        public async Task Start(IntervalUnit intervalUnit, DayOfWeek dayofWeek, int hour, int minute)
        {

            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<SendMailJob>().Build();

            _trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                    s.WithInterval(1, intervalUnit)
                    .OnDaysOfTheWeek(dayofWeek)
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, minute))
                  )
                .Build();
            await _scheduler.ScheduleJob(_job, _trigger);
        }

        public async Task<bool> checkScheduleStart()
        {
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            return _scheduler.IsStarted;


        }

        public async Task Stop()
        {
            if (_scheduler.IsStarted)
            {
                await _scheduler.Shutdown();
            }
        }
    }
}
