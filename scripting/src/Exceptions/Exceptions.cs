// [Serializable]
namespace Ember.Scripting;

public class NoFileWithThisClassNameFoundException : Exception
{

}
public class CompilationFailedException : Exception
{

}
public class ScriptExecutionException : Exception
{

}
public class ActionScriptExecutionException : Exception
{

}
public class ConditionScriptExecutionException : Exception
{

}
public class GetScriptPathFromFolderException : Exception
{

}
public class CreateStringFromCsFileException : Exception
{

}
public class NoClassFoundInScriptFileException : Exception
{

}
public class MoreThanOneClassFoundInScriptException : Exception
{

}
public class ValidationBeforeCompilationException : Exception
{
    // 1. Default constructor
    public ValidationBeforeCompilationException() : base()
    {
    }

    // 2. Constructor that takes a custom message
    public ValidationBeforeCompilationException(string message) : base(message)
    {
    }

    // 3. Constructor that takes a custom message AND the original exception (Inner Exception)
    public ValidationBeforeCompilationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
public class ClassNameOrBaseNameNullException : Exception
{

}
public class ForbiddenNamespaceException : Exception
{
    public ForbiddenNamespaceException() : base()
    {
    }

    // 2. Constructor that takes a custom message
    public ForbiddenNamespaceException(string message) : base(message)
    {
    }

    // 3. Constructor that takes a custom message AND the original exception (Inner Exception)
    public ForbiddenNamespaceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
public class ScriptTimeoutException : Exception
{
    new string Message;
    public ScriptTimeoutException(string message)
    {
        Message = message;
    }
}
