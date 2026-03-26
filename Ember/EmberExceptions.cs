namespace Ember.Simulation;

public abstract class EmberException : Exception
{
    public EmberException() : base() { }
    public EmberException(string message) : base(message) { }
    public EmberException(string message, Exception innerException) : base(message, innerException) { }

}

public class SourceCodeNullWhenDowngradeException : EmberException
{
    public SourceCodeNullWhenDowngradeException() : base() { }
    public SourceCodeNullWhenDowngradeException(string message) : base(message) { }
    public SourceCodeNullWhenDowngradeException(string message, Exception innerException) : base(message, innerException) { }
}

public class NoContextClassDefinedForApiVException : EmberException
{
    public NoContextClassDefinedForApiVException() : base() { }
    public NoContextClassDefinedForApiVException(string message) : base(message) { }
    public NoContextClassDefinedForApiVException(string message, Exception innerException) : base(message, innerException) { }
}

public class DowngradeFailedInEmberException : EmberException
{
    public DowngradeFailedInEmberException() : base() { }
    public DowngradeFailedInEmberException(string message) : base(message) { }
    public DowngradeFailedInEmberException(string message, Exception innerException) : base(message, innerException) { }
}

public class LoopExecutedTooManyTimesException : EmberException
{
    public LoopExecutedTooManyTimesException() : base() { }
    public LoopExecutedTooManyTimesException(string message) : base(message) { }
    public LoopExecutedTooManyTimesException(string message, Exception innerException) : base(message, innerException) { }
}