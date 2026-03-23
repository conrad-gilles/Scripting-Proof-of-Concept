using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlazorUI.Components;
using BlazorUI.Services;
using Ember.Scripting;

namespace BlazorUI.Helpers
{
    public class MonacoEditorHelper
    {
        private readonly ISccriptManagerDeleteAfter _scriptManager;
        private readonly MonacoEditor _editor;
        private readonly ConsoleService _console;

        public bool IsBusy { get; private set; } = false;
        public string ScriptName { get; set; } = string.Empty;

        // If this is Guid.Empty, the helper knows it's a new script.
        public Guid CurrentScriptId { get; set; } = Guid.Empty;

        public MonacoEditorHelper(ISccriptManagerDeleteAfter scriptManager, MonacoEditor editor, ConsoleService console)
        {
            _scriptManager = scriptManager;
            _editor = editor;
            _console = console;
        }

        public Guid GetId(Guid? scriptId)
        {
            if (scriptId == null)
            {
                if (CurrentScriptId == Guid.Empty)
                {
                    CurrentScriptId = Guid.NewGuid();
                }
                scriptId = CurrentScriptId;
            }
            return (Guid)scriptId;
        }
        public async Task HandleValidate(Guid? scriptId = null, int? selectedVersion = null)
        {
            scriptId = GetId(scriptId);
            var code = await _editor.GetValueAsync();
            CurrentScriptId = (Guid)scriptId;
            _console.Log("Validating script:");
            try
            {
                _scriptManager.BasicValidationBeforeCompiling(code);
                _console.Log("Validation successfull!");
            }
            catch (Exception e)
            {
                _console.Log(e.ToString());
            }
        }

        public async Task HandleCompile(Guid? scriptId = null, int? selectedVersion = null)
        {
            try
            {
                IsBusy = true;
                scriptId = GetId(scriptId);
                CurrentScriptId = (Guid)scriptId;
                var code = await _editor.GetValueAsync();
                try
                {
                    //first tries to insert and compile new script
                    Guid newScript = await _scriptManager.CreateScript(code);
                    CurrentScriptId = newScript;
                    _console.Log("Compilation successful, script was created and inserted into the DB.");
                }
                catch
                {
                    try
                    {
                        await _scriptManager.UpdateScriptAndCompile((Guid)scriptId, code, apiVersion: selectedVersion);
                        _console.Log("Script successfully updated and compiled.");
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            bool result = await _scriptManager.ThrowCompilationErrors(code);
                            _console.Log("Script successfully compiled but did not get saved.");
                        }
                        catch
                        {
                            _console.Log("Compilation failed: " + e.ToString());
                        }
                    }
                }
                IsBusy = false;
            }
            catch (Exception e)
            {
                IsBusy = false;
                _console.Log("Somethign went wrong in line 99: " + e.ToString());
            }

        }

        public async Task HandleSave(Guid? scriptId = null, int? selectedVersion = null)
        {
            IsBusy = true;
            scriptId = GetId(scriptId);
            CurrentScriptId = (Guid)scriptId;
            var code = await _editor.GetValueAsync();
            try
            {
                try
                {
                    //first tries to insert and compile new script
                    Guid newScript = await _scriptManager.CreateScript(code);
                    CurrentScriptId = newScript;
                    _console.Log("Compilation successful, script was created and inserted into the DB.");
                }
                catch
                {
                    try
                    {
                        await _scriptManager.UpdateScriptAndCompile((Guid)scriptId, code, apiVersion: selectedVersion);
                        _console.Log("Script successfully updated and compiled.");
                    }
                    catch (Exception e)
                    {
                        await _scriptManager.UpdateScript((Guid)scriptId, code, apiVersion: selectedVersion);
                        _console.Log("Script did not compile successfully but nevertheless it got saved, here are the compilation errors:");
                        _console.Log(e.ToString());
                    }
                }
                IsBusy = false;
            }
            catch (System.Exception e)
            {
                // //Rare edge case when in ScriptTemplate and compiling and it doesnt find the id because it is not in the variable because one switched tabs
                // var vali = _scriptManager.BasicValidationBeforeCompiling(code);
                // Guid retrievedId = await _scriptManager.GetScriptId(vali.ClassName, vali.BaseTypeName)
                // // _console.Log("Something went wrong in line 139: " + e.ToString());
            }
        }










        // public async Task HandleCompile()
        // {
        //     IsBusy = true;
        //     try
        //     {
        //         await ExecuteCompileProcess();
        //     }
        //     finally
        //     {
        //         IsBusy = false;
        //     }
        // }

