public class RandomMethods
{
    public static async Task<Dictionary<int, Guid>> ListAllCompiledFromDB()
    {
        var db = new DbHelper();
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

    public static async Task<Dictionary<int, Guid>> ListAllStoredSourceCodes()
    {
        var db = new DbHelper();
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
            Console.WriteLine(str);
        }
        if (sourceDict.Count() == 0)
        {
            Console.WriteLine("Script Source Code repo is Empty!");
        }
        return sourceDict;
    }

    public static async Task CompileAllScriptsInFolderAndSaveToDB(string folderPath, string userName, int currentApiVersion)
    {

        string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);  //todo check for infinite loop  https://msdn.microsoft.com/en-us/library/ms143316(v=vs.110).aspx
        for (int i = 0; i < files.Length; i++)
        {

            try
            {
                var db = new DbHelper();
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



    public static async Task EditScriptInSwitch(Guid id, string userName, int currentApiVersion)
    {
        var db = new DbHelper();
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
    public static async Task GetSourceCodeInSwitch()
    {
        Console.WriteLine("Enter the the script you want to read: ");
        string userInput = Console.ReadLine();
        var db = new DbHelper();
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


}