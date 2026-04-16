namespace Ember.Scripting.ScriptingFramework;

public abstract class CustomReturnType : UpgradeableReturnValue
{
    public abstract object Upgrade(object returnValue);
}
public interface UpgradeableReturnValue
{
    public object Upgrade(object returnValue);
}