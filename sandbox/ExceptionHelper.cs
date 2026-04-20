using System;
using System.Collections.Generic;

namespace Sandbox;


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

    public static string PrintExceptionListToConsole(Exception ex)
    {
        string returnedString = "";
        List<Exception> ls = GetExceptionList(ex);
        for (int i = 0; i < ls.Count(); i++)
        {
            Console.WriteLine("Exception " + i + " :" + ls[i].GetType().Name);
            returnedString = returnedString + "Exception " + i + " :" + ls[i].Message;
        }
        return returnedString;
    }
}