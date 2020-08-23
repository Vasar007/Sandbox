using System;
using System.Threading.Tasks;

namespace Sandbox.DataflowPipeline
{
    internal static class Utils
    {
        public static async Task<string> FindMostCommonAsync(string input)
        {
            if (Options.Output)
            {
                Console.WriteLine("FindMostCommonAsync");
            }

            await Task.Delay(Options.Delay);

            if (Options.ShouldThrowException)
            {
                ThrowException();
            }

            return WordFinder.FindMostCommonWord(input);
        }

        public static string FindMostCommon(string input)
        {
            if (Options.Output)
            {
                Console.WriteLine("FindMostCommon");
            }

            if (Options.ShouldThrowException)
            {
                ThrowException();
            }

            return WordFinder.FindMostCommonWord(input);
        }

        public static int CountChars(string input)
        {
            if (Options.Output)
            {
                Console.WriteLine("CountChars");
            }

            if (Options.ShouldThrowException)
            {
                ThrowException();
            }

            return input.Length;
        }

        public static bool IsOdd(int number)
        {
            if (Options.Output)
            {
                Console.WriteLine("IsOdd");
            }

            if (Options.ShouldThrowException)
            {
                ThrowException();
            }

            return number % 2 == 1;
        }

        public static void ThrowException()
        {
            Console.WriteLine("Exception was thrown.");
            throw new Exception("It is a critical exception.");
        }
    }
}
