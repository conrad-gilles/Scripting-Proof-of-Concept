namespace Ember.Scripting;

public abstract class ScriptManagerExceptions : ScriptingFrameworkException
{
    public ScriptManagerExceptions() : base() { }
    public ScriptManagerExceptions(string message) : base(message) { }
    public ScriptManagerExceptions(string message, Exception innerException) : base(message, innerException) { }

}
public class CouldNotAssignBaseTypeException : ScriptManagerExceptions
{
    public CouldNotAssignBaseTypeException() : base() { }
    public CouldNotAssignBaseTypeException(string message) : base(message) { }
    public CouldNotAssignBaseTypeException(string message, Exception innerException) : base(message, innerException) { }
}