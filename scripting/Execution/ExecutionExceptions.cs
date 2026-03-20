namespace Ember.Scripting;

public class ScriptExecutionException : Exception
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
public class NoClassFoundInScriptFileException : Exception
{
    public NoClassFoundInScriptFileException() : base() { }
    public NoClassFoundInScriptFileException(string message) : base(message) { }
    public NoClassFoundInScriptFileException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptTimeoutException : Exception
{
    public ScriptTimeoutException() : base() { }
    public ScriptTimeoutException(string message) : base(message) { }
    public ScriptTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
