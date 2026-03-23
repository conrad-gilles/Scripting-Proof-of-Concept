namespace Ember.Scripting;

public abstract class VersioningExceptions : ScriptingFrameworkException
{
    public VersioningExceptions() : base() { }
    public VersioningExceptions(string message) : base(message) { }
    public VersioningExceptions(string message, Exception innerException) : base(message, innerException) { }

}
public class MetaDataAttribueNullException : VersioningExceptions
{
    public MetaDataAttribueNullException() : base() { }
    public MetaDataAttribueNullException(string message) : base(message) { }
    public MetaDataAttribueNullException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypeMoreThanOnceInAssemblyException : VersioningExceptions
{
    public TypeMoreThanOnceInAssemblyException() : base() { }
    public TypeMoreThanOnceInAssemblyException(string message) : base(message) { }
    public TypeMoreThanOnceInAssemblyException(string message, Exception innerException) : base(message, innerException) { }
}

public class VersionIntMoreThanOnceInAssemblyException : VersioningExceptions
{
    public VersionIntMoreThanOnceInAssemblyException() : base() { }
    public VersionIntMoreThanOnceInAssemblyException(string message) : base(message) { }
    public VersionIntMoreThanOnceInAssemblyException(string message, Exception innerException) : base(message, innerException) { }
}