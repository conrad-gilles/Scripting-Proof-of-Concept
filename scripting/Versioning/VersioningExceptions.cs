namespace Ember.Scripting.Versioning;

public abstract class VersioningExceptions : ScriptingFrameworkException
{
    public VersioningExceptions() : base() { }
    public VersioningExceptions(string message) : base(message) { }
    public VersioningExceptions(string message, Exception innerException) : base(message, innerException) { }

}
public abstract class MetaDataAttribueNullException : VersioningExceptions
{
    public MetaDataAttribueNullException() : base() { }
    public MetaDataAttribueNullException(string message) : base(message) { }
    public MetaDataAttribueNullException(string message, Exception innerException) : base(message, innerException) { }
}
public class ActionResultVSNullException : MetaDataAttribueNullException
{
    public ActionResultVSNullException() : base() { }
    public ActionResultVSNullException(string message) : base(message) { }
    public ActionResultVSNullException(string message, Exception innerException) : base(message, innerException) { }
}
public class MetaDataAttribueNullSVSException : MetaDataAttribueNullException
{
    public MetaDataAttribueNullSVSException() : base() { }
    public MetaDataAttribueNullSVSException(string message) : base(message) { }
    public MetaDataAttribueNullSVSException(string message, Exception innerException) : base(message, innerException) { }
}
public class MetaDataAttribueNullCVSCException : MetaDataAttribueNullException
{
    public MetaDataAttribueNullCVSCException() : base() { }
    public MetaDataAttribueNullCVSCException(string message) : base(message) { }
    public MetaDataAttribueNullCVSCException(string message, Exception innerException) : base(message, innerException) { }
}
public class MetaDataAttribueNullCVSIException : MetaDataAttribueNullException
{
    public MetaDataAttribueNullCVSIException() : base() { }
    public MetaDataAttribueNullCVSIException(string message) : base(message) { }
    public MetaDataAttribueNullCVSIException(string message, Exception innerException) : base(message, innerException) { }
}
public abstract class TypeMoreThanOnceInAssemblyException : VersioningExceptions
{
    public TypeMoreThanOnceInAssemblyException() : base() { }
    public TypeMoreThanOnceInAssemblyException(string message) : base(message) { }
    public TypeMoreThanOnceInAssemblyException(string message, Exception innerException) : base(message, innerException) { }
}
public class TypeMoreThanOnceInAssemblyCVSCException : VersioningExceptions
{
    public TypeMoreThanOnceInAssemblyCVSCException() : base() { }
    public TypeMoreThanOnceInAssemblyCVSCException(string message) : base(message) { }
    public TypeMoreThanOnceInAssemblyCVSCException(string message, Exception innerException) : base(message, innerException) { }
}
public class TypeMoreThanOnceInAssemblyCVSIException : TypeMoreThanOnceInAssemblyException
{
    public TypeMoreThanOnceInAssemblyCVSIException() : base() { }
    public TypeMoreThanOnceInAssemblyCVSIException(string message) : base(message) { }
    public TypeMoreThanOnceInAssemblyCVSIException(string message, Exception innerException) : base(message, innerException) { }
}
public abstract class VersionIntMoreThanOnceInAssemblyException : VersioningExceptions
{
    public VersionIntMoreThanOnceInAssemblyException() : base() { }
    public VersionIntMoreThanOnceInAssemblyException(string message) : base(message) { }
    public VersionIntMoreThanOnceInAssemblyException(string message, Exception innerException) : base(message, innerException) { }
}
public class VersionIntMoreThanOnceInAssemblyARVSException : VersionIntMoreThanOnceInAssemblyException
{
    public VersionIntMoreThanOnceInAssemblyARVSException() : base() { }
    public VersionIntMoreThanOnceInAssemblyARVSException(string message) : base(message) { }
    public VersionIntMoreThanOnceInAssemblyARVSException(string message, Exception innerException) : base(message, innerException) { }
}
public class VersionIntMoreThanOnceInAssemblyCVSCException : VersionIntMoreThanOnceInAssemblyException
{
    public VersionIntMoreThanOnceInAssemblyCVSCException() : base() { }
    public VersionIntMoreThanOnceInAssemblyCVSCException(string message) : base(message) { }
    public VersionIntMoreThanOnceInAssemblyCVSCException(string message, Exception innerException) : base(message, innerException) { }
}
public class VersionIntMoreThanOnceInAssemblyCVSIException : VersionIntMoreThanOnceInAssemblyException
{
    public VersionIntMoreThanOnceInAssemblyCVSIException() : base() { }
    public VersionIntMoreThanOnceInAssemblyCVSIException(string message) : base(message) { }
    public VersionIntMoreThanOnceInAssemblyCVSIException(string message, Exception innerException) : base(message, innerException) { }
}
public class MetadDataAttributeNotDefinedException : VersioningExceptions
{
    public MetadDataAttributeNotDefinedException() : base() { }
    public MetadDataAttributeNotDefinedException(string message) : base(message) { }
    public MetadDataAttributeNotDefinedException(string message, Exception innerException) : base(message, innerException) { }
}
public class NoVersionListFoundException : VersioningExceptions
{
    public NoVersionListFoundException() : base() { }
    public NoVersionListFoundException(string message) : base(message) { }
    public NoVersionListFoundException(string message, Exception innerException) : base(message, innerException) { }
}
public class NoNextVersionFoundException : VersioningExceptions
{
    public NoNextVersionFoundException() : base() { }
    public NoNextVersionFoundException(string message) : base(message) { }
    public NoNextVersionFoundException(string message, Exception innerException) : base(message, innerException) { }
}
public class UpgradeFailedException : VersioningExceptions
{
    public UpgradeFailedException() : base() { }
    public UpgradeFailedException(string message) : base(message) { }
    public UpgradeFailedException(string message, Exception innerException) : base(message, innerException) { }
}