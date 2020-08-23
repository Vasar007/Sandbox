using System;
using System.IO;
using OfficeOpenXml;

namespace Sandbox.TestConsoleApp.Epplus
{
    internal static class EpplusSamples
    {
        public static void TestEpplusExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage())
            {
                var ws = p.Workbook.Worksheets.Add("MySheet");
                ws.Cells["B1"].Value = 0.5;
                ws.Cells[0, 1].Value = 0.5;
                ws.Cells["B2"].Value = 2;
                ws.Cells["A1"].Formula = "SUM(B1, B2)";

                ws.Calculate();
                p.SaveAs(new FileInfo("myworkbook.xlsx"));
            }
        }

        public static void TestEpplusExcel2()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage(new FileInfo("test.xlsx")))
            {
                var ws = p.Workbook.Worksheets["Sheet1-2"];

                const string cellAddress = "H14";
                var cell = ws.Cells[cellAddress];

                Console.WriteLine($"{cellAddress} = {cell.Value.ToString()}");
            }
        }
    }
}
