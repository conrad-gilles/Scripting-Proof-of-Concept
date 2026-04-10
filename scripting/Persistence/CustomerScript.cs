using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace Ember.Scripting;

// [Microsoft.EntityFrameworkCore.PrimaryKey(nameof(ScriptName), nameof(ScriptType))]
[Table("customer_scripts")]
public class CustomerScript
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("script_name")]
    public string? ScriptName { get; set; }

    [Column("script_type")]
    public string? ScriptType { get; set; }

    [Column("source_code")]
    public string? SourceCode { get; set; }

    [Column("min_api_version")]
    public int MinApiVersion { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

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
        Type sType;
        switch (ScriptType)
        {
            case nameof(IActionScript):
                sType = typeof(IActionScript);
                break;
            case nameof(IConditionScript):
                sType = typeof(IConditionScript);
                break;
            default:
                throw new CouldNotAssignBaseTypeException(message: "Could not assign baseTypeName");
        }
        return sType;
    }
    public override string ToString()
    {
        string str =
        "ID: " + Id.ToString() +
        "@" + "Script Name: " + ScriptName + "@" + "Script Type: "
        + ScriptType + "@"
        + "Source Code: " + SourceCode
        + "@" + "MinApiVersion: "
        + MinApiVersion.ToString() + "@" + "Created at: " + CreatedAt.ToString() + "@"
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
        + MinApiVersion.ToString() + ", Created at: " + CreatedAt.ToString() + ", Modified at: "
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

