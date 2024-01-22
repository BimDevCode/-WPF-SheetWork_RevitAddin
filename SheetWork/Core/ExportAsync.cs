using System.Data;
using SheetWork.Models;
using SheetWork.Models.Contract;
using Microsoft.Office.Interop.Excel;
using System.IO;

namespace SheetWork.Core;
/// <summary>
/// Export all data from table to Excel
/// </summary>
[UsedImplicitly]
public class ExportAsync
{
    public async Task RaiseAsync(IExcelExportData[] data)
    {
        await Task.Run(() =>
        {
            var projLocation = RevitApi.CurrentDocument.PathName;
            
            var folderPath = Path.GetDirectoryName(projLocation);
            var path = Path.Combine(folderPath!, $"{RevitApi.CurrentDocument.Title}"
                                                 +$"{DateTime.Now:_MM-dd-yyyy_HH-mm}"
                                                 +".xlsx");

            Application excelApp = new Application();
            // Create a new workbook
            Workbook workbook = excelApp.Workbooks.Add();
            // Get the active worksheet
            Worksheet worksheet = (Worksheet)workbook.ActiveSheet;
            // Write data to the worksheet

            worksheet.Cells[1, 1] = "Name";
            worksheet.Cells[1, 2] = "Number";
            worksheet.Cells[1, 3] = "NumberPlacedViews";

            for (var i = 0; i < data.Length; i++)
            {
                var excelExportData = (SheetModel)data[i];

                worksheet.Cells[i + 2, 1] = excelExportData.Name;
                worksheet.Cells[i + 2, 2] = excelExportData.Number;
                worksheet.Cells[i + 2, 3] = excelExportData.NumberPlacedViews;
            }

            // Save the workbook
            workbook.SaveAs(path);

            // Close the workbook and Excel application
            workbook.Close();
            excelApp.Quit();
        });
    }
}
