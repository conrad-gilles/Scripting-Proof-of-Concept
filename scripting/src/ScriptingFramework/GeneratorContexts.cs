using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
namespace Ember.Scripting;

[MetaDataIGeneratorIntrfc(version: 0, type: TypeInfo.AbstractBaseInSF)]
public interface IGeneratorBaseInterface
{
}

[MetaDataGeneratorClass(version: 0, type: TypeInfo.AbstractBaseInSF)]
public abstract class GeneratorContext
{
    public abstract GeneratorContext Downgrade();
    public abstract GeneratorContext CreateUsingData(DataAbstractClass data);
}

public abstract class DataAbstractClass
{

}