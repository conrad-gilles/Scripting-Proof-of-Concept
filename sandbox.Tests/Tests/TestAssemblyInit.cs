using Microsoft.VisualStudio.TestTools.UnitTesting;

// Disable cross-class parallelism — tests run sequentially across all classes
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]

// namespace FirstTests;

// [TestClass]
// public static class TestAssemblyInit
// {
//     [AssemblyInitialize]
//     public static async Task Init(TestContext _)
//     {
//         var db = new DbHelper();
//         await db.EnsureDeletedCreated(); // Create DB exactly once
//     }

//     [AssemblyCleanup]
//     public static async Task Cleanup()
//     {
//         // Optional: leave DB around for post-run inspection,
//         // or drop it here if you prefer a clean slate.
//     }
// }
