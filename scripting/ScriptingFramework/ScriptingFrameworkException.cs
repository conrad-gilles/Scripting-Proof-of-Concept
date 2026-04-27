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

public class NoContextClassDefinedForApiVException : ScriptingFrameworkException
{
    public NoContextClassDefinedForApiVException() : base() { }
    public NoContextClassDefinedForApiVException(string message) : base(message) { }
    public NoContextClassDefinedForApiVException(string message, Exception innerException) : base(message, innerException) { }
}

public class DowngradeFailedInEmberException : ScriptingFrameworkException
{
    public DowngradeFailedInEmberException() : base() { }
    public DowngradeFailedInEmberException(string message) : base(message) { }
    public DowngradeFailedInEmberException(string message, Exception innerException) : base(message, innerException) { }
}

public class LoopExecutedTooManyTimesException : ScriptingFrameworkException
{
    public LoopExecutedTooManyTimesException() : base() { }
    public LoopExecutedTooManyTimesException(string message) : base(message) { }
    public LoopExecutedTooManyTimesException(string message, Exception innerException) : base(message, innerException) { }
}
public class NonDowngradeableException : ScriptingFrameworkException
{
    public NonDowngradeableException() : base() { }
    public NonDowngradeableException(string message) : base(message) { }
    public NonDowngradeableException(string message, Exception innerException) : base(message, innerException) { }
}