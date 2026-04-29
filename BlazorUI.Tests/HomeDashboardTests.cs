using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BlazorUI.Components.Pages;
using Ember.Scripting;
// using EFModeling.EntityProperties.FluentAPI.Required;
using System.Collections.Generic;
using System.Threading;

namespace sandbox.Tests.UI;

// [TestClass]
public class HomeDashboardTests : BunitContext // Inheriting from BunitContext manages setup/teardown automatically
{
    // [TestMethod]
    public void Home_ShouldRender_DashboardTitle_AndEmptyState()
    {
        // 1. Setup InMemory Database for IDbContextFactory
        var options = new DbContextOptionsBuilder<ScriptDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDashboardDb")
            .Options;

        var mockDbFactory = new Mock<IDbContextFactory<ScriptDbContext>>();
        mockDbFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ScriptDbContext(options));

        Services.AddSingleton(mockDbFactory.Object);

        // 2. Setup Mock ScriptManager
        var mockScriptManager = new Mock<IScriptManagerBaseExtended>();

        // FIX 1: Provide both parameters explicitly for Moq (Filter and bool)
        mockScriptManager.Setup(m => m.ListScripts(It.IsAny<CustomerScriptFilter>(), true))
            .ReturnsAsync(new List<CustomerScript>());

        // Fallback catch-all in case the component passes null directly
        mockScriptManager.Setup(m => m.ListScripts(null, true))
            .ReturnsAsync(new List<CustomerScript>());

        mockScriptManager.Setup(m => m.GetRunningApiVersion())
            .Returns(1);

        Services.AddSingleton(mockScriptManager.Object);

        // 3. Act: Render the Home component
        // FIX 2: Use the new Render<T>() method instead of RenderComponent<T>()
        var cut = Render<Home>();

        // 4. Assert: Verify the dashboard rendered successfully
        cut.Find("h2").MarkupMatches("<h2>Dashboard</h2>");

        // Verify the empty state shows up for ember instances
        Assert.IsTrue(cut.Markup.Contains("No Ember instances registered."));
    }
}