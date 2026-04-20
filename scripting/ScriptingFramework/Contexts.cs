using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
using TypeInfo = Ember.Scripting.Versioning.TypeInfo;
namespace Ember.Scripting.ScriptingFramework;

public abstract class Context : IDowngradeableContext
{
    public abstract Context Downgrade();
}

public interface IContext
{

}

public interface IDowngradeableContext
{
    public Context Downgrade();
}