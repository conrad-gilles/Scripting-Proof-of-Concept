namespace Ember.Scripting;

public class MetaDataAttribueNullException : Exception
{
    public MetaDataAttribueNullException() : base() { }
    public MetaDataAttribueNullException(string message) : base(message) { }
    public MetaDataAttribueNullException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypeMoreThanOnceInAssemblyException : Exception
{
    public TypeMoreThanOnceInAssemblyException() : base() { }
    public TypeMoreThanOnceInAssemblyException(string message) : base(message) { }
    public TypeMoreThanOnceInAssemblyException(string message, Exception innerException) : base(message, innerException) { }
}

public class VersionIntMoreThanOnceInAssemblyException : Exception
{
    public VersionIntMoreThanOnceInAssemblyException() : base() { }
    public VersionIntMoreThanOnceInAssemblyException(string message) : base(message) { }
    public VersionIntMoreThanOnceInAssemblyException(string message, Exception innerException) : base(message, innerException) { }
}