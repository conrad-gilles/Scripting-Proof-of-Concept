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

// [assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]

namespace BlazorUI.Tests;

[TestClass]
public class HelloWorldTest : BunitContext
{
    [TestInitialize]
    public void SetupRealEnvironment()
    {
        Services.AddDbContextFactory<EFModeling.EntityProperties.FluentAPI.Required.ScriptDbContext>();

        Services.AddLogging(builder => builder.AddSerilog(dispose: true));
        Services.AddSingleton<IUserSession, SandBoxUserSession>();

        ScriptingServiceCollectionExtensions.AddEmberScripting(
            Services,
            EmberMethods.GetReferences(),
            EmberMethods.GetEmberApiVersion()
        );
    }

    [TestMethod]
    public void HelloWorldComponentRendersCorrectly()
    {
        var cut = Render<Home>();
        var h3Elements = cut.FindAll("h3");

        // Verify the very first <h3> contains exactly "0"
        h3Elements[0].MarkupMatches("<h3>0</h3>");
    }

    [TestMethod]
    public void Home_TotalScripts_ShouldBeZero()
    {
        var cut = Render<Home>();

        // Assert: Find the <h3> that is a child of the element with id 'total-scripts-card'
        var totalScriptsH3 = cut.Find("#total-scripts-card h3");

        totalScriptsH3.MarkupMatches("<h3>0</h3>");
    }
}