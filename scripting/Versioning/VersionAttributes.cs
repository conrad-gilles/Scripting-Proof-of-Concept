namespace Ember.Scripting.Versioning;

[AttributeUsage(AttributeTargets.Interface)]
public class MetaDataIScript : Attribute
{
    public int Version { get; }
    public IMDScriptType Type { get; }
    public IMDScriptReturnType ReturnType { get; }
    public MetaDataIScript(int version, IMDScriptReturnType returnType, IMDScriptType type)
    {
        Version = version;
        ReturnType = returnType;
        Type = type;
    }

    public MetaDataIScript(int version)
    {
        Version = version;
        ReturnType = IMDScriptReturnType.Action;
        Type = IMDScriptType.DefaultVersioned;
    }
}

public enum IMDScriptType
{
    AbstractBaseInSF,
    GenericSimple,
    Generic,
    DefaultVersioned
}
public enum IMDScriptReturnType
{
    Condition, Action
}

[AttributeUsage(AttributeTargets.Class)]
public class MetaDataCustomReturn : Attribute
{
    public int Version { get; }

    public MetaDataCustomReturn(int version)
    {
        Version = version;

    }

}

public enum TypeInfo
{
    AbstractBaseInSF,
    DefaultVersioned
}


[AttributeUsage(AttributeTargets.Class)]
public class MetaDataClass : Attribute
{
    public int Version { get; }
    public TypeInfo Type { get; }

    public MetaDataClass(int version)
    {
        Version = version;
        Type = TypeInfo.DefaultVersioned;
    }

    public MetaDataClass(int version, TypeInfo type)
    {
        Version = version;
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Interface)]
public class MetaDataIntrfc : Attribute
{
    public int Version { get; }
    public TypeInfo Type { get; }

    public MetaDataIntrfc(int version)
    {
        Version = version;
        Type = TypeInfo.DefaultVersioned;
    }

    public MetaDataIntrfc(int version, TypeInfo type)
    {
        Version = version;
        Type = type;
    }
}
