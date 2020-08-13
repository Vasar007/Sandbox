using System;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarking
{
    internal static class Program
    {
        private static int Main()
        {
            Console.WriteLine("App started.");

            try
            {
                RunBenchmarks();
                //EnsureBenchmarkIsOk();

                //Console.ReadLine();

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

        private static Summary RunBenchmarks()
        {
            return BenchmarkRunner.Run<BenchmarkingClass>();
        }

        private static void EnsureBenchmarkIsOk()
        {
            var obj = new BenchmarkingClass();

            obj.GlobalSetup();
            obj.SetReadOnlyPropertyFullyReflection();
            obj.SetReadOnlyPropertyReflectionWithIL();
        }
    }
}
