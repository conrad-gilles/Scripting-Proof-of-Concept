namespace Ember.Scripting;

public abstract class CompilationException : ScriptingFrameworkException
{
    public CompilationException() : base() { }
    public CompilationException(string message) : base(message) { }
    public CompilationException(string message, Exception innerException) : base(message, innerException) { }

}
public class CompilationFailedException : CompilationException
{
    string? CompilerErrors { get; }

    public CompilationFailedException() : base() { }
    public CompilationFailedException(string message) : base(message) { }
    public CompilationFailedException(string message, Exception innerException) : base(message, innerException) { }

    public CompilationFailedException(string message, string compilerErrors) : base(message)
    {
        CompilerErrors = compilerErrors;
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

public class ReferencesForVersionNotFound : CompilationException
{
    public ReferencesForVersionNotFound() : base() { }
    public ReferencesForVersionNotFound(string message) : base(message) { }
    public ReferencesForVersionNotFound(string message, Exception innerException) : base(message, innerException) { }
}