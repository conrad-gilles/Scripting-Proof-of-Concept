using TypeInfo = Ember.Scripting.Versioning.TypeInfo;

namespace Ember.Scripting.ScriptingFramework;

[MetaDataActionResult(version: 0, type: TypeInfo.AbstractBaseInSF)]
public abstract class ActionResultSF : UpgradeableReturnValue
{
    // public abstract ActionResultSF Upgrade(ActionResultSF actionResult);

    public abstract object Upgrade(object actionResult);
}
public interface UpgradeableReturnValue
{
    public object Upgrade(object returnValue);
}