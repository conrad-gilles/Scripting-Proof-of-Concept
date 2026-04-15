namespace Ember.Scripting.Execution;

public abstract class ExecutionException : ScriptingFrameworkException
{
    public ExecutionException() : base() { }
    public ExecutionException(string message) : base(message) { }
    public ExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
public abstract class ScriptExecutionException : ExecutionException
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
public class CouldNotFindMethodException : ActionScriptExecutionException
{
    public CouldNotFindMethodException() : base() { }
    public CouldNotFindMethodException(string message) : base(message) { }
    public CouldNotFindMethodException(string message, Exception innerException) : base(message, innerException) { }
}
public class CompiledScriptWasTooLargeException : ScriptExecutionException
{
    public CompiledScriptWasTooLargeException() : base() { }
    public CompiledScriptWasTooLargeException(string message) : base(message) { }
    public CompiledScriptWasTooLargeException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptWasNotOfCorrectTypeException : ScriptExecutionException
{
    public ScriptWasNotOfCorrectTypeException() : base() { }
    public ScriptWasNotOfCorrectTypeException(string message) : base(message) { }
    public ScriptWasNotOfCorrectTypeException(string message, Exception innerException) : base(message, innerException) { }
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
public abstract class ScriptTimeoutException : ExecutionException
{
    public ScriptTimeoutException() : base() { }
    public ScriptTimeoutException(string message) : base(message) { }
    public ScriptTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
public class ConditionScriptTimeoutException : ExecutionException
{
    public ConditionScriptTimeoutException() : base() { }
    public ConditionScriptTimeoutException(string message) : base(message) { }
    public ConditionScriptTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
public class ActionScriptTimeoutException : ExecutionException
{
    public ActionScriptTimeoutException() : base() { }
    public ActionScriptTimeoutException(string message) : base(message) { }
    public ActionScriptTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
public class MoreThanOneClassFoundInScriptExecutionException : ExecutionException
{
    public MoreThanOneClassFoundInScriptExecutionException() : base() { }
    public MoreThanOneClassFoundInScriptExecutionException(string message) : base(message) { }
    public MoreThanOneClassFoundInScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
