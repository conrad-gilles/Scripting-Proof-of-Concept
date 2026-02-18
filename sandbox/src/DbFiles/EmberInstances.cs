using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ember_instances")] //might be unnessesary todo
public class EmberInstance
{
    [Key]
    [Column("instance_id")]
    public Guid InstanceId { get; set; }

    [Column("instance_name")]
    public string? InstanceName { get; set; }

    [Column("ember_version")]
    public string EmberVersion { get; set; }

    [Column("sdk_version")]
    public int SdkVersion { get; set; }

    [Column("last_heartbeat")]
    public DateTime LastHeartbeat { get; set; }

    [Column("hostname")]
    public string? Hostname { get; set; }

    public override string ToString()
    {
        string str = "Instance id: " + InstanceId.ToString();
        str = str.Replace("@", Environment.NewLine);
        return str;
    }
}

