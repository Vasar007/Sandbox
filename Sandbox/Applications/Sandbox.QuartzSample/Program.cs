using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.QuartzSample
{
    public static class Program
    {
        private static async Task<int> Main()
        {
            Console.WriteLine("App started.");

            try
            {
                await Task.CompletedTask;
                //Console.ReadLine();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured:{Environment.NewLine}{ex}");
                return -1;
            }
            finally
            {
                Console.WriteLine("App finished.");
            }
        }
    }
}
