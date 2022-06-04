using System;
using System.IO;
using MathNet.Numerics;

namespace Sandbox.TestConsoleApp
{
    public static class Program
    {
        private static int Main()
        {
            try
            {
                Console.WriteLine("Console application started.");

                TestDateTime();
                Epplus.EpplusSamples.TestEpplusExcel();
                RollbackEngine.RollbackEngineSamples.TestTaskEngine();
                TestDirectory();
                TestPath();
                TestMath();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred:{Environment.NewLine}{ex.Message}");
                return -1;
            }
            finally
            {
                Console.WriteLine("Console application stopped.");
                Console.WriteLine("Press any key to close this window...");
                Console.ReadKey();
            }
        }

        private static void TestDateTime()
        {
            var dt = DateTime.Now;
            Console.WriteLine($"ToString: {dt.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"ToShortDateString: {dt.ToShortDateString()}");
            Console.WriteLine($"ToShortTimeString: {dt.ToShortTimeString()}");
            Console.WriteLine($"ToLongDateString: {dt.ToLongDateString()}");
            Console.WriteLine($"ToLongTimeString: {dt.ToLongTimeString()}");
        }

        private static void TestDirectory()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string myPath = "TestConsoleApp/logs";

            string finalPath = Path.Combine(folderPath, myPath);
            Console.WriteLine($"DirectoryExists: {Directory.Exists(finalPath).ToString()}");

            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }
        }

        private static void TestPath()
        {
            string path = "C:/Folder/Abc";

            string? finalPath = Path.GetDirectoryName(path);
            Console.WriteLine($"FinalPath: {finalPath}");
        }

        private static void TestMath()
        {
            var observed = new double[] { 1, 2, 3, 4, 5 };
            var modelled = new double[] { 1.1, 2.1, 3.1, 4.1, 5.1 };

            _ = GoodnessOfFit.CoefficientOfDetermination(observed, modelled);
            _ = GoodnessOfFit.RSquared(observed, modelled);
        }
    }
}
