using System.Globalization;
using Ember.Scripting;
public class RandomMethods
{
    private readonly DbHelper db;

    public RandomMethods(DbHelper db2)
    {
        db = db2;
    }
    public async Task<Dictionary<int, Guid>> ListAllCompiledFromDB()
    {
        // var db = new DbHelper(UsefulMethods.GetReferences());
        List<ScriptCompiledCache> caches = await db.GetAllCompiledScriptCaches();

        Dictionary<int, Guid> cacheDict = new Dictionary<int, Guid>();
        for (int i = 0; i < caches.Count(); i++)
        {
            Console.WriteLine(i + 1 + ". " + caches[i].ToString());
            cacheDict.Add(i + 1, caches[i].ScriptId);
        }
        if (cacheDict.Count() == 0)
        {
            Console.WriteLine("Cache Dictionary is Empty!");
        }
        return cacheDict;
    }

    public async Task<Dictionary<int, Guid>> ListAllStoredSourceCodes(bool dontPrint = false)
    {
        // var db = new DbHelper(UsefulMethods.GetReferences());
        List<CustomerScript> sourceCodes = await db.GetAllCustomerScripts(includeCaches: true);

        Dictionary<int, Guid> sourceDict = new Dictionary<int, Guid>();
        for (int i = 0; i < sourceCodes.Count; i++)
        {
            // Console.WriteLine(i + 1 + ". " + sourceCodes[i].ToString());
            // Console.WriteLine(i + 1 + ". " + sourceCodes[i].ToStringShorter());
            string str = (i + 1).ToString() + ". Name: " + sourceCodes[i].ScriptName + ", Created by: " + sourceCodes[i].CreatedBy
            + ", Created at: " + sourceCodes[i].CreatedAt + ", MinApiVersion: " + sourceCodes[i].MinApiVersion + ", Modified at: " + sourceCodes[i].ModifiedAt
            + ", Compiled count [" + sourceCodes[i].CompiledCaches.Count() + "]";
            sourceDict.Add(i + 1, sourceCodes[i].Id);
            if (dontPrint == false) { Console.WriteLine(str); }
        }
        if (sourceDict.Count() == 0)
        {
            Console.WriteLine("Script Source Code repo is Empty!");
        }
        return sourceDict;
    }

    public async Task CompileAllScriptsInFolderAndSaveToDB(string folderPath, string userName, int currentApiVersion)
    {

        string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);  //todo check for infinite loop  https://msdn.microsoft.com/en-us/library/ms143316(v=vs.110).aspx
        for (int i = 0; i < files.Length; i++)
        {

            try
            {
                // var db = new DbHelper(UsefulMethods.GetReferences());
                string scriptString = UsefulMethods.CreateStringFromCsFile(files[i]);
                Guid id = Guid.NewGuid();
                CustomerScript randomTestScript2 = await db.CreateAndInsertCustomerScript(scriptString, id, userName);
                // Console.WriteLine(randomTestScript2.ScriptName + "Added script N" + i + ". to both tables.");

            }
            catch (CompilationFailedException e)    //so if one fails not all get cancelled
            {
                Console.WriteLine(e.ToString());
            }
            catch (ValidationBeforeCompilationException e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }



    public async Task EditScriptInSwitch(Guid id, string userName, int currentApiVersion)
    {
        // var db = new DbHelper(UsefulMethods.GetReferences());
        var customerScript = await db.GetCustomerScript(id);
        var creationDate = customerScript.CreatedAt;
        Console.WriteLine("Here is the old version of the script source code:");
        Console.WriteLine(customerScript.SourceCode);

        Console.WriteLine("Copy paste your new version file path now:");
        string userInput2 = Console.ReadLine();

        string str = UsefulMethods.CreateStringFromCsFile(userInput2);
        await db.DeleteCustomerScript(id);

        //In reality it would be better like this but doesnt work because cant paste too much in console:
        // Console.WriteLine("Copy paste your new version now:");
        // string userInput2 = Console.ReadLine();

        await db.CreateAndInsertCustomerScript(str, id, userName, createdAt: (DateTime)creationDate); //todo unsafe af


    }
    public async Task GetSourceCodeInSwitch()
    {
        Console.WriteLine("Enter the the script you want to read: ");
        string userInput = Console.ReadLine();
        // var db = new DbHelper(UsefulMethods.GetReferences());
        if (userInput == null || userInput == "")
        {
            // string userInput = Console.ReadLine();

            List<CustomerScript> customerScripts = await db.GetAllCustomerScripts();
            for (int i = 0; i < customerScripts.Count; i++)
            {
                Console.WriteLine(customerScripts[i]);
            }
        }
        else
        {
            var listAllCompiledFromDB = await ListAllCompiledFromDB();
            Guid idEdit = listAllCompiledFromDB[Int32.Parse(userInput)];
            CustomerScript scr = await db.GetCustomerScript(idEdit);
            Console.WriteLine(scr.SourceCode);
        }

    }
    public static ActionResultV3NoInheritance UpgradeActionResult(object resultValue)
    {
        // var facade = new ScriptManagerFacade(UsefulMethods.GetReferences());
        // var newestVersion = await facade.GetRecentApiVersion();
        object finalActionResult = resultValue;
        int loopBreaker = 0;   //I am assuming not 1000 versions will be written                // will probably fail in real application todo fix mabe with reflection i heard?
        while (finalActionResult is not ActionResultV3NoInheritance && loopBreaker < 1000)    //could fail if loaded from diffrent assembly should probably replace the is statements with something like get type.name
        {
            loopBreaker++;
            // if (finalActionResult is ActionResultV2 v2Script)
            if (finalActionResult.GetType().Name == "ActionResultV2")
            {
                try
                {
                    ActionResultV2 v2Script2 = (ActionResultV2)finalActionResult;
                    finalActionResult = ActionResultV3NoInheritance.UpgradeV2(v2Script2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            // else if (finalActionResult is ActionResult v1Script)
            else if (finalActionResult.GetType().Name == "ActionResult")
            {
                try
                {
                    ActionResult v1Script2 = (ActionResult)finalActionResult;
                    List<string> loggedActions = [];
                    finalActionResult = ActionResultV2.UpgradeV1(v1Script2, loggedActions);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        // if (finalActionResult is ActionResultV3NoInheritance v3Script)
        if (finalActionResult.GetType().Name == "ActionResultV3NoInheritance")
        {
            ActionResultV3NoInheritance v3Script2 = (ActionResultV3NoInheritance)finalActionResult;
            return (ActionResultV3NoInheritance)v3Script2;
        }
        else
        {
            throw new Exception(message: "UpgradeActionResult in ScriptExecutor failed.");
        }
    }
    public async Task<Guid> GetIdInConsoleAsync(bool fromSrc = false)
    {
        Dictionary<int, Guid> cacheDict = [];
        if (fromSrc == false)
        { cacheDict = await ListAllCompiledFromDB(); }

        else { cacheDict = await ListAllStoredSourceCodes(); }

        Dictionary<int, Guid> sourceDict = [];
        Console.WriteLine("Enter the number of the script ");
        string userInputForEdit = Console.ReadLine();
        Guid id = cacheDict[Int32.Parse(userInputForEdit)];
        return id;
    }

}
