namespace Ember.Scripting;

public abstract class PersistenceException : ScriptingFrameworkException
{
    public PersistenceException() : base() { }
    public PersistenceException(string message) : base(message) { }
    public PersistenceException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptRepositoryException : PersistenceException
{
    public ScriptRepositoryException() : base() { }
    public ScriptRepositoryException(string message) : base(message) { }
    public ScriptRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}


public class SaveScriptWithoutCompilingException : PersistenceException
{
    public SaveScriptWithoutCompilingException() : base() { }
    public SaveScriptWithoutCompilingException(string message) : base(message) { }
    public SaveScriptWithoutCompilingException(string message, Exception innerException) : base(message, innerException) { }
}
