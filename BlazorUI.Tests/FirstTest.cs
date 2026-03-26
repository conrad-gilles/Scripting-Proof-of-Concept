using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunit;
using BlazorUI.Components.Pages;
using Ember.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EFModeling.EntityProperties.FluentAPI.Required;
using Moq;
// using Sandbox.Tests.Helpers;

namespace BlazorUI.Tests;

[TestClass]
public class HelloWorldTest : BunitContext
{
    // REMOVE THIS LINE:
    // ISccriptManagerDeleteAfter _scriptManager = TestHelper.InitScriptManager();

    [TestInitialize]
    public void Setup()
    {
        // 1. Setup Database Mock 
        var options = new DbContextOptionsBuilder<ScriptDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique DB name per test!
            .Options;

        var mockDbFactory = new Mock<IDbContextFactory<ScriptDbContext>>();
        mockDbFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ScriptDbContext(options));

        Services.AddSingleton(mockDbFactory.Object);

        // 2. Mock the ScriptManager entirely, instead of using the heavy TestHelper
        // Since we are strictly testing UI rendering, we don't need the real Roslyn compiler!
        var mockScriptManager = new Mock<ISccriptManagerDeleteAfter>();

        mockScriptManager.Setup(m => m.ListScripts(It.IsAny<CustomerScriptFilter>(), true))
            .ReturnsAsync(new System.Collections.Generic.List<CustomerScript>());

        mockScriptManager.Setup(m => m.GetRunningApiVersion())
            .Returns(1);

        Services.AddSingleton<ISccriptManagerDeleteAfter>(mockScriptManager.Object);
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