using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Microsoft.EntityFrameworkCore.PrimaryKey(nameof(ScriptId), nameof(ApiVersion))]    //composite key because one script id can link multiple compiled
[Table("script_compiled_cache")]
public class ScriptCompiledCache
{
    // [Key]   
    [ForeignKey(nameof(CustomerScript))]
    [Column("script_id")]
    public Guid ScriptId { get; set; }

    [Column("api_version")]
    public int ApiVersion { get; set; }

    [Column("assembly_bytes")]
    public byte[]? AssemblyBytes { get; set; }  //i changed this to allow null to store if a script failed for later recompilation maybe?

    [Column("compilation_date")]
    public DateTime CompilationDate { get; set; }

    [Column("compilation_success")]
    public bool CompilationSuccess { get; set; }

    [Column("copilation_errors")]
    public string? CompilationErrors { get; set; }

    // Navigation property back to the script
    public CustomerScript CustomerScript { get; set; }

    public override string ToString()
    {
        string str = "Compiled Script: Script Name: " + CustomerScript.ScriptName + "Script ID: " + ScriptId.ToString() +
        // " Created: "+CompilationDate
        " Script Created: " + CustomerScript.CreatedAt
        ;
        // str = str.Replace("@", Environment.NewLine);
        return str;
    }
    public string ToStringBetter()
    {
        string str = "Compiled Script: Script Name: " + CustomerScript.ScriptName + " Compiled for V" + ApiVersion + " Created: " + CompilationDate +
        " Script Created: " + CustomerScript.CreatedAt;
        return str;
    }
}
