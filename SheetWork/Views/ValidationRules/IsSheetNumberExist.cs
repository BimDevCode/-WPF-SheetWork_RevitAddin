using Autodesk.Revit.DB;
using SheetWork.Core;
using SheetWork.Helpers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SheetWork.Views.ValidationRules;

/// <summary>
/// Validate condition that input value for number of sheet is Unique
/// </summary>
public class IsSheetNumberExist: ValidationRule
{
    public string ErrorMessage { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var stringValue = (string)value;
        if (stringValue == null) return new ValidationResult(false, ErrorMessage);

        var regex = new Regex(@"\d+"); // matches one or more digits
        MatchEvaluator evaluator = Utils.ToMatch;
        var sheetNum = regex.Replace(stringValue, evaluator);

        var elements = RevitApi.CurrentDocument.GetElements()
            .WhereElementIsViewIndependent()
            .OfClass(typeof(ViewSheet))
            .ToElements().Cast<ViewSheet>();
        var isNotValid = elements.Any(viewSheet => regex.Replace(viewSheet.SheetNumber, evaluator) == sheetNum);
        return isNotValid
            ? new ValidationResult(false, ErrorMessage) 
            : ValidationResult.ValidResult;
    }
}