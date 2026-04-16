using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
using TypeInfo = Ember.Scripting.Versioning.TypeInfo;
namespace Ember.Scripting.ScriptingFramework;

public abstract class Context : Downgradeable
{
    public abstract Context Downgrade();
}

public interface IContext
{

}

public interface Downgradeable
{
    public Context Downgrade();
}