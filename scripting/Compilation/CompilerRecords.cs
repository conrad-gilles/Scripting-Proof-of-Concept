using Microsoft.CodeAnalysis;

namespace Ember.Scripting;

public record ValidationRecord
{
    public required string ClassName { get; init; }
    // public required ScriptTypes BaseTypeName { get; init; }
    public required Type ScriptType { get; init; }

    public required int Version { get; init; }
    public required int? ExecutionTime { get; init; }

    public List<MethodRecord>? methods { get; init; }

    public string BaseTypeAsString()
    {
        string sScriptType;
        if (ScriptType == typeof(IGeneratorActionScript))
        {
            sScriptType = nameof(IGeneratorActionScript);
        }
        else if (ScriptType == typeof(IGeneratorConditionScript))
        {
            sScriptType = nameof(IGeneratorConditionScript);
        }
        else
        {
            throw new CouldNotMatchBaseTypeInRecord("Could not match the BaseTypeNAme to a String this should never happen!");
        }

        return sScriptType;
    }
}

// public record GetBaseTypeReturn
// {
//     public required Microsoft.CodeAnalysis.INamedTypeSymbol? BaseType { get; init; }
//     public required Microsoft.CodeAnalysis.ITypeSymbol? MyClassSymbol { get; init; }
//     public required Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax? MyClass { get; init; }
//     public required SyntaxTree Tree { get; init; }
//     public required Microsoft.CodeAnalysis.SemanticModel? Model { get; init; }
// }

public record ScriptCompilationError(
    string Id,
    string Message,
    int Line,
    int Column,
    int EndLine,
    int EndColumn,
    bool IsError)
{
    public override string ToString()
    {
        return "An error occurred in Line: " + Line + ", Column: " + Column + " Message: " + Message;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ExecutionTime : Attribute
{
    public static int MinimumDuration = ((int)ExecutionTimeGroups.Short);
    public static int MaximumDuration = ((int)ExecutionTimeGroups.ExtraLong);
    public int MS { get; set; }

    public ExecutionTime(int ms)
    {
        MS = GetSafeDuration(ms);
    }
    public ExecutionTime(ExecutionTimeGroups group)
    {
        MS = GetSafeDuration(((int)group));
    }

    private static int GetSafeDuration(int time)
    {
        int result = time;

        if (time < MinimumDuration)
        {
            result = MinimumDuration;
        }
        if (time > MaximumDuration)
        {
            result = MaximumDuration;
        }
        if (result != MinimumDuration && result != MaximumDuration)
        {
            result = time;
        }
        return result;
    }

    public static int? GetDurationFromEnumString(string enumString)
    {
        ExecutionTimeGroups result;
        switch (enumString)
        {
            case nameof(ExecutionTimeGroups.Short):
                result = ExecutionTimeGroups.Short;
                break;
            case nameof(ExecutionTimeGroups.Medium):
                result = ExecutionTimeGroups.Medium;
                break;
            case nameof(ExecutionTimeGroups.Long):
                result = ExecutionTimeGroups.Long;
                break;
            case nameof(ExecutionTimeGroups.ExtraLong):
                result = ExecutionTimeGroups.ExtraLong;
                break;
            default:
                try
                {
                    return GetSafeDuration(Int32.Parse(enumString));
                }
                catch
                {
                    //Here debatable what should be done, as it is not as important one could just give the max or medium duration instead of throw an exception, Examples below:
                    // result = ExecutionTimeGroups.ExtraLong;
                    // result = ExecutionTimeGroups.Short;

                    throw new ExecutionTimeCouldNotBeAssigned();
                }

        }
        return GetSafeDuration(((int)result));
    }
}
public enum ExecutionTimeGroups
{
    Short = 100, Medium = 500, Long = 1000, ExtraLong = 5000
}

public record MethodRecord
{
    public required string Name { get; init; }
    public required string ReturnType { get; init; }

    public required List<ParameterRecord> Parameters { get; init; }

    public static bool IsTheSame(MethodRecord record1, MethodRecord record2)
    {
        if (record1.Name != record2.Name)
        {
            return false;
        }
        if (record1.ReturnType != record2.ReturnType)
        {
            return false;
        }
        if (record1.Parameters.Count() != record2.Parameters.Count())
        {
            return false;
        }
        // bool isInside =false;
        foreach (var param1 in record1.Parameters)
        {
            bool isInside = false;
            foreach (var param2 in record2.Parameters)
            {

                if (ParameterRecord.IsTheSame(param1, param2))
                {
                    isInside = true;
                }
            }
            if (isInside == false)
            {
                return false;
            }
        }
        return true;
    }
    public override string ToString()
    {
        return nameof(MethodRecord) + ": Name: " + Name + ", ReturnType: " + ReturnType + ", Parameters: " + Parameters;
    }
}

public record ParameterRecord
{
    public required string Name { get; init; }
    public required string ReturnType { get; init; }

    public static bool IsTheSame(ParameterRecord param1, ParameterRecord param2)
    {
        if (param1.Name != param2.Name)
        {
            return false;
        }
        if (param1.ReturnType != param2.ReturnType)
        {
            return false;
        }
        return true;
    }
    public override string ToString()
    {
        return nameof(ParameterRecord) + ": Name: " + Name + ", ReturnType: " + ReturnType;
    }
}