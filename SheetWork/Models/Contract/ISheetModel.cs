
namespace SheetWork.Models.Contract;
/// <summary>
/// Describe main sheets property
/// </summary>
public interface ISheetModel
{
    string Name { get; set; }
    string Number { get; set; }
}