using Microsoft.Extensions.Logging;
using Ember.Scripting;
namespace Ember.Scripting;

public interface IGeneratorBaseInterface
{
    // public static abstract int IVersion { get; }
}

public abstract class GeneratorContext
{
    public abstract int Version { get; }
}