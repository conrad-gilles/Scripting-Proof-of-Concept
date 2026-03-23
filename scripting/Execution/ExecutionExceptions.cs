namespace Ember.Scripting;

public abstract class ExecutionException : ScriptingFrameworkException
{
    public ExecutionException() : base() { }
    public ExecutionException(string message) : base(message) { }
    public ExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptExecutionException : ExecutionException
{
    public ScriptExecutionException() : base() { }
    public ScriptExecutionException(string message) : base(message) { }
    public ScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
}

public class ActionScriptExecutionException : ScriptExecutionException
{
    public ActionScriptExecutionException() : base() { }
    public ActionScriptExecutionException(string message) : base(message) { }
    public ActionScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
}

public class ConditionScriptExecutionException : ScriptExecutionException
{
    public ConditionScriptExecutionException() : base() { }
    public ConditionScriptExecutionException(string message) : base(message) { }
    public ConditionScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
public class NoClassFoundInScriptFileException : ExecutionException
{
    public NoClassFoundInScriptFileException() : base() { }
    public NoClassFoundInScriptFileException(string message) : base(message) { }
    public NoClassFoundInScriptFileException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptTimeoutException : ExecutionException
{
    public ScriptTimeoutException() : base() { }
    public ScriptTimeoutException(string message) : base(message) { }
    public ScriptTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
public class MoreThanOneClassFoundInScriptExecutionException : ExecutionException
{
    public MoreThanOneClassFoundInScriptExecutionException() : base() { }
    public MoreThanOneClassFoundInScriptExecutionException(string message) : base(message) { }
    public MoreThanOneClassFoundInScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
