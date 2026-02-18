using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
public class ScriptExecutor
{
    public byte[] compiledScript;
    public GeneratorContext genContext;
    public ScriptExecutor(byte[] pCompiledScript, GeneratorContext pGenContext)
    {
        compiledScript = pCompiledScript;
        genContext = pGenContext;
    }

    public T RunScriptExecution<T>()
    // public void RunScriptExecution()
    {
        try
        {
            Assembly assembly = Assembly.Load(compiledScript);

            Type[] unfilteredTypeArray = assembly.GetTypes();   //even though there can be only one class defined in the script file, the compiler adds classes making the array.lenght over 1 which is unsafe so it is better to filer based on our predefined classes for scripts
            List<Type> typeArrayList = [];
            for (int i = 0; i < unfilteredTypeArray.Length; i++)
            {
                if (typeof(IGeneratorActionScript).IsAssignableFrom(unfilteredTypeArray[i]))
                {
                    typeArrayList.Add(unfilteredTypeArray[i]);
                }
                if (typeof(IGeneratorConditionScript).IsAssignableFrom(unfilteredTypeArray[i]))
                {
                    typeArrayList.Add(unfilteredTypeArray[i]);
                }
            }
            Type[] typeArray = typeArrayList.ToArray();

            if (typeArray.Length == 0)
            {
                throw new NoClassFoundInScriptFileException();
            }
            else if (typeArray.Length > 1)
            {
                Console.WriteLine("more than one class found in script");
                throw new MoreThanOneClassFoundInScriptException();   //to implement more than one name you would need to pass name of class into this class
            }

            Type type = typeArray[0];

            object scriptInstance = Activator.CreateInstance(type);     //if null here probably typo in file name somewhere, like pedriatic instead of pediatic :(


            // if (typeof(IGeneratorConditionScript).IsAssignableFrom(type))
            if (typeof(IGeneratorConditionScript).IsAssignableFrom(type))    //checks if type implements the generator specific interface  //check if runs
            {
                var result = runConditionScript(type, scriptInstance);
                return (T)(object)result;
            }
            else if (typeof(IGeneratorActionScript).IsAssignableFrom(type))
            {
                var result = runActionScript(type, scriptInstance);
                return (T)(object)result;
            }
            else
            {
                Console.WriteLine("Could not run your script because it is neither a ActionScript nor a ConditionScript.");
                throw new ScriptExecutionException();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when trying to execute your code, here are some details:");
            // Console.WriteLine(e.Message);
            // Console.WriteLine(e.StackTrace);
            Console.WriteLine(e.ToString());
            throw new ScriptExecutionException();
        }

    }
    public bool runConditionScript(Type type, object scriptInstance)
    {
        try
        {
            MethodInfo method = type.GetMethod("EvaluateAsync");
            // 2. Invoke method on the instance, passing context as a parameter
            // Note: EvaluateAsync returns a Task<bool>, so 'result' will be a Task
            var resultTask = (Task)method.Invoke(scriptInstance, new object[] { genContext });

            // 3. Wait for the task to complete and get the result
            resultTask.Wait();

            // access the "Result" property of the Task
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultValue = resultProperty.GetValue(resultTask);

            Console.WriteLine($"Result: {resultValue}");    //todo probably unnessesary but good for debugging
            return (bool)resultValue; // todo very important error handling
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new ConditionScriptExecutionException();
        }

    }
    public ActionResult runActionScript(Type type, object scriptInstance)
    {
        try
        {
            MethodInfo method = type.GetMethod("ExecuteAsync"); //todo
                                                                // 2. Invoke method on the instance, passing context as a parameter
                                                                // Note: EvaluateAsync returns a Task<bool>, so 'result' will be a Task
                                                                //todo implement check for old versions of GeneratorContext class because GeneratorContextV2 might not be compatible


            var resultTask = (Task)method.Invoke(scriptInstance, new object[] { genContext });

            // 3. Wait for the task to complete and get the result
            resultTask.Wait();

            // access the "Result" property of the Task
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultValue = resultProperty.GetValue(resultTask);

            Console.WriteLine($"Result in 86 sExecuter: {resultValue}");    //todo probably unnessesary but good for debugging
            return (ActionResult)resultValue; // todo very important error handling
                                              // return resultValue; // todo very important error handling
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine("You might have passed the wrong GeneratorContext class, ex V1 instead of V2");
            throw new ActionScriptExecutionException();
        }

    }

}