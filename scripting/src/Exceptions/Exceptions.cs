using System;
using System.Collections.Generic;

namespace Ember.Scripting;

public class NoFileWithThisClassNameFoundException : Exception
{
    public NoFileWithThisClassNameFoundException() : base() { }
    public NoFileWithThisClassNameFoundException(string message) : base(message) { }
    public NoFileWithThisClassNameFoundException(string message, Exception innerException) : base(message, innerException) { }
}

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

public class ScriptExecutionException : Exception
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

public class ConditionScriptExecutionException : ScriptExecutionException
{
    public ConditionScriptExecutionException() : base() { }
    public ConditionScriptExecutionException(string message) : base(message) { }
    public ConditionScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
}

public class GetScriptPathFromFolderException : Exception
{
    public GetScriptPathFromFolderException() : base() { }
    public GetScriptPathFromFolderException(string message) : base(message) { }
    public GetScriptPathFromFolderException(string message, Exception innerException) : base(message, innerException) { }
}

public class CreateStringFromCsFileException : Exception
{
    public CreateStringFromCsFileException() : base() { }
    public CreateStringFromCsFileException(string message) : base(message) { }
    public CreateStringFromCsFileException(string message, Exception innerException) : base(message, innerException) { }
}

public class NoClassFoundInScriptFileException : Exception
{
    public NoClassFoundInScriptFileException() : base() { }
    public NoClassFoundInScriptFileException(string message) : base(message) { }
    public NoClassFoundInScriptFileException(string message, Exception innerException) : base(message, innerException) { }
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

public class ScriptTimeoutException : Exception
{
    public ScriptTimeoutException() : base() { }
    public ScriptTimeoutException(string message) : base(message) { }
    public ScriptTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}

public class DbHelperException : Exception
{
    public DbHelperException() : base() { }
    public DbHelperException(string message) : base(message) { }
    public DbHelperException(string message, Exception innerException) : base(message, innerException) { }
}
public class FacadeException : Exception
{
    public FacadeException() : base() { }
    public FacadeException(string message) : base(message) { }
    public FacadeException(string message, Exception innerException) : base(message, innerException) { }
}

public class ExceptionHelper    //for future to traverse exception chain
{
    public static Exception GetExceptionFromChain(Exception ex, int i)
    {
        Exception baseException = ex.GetBaseException();
        int indexInChain = 0;

        Exception innerEx = ex;
        while (innerEx.Equals(baseException) == false && indexInChain != i)
        {
            innerEx = innerEx.InnerException!;
            indexInChain++;
        }
        if (indexInChain < i)
        {
            throw new Exception(message: "Index is out of bounds of the Exception chain!");
        }
        return innerEx;
    }
    public static Exception GetExceptionFromChainReversed(Exception ex, int i)  //todo fix use index base lookup get rid of for loop
    {
        List<Exception> exceptions = [];
        Exception baseException = ex.GetBaseException();

        while (ex.Equals(baseException) == false)
        {
            exceptions.Add(ex);
            ex = ex.InnerException!;
        }

        exceptions.Reverse();

        for (int j = 0; j < exceptions.Count(); j++)
        {
            if (j == i)
            {
                return exceptions[i];
            }
        }
        throw new Exception(message: "Could not find your exception in the List!");
    }
}