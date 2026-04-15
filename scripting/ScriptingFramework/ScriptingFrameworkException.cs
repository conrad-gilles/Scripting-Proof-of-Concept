namespace Ember.Scripting.ScriptingFramework;

public abstract class ScriptingFrameworkException : Exception
{
    public ScriptingFrameworkException() : base() { }
    public ScriptingFrameworkException(string message) : base(message) { }
    public ScriptingFrameworkException(string message, Exception innerException) : base(message, innerException) { }
}

public class MethodNotImplementedException : ScriptingFrameworkException
{

    public MethodNotImplementedException() : base() { }
    public MethodNotImplementedException(string message) : base(message) { }
    public MethodNotImplementedException(string message, Exception innerException) : base(message, innerException) { }
}