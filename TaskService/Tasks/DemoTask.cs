﻿using DistributedLock;
using Repository.Database;
using TaskService.Libraries.QueueTask;
using TaskService.Libraries.ScheduleTask;

namespace TaskService.Tasks
{
    public class DemoTask(IServiceProvider serviceProvider, ILogger<DemoTask> logger) : BackgroundService
    {
        private readonly ILogger logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ScheduleTaskBuilder.Builder(this);
            QueueTaskBuilder.Builder(this);

            await Task.Delay(-1, stoppingToken);
        }



        [ScheduleTask(Cron = "0/1 * * * * ?")]
        public void ShowTime()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var distLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();

                Console.WriteLine(DateTime.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DemoTask.WriteHello");
            }
        }


        [QueueTask(Name = "ShowName", Semaphore = 1)]
        public void ShowName(string name)
        {
            Console.WriteLine(DateTime.Now + "姓名：" + name);
        }


    }
}
