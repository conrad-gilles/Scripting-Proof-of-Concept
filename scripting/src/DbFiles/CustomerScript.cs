using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ember.Scripting;

[Table("customer_scripts")] //might be unnessesary todo //added ? to values that can be null
public class CustomerScript
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("script_name")]
    public string? ScriptName { get; set; }

    [Column("script_type")]
    public string? ScriptType { get; set; } // "GeneratorCondition" or "GeneratorAction"

    [Column("source_code")]
    public string SourceCode { get; set; }

    [Column("min_api_version")]
    public int MinApiVersion { get; set; }  //maybe do ?

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    // Navigation property for compiled caches
    // public ICollection<ScriptCompiledCache> CompiledCaches { get; set; }
    public List<ScriptCompiledCache> CompiledCaches { get; set; }       //todo redo into collection because no duplicates etc

    public CustomerScript()
    {
        CompiledCaches = new List<ScriptCompiledCache>();
    }

    public override string ToString()
    {
        string str = "ID: " + Id.ToString() + "@" + "Script Name: " + ScriptName + "@" + "Script Type: "
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
}

