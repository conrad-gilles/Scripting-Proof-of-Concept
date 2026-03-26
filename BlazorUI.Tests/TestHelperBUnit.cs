using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunit;
using BlazorUI.Components.Pages;
using Ember.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EFModeling.EntityProperties.FluentAPI.Required;
using Moq;
using Serilog;
using Sandbox;

namespace BlazorUI.Tests;

public static class TestHelperBUnit
{
    public static void SetupRealEnvironmentBUnit(IServiceCollection Services)
    {
        Services.AddDbContextFactory<EFModeling.EntityProperties.FluentAPI.Required.ScriptDbContext>();

        Services.AddLogging(builder => builder.AddSerilog(dispose: true));
        Services.AddSingleton<BlazorUI.Services.ConsoleService>();
        Services.AddSingleton<IUserSession, SandBoxUserSession>();

        ScriptingServiceCollectionExtensions.AddEmberScripting(
            Services,
            EmberMethods.GetReferences(),
            EmberMethods.GetEmberApiVersion()
        );

    }
}
