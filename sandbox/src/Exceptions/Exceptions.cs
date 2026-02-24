// [Serializable]
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

}
public class ClassNameOrBaseNameNullException : Exception
{

}
public class ForbiddenNamespaceException : Exception
{
    string Message;
    public ForbiddenNamespaceException(string message)
    {
        Message = message;
    }
}
public class ScriptTimeoutException : Exception
{
    string Message;
    public ScriptTimeoutException(string message)
    {
        Message = message;
    }
}