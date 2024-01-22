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
public class DeleteSheetAsyncEvent 
{
    private readonly AsyncEventHandler<SheetModel> _event;

    public IRevitModel SheetModel;

    public DeleteSheetAsyncEvent( IRevitModel sheetModel = null)
    {
        _event = new AsyncEventHandler<SheetModel>();
        SheetModel = sheetModel;
    }

    public async Task<SheetModel> RaiseAsync()
    {
        return SheetModel is not null ? await _event.RaiseAsync(Delete) : null;
    }

    private SheetModel Delete(UIApplication application)
    {
        var sheetToDeleteId = new ElementId(SheetModel.ElementId);
        using var transaction = new Transaction(RevitApi.CurrentDocument);
        transaction.Start("Axiom Sheet Delete");

        RevitApi.CurrentDocument.Delete(sheetToDeleteId);

        transaction.Commit();

        return (SheetModel)SheetModel;
    }
}
