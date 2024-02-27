using Autodesk.Revit.UI;
using SheetWork.Assemblies;
using SheetWork.Core;
using SheetWork.ExternalCommands;
using Nice3point.Revit.Toolkit.External;

namespace SheetWork;

/// <summary>
/// Add plugin to ribbon panel
/// </summary>
[UsedImplicitly]
public class ConbentApplication : ExternalApplication
{
    public static PushButton PushButton;
    public override void OnStartup()
    {
        FunctionsAssemblyResolver.RedirectAssembly();
        CreateRibbon();
        Host.StartHost();
    }

    private void CreateRibbon()
    {
        RevitApi.UiApplication = UiApplication;

        var panel = Application.CreatePanel("Exporter", "SheetWork");

        var showButton = panel.AddPushButton<MainApplication>("Export Sheet");
        showButton.SetImage("/SheetWork;component/Resources/Icons/CONBENT.png");
        showButton.SetLargeImage("/SheetWork;component/Resources/Icons/CONBENT.png");
        PushButton = showButton;
    }

    public override async void OnShutdown()
    {
        await Host.StopHost();
    }
}
