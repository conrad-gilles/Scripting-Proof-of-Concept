namespace Ember.Scripting.ScriptingFramework;

public abstract class CustomReturnType : IUpgradeableReturnValue
{
    public abstract object Upgrade(object returnValue);
}
public interface IUpgradeableReturnValue
{
    public object Upgrade(object returnValue);
}