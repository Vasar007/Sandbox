using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.TestConsoleApp.AsyncFromSync
{
    public static class ElisingAwaitIsBadIdea
    {
        public static async Task Test()
        {
            Console.WriteLine("App started.");

            try
            {
                //await FooFalseAsync();
                await MainAsync();

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured:{Environment.NewLine}{ex}");
            }
            finally
            {
                Console.WriteLine("App finished.");
            }
        }

        // Issue with unsaving callstack during throwing exception.
        private static async Task FooTrueAsync(bool manualCall = false)
        {
            Console.WriteLine($"Call method '{nameof(FooTrueAsync)}'.");
            await Task.Yield();
            throw new Exception($"Some exception, manual call: {manualCall.ToString()}.");
        }

        private static Task FooFalseAsync()
        {
            Console.WriteLine($"Call method '{nameof(FooFalseAsync)}'.");
            return FooTrueAsync(false);
        }

        // Issue with changing async local variables.
        private static readonly AsyncLocal<int> context = new AsyncLocal<int>() { Value = 42 };

        private static async Task MainAsync()
        {
            context.Value = 1;
            Console.WriteLine("Should be 1: " + context.Value);
            await Async();
            Console.WriteLine("Should be 1: " + context.Value);
        }

        private static async Task Async()
        {
            Console.WriteLine("Should be 1: " + context.Value);
            Sync();
            Console.WriteLine("Should be 2: " + context.Value);
            await Task.Yield();
            Console.WriteLine("Should be 2: " + context.Value);
        }

        private static void Sync()
        {
            Console.WriteLine("Should be 1: " + context.Value);
            context.Value = 2;
            Console.WriteLine("Should be 2: " + context.Value);
        }
    }
}
