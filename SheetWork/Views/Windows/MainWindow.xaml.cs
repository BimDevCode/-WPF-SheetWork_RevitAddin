// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Contracts;
using Wpf.Ui.Controls.Navigation;

namespace SheetWork.Views.Windows;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INavigationWindow
{
    public ViewModels.MainWindowViewModel ViewModel
    {
        get;
    }

    public MainWindow(IServiceProvider serviceProvider, 
        IPageService pageService, 
        INavigationService navigationService)
    {
        Wpf.Ui.Application.Current = this;
        var assembly = Assembly.GetExecutingAssembly();
        Application.ResourceAssembly = assembly;
        ConbentApplication.PushButton.Enabled = false;
        InitializeComponent();

        SetPageService(pageService);

        var snackbarService = Host.GetService<ISnackbarService>()!;
        snackbarService.SetSnackbarControl(RootSnackbar);
        snackbarService.Timeout = 3000;

        ViewModel = (serviceProvider.GetService(typeof(ViewModels.MainWindowViewModel)) as ViewModels.MainWindowViewModel)!;
        DataContext = this;
        navigationService.SetNavigationControl(RootNavigation);
    }

    #region INavigationWindow methods

    public INavigationView GetNavigation()
        => RootNavigation;

    public bool Navigate(Type pageType)
        => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService)
        => RootNavigation.SetPageService(pageService);

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();

    #endregion INavigationWindow methods

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ConbentApplication.PushButton.Enabled = true;

        Wpf.Ui.Application.Current.Close();
    }

    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
}