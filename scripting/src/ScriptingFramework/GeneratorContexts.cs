using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
namespace Ember.Scripting;

[MetaDataIGeneratorIntrfc(version: 0, type: TypeInfo.AbstractBaseInSF)]
public interface IGeneratorBaseInterface
{
    int IVersion { get; }
}

[MetaDataGeneratorClass(version: 0, type: TypeInfo.AbstractBaseInSF)]
public abstract class GeneratorContext
{
    public abstract int Version { get; }

    public abstract GeneratorContext Downgrade();
}