using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using System.Threading;
namespace DMR_API.SchedulerHelper
{
    public class SchedulerBase<TClass> where TClass : IJob
    {
        IScheduler _scheduler;
        IJobDetail _job;
        ITrigger _trigger;

        public SchedulerBase()
        {
        }
        public async Task Start(int hour, int minute)
        {

            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<TClass>().Build();

            _trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, minute))
                  )

                .Build();
            await _scheduler.ScheduleJob(_job, _trigger);
        }
        public async Task Start(int repeatMinute, TimeSpan startHourAt, TimeSpan endHourAt)
        {
            var ct = DateTime.Now.ToLocalTime();
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<TClass>().Build();
            var st = DateTime.Now.Date.Add(startHourAt);
            var end = DateTimeOffset.Now.Date.Add(endHourAt);
            _trigger = TriggerBuilder.Create()
                        .StartAt(st)
                        .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(repeatMinute))
                        .EndAt(end)
                        .Build();
            await _scheduler.ScheduleJob(_job, _trigger);
        }
        public async Task Start(SimpleScheduleBuilder repeatMinute, TimeSpan startHourAt, TimeSpan endHourAt)
        {
            var ct = DateTime.Now.ToLocalTime();
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<TClass>().Build();
            var st = DateTime.Now.Date.Add(startHourAt);
            var end = DateTimeOffset.Now.Date.Add(endHourAt);
            _trigger = TriggerBuilder.Create()
                        .StartAt(st)
                        .WithSchedule(repeatMinute)
                        .EndAt(end)
                        .Build();
            await _scheduler.ScheduleJob(_job, _trigger);
        }
        public async Task Start(IntervalUnit intervalUnit, DayOfWeek dayofWeek, int hour, int minute)
        {

            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<TClass>().Build();

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
        public async Task Start(IntervalUnit intervalUnit, int hour, int minute)
        {

            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<TClass>().Build();

            _trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                    s.WithInterval(1, intervalUnit)
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, minute))
                  )
                .Build();
            await _scheduler.ScheduleJob(_job, _trigger);
        }
        public async Task Start(int interval = 1, IntervalUnit intervalUnit = IntervalUnit.Hour, int hour = 6, int minute = 0)
        {

            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            _job = JobBuilder.Create<TClass>().Build();

            _trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                    s.WithInterval(interval, intervalUnit)
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
