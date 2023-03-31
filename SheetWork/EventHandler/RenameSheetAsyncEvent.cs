using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SheetWork.Core;
using SheetWork.Models;
using SheetWork.Models.Contract;
using Nice3point.Revit.Toolkit.External.Handlers;

namespace SheetWork.EventHandler;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class RenameSheetAsyncEvent 
{
    private readonly AsyncEventHandler<SheetModel> _event;

    private string _suffix = String.Empty;
    private string _prefix = String.Empty;
    private string _number;

    public IRevitModel SheetModel;

    public RenameSheetAsyncEvent(IRevitModel sheetModel = null)
    {
        _event = new AsyncEventHandler<SheetModel>();
        SheetModel = sheetModel;
    }

    public async Task<SheetModel> RaiseAsync(string prefix, string suffix, string number)
    {
        _suffix = suffix;
        _prefix = prefix;
        _number = number;
        return SheetModel is not null ? await _event.RaiseAsync(Rename) : null;
    }

    private SheetModel Rename(UIApplication application)
    {
        var sheetToRenameId = new ElementId(SheetModel.ElementId);
        using var transaction = new Transaction(RevitApi.CurrentDocument);
        transaction.Start("Axiom Sheet Rename");

        if (RevitApi.CurrentDocument.GetElement(sheetToRenameId) is not ViewSheet sheetToRename)
            throw new Exception("Can not find this sheet in document");

        sheetToRename.SheetNumber = _number;
        _prefix = _prefix.Length == 0 ? " " : _prefix;
        _suffix = _suffix.Length == 0 ? " " : _suffix;
        var newSheetName= _prefix + sheetToRename.Name + _suffix;
        sheetToRename.Name = newSheetName;//for changing on panel sheetToRename.Name must changed
        RevitApi.CurrentDocument.Regenerate();

        transaction.Commit();

        return new SheetModel()
        {
            Name = sheetToRename.Title,
            Number = _number,
            ElementId = sheetToRename.Id.IntegerValue,
            NumberPlacedViews = sheetToRename.GetAllPlacedViews().Count

        };
    }
}
