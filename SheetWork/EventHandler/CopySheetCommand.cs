using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SheetWork.Core;
using SheetWork.Helpers;
using SheetWork.Models;
using SheetWork.Models.Contract;
using Nice3point.Revit.Toolkit.External.Handlers;
using Nice3point.Revit.Toolkit.Options;
using System.Text.RegularExpressions;
using static Autodesk.Revit.DB.SpecTypeId;

namespace SheetWork.EventHandler;

/// <summary>
/// Copy sheet with detailed view
/// (create new views, schedules and details)
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class CopySheetAsyncEvent 
{
    #region Fields

    private readonly AsyncEventHandler<SheetModel> _event;

    private List<string> _sheetsNumber = new();

    private const ViewDuplicateOption DuplicateOption = ViewDuplicateOption.WithDetailing;

    public IRevitModel SheetModel;

    #endregion

    public CopySheetAsyncEvent( IRevitModel sheetModel = null)
    {
        _event = new AsyncEventHandler<SheetModel>();
        SheetModel = sheetModel;
    }

    #region Methods

    public async Task<SheetModel> RaiseAsync(IList<string> sheetsNumber)
    {
        _sheetsNumber = (List<string>)sheetsNumber;
        return SheetModel is not null ? await _event.RaiseAsync(Copy) : null;
    }

    /// <summary>
    /// Main method copy
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private SheetModel Copy(UIApplication application)
    {
        var sheetToCopyId = new ElementId(SheetModel.ElementId);
        using var transaction = new Transaction(RevitApi.CurrentDocument);
        transaction.Start("Axiom Sheet Copy");

        if (RevitApi.CurrentDocument.GetElement(sheetToCopyId) is not ViewSheet sheetToCopy)
            throw new Exception("Can not find sheet in document");

        var title = RevitApi.CurrentDocument.GetElements(sheetToCopyId).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_TitleBlocks).Cast<FamilyInstance>().First(q => q.OwnerViewId == sheetToCopy.Id);

        var copiedSheet = ViewSheet.Create(RevitApi.CurrentDocument, title.GetTypeId());

        var regex = new Regex(@"\d+"); // matches one or more digits
        MatchEvaluator evaluator = Utils.AddOneToMatch;

        var sheetOnlyNum = regex.Replace(sheetToCopy.SheetNumber, evaluator);
        if (!Regex.IsMatch(sheetOnlyNum, @"^[a-zA-Z]+$"))
        {
            while (_sheetsNumber.Contains(sheetOnlyNum))
            {
                sheetOnlyNum = regex.Replace(sheetOnlyNum, evaluator);
            }
        }
        

        copiedSheet.SheetNumber = sheetOnlyNum;
        copiedSheet.Name = sheetToCopy.Name;

        var excludeViewIds = RevitApi.CurrentDocument.GetElements(sheetToCopyId)
            .WherePasses(new ElementMulticlassFilter(new List<Type>
            {
                typeof(View),
                typeof(Viewport)
            }))
            .ToElementIds();
        var excludeTitleBlockIds = RevitApi.CurrentDocument.GetElements(sheetToCopyId)
            .WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks))
            .ToElementIds();
        var excludeElementIds = (excludeViewIds.Concat(excludeTitleBlockIds)).ToList();
        var elementsToCopy = excludeElementIds.Count == 0 ?
            RevitApi.CurrentDocument.GetElements(sheetToCopyId).ToElementIds() :
            RevitApi.CurrentDocument.GetElements(sheetToCopyId).Excluding(excludeElementIds).ToElementIds();

        var elementsToCopyWithoutTitle = elementsToCopy
            .Where(x => !x.AreEquals(BuiltInCategory.OST_TitleBlocks))
            .ToList();

        var options = new CopyPasteOptions();
        options.SetDuplicateTypeNamesHandler(new DuplicateTypeNamesHandler(DuplicateTypeAction.UseDestinationTypes));
        ElementTransformUtils.CopyElements(sheetToCopy,
            elementsToCopyWithoutTitle, copiedSheet, null, options);

        var idsAlreadyCopied = elementsToCopy.Select(x => x.IntegerValue).ToList();

        CopyViews(sheetToCopy, copiedSheet, idsAlreadyCopied);
        CopySchedule(sheetToCopy, copiedSheet, idsAlreadyCopied);

        transaction.Commit();

        return new SheetModel()
        {
            Name = copiedSheet.Title,
            Number = sheetOnlyNum,
            ElementId = copiedSheet.Id.IntegerValue,
            NumberPlacedViews = copiedSheet.GetAllPlacedViews().Count
        };
    }

    /// <summary>
    /// Copy all views
    /// </summary>
    /// <param name="sheetToCopy"></param>
    /// <param name="copiedSheet"></param>
    /// <param name="elementsAlreadyCopied"></param>
    /// <exception cref="Exception"></exception>
    private static void CopyViews(ViewSheet sheetToCopy, ViewSheet copiedSheet, ICollection<int> elementsAlreadyCopied)
    {
        // all views but schedules
        foreach (var placedView in sheetToCopy.GetAllPlacedViews())
        {
            if (elementsAlreadyCopied.Contains(placedView.IntegerValue)) continue;
            if (RevitApi.CurrentDocument.GetElement(placedView) is not View view)
                throw new Exception("view on this sheet is not exist");

            // legends and all non-legend and non-schedule views
            var copiedView = view.ViewType == ViewType.Legend ? view : RevitApi.CurrentDocument.GetElement(
                view.Duplicate(DuplicateOption)) as View;
            if (copiedView is null) throw new Exception("can not duplicate view on this sheet");
            copiedView.Name = view.Name + " " + copiedSheet.SheetNumber;

            // Viewports
            foreach (var element in new FilteredElementCollector(RevitApi.CurrentDocument).OfClass(typeof(Viewport)))
            {
                var viewport = (Viewport)element;

                if (viewport.SheetId != sheetToCopy.Id || viewport.ViewId != view.Id) continue;
                var boundingBox = viewport.get_BoundingBox(sheetToCopy);
                var initialCenter = (boundingBox.Max + boundingBox.Min) / 2;

                var createdViewport = Viewport.Create(RevitApi.CurrentDocument, copiedSheet.Id, copiedView.Id, XYZ.Zero);

                var boundingBoxCreatedViewport = createdViewport.get_BoundingBox(copiedSheet);
                var newCenter = (boundingBoxCreatedViewport.Max +
                                 boundingBoxCreatedViewport.Min) / 2;

                ElementTransformUtils.MoveElement(RevitApi.CurrentDocument, createdViewport.Id, new XYZ(
                    initialCenter.X - newCenter.X,
                    initialCenter.Y - newCenter.Y,
                    0));
            }
        }
    }

    /// <summary>
    /// Copy all Schedules
    /// </summary>
    /// <param name="sheetToCopy"></param>
    /// <param name="copiedSheet"></param>
    /// <param name="elementsAlreadyCopied"></param>
    private static void CopySchedule(ViewSheet sheetToCopy, ViewSheet copiedSheet, ICollection<int> elementsAlreadyCopied)
    {
        // schedules
        foreach (var schedule in new FilteredElementCollector(RevitApi.CurrentDocument)
                     .OfClass(typeof(ScheduleSheetInstance))
                     .Cast<ScheduleSheetInstance>())
        {
            if (elementsAlreadyCopied.Contains(schedule.Id.IntegerValue)
                || schedule.OwnerViewId != sheetToCopy.Id
                || schedule.IsTitleblockRevisionSchedule) continue;

            foreach (var viewSchedule in new FilteredElementCollector(RevitApi.CurrentDocument)
                         .OfClass(typeof(ViewSchedule))
                         .Cast<ViewSchedule>())
            {
                if (schedule.ScheduleId != viewSchedule.Id) continue;
                var boundingBox = schedule.get_BoundingBox(sheetToCopy);
                var initialCenter = (boundingBox.Max + boundingBox.Min) / 2;

                var createdScheduleInstance = ScheduleSheetInstance.Create(RevitApi.CurrentDocument, copiedSheet.Id, viewSchedule.Id, XYZ.Zero);

                var boundingBoxCreatedScheduleInstance = createdScheduleInstance.get_BoundingBox(copiedSheet);
                var newCenter = (boundingBoxCreatedScheduleInstance.Max
                                 + boundingBoxCreatedScheduleInstance.Min) / 2;

                ElementTransformUtils.MoveElement(RevitApi.CurrentDocument, createdScheduleInstance.Id, new XYZ(
                    initialCenter.X - newCenter.X,
                    initialCenter.Y - newCenter.Y,
                    0));
            }
        }
    }

    #endregion
}
