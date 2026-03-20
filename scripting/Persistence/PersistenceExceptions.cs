namespace Ember.Scripting;

public class ScriptRepositoryException : Exception
{
    public ScriptRepositoryException() : base() { }
    public ScriptRepositoryException(string message) : base(message) { }
    public ScriptRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}


public class SaveScriptWithoutCompilingException : Exception
{
    public SaveScriptWithoutCompilingException() : base() { }
    public SaveScriptWithoutCompilingException(string message) : base(message) { }
    public SaveScriptWithoutCompilingException(string message, Exception innerException) : base(message, innerException) { }
}
