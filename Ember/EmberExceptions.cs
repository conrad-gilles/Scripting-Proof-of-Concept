namespace Ember.Simulation;

public abstract class EmberException : Exception
{
    public EmberException() : base() { }
    public EmberException(string message) : base(message) { }
    public EmberException(string message, Exception innerException) : base(message, innerException) { }

}

public class NoFileWithThisClassNameFoundException : Exception
{
    public NoFileWithThisClassNameFoundException() : base() { }
    public NoFileWithThisClassNameFoundException(string message) : base(message) { }
    public NoFileWithThisClassNameFoundException(string message, Exception innerException) : base(message, innerException) { }
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


