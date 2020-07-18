using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsoleApp.AsyncFromSync
{
    internal static class AsyncFromSyncSamples
    {
        public static int MaximumConcurrencyLevel { get; } = 1;

        public static IReadOnlyList<Task> CreateTaskList(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var taskScheduler = new LimitedConcurrencyLevelTaskScheduler(MaximumConcurrencyLevel);
            return Enumerable
                .Range(1, count)
                .Select(i => StartTask(i, taskScheduler))
                .ToList();
        }

        public static Task StartTask(int index, LimitedConcurrencyLevelTaskScheduler taskScheduler)
        {
            return Task.Factory.StartNew(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                Console.WriteLine($"Index is {index.ToString()}.");
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
        }
    }
}
