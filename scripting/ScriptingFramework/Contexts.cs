using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
using TypeInfo = Ember.Scripting.Versioning.TypeInfo;
namespace Ember.Scripting.ScriptingFramework;

public abstract class Context : Downgradeable, Data
{
    public abstract Context Downgrade();
    public abstract Context CreateUsingData(DataAbstractClass data);
}

public interface IContext
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