﻿using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Office.Interop.Excel;

namespace Sandbox.TestConsoleApp.InteropExcel
{
    [SupportedOSPlatform("windows")]
    internal sealed class InteropExcelSamples
    {
        public static void WrapRawExcelCall()
        {
            TestRawExcel();
            GC.Collect();
        }

        public static void TestRawExcel()
        {
            var excelApp = new Application();
            double x = excelApp.WorksheetFunction.ChiSq_Inv_RT(0.5, 2);
            Console.WriteLine($"Sum = {x.ToString()}");

            ReleaseComObject(excelApp.WorksheetFunction);
            excelApp.Quit();
            ReleaseComObject(excelApp);
        }

        public static void ReleaseComObject(object obj)
        {
            try
            {
                Marshal.ReleaseComObject(obj);
                Marshal.FinalReleaseComObject(obj);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to release the COM object: {ex}");
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
