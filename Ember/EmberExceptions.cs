namespace Ember.Simulation;

public abstract class EmberException : Exception
{
    public EmberException() : base() { }
    public EmberException(string message) : base(message) { }
    public EmberException(string message, Exception innerException) : base(message, innerException) { }

}