        // public async Task HandleSave()
        // {
        //     IsBusy = true;
        //     try
        //     {
        //         // Try to compile and save normally
        //         bool compileSuccess = await ExecuteCompileProcess();

        //         // If compile fails, force save without compiling
        //         if (!compileSuccess)
        //         {
        //             _console.Log("Compilation Failed, yet script is still being saved...");
        //             var code = await _editor.GetValueAsync();

        //             try
        //             {
        //                 if (CurrentScriptId == Guid.Empty)
        //                 {
        //                     // If your backend doesn't support creating a new script without compiling it,
        //                     // you must warn the user. Otherwise, you can call a special create method here.
        //                     _console.Log("Cannot save a brand new script with syntax errors. Please fix the errors first.");
        //                 }
        //                 else
        //                 {
        //                     await _scriptManager.SaveScriptWithoutCompiling(CurrentScriptId, code);
        //                     _console.Log("Script saved successfully without compilation.");
        //                 }
        //             }
        //             catch (Exception ex)
        //             {
        //                 _console.Log($"Critical Error saving script: {ex.Message}");
        //             }
        //         }
        //     }
        //     finally
        //     {
        //         IsBusy = false;
        //     }
        // }

        // // Shared logic: this runs the regex parsing whether the user clicked Compile OR Save.
        // private async Task<bool> ExecuteCompileProcess()
        // {
        //     var code = await _editor.GetValueAsync();
        //     await _editor.ClearErrorsAsync();
        //     _console.Log($"Compiling '{ScriptName}'...");

        //     try
        //     {

        //         try
        //         {
        //             // CREATE NEW SCRIPT
        //             // This method likely already handles compiling and inserting the initial cache

        //             // _console.Log("Reached this line");
        //             Guid newScript = await _scriptManager.CreateScript(code);

        //             // SAVE THE ID so if the user clicks Save again, it updates instead of duplicating!
        //             CurrentScriptId = newScript;
        //             _console.Log("Compilation successful, script was created and inserted into the DB.");
        //             return true; // Success!
        //         }
        //         catch
        //         {
        //             try
        //             {
        //                 var validation = _scriptManager.BasicValidationBeforeCompiling(code);
        //                 ScriptTypes sType;
        //                 switch (validation.BaseTypeName)
        //                 {
        //                     case "IGeneratorConditionScript":
        //                         sType = ScriptTypes.GeneratorConditionScript;
        //                         break;
        //                     case "IGeneratorActionScript":
        //                         sType = ScriptTypes.GeneratorActionScript;
        //                         break;
        //                     default:
        //                         throw new Exception();
        //                 }
        //                 Guid id = await _scriptManager.GetScriptId(validation.ClassName, sType);
        //                 CurrentScriptId = id;
        //             }
        //             catch (System.Exception) { }
        //             try
        //             {
        //                 // UPDATE EXISTING SCRIPT

        //                 // 1. First, save the updated source code to the database
        //                 await _scriptManager.UpdateScriptAndCompile(CurrentScriptId, code);

        //                 // 2. Get the current running API version needed for the recompilation
        //                 int currentApiVersion = _scriptManager.GetRunningApiVersion();

        //                 // 3. Call your new RecompileCache method instead of CompileScript!
        //                 // This safely replaces/updates the old cache in the DbHelper.
        //                 // await _scriptManager.RecompileCache(CurrentScriptId, currentApiVersion);

        //                 _console.Log("Update compiled successfully, script was updated and inserted into the DB.");
        //                 return true; // Success!
        //             }
        //             catch
        //             {
        //                 _console.Log("Failed to UpdateScriptAndCompile");
        //                 return false;
        //             }

        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _console.Log($"Compilation Failed: {ex.Message}");

        //         // Parse and show errors in the editor UI
        //         var errorsList = new List<object>();
        //         var regex = new Regex(@"Line (\d+), Col (\d+): (.*)");
        //         var matches = regex.Matches(ex.Message);

        //         foreach (Match match in matches)
        //         {
        //             if (match.Success)
        //             {
        //                 errorsList.Add(new
        //                 {
        //                     line = int.Parse(match.Groups[1].Value),
        //                     column = int.Parse(match.Groups[2].Value),
        //                     message = match.Groups[3].Value
        //                 });
        //             }
        //         }

        //         if (errorsList.Any())
        //         {
        //             await _editor.SetErrorsAsync(errorsList);
        //         }

        //         return false; // Failed!
        //     }
        // }
    }
}
