using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
namespace Ember.Scripting;

public abstract class Context : Downgradeable, Data
{
    public abstract Context Downgrade();
    public abstract Context CreateUsingData(DataAbstractClass data);
}
public interface IContext
{
}

[MetaDataIGeneratorIntrfc(version: 0, type: TypeInfo.AbstractBaseInSF)]
public interface IGeneratorContextBaseInterfaceSF : IContext
{
}
[MetaDataGeneratorClass(version: 0, type: TypeInfo.AbstractBaseInSF)]
public abstract class GeneratorContextSF : Context
{

}
public interface Downgradeable
{
    public Context Downgrade();
}
public interface Data
{
    public Context CreateUsingData(DataAbstractClass data);
}
public abstract class DataAbstractClass
{

}