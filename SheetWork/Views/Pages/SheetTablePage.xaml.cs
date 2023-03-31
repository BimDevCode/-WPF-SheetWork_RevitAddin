// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls.Navigation;

namespace SheetWork.Views.Pages;

/// <summary>
/// Interaction logic for SheetTablePage.xaml
/// </summary>
public partial class SheetTablePage : INavigableView<ViewModels.SheetTableViewModel>
{
    public ViewModels.SheetTableViewModel ViewModel
    {
        get;
    }

    public SheetTablePage(ViewModels.SheetTableViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}
