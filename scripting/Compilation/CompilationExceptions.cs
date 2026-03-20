namespace Ember.Scripting;

public class CompilationFailedException : Exception
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

public class MoreThanOneClassFoundInScriptException : Exception
{
    public MoreThanOneClassFoundInScriptException() : base() { }
    public MoreThanOneClassFoundInScriptException(string message) : base(message) { }
    public MoreThanOneClassFoundInScriptException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationBeforeCompilationException : Exception
{
    public ValidationBeforeCompilationException() : base() { }
    public ValidationBeforeCompilationException(string message) : base(message) { }
    public ValidationBeforeCompilationException(string message, Exception innerException) : base(message, innerException) { }
}

public class ClassNameOrBaseNameNullException : Exception
{
    public ClassNameOrBaseNameNullException() : base() { }
    public ClassNameOrBaseNameNullException(string message) : base(message) { }
    public ClassNameOrBaseNameNullException(string message, Exception innerException) : base(message, innerException) { }
}

public class ForbiddenNamespaceException : Exception
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