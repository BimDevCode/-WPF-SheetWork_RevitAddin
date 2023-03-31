using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SheetWork.Core;
/// <summary>
/// The class contains wrapping methods for working with the Revit API.
/// </summary>
public static class RevitApi
{
    public static UIApplication UiApplication { get; set; }
    public static Autodesk.Revit.ApplicationServices.Application Application => UiApplication.Application;
    public static UIDocument UiDocument => UiApplication.ActiveUIDocument;
    public static Document CurrentDocument => UiApplication.ActiveUIDocument.Document;
    public static View ActiveView => UiApplication.ActiveUIDocument.ActiveView;
}
