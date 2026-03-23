namespace Ember.Scripting;

public abstract class ScriptingFrameworkException : Exception
{
    public ScriptingFrameworkException() : base() { }
    public ScriptingFrameworkException(string message) : base(message) { }
    public ScriptingFrameworkException(string message, Exception innerException) : base(message, innerException) { }
}