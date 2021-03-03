using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace Sandbox.QuartzSample
{
    public static class Program
    {
        private static async Task<int> Main()
        {
            Console.WriteLine("App started.");

            try
            {
                await QuartzStartSheduller();
                await QuartzCreateJobWithTriggerAndSheduleJob();
                Console.ReadLine();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred:{Environment.NewLine}{ex}");
                return -1;
            }
            finally
            {
                Console.WriteLine("App finished.");
            }
        }

        private static async Task QuartzStartSheduller()
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            // Grab the Scheduler instance from the Factory
            var factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            // and start it off
            await scheduler.Start();

            // some sleep to show what's happening
            await Task.Delay(TimeSpan.FromSeconds(60));

            // and last shut down the scheduler when you are ready to close your program
            await scheduler.Shutdown();
        }

        private static async Task QuartzCreateJobWithTriggerAndSheduleJob()
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            // Grab the Scheduler instance from the Factory
            var factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            // and start it off
            await scheduler.Start();

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<HelloJob>()
                .WithIdentity("job1", "group1")
                .Build();

            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .RepeatForever())
                .Build();

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger);

            // some sleep to show what's happening
            await Task.Delay(TimeSpan.FromSeconds(60));

            // and last shut down the scheduler when you are ready to close your program
            await scheduler.Shutdown();
        }
    }

    // simple log provider to get something to the console
    public class ConsoleLogProvider : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= LogLevel.Info && func != null)
                {
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] [{level}] {func()}", parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }
    }

    public class HelloJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync("Greetings from HelloJob!");
        }
    }
}
