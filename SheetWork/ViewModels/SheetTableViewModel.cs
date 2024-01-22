// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Autodesk.Revit.DB;
using SheetWork.Core;
using SheetWork.EventHandler;
using SheetWork.Helpers;
using SheetWork.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Wpf.Ui.Common;
using Wpf.Ui.Contracts;
using Wpf.Ui.Controls;

namespace SheetWork.ViewModels;
/// <summary>
/// Define all logic related to SheetTable
/// </summary>
public partial class SheetTableViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty]
    private ObservableCollection<SheetModel> _projectSheets = new();

    [ObservableProperty]
    private string _prefix = "_";

    [ObservableProperty]
    private string _suffix = "_";

    [ObservableProperty]
    private string _number = String.Empty;

    private readonly CopySheetAsyncEvent _copySheetEvent;
    private readonly DeleteSheetAsyncEvent _deleteSheetEvent;
    private readonly RenameSheetAsyncEvent _renameSheetEvent;

    private readonly ExportAsync _export;
    private readonly ISnackbarService _snackbarService;

    #endregion

    #region Commands

    /// <summary>
    /// Export to xlsx
    /// </summary>
    [RelayCommand]
    private async void OnExportToExcel()
    {
        try
        {
            await _export.RaiseAsync(ProjectSheets.ToArray());
            await _snackbarService.ShowAsync("Success",
                "Data exported to Excel file", SymbolRegular.ErrorCircle24, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            await _snackbarService.ShowAsync("Unexpected error",
                ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    /// <summary>
    /// Create copy <see cref="ViewSheet"/> in project and table
    /// </summary>
    /// <param name="selectedSheetModel"></param>
    [RelayCommand]
    private async void OnCopySelectedSheet(SheetModel selectedSheetModel)
    {
        try
        {
            _copySheetEvent.SheetModel = selectedSheetModel;

            var copiedSheet = await _copySheetEvent.RaiseAsync(ProjectSheets.Select(x => x.Number).ToList());
            var sheetIndex = ProjectSheets.IndexOf(selectedSheetModel);
            ProjectSheets?.Insert(sheetIndex + 1, copiedSheet);
            await _snackbarService.ShowAsync("Success Copy",
                "Created new sheet", SymbolRegular.ErrorCircle24, ControlAppearance.Success);
        }
        catch (Autodesk.Revit.Exceptions.InvalidOperationException invalidException)
            when (invalidException.Message == "View cannot be duplicated")
        {
            await _snackbarService.ShowAsync("Can not Copy this view",
                invalidException.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
        catch (Exception ex)
        {
            await _snackbarService.ShowAsync("Unexpected error",
                ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    /// <summary>
    /// Delete <see cref="ViewSheet"/> from project and table
    /// </summary>
    /// <param name="selectedSheetModel"></param>
    [RelayCommand]
    private async void OnDeleteSelectedSheet(SheetModel selectedSheetModel)
    {
        try
        {
            _deleteSheetEvent.SheetModel = selectedSheetModel;
            var deletedSheet = await _deleteSheetEvent.RaiseAsync();
            ProjectSheets?.Remove(deletedSheet);
            await _snackbarService.ShowAsync("Success Delete",
                "Delete sheet from project", SymbolRegular.ErrorCircle24, ControlAppearance.Success);
        }
        catch (Exception exception) when (exception.Message.Contains("ElementId cannot be deleted"))
        {
            await _snackbarService.ShowAsync("Can not delete this sheet",
                "ElementId cannot be deleted", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
        catch (Exception exception) when (exception.Message.Contains("not found in this"))
        {
            ProjectSheets?.Remove(selectedSheetModel);
            await _snackbarService.ShowAsync("Sheet in project is not exist",
                "This sheet will be delete from the table", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
        catch (Exception ex)
        {
            await _snackbarService.ShowAsync("Unexpected error",
                ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    /// <summary>
    /// Rename <see cref="ViewSheet"/> in project and table
    /// </summary>
    /// <param name="selectedSheetModel"></param>
    [RelayCommand]
    private async void OnRenameSelectedSheet(SheetModel selectedSheetModel)
    {
        try
        {
            _renameSheetEvent.SheetModel = selectedSheetModel;
            if (ProjectSheets.Select(x => x.Number).ToList().Contains(Number))
                throw new Exception("Sheet Name exist");
            var renamedSheet = await _renameSheetEvent.RaiseAsync(
                Prefix,
                Suffix,
                Number);

            var sheetIndex = ProjectSheets.IndexOf(selectedSheetModel);
            ProjectSheets?.RemoveAt(sheetIndex);
            ProjectSheets?.Insert(sheetIndex, renamedSheet);
            await _snackbarService.ShowAsync("Success Rename",
                "Rename sheet properties", SymbolRegular.ErrorCircle24, ControlAppearance.Success);
        }
        catch (Exception exception) when (exception.Message.Contains("Sheet Name exist"))
        {
            await _snackbarService.ShowAsync("Sheet Name exist",
                "Fill in another value", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
        catch (Exception ex)
        {
            await _snackbarService.ShowAsync("Unexpected error",
                ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    #endregion

    public SheetTableViewModel(ISnackbarService snackbarService, 
        CopySheetAsyncEvent copySheetEvent, 
        DeleteSheetAsyncEvent deleteSheetAsyncEvent, 
        RenameSheetAsyncEvent renameSheetAsyncEvent,
        ExportAsync export)
    {
        _snackbarService = snackbarService;
        _copySheetEvent = copySheetEvent;
        _deleteSheetEvent = deleteSheetAsyncEvent;
        _renameSheetEvent = renameSheetAsyncEvent;
        _export = export;

        var elements = RevitApi.CurrentDocument.GetElements()
            .WhereElementIsViewIndependent()
            .OfClass(typeof(ViewSheet)).ToElements();

        foreach (var element in elements)
        {
            var viewSheet = (ViewSheet)element;
             
            var regex = new Regex(@"\d+"); // matches one or more digits
            MatchEvaluator evaluator = Utils.ToMatch;
            var sheetNum = regex.Replace(viewSheet.SheetNumber, evaluator);

            ProjectSheets.Add(new SheetModel()
            {
                Name = viewSheet.Title,
                Number = sheetNum,
                ElementId = viewSheet.Id.IntegerValue,
                NumberPlacedViews = viewSheet.GetAllPlacedViews().Count
            });
        }
    }
}
