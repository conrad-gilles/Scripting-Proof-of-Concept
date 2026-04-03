using Ember.Scripting;

namespace BlazorUI.Settings;

public static class AppSettings
{
    public static bool UseConsole = true;
    public static bool UsePopup = true;
    public static bool PrintDetailedInConsole = false;
    public static int EmberApiVersion = EmberMethods.GetEmberApiVersion();

    // public static async Task<string> UpdateApiVersion()
    // {

    //     try
    //     {
    //         ServiceCollection services2 = new ServiceCollection();

    //         LoggerForScripting? logger = new LoggerForScripting();
    //         Log.Debug("Sandbox launched.");

    //         services2 = new ServiceCollection();
    //         services2.AddLogging(builder =>
    //         {
    //             builder.AddSerilog(dispose: true);
    //         });

    //         services2.AddDbContextFactory<EFModeling.EntityProperties.FluentAPI.Required.ScriptDbContext>();
    //         services2.AddSingleton<IUserSession, Sandbox.SandBoxUserSession>();

    //         ScriptingServiceCollectionExtensions.AddEmberScripting(services2, EmberMethods.GetReferences(),
    //             EmberMethods.GetEmberApiVersion(testingDiffrentVersion: _selectedApiVersion));

    //         var provider2 = services2.BuildServiceProvider();

    //         ScriptManager = provider2.GetRequiredService<ISccriptManagerDeleteAfter>();
    //         Console.Log($"User selected API Version: {_selectedApiVersion}");

    //         await LoadScripts();
    //     }
    //     catch (Exception e)
    //     {
    //         Console.LogException(e);
    //         myPopup.CreateErrorPopup(e.GetType().Name, e.Message);
    //     }

    //     return null;
    // }
}