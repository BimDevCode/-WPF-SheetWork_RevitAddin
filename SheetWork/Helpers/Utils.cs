using System.Text.RegularExpressions;

namespace SheetWork.Helpers;

/// <summary>
/// Define static Utils
/// </summary>
public static class Utils
{
    /// <summary>
    /// String from Increase number from match
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    public static string AddOneToMatch(Match match)
    {
        var num = int.Parse(match.Value);
        var newNum = num + 1;
        return newNum.ToString();
    }

    /// <summary>
    /// String from match
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    public static string ToMatch(Match match)
    {
        return int.Parse(match.Value).ToString();
    }
}