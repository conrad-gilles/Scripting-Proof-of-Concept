using System;
using Serilog;
using Ember.Scripting;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.Grafana.Loki;
// using BlazorUi.

public class LoggerForScripting
{
    private Serilog.Core.Logger? _serilogLogger = null;
    private ILoggerFactory? _factory = null;

    //  public BlazorSink MemorySink { get; } = new BlazorSink();

    public LoggerForScripting()
    {

    }
    public Serilog.Core.Logger SetUpAndGetSeriLogger()
    {
        Serilog.Core.Logger? serilogLoggerSet = new LoggerConfiguration()
            .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 5).WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
        // .WriteTo.Seq("http://localhost:5341", bufferBaseFilename: "logs/seq-offline-buffer")
        // .WriteTo.Sink(MemorySink)   //for seeing it in blazor webapp
        .WriteTo.GrafanaLoki(
        "http://localhost:3100",
        labels: new[] { new LokiLabel { Key = "app", Value = "ember-scripting-sandbox" } })
        .CreateLogger();
        return serilogLoggerSet;
    }

    // public Microsoft.Extensions.Logging.Logger<ScriptManagerFacade> GetMicrosoftLogger()
    public Microsoft.Extensions.Logging.Logger<T> GetMicrosoftLogger<T>()
    {
        if (_serilogLogger == null)
        {
            _serilogLogger = SetUpAndGetSeriLogger();
            Log.Logger = _serilogLogger;
        }

        if (_factory == null)
        {
            _factory = new SerilogLoggerFactory(_serilogLogger);
        }

        // using var factory = new SerilogLoggerFactory(serilogLogger);

        Microsoft.Extensions.Logging.Logger<T>? microsoftLogger =
              new Microsoft.Extensions.Logging.Logger<T>(_factory);
        return microsoftLogger;
    }
    public void Dispose()
    {
        _factory?.Dispose();
        _serilogLogger?.Dispose();
    }
    // Source - https://stackoverflow.com/a/68559937
    // Posted by mihails.kuzmins, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-02-26, License - CC BY-SA 4.0
}
