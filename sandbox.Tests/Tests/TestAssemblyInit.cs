#pragma warning disable CS0436

using Ember.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Disable cross-class parallelism — tests run sequentially across all classes
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]

namespace sandbox.Tests;

public static class TestConfig
{
    public static bool DuplicatesAllowed { get; set; } = false;
}


[TestClass]
public static class TestAssemblyInit
{
    [AssemblyInitialize]
    public static async Task Init(TestContext _)
    {
        //this block is ecessary at least once in the code everytime you modify the init.sql else the db wont be initialized somehow
        using (var db = new ScriptDbContext())
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        IScriptManagerBaseExtended ScriptManager = TestHelper.InitScriptManager();
        await ScriptManager.DeleteAllData();
    }
}
