namespace Ember.Scripting.Compilation;

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

public class ExecutionTimeCouldNotBeAssigned : CompilationException
{
    public ExecutionTimeCouldNotBeAssigned() : base() { }
    public ExecutionTimeCouldNotBeAssigned(string message) : base(message) { }
    public ExecutionTimeCouldNotBeAssigned(string message, Exception innerException) : base(message, innerException) { }
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

public class ClassNameOrBaseNameNullException : ValidationBeforeCompilationException
{
    public ClassNameOrBaseNameNullException() : base() { }
    public ClassNameOrBaseNameNullException(string message) : base(message) { }
    public ClassNameOrBaseNameNullException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptWasEmptyOrNullException : ValidationBeforeCompilationException
{
    public ScriptWasEmptyOrNullException() : base() { }
    public ScriptWasEmptyOrNullException(string message) : base(message) { }
    public ScriptWasEmptyOrNullException(string message, Exception innerException) : base(message, innerException) { }
}
public class ScriptFieldNullException : ValidationBeforeCompilationException
{
    public ScriptFieldNullException() : base() { }
    public ScriptFieldNullException(string message) : base(message) { }
    public ScriptFieldNullException(string message, Exception innerException) : base(message, innerException) { }
}
public class ForbiddenNamespaceException : ValidationBeforeCompilationException
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
public class UndefinedMethodException : ValidationBeforeCompilationException
{
    public MethodRecord? Method { get; }

    public UndefinedMethodException() : base() { }
    public UndefinedMethodException(string message) : base(message) { }
    public UndefinedMethodException(string message, Exception innerException) : base(message, innerException) { }

    public UndefinedMethodException(string message, MethodRecord method) : base(message)
    {
        Method = method;
    }
}
public class WrongReturnTypeException : ValidationBeforeCompilationException
{
    public Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? Method { get; }

    public WrongReturnTypeException() : base() { }
    public WrongReturnTypeException(string message) : base(message) { }
    public WrongReturnTypeException(string message, Exception innerException) : base(message, innerException) { }

    public WrongReturnTypeException(string message, Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? method) : base(message)
    {
        Method = method;
    }
}
public class WrongParameterTypeException : ValidationBeforeCompilationException
{
    public Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? Method { get; }

    public WrongParameterTypeException() : base() { }
    public WrongParameterTypeException(string message) : base(message) { }
    public WrongParameterTypeException(string message, Exception innerException) : base(message, innerException) { }

    public WrongParameterTypeException(string message, Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? method) : base(message)
    {
        Method = method;
    }
}
public class CouldNotFindMethodTypeException : ValidationBeforeCompilationException
{
    public Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? Method { get; }

    public CouldNotFindMethodTypeException() : base() { }
    public CouldNotFindMethodTypeException(string message) : base(message) { }
    public CouldNotFindMethodTypeException(string message, Exception innerException) : base(message, innerException) { }

    public CouldNotFindMethodTypeException(string message, Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? method) : base(message)
    {
        Method = method;
    }
}
public class CouldNotFindParameterException : ValidationBeforeCompilationException
{
    public Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? Method { get; }

    public CouldNotFindParameterException() : base() { }
    public CouldNotFindParameterException(string message) : base(message) { }
    public CouldNotFindParameterException(string message, Exception innerException) : base(message, innerException) { }

    public CouldNotFindParameterException(string message, Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? method) : base(message)
    {
        Method = method;
    }
}
public class RecordCouldNotBeMatchedException : ValidationBeforeCompilationException
{
    public Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? Method { get; }

    public RecordCouldNotBeMatchedException() : base() { }
    public RecordCouldNotBeMatchedException(string message) : base(message) { }
    public RecordCouldNotBeMatchedException(string message, Exception innerException) : base(message, innerException) { }
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