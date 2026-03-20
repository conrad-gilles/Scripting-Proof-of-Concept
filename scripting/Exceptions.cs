using System;
using System.Collections.Generic;

namespace Ember.Scripting;

public class NoFileWithThisClassNameFoundException : Exception
{
    public NoFileWithThisClassNameFoundException() : base() { }
    public NoFileWithThisClassNameFoundException(string message) : base(message) { }
    public NoFileWithThisClassNameFoundException(string message, Exception innerException) : base(message, innerException) { }
}





public class GetScriptPathFromFolderException : Exception
{
    public GetScriptPathFromFolderException() : base() { }
    public GetScriptPathFromFolderException(string message) : base(message) { }
    public GetScriptPathFromFolderException(string message, Exception innerException) : base(message, innerException) { }
}

public class CreateStringFromCsFileException : Exception
{
    public CreateStringFromCsFileException() : base() { }
    public CreateStringFromCsFileException(string message) : base(message) { }
    public CreateStringFromCsFileException(string message, Exception innerException) : base(message, innerException) { }
}









public class DbHelperException : Exception
{
    public DbHelperException() : base() { }
    public DbHelperException(string message) : base(message) { }
    public DbHelperException(string message, Exception innerException) : base(message, innerException) { }
}
public class FacadeException : Exception
{
    public FacadeException() : base() { }
    public FacadeException(string message) : base(message) { }
    public FacadeException(string message, Exception innerException) : base(message, innerException) { }
}

public class SaveScriptWithoutCompilingException : Exception
{
    public SaveScriptWithoutCompilingException() : base() { }
    public SaveScriptWithoutCompilingException(string message) : base(message) { }
    public SaveScriptWithoutCompilingException(string message, Exception innerException) : base(message, innerException) { }
}

public class ExceptionHelper    //for future to traverse exception chain
{
    public static Exception GetExceptionFromChain(Exception ex, int i)
    {
        Exception baseException = ex.GetBaseException();
        int indexInChain = 0;

        Exception innerEx = ex;
        while (innerEx.Equals(baseException) == false && indexInChain != i)
        {
            innerEx = innerEx.InnerException!;
            indexInChain++;
        }
        if (indexInChain < i)
        {
            throw new Exception(message: "Index is out of bounds of the Exception chain!");
        }
        return innerEx;
    }
    public static Exception GetExceptionFromChainReversed(Exception ex, int i)  //todo fix use index base lookup get rid of for loop
    {
        List<Exception> exceptions = GetExceptionList(ex);
        exceptions.Reverse();

        for (int j = 0; j < exceptions.Count(); j++)
        {
            if (j == i)
            {
                return exceptions[i];
            }
        }
        throw new Exception(message: "Could not find your exception in the List!");
    }
    public static int GetBaseExceptionIndex(Exception ex)
    {
        List<Exception> exceptions = GetExceptionList(ex);
        return exceptions.Count() - 1;
    }
    public static List<Exception> GetExceptionList(Exception ex)
    {
        List<Exception> exceptions = [];
        Exception baseException = ex.GetBaseException();

        while (ex.Equals(baseException) == false)
        {
            exceptions.Add(ex);
            ex = ex.InnerException!;
        }
        exceptions.Add(baseException);
        return exceptions;
    }

    public static void PrintExceptionListToConsole(Exception ex)
    {
        List<Exception> ls = GetExceptionList(ex);
        for (int i = 0; i < ls.Count(); i++)
        {
            Console.WriteLine("Exception " + i + " :" + ls[i].Message);
        }
    }
}