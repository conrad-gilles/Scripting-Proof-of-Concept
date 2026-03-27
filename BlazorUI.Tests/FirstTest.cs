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

[TestClass]
public class HomeTests : BunitContext
{
    [TestInitialize]
    public void SetupRealEnvironment()
    {
        TestHelperBUnit.SetupRealEnvironmentBUnit(Services);
    }

    // [TestMethod]
    public void TotalScriptsTests()
    {
        var cut = Render<Home>();

        // Assert initial state
        var totalScriptsH3 = cut.Find("#total-scripts-card h3");
        totalScriptsH3.MarkupMatches("<h3>0</h3>");

        // Click the button
        var compileAllDefaultButton = cut.Find("#compileAllDefaultScripts");
        compileAllDefaultButton.Click();

        // Give it plenty of time (20 seconds is good for Roslyn compilation)
        cut.WaitForAssertion(() =>
        {
            // FIX: Re-find the element INSIDE the assertion block to avoid stale DOM references
            var updatedH3 = cut.Find("#total-scripts-card h3");

            // Use string interpolation to see the actual text if it fails
            Assert.AreNotEqual("0", updatedH3.TextContent.Trim(),
                $"Expected text to change from 0, but it was still '{updatedH3.TextContent}'. Check if an error occurred in HandleCompileAllScriptsFromFolder.");

            Assert.IsTrue(updatedH3.TextContent.Trim() == "5");
        }, TimeSpan.FromSeconds(2));
    }

    // [TestMethod]
    public void TestMonacoEditor()
    {
        var myCustomScript = "public class MyTestScript { public void Execute() { } }";

        JSInterop.Mode = JSRuntimeMode.Loose;

        JSInterop.Setup<string>("window.monacoEditor.getValue", _ => true)
                 .SetResult(myCustomScript);

        JSInterop.SetupVoid("window.monacoEditor.initialize", _ => true);
        JSInterop.SetupVoid("window.monacoEditor.refreshTheme", _ => true);
        JSInterop.SetupVoid("window.monacoEditor.dispose", _ => true);

        var cut = Render<CreateScript>();

        var saveButton = cut.Find("button.btn-success");

        saveButton.Click();

        cut.WaitForAssertion(() =>
       {

       }, TimeSpan.FromSeconds(2));
    }

    // [TestMethod]
    public void TestMonacoEditor2()
    {
        var myCustomScript = @"
using System;
using System.Threading.Tasks;
using Ember.Scripting;

public class MyTestScript : GeneratorScriptsV3.IGeneratorActionScript 
{ 
    public async Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContextV4.IGeneratorContext context) 
    { 
        return ActionResultV3.ActionResult.Success(); 
    } 
}";

        JSInterop.Mode = JSRuntimeMode.Loose;

        // MATCH EXACTLY WHAT THE COMPONENT CALLS:
        // The component calls "monacoEditor.getValue" (no window.), and passes the EditorId as an argument
        JSInterop.Setup<string>("monacoEditor.getValue", _ => true)
                 .SetResult(myCustomScript);

        // Also match the exact initialize name
        JSInterop.SetupVoid("monacoEditor.initialize", _ => true);
        JSInterop.SetupVoid("monacoEditor.dispose", _ => true);

        var cut = Render<CreateScript>();

        // IMPORTANT: Wait for the editor to actually be ready before clicking save!
        // The OnAfterRenderAsync lifecycle must complete so editorReady = true.
        cut.WaitForAssertion(() =>
        {
            // Wait until the editor has finished initializing
            // If we don't wait, HandleSave is clicked while editorReady == false, returning an empty string.
            var saveBtn = cut.Find("button.btn-success");
            Assert.IsFalse(saveBtn.HasAttribute("disabled"), "Button should not be disabled");
        }, TimeSpan.FromSeconds(2));

        var saveButton = cut.Find("button.btn-success");
        saveButton.Click();

        cut.WaitForAssertion(() =>
        {
            var contextFactory = Services.GetRequiredService<IDbContextFactory<EFModeling.EntityProperties.FluentAPI.Required.ScriptDbContext>>();
            using var context = contextFactory.CreateDbContext();

            var savedScript = context.CustomerScripts.FirstOrDefault(s => s.ScriptName == "MyTestScript");
            Assert.IsNotNull(savedScript, "Script was not saved to the database.");
        }, TimeSpan.FromSeconds(5));
    }
}