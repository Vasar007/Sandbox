using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace TestConsoleApp
{
    public static class Program
    {
        private static async Task Main()
        {
            Console.WriteLine("App started.");

            try
            {
                //TestDateTime();
                //Epplus.EpplusSamples.TestEpplusExcel();
                //RollbackEngine.RollbackEngineSamples.TestTaskEngine();
                //TestDirectory();
                //TestPath();
                TestMath();

                //Console.ReadLine();
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

            var a = GoodnessOfFit.CoefficientOfDetermination(observed, modelled);
            var b = GoodnessOfFit.RSquared(observed, modelled);
        }
    }
}
