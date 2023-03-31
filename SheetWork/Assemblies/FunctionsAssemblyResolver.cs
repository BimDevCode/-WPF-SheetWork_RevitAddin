using System.Reflection;

namespace SheetWork.Assemblies;
/// <summary>
/// This resolver created to load in force mode all external DLL
/// that do no load to revit directly
/// </summary>
public class FunctionsAssemblyResolver
{
    public static void RedirectAssembly()
    {
        var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName).Select(a => a.FullName).ToList();
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var requestedAssembly = new AssemblyName(args.Name);
        Assembly assembly = null;
        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        try
        {
            assembly = Assembly.Load(requestedAssembly.Name);
        }
        catch (Exception ex)// pass for normal work if revit have troubles with dll load
        {
        }
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        return assembly;
    }
}