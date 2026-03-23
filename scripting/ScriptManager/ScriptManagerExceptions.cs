namespace Ember.Scripting;

public class CouldNotAssignBaseTypeException : Exception
{
    public CouldNotAssignBaseTypeException() : base() { }
    public CouldNotAssignBaseTypeException(string message) : base(message) { }
    public CouldNotAssignBaseTypeException(string message, Exception innerException) : base(message, innerException) { }
}