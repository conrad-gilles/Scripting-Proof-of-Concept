using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
namespace Ember.Scripting;

[MetaDataIGeneratorIntrfc(version: 0, type: TypeInfo.AbstractBaseInSF)]
public interface IGeneratorBaseInterfaceSF
{
}

public abstract class Context
{
    public abstract Context Downgrade();
    public abstract Context CreateUsingData(DataAbstractClass data);
}

[MetaDataGeneratorClass(version: 0, type: TypeInfo.AbstractBaseInSF)]
public abstract class GeneratorContextSF : Context
{

}
public abstract class DataAbstractClass
{

}