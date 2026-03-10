using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
namespace Ember.Scripting;

public interface IGeneratorBaseInterface
{
    int IVersion { get; }
}

// [TypeLibVersion (4)]
public abstract class GeneratorContext
{
    public abstract int Version { get; }

    public abstract GeneratorContext Downgrade();
}