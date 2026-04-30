using System.Reflection;

namespace Ember.Scripting.Compilation;

public record ValidationRecord
{
    public required string ClassName { get; init; }
    public required Type ScriptType { get; init; }

    public required int Version { get; init; }
    public required int? ExecutionTime { get; init; }

    public List<MethodRecord>? methods { get; init; }
    public string ParentSymbol = "Default";

    public string BaseTypeAsString()
    {
        string sScriptType;
        sScriptType = ScriptType.Name;
        return sScriptType;
    }
}

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

    internal static int? GetDurationFromEnumString(string enumString)
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

    public static MethodRecord GetMethodRecord(IMethodSymbol methodSymbol)
    {
        string name = methodSymbol.Name;

        ITypeSymbol returnTypeSymbol = methodSymbol.ReturnType;
        if (returnTypeSymbol is INamedTypeSymbol namedReturnType && namedReturnType.IsGenericType)
        {
            returnTypeSymbol = namedReturnType.TypeArguments[0];
        }

        string returnType = returnTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (returnType.StartsWith("global::"))
        {
            returnType = returnType.Substring("global::".Length);
        }

        List<ParameterRecord> parameters = [];

        foreach (IParameterSymbol paramSymbol in methodSymbol.Parameters)
        {
            string paramName = paramSymbol.Name;
            ITypeSymbol typeSymbol = paramSymbol.Type;

            if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                typeSymbol = namedType.TypeArguments[0];
            }

            string paramType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (paramType.StartsWith("global::"))
            {
                paramType = paramType.Substring("global::".Length);
            }
            parameters.Add(new ParameterRecord
            {
                Name = paramName,
                ReturnType = paramType
            });

        }
        return new MethodRecord
        {
            Name = name,
            ReturnType = returnType,
            Parameters = parameters
        };
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
}