using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
namespace Ember.Scripting;

[MetaDataIGeneratorIntrfc(version: 0, type: TypeInfo.AbstractBaseInSF)]
public interface IGeneratorBaseInterfaceSF
{
}

[MetaDataGeneratorClass(version: 0, type: TypeInfo.AbstractBaseInSF)]
public abstract class GeneratorContextSF    //interface
{
    public abstract GeneratorContextSF Downgrade();
    public abstract GeneratorContextSF CreateUsingData(DataAbstractClass data);
}

// public interface IContextFactory
// {
//     public static abstract GeneratorContextSF Create(IServiceProvider serviceProvider);
// }
public abstract class DataAbstractClass
{

}