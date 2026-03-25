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

public class CompilationOfUpdatedScriptException : PersistenceException
{
    public CompilationOfUpdatedScriptException() : base() { }
    public CompilationOfUpdatedScriptException(string message) : base(message) { }
    public CompilationOfUpdatedScriptException(string message, Exception innerException) : base(message, innerException) { }
}

public class CouldNotMatchBaseTypeInPersistence : PersistenceException
{
    public CouldNotMatchBaseTypeInPersistence() : base() { }
    public CouldNotMatchBaseTypeInPersistence(string message) : base(message) { }
    public CouldNotMatchBaseTypeInPersistence(string message, Exception innerException) : base(message, innerException) { }
}

public class NoErrorsInScriptException : PersistenceException
{
    public NoErrorsInScriptException() : base() { }
    public NoErrorsInScriptException(string message) : base(message) { }
    public NoErrorsInScriptException(string message, Exception innerException) : base(message, innerException) { }
}