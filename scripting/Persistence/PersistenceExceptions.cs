namespace Ember.Scripting.Persistence;

public abstract class PersistenceException : ScriptingFrameworkException
{
    public PersistenceException() : base() { }
    public PersistenceException(string message) : base(message) { }
    public PersistenceException(string message, Exception innerException) : base(message, innerException) { }
}
public abstract class ScriptRepositoryException : PersistenceException
{
    public ScriptRepositoryException() : base() { }
    public ScriptRepositoryException(string message) : base(message) { }
    public ScriptRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}

public class ClearScriptCacheException : ScriptRepositoryException
{
    public ClearScriptCacheException() : base() { }
    public ClearScriptCacheException(string message) : base(message) { }
    public ClearScriptCacheException(string message, Exception innerException) : base(message, innerException) { }
}
public class CompileAllScriptsInFolderException : ScriptRepositoryException
{
    public CompileAllScriptsInFolderException() : base() { }
    public CompileAllScriptsInFolderException(string message) : base(message) { }
    public CompileAllScriptsInFolderException(string message, Exception innerException) : base(message, innerException) { }
}
public class CreateAndInsertCustomerScriptException : ScriptRepositoryException
{
    public CreateAndInsertCustomerScriptException() : base() { }
    public CreateAndInsertCustomerScriptException(string message) : base(message) { }
    public CreateAndInsertCustomerScriptException(string message, Exception innerException) : base(message, innerException) { }
}
public class CreateAndInsertCompiledScriptException : ScriptRepositoryException
{
    public CreateAndInsertCompiledScriptException() : base() { }
    public CreateAndInsertCompiledScriptException(string message) : base(message) { }
    public CreateAndInsertCompiledScriptException(string message, Exception innerException) : base(message, innerException) { }
}
public class UpdateScriptException : ScriptRepositoryException
{
    public UpdateScriptException() : base() { }
    public UpdateScriptException(string message) : base(message) { }
    public UpdateScriptException(string message, Exception innerException) : base(message, innerException) { }
}
public class GetScriptIdException : ScriptRepositoryException
{
    public GetScriptIdException() : base() { }
    public GetScriptIdException(string message) : base(message) { }
    public GetScriptIdException(string message, Exception innerException) : base(message, innerException) { }
}
public class DetectDuplicatesException : ScriptRepositoryException
{
    public DetectDuplicatesException() : base() { }
    public DetectDuplicatesException(string message) : base(message) { }
    public DetectDuplicatesException(string message, Exception innerException) : base(message, innerException) { }
}
public class RemoveDuplicatesException : ScriptRepositoryException
{
    public RemoveDuplicatesException() : base() { }
    public RemoveDuplicatesException(string message) : base(message) { }
    public RemoveDuplicatesException(string message, Exception innerException) : base(message, innerException) { }
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
public class CollisionOccuredException : PersistenceException
{
    public CollisionOccuredException() : base() { }
    public CollisionOccuredException(string message) : base(message) { }
    public CollisionOccuredException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptTypeNotSetException : PersistenceException
{
    public ScriptTypeNotSetException() : base() { }
    public ScriptTypeNotSetException(string message) : base(message) { }
    public ScriptTypeNotSetException(string message, Exception innerException) : base(message, innerException) { }
}