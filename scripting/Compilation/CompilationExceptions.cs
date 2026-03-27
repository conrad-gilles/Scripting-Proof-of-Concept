namespace Ember.Scripting;

public abstract class CompilationException : ScriptingFrameworkException
{
    public CompilationException() : base() { }
    public CompilationException(string message) : base(message) { }
    public CompilationException(string message, Exception innerException) : base(message, innerException) { }

}
public class CompilationFailedException : CompilationException
{
    public List<ScriptCompilationError> Errors = [];

    public CompilationFailedException() : base() { }
    public CompilationFailedException(string message) : base(message) { }
    public CompilationFailedException(string message, Exception innerException) : base(message, innerException) { }

    public CompilationFailedException(string message, List<ScriptCompilationError> errors)
        : base(message)
    {
        Errors = errors;
    }
}

public class MoreThanOneClassFoundInScriptException : CompilationException
{
    public MoreThanOneClassFoundInScriptException() : base() { }
    public MoreThanOneClassFoundInScriptException(string message) : base(message) { }
    public MoreThanOneClassFoundInScriptException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationBeforeCompilationException : CompilationException
{
    public ValidationBeforeCompilationException() : base() { }
    public ValidationBeforeCompilationException(string message) : base(message) { }
    public ValidationBeforeCompilationException(string message, Exception innerException) : base(message, innerException) { }
}

public class ContextNameOccuredMoreThanOnceException : ValidationBeforeCompilationException
{
    public ContextNameOccuredMoreThanOnceException() : base() { }
    public ContextNameOccuredMoreThanOnceException(string message) : base(message) { }
    public ContextNameOccuredMoreThanOnceException(string message, Exception innerException) : base(message, innerException) { }
}
public class VersionIntNotAssignedException : ValidationBeforeCompilationException
{
    public VersionIntNotAssignedException() : base() { }
    public VersionIntNotAssignedException(string message) : base(message) { }
    public VersionIntNotAssignedException(string message, Exception innerException) : base(message, innerException) { }
}

public class ClassNameOrBaseNameNullException : CompilationException
{
    public ClassNameOrBaseNameNullException() : base() { }
    public ClassNameOrBaseNameNullException(string message) : base(message) { }
    public ClassNameOrBaseNameNullException(string message, Exception innerException) : base(message, innerException) { }
}

public class ForbiddenNamespaceException : CompilationException
{
    public string? AttemptedNamespace { get; }

    public ForbiddenNamespaceException() : base() { }
    public ForbiddenNamespaceException(string message) : base(message) { }
    public ForbiddenNamespaceException(string message, Exception innerException) : base(message, innerException) { }

    public ForbiddenNamespaceException(string message, string attemptedNamespace) : base(message)
    {
        AttemptedNamespace = attemptedNamespace;
    }
}
public class ForbiddenTypeAccessException : ForbiddenNamespaceException
{
    public new string? AttemptedNamespace { get; }

    public ForbiddenTypeAccessException() : base() { }
    public ForbiddenTypeAccessException(string message) : base(message) { }
    public ForbiddenTypeAccessException(string message, Exception innerException) : base(message, innerException) { }

    public ForbiddenTypeAccessException(string message, string attemptedNamespace) : base(message)
    {
        AttemptedNamespace = attemptedNamespace;
    }
}

public class ReferencesForVersionNotFound : CompilationException
{
    public ReferencesForVersionNotFound() : base() { }
    public ReferencesForVersionNotFound(string message) : base(message) { }
    public ReferencesForVersionNotFound(string message, Exception innerException) : base(message, innerException) { }
}

public class CouldNotMatchBaseTypeInCompiler : CompilationException
{
    public CouldNotMatchBaseTypeInCompiler() : base() { }
    public CouldNotMatchBaseTypeInCompiler(string message) : base(message) { }
    public CouldNotMatchBaseTypeInCompiler(string message, Exception innerException) : base(message, innerException) { }
}
public class CouldNotMatchBaseTypeInRecord : CompilationException
{
    public CouldNotMatchBaseTypeInRecord() : base() { }
    public CouldNotMatchBaseTypeInRecord(string message) : base(message) { }
    public CouldNotMatchBaseTypeInRecord(string message, Exception innerException) : base(message, innerException) { }
}

public class ConcellationTokenUncheckedException : CompilationException
{
    public ConcellationTokenUncheckedException() : base() { }
    public ConcellationTokenUncheckedException(string message) : base(message) { }
    public ConcellationTokenUncheckedException(string message, Exception innerException) : base(message, innerException) { }
}