using System;
using System.Collections.Generic;

namespace Ember.Scripting;

public class NoFileWithThisClassNameFoundException : Exception
{
    public string? ClassName { get; }
    public string? SearchDirectory { get; }

    public NoFileWithThisClassNameFoundException() : base() { }
    public NoFileWithThisClassNameFoundException(string message) : base(message) { }
    public NoFileWithThisClassNameFoundException(string message, Exception innerException) : base(message, innerException) { }

    public NoFileWithThisClassNameFoundException(string className, string searchDirectory)
        : base($"No file containing the class '{className}' was found in directory '{searchDirectory}'.")
    {
        ClassName = className;
        SearchDirectory = searchDirectory;
    }
}

public class CompilationFailedException : Exception
{
    public IEnumerable<string>? CompilerErrors { get; }

    public CompilationFailedException() : base() { }
    public CompilationFailedException(string message) : base(message) { }
    public CompilationFailedException(string message, Exception innerException) : base(message, innerException) { }

    public CompilationFailedException(string message, IEnumerable<string> compilerErrors) : base(message)
    {
        CompilerErrors = compilerErrors;
    }
}

public class ScriptExecutionException : Exception
{
    public Guid? ScriptId { get; }

    public ScriptExecutionException() : base() { }
    public ScriptExecutionException(string message) : base(message) { }
    public ScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }

    public ScriptExecutionException(string message, Guid scriptId, Exception innerException) : base(message, innerException)
    {
        ScriptId = scriptId;
    }
}

public class ActionScriptExecutionException : ScriptExecutionException
{
    public ActionScriptExecutionException() : base() { }
    public ActionScriptExecutionException(string message) : base(message) { }
    public ActionScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
    public ActionScriptExecutionException(string message, Guid scriptId, Exception innerException) : base(message, scriptId, innerException) { }
}

public class ConditionScriptExecutionException : ScriptExecutionException
{
    public ConditionScriptExecutionException() : base() { }
    public ConditionScriptExecutionException(string message) : base(message) { }
    public ConditionScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
    public ConditionScriptExecutionException(string message, Guid scriptId, Exception innerException) : base(message, scriptId, innerException) { }
}

public class GetScriptPathFromFolderException : Exception
{
    public string? FolderPath { get; }

    public GetScriptPathFromFolderException() : base() { }
    public GetScriptPathFromFolderException(string message) : base(message) { }
    public GetScriptPathFromFolderException(string message, Exception innerException) : base(message, innerException) { }

    public GetScriptPathFromFolderException(string message, string folderPath) : base(message)
    {
        FolderPath = folderPath;
    }
}

public class CreateStringFromCsFileException : Exception
{
    public string? FilePath { get; }

    public CreateStringFromCsFileException() : base() { }
    public CreateStringFromCsFileException(string message) : base(message) { }
    public CreateStringFromCsFileException(string message, Exception innerException) : base(message, innerException) { }

    public CreateStringFromCsFileException(string message, string filePath, Exception innerException) : base(message, innerException)
    {
        FilePath = filePath;
    }
}

public class NoClassFoundInScriptFileException : Exception
{
    public string? FilePath { get; }

    public NoClassFoundInScriptFileException() : base() { }
    public NoClassFoundInScriptFileException(string message) : base(message) { }
    public NoClassFoundInScriptFileException(string message, Exception innerException) : base(message, innerException) { }

    // Custom constructor dynamically building the message
    public NoClassFoundInScriptFileException(string filePath, bool useDefaultMessage = true)
        : base(useDefaultMessage ? $"No valid class definition was found in the script file: {filePath}" : filePath)
    {
        FilePath = filePath;
    }
}

public class MoreThanOneClassFoundInScriptException : Exception
{
    public string? FilePath { get; }
    public int ClassCount { get; }

    public MoreThanOneClassFoundInScriptException() : base() { }
    public MoreThanOneClassFoundInScriptException(string message) : base(message) { }
    public MoreThanOneClassFoundInScriptException(string message, Exception innerException) : base(message, innerException) { }

    public MoreThanOneClassFoundInScriptException(string filePath, int classCount)
        : base($"Expected 1 class, but found {classCount} classes in the script file: {filePath}")
    {
        FilePath = filePath;
        ClassCount = classCount;
    }
}

public class ValidationBeforeCompilationException : Exception
{
    public IEnumerable<string>? ValidationErrors { get; }

    public ValidationBeforeCompilationException() : base() { }
    public ValidationBeforeCompilationException(string message) : base(message) { }
    public ValidationBeforeCompilationException(string message, Exception innerException) : base(message, innerException) { }

    public ValidationBeforeCompilationException(string message, IEnumerable<string> validationErrors) : base(message)
    {
        ValidationErrors = validationErrors;
    }
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

public class ScriptTimeoutException : Exception
{
    public int TimeoutMilliseconds { get; }

    public ScriptTimeoutException() : base() { }
    public ScriptTimeoutException(string message) : base(message) { }
    public ScriptTimeoutException(string message, Exception innerException) : base(message, innerException) { }

    public ScriptTimeoutException(int timeoutMilliseconds)
        : base($"The script execution timed out after {timeoutMilliseconds} milliseconds.")
    {
        TimeoutMilliseconds = timeoutMilliseconds;
    }
}

public class DbHelperException : Exception
{
    public DbHelperException() : base() { }
    public DbHelperException(string message) : base(message) { }
    public DbHelperException(string message, Exception innerException) : base(message, innerException) { }
}
