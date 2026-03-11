using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace FirstTests;

[TestClass]
public class DbHelperTests
{
    private ISccriptManagerDeleteAfter? facade;
    string? sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    private string? sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    private string? sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    private EmberMethods? em;

    [TestInitialize]
    public async Task Setup()
    {
        facade = EmberMethods.GetNewScriptManagerInstance();
        em = new EmberMethods(facade);

        await facade.ClearAllCaches();
        var existing = await facade.ListScripts(includeCaches: false);
        foreach (var s in existing)
        {
            await facade.DeleteScript(s.Id);
        }
    }

    [TestMethod]
    public async Task CreateAndInsertCompiledCacheTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        await facade.ClearScriptCache(id);
        var before = await facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, before.CompiledCaches.Count);


        await facade.CompileScript(id);

        var after = await facade.GetScript(id, includeCaches: true);
        var getCache = await facade.GetCompiledCache(id, EmberMethods.GetEmberApiVersion());
        Assert.IsTrue(after.CompiledCaches.Count == 1);
        Assert.IsTrue(getCache != null && (getCache.ApiVersion == EmberMethods.GetEmberApiVersion()));
        Assert.IsTrue(after.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));

        var e = await Assert.ThrowsExceptionAsync<Ember.Scripting.DbHelperException>(async () =>
         {
             await facade.CompileScript(id, EmberMethods.GetEmberApiVersion());
             var after3 = await facade.GetScript(id, includeCaches: true);
             Assert.IsTrue(after3.CompiledCaches.Count == 1);
             Assert.IsTrue(after3.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));
         });
        Console.WriteLine(e.ToString());

        // Assert.IsFalse(true);
        int oldV = 3;
        await facade.CompileScript(id, oldV);

        var after2 = await facade.GetScript(id, includeCaches: true);
        ScriptCompiledCache cache = await facade.GetCompiledCache(id, oldV);
        Assert.IsTrue(after2.CompiledCaches.Count == 2);
        Console.WriteLine("Api Version = " + cache.ApiVersion);
        Assert.IsTrue(cache != null && (cache.ApiVersion == oldV));
        Assert.IsTrue(after2.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));
    }

    [TestMethod]
    public async Task IsDuplicateTestAndListScriptsFilterTest()
    {
        Guid id1 = await facade!.CreateScript(sourceCodePedia!);
        CustomerScript script1 = await facade.GetScript(id1);

        Guid id2 = await facade!.CreateScript(sourceCodePedia!);
        CustomerScript script2 = await facade.GetScript(id2);

        Assert.IsTrue(script1.Equals(script2));
        var e = await Assert.ThrowsExceptionAsync<Ember.Scripting.DbHelperException>(async () =>
        {
            Guid id3 = await facade!.CreateScript(sourceCodePedia!, checkForDuplicates: true);
        });

        var allScripts = await facade.ListScripts();
        Console.WriteLine("Count of allScrips= " + allScripts.Count());
        Assert.IsTrue(allScripts.Count() == 2);

        CustomerScriptFilter filter = new CustomerScriptFilter(scriptName: script1.ScriptName, sourceCode: script1.SourceCode);
        var allScripts2 = await facade.ListScripts(filter);
        Console.WriteLine("Count of allScrips= " + allScripts2.Count());
        Assert.IsTrue(allScripts2.Count() == 2);

        await facade!.CreateScript(sourceCodeActionV1!);
        await facade!.CreateScript(sourceCodeActionV3!);
        await facade!.CreateScript(sourceCodeActionV3!);

        await facade!.CreateScript(sourceCodePedia!);
        await facade!.CreateScript(sourceCodePedia!);
        await facade!.CreateScript(sourceCodePedia!);
        await facade!.CreateScript(sourceCodePedia!);
        var allScripts3 = await facade.ListScripts(filter);
        Console.WriteLine("Count of allScrips= " + allScripts3.Count());
        Assert.IsTrue(allScripts3.Count() == 6);
    }

    [TestMethod]
    public void ExceptionHelperTestsAsync()
    {
        try
        {
            try
            {
                try
                {

                    try
                    {
                        try
                        {
                            throw new NoFileWithThisClassNameFoundException("random message0");
                        }
                        catch (Exception e)
                        {

                            throw new CompilationFailedException("random message1", e);
                        }
                    }
                    catch (Exception e)
                    {

                        throw new ScriptExecutionException("random message2", e);
                    }
                }
                catch (Exception e)
                {

                    throw new GetScriptPathFromFolderException("random message3", e);
                }
            }
            catch (Exception e)
            {

                throw new CreateStringFromCsFileException("random message4", e);
            }
        }
        catch (Exception e)
        {

            int baseExceptionIndex = ExceptionHelper.GetBaseExceptionIndex(e);
            Assert.IsTrue(baseExceptionIndex == 4);

            Exception baseException = e.GetBaseException();
            Assert.IsInstanceOfType(baseException, typeof(NoFileWithThisClassNameFoundException));
            // Assert.IsTrue(e.)

            Exception exAtIndex0 = ExceptionHelper.GetExceptionFromChain(e, 0);
            Console.WriteLine(exAtIndex0.GetType().Name);
            Assert.IsInstanceOfType(exAtIndex0, typeof(CreateStringFromCsFileException));
            Assert.IsTrue(exAtIndex0.Message == "random message4");

            Exception exAtIndex1 = ExceptionHelper.GetExceptionFromChain(e, 1);
            Console.WriteLine(exAtIndex1.GetType().Name);
            Assert.IsInstanceOfType(exAtIndex1, typeof(GetScriptPathFromFolderException));

            Exception exAtIndex2 = ExceptionHelper.GetExceptionFromChain(e, 2);
            Console.WriteLine(exAtIndex2.GetType().Name);
            Assert.IsInstanceOfType(exAtIndex2, typeof(ScriptExecutionException));

            Exception exAtIndex3 = ExceptionHelper.GetExceptionFromChain(e, 3);
            Console.WriteLine(exAtIndex3.GetType().Name);
            Assert.IsInstanceOfType(exAtIndex3, typeof(CompilationFailedException));
            Assert.IsTrue(exAtIndex3.Message == "random message1");

            Exception exAtIndex4 = ExceptionHelper.GetExceptionFromChain(e, 4);
            Console.WriteLine(exAtIndex4.GetType().Name);
            Assert.IsInstanceOfType(exAtIndex4, typeof(NoFileWithThisClassNameFoundException));


            Assert.ThrowsException<Exception>(() =>
      {
          Exception exAtIndex5 = ExceptionHelper.GetExceptionFromChain(e, 5);
          Console.WriteLine(exAtIndex5.GetType().Name);
          // Assert.IsInstanceOfType(exAtIndex5, typeof(CreateStringFromCsFileException));
      });

            Assert.ThrowsException<Exception>(() =>
   {
       Exception exAtIndex6 = ExceptionHelper.GetExceptionFromChain(e, 6);
       Console.WriteLine(exAtIndex6.GetType().Name);
       //    Assert.IsInstanceOfType(exAtIndex6, typeof(CreateStringFromCsFileException));
   });

            Console.WriteLine("First tests ran.");

            Exception exAtNegIndex0 = ExceptionHelper.GetExceptionFromChainReversed(e, 0);
            Console.WriteLine(exAtNegIndex0.GetType().Name);
            Assert.IsInstanceOfType(exAtNegIndex0, typeof(NoFileWithThisClassNameFoundException));

            Exception exAtNegIndex1 = ExceptionHelper.GetExceptionFromChainReversed(e, 1);
            Console.WriteLine(exAtNegIndex1.GetType().Name);
            Assert.IsInstanceOfType(exAtNegIndex1, typeof(CompilationFailedException));

            Exception exAtNegIndex2 = ExceptionHelper.GetExceptionFromChainReversed(e, 2);
            Console.WriteLine(exAtNegIndex2.GetType().Name);
            Assert.IsInstanceOfType(exAtNegIndex2, typeof(ScriptExecutionException));

            Exception exAtNegIndex3 = ExceptionHelper.GetExceptionFromChainReversed(e, 3);
            Console.WriteLine(exAtNegIndex3.GetType().Name);
            Assert.IsInstanceOfType(exAtNegIndex3, typeof(GetScriptPathFromFolderException));

            Exception exAtNegIndex4 = ExceptionHelper.GetExceptionFromChainReversed(e, 4);
            Console.WriteLine(exAtNegIndex4.GetType().Name);
            Assert.IsInstanceOfType(exAtNegIndex4, typeof(CreateStringFromCsFileException));


            Assert.ThrowsException<Exception>(() =>
              {
                  Exception exAtNegIndex5 = ExceptionHelper.GetExceptionFromChainReversed(e, 5);
                  Console.WriteLine(exAtNegIndex5.GetType().Name);
                  Assert.IsInstanceOfType(exAtNegIndex5, typeof(CreateStringFromCsFileException));
              });
            Assert.ThrowsException<Exception>(() =>
              {
                  Exception exAtNegIndex6 = ExceptionHelper.GetExceptionFromChainReversed(e, 6);
                  Console.WriteLine(exAtNegIndex6.GetType().Name);
                  Assert.IsInstanceOfType(exAtNegIndex6, typeof(CreateStringFromCsFileException));
              });

            // Assert.IsTrue(false);
        }
    }


}
