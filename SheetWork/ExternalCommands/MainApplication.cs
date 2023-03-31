using System.Windows;
using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using Wpf.Ui.Contracts;

namespace SheetWork.ExternalCommands;

/// <summary>
/// Start Revit ExternalCommand with app
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class MainApplication : ExternalCommand
{
    public override void Execute()
    {
        try
        {
            var navigationWindow = Host.GetService<INavigationWindow>();
            navigationWindow!.ShowWindow();
            navigationWindow.Navigate(typeof(Views.Pages.SheetTablePage));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Open main Application error");
        }
    }
}

