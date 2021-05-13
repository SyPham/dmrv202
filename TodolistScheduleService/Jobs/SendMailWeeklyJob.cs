using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TodolistScheduleService.Jobs
{
    public class SendMailWeeklyJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var doneList = dataMap.GetString("DoneList");
                var cost = dataMap.GetString("Cost");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            await Console.Out.WriteLineAsync($"SendMailWeeklyJob: Yeu cau server gui mail vao luc: {DateTime.Now.Hour}:{DateTime.Now.Minute}");
        }
    }
}
