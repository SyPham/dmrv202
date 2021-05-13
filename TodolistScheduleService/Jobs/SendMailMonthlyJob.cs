using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TodolistScheduleService.Jobs
{
    public class SendMailMonthlyJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync($"SendMailMonthlyJob: Yeu cau server gui mail vao luc: {DateTime.Now.Hour}:{DateTime.Now.Minute}");

        }
    }
}
