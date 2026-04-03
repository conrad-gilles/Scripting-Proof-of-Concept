using Ember.Scripting;

namespace BlazorUI.Settings;

public static class AppSettings
{
    public static bool UseConsole = true;
    public static bool UsePopup = true;
    public static bool PrintDetailedInConsole = false;
    public static int EmberApiVersion = EmberMethods.GetEmberApiVersion();
}