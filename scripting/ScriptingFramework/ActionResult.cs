using TypeInfo = Ember.Scripting.Versioning.TypeInfo;

namespace Ember.Scripting.ScriptingFramework;

[MetaDataActionResult(version: 0, type: TypeInfo.AbstractBaseInSF)]
public abstract class ActionResultSF
{
    public abstract ActionResultSF Upgrade(ActionResultSF actionResult);
}