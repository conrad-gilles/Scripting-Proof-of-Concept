namespace Ember.Scripting.Versioning;

[AttributeUsage(AttributeTargets.Interface)]
public class MetaDataIGeneratorScript : Attribute
{
    public int Version { get; }
    public IGeneratorScriptType Type { get; }
    public IGeneratorScriptReturnType ReturnType { get; }
    public MetaDataIGeneratorScript(int version, IGeneratorScriptReturnType returnType, IGeneratorScriptType type)
    {
        Version = version;
        ReturnType = returnType;
        Type = type;
    }

    public MetaDataIGeneratorScript(int version)
    {
        Version = version;
        ReturnType = IGeneratorScriptReturnType.Action;
        Type = IGeneratorScriptType.DefaultVersioned;
    }
}

public enum IGeneratorScriptType
{
    AbstractBaseInSF,
    GenericSimple,
    Generic,
    DefaultVersioned
}
public enum IGeneratorScriptReturnType
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
public class MetaDataGeneratorClass : Attribute
{
    public int Version { get; }
    public TypeInfo Type { get; }

    public MetaDataGeneratorClass(int version)
    {
        Version = version;
        Type = TypeInfo.DefaultVersioned;
    }

    public MetaDataGeneratorClass(int version, TypeInfo type)
    {
        Version = version;
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Interface)]
public class MetaDataIGeneratorIntrfc : Attribute
{
    public int Version { get; }
    public TypeInfo Type { get; }

    public MetaDataIGeneratorIntrfc(int version)
    {
        Version = version;
        Type = TypeInfo.DefaultVersioned;
    }

    public MetaDataIGeneratorIntrfc(int version, TypeInfo type)
    {
        Version = version;
        Type = type;
    }
}
