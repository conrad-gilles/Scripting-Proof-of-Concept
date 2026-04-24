using System.ComponentModel.DataAnnotations.Schema;

namespace Ember.Scripting.Persistence;

// [Microsoft.EntityFrameworkCore.PrimaryKey(nameof(ScriptName), nameof(ScriptType))]
[Table("customer_scripts")]
public class CustomerScript
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("script_name")]
    public string ScriptName { get; set; }

    [Column("script_type")]
    public string ScriptType { get; set; }

    [Column("source_code")]
    public string? SourceCode { get; set; }

    [Column("min_api_version")]
    public int ScriptApiVersion { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("execution_time")]
    public int? ExecutionTimeInMS { get; set; }

    public List<CompiledScript> CompiledCaches { get; set; }

    public CustomerScript()
    {
        CompiledCaches = new List<CompiledScript>();
    }

    public Type GetScriptType()
    {
        HashSet<Type> rootScriptTypes = GetRootTypes();

        foreach (Type root in rootScriptTypes)
        {
            if (root.FullName!.Contains(ScriptType))
            {
                return root;
            }
        }
        throw new ScriptTypeNotSetException("ScriptType not set baseType.ToDisplayString(): " + ScriptType);
    }
    public static HashSet<Type> GetRootTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.IsInterface &&
                typeof(IScriptType).IsAssignableFrom(t) &&
                t != typeof(IScriptType) &&
                !typeof(IScriptVersion).IsAssignableFrom(t))
            .ToHashSet();
    }
    public static Type GetScriptType(string scriptTypeString)
    {
        if (scriptTypeString.Contains("<"))
        {
            scriptTypeString = scriptTypeString.Split('<')[0];
        }

        HashSet<Type> versionedScriptInterfaces = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.IsInterface &&
                typeof(IScriptVersion).IsAssignableFrom(t) &&
                t != typeof(IScriptVersion))
            .ToHashSet();

        HashSet<Type> rootScriptTypes = GetRootTypes();

        Dictionary<Type, Type> mappedTypes = [];
        foreach (Type versioned in versionedScriptInterfaces)
        {
            foreach (Type root in rootScriptTypes)
            {
                if (root.IsAssignableFrom(versioned))
                {
                    mappedTypes.Add(versioned, root);
                }
            }
            if (mappedTypes[versioned] == null)
            {
                throw new Exception();
            }
        }
        foreach (Type versioned in versionedScriptInterfaces)
        {
            Console.WriteLine(versioned.FullName);
            if (versioned.FullName!.Contains(scriptTypeString))
            {
                return mappedTypes[versioned];
            }
        }
        throw new ScriptTypeNotSetException("ScriptType not set baseType.ToDisplayString(): " + scriptTypeString);
    }
    public override string ToString()
    {
        string str =
        "ID: " + Id.ToString() +
        "@" + "Script Name: " + ScriptName + "@" + "Script Type: "
        + ScriptType + "@"
        + "Source Code: " + SourceCode
        + "@" + "MinApiVersion: "
        + ScriptApiVersion.ToString() + "@" + "Created at: " + CreatedAt.ToString() + "@"
        + "Modified at: " + ModifiedAt.ToString() + "@" + "Created by " + CreatedBy;

        string compiledCachesString = "@ Compiled Caches: { @";
        for (int i = 0; i < CompiledCaches.Count(); i++)
        {
            compiledCachesString = compiledCachesString + "@" + CompiledCaches[i].ToString();
        }
        compiledCachesString = compiledCachesString + "@ }";
        str = str + compiledCachesString;
        str = str.Replace("@", Environment.NewLine);
        return str;
    }
    public string ToStringShorter()
    {
        string str = "Script Name: " + ScriptName + ", Script Type: " + ScriptType + ", MinApiVersion: "
        + ScriptApiVersion.ToString() + ", Created at: " + CreatedAt.ToString() + ", Modified at: "
        + ModifiedAt.ToString() + ", Created by " + CreatedBy;

        string compiledCachesString = "@ Compiled Caches: { @";
        for (int i = 0; i < CompiledCaches.Count(); i++)
        {
            compiledCachesString = compiledCachesString + "@" + CompiledCaches[i].ToStringBetter();
        }
        compiledCachesString = compiledCachesString + "@ }";
        str = str + compiledCachesString;
        str = str.Replace("@", Environment.NewLine);
        return str;
    }

    public bool Equals(CustomerScript script)
    {
        if (this.ScriptName == script.ScriptName
        && this.SourceCode == script.SourceCode)
        {
            return true;
        }
        return false;
    }
}