using System;
using Serilog;

public class LoggerForScripting
{
    public LoggerForScripting()
    {

    }
    public void SetUpLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 5).WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341", bufferBaseFilename: "logs/seq-offline-buffer")

        .CreateLogger();
    }
}