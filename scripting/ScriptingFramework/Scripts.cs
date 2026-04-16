namespace Ember.Scripting.ScriptingFramework;


public interface IScriptType
{

}

public interface IUserSession
{
    Guid Id { get; }
    string UserName { get; }
}