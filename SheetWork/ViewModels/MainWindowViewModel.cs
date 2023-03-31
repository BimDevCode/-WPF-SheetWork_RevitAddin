using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Contracts;


namespace SheetWork.ViewModels;

/// <summary>
/// Define main navigation window
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _applicationTitle = String.Empty;

    public MainWindowViewModel(INavigationService navigationService)
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        ApplicationTitle = "SheetWork.Revit";

        _isInitialized = true;
    }
}
