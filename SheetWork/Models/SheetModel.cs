using SheetWork.Models.Contract;

namespace SheetWork.Models;

public class SheetModel : ISheetModel, IRevitModel, IExcelExportData
{
    public string Name { get; set; } = string.Empty;
    public int NumberPlacedViews { get; set; } = 0;
    public string Number { get; set; } = string.Empty;
    public int ElementId { get; set; } = 0;
}
