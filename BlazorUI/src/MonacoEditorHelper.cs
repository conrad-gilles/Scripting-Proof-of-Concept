using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlazorUI.Components;
using BlazorUI.Components.Pages;
using BlazorUI.Services;
using Ember.Scripting;
using Ember.Simulation;
using Microsoft.JSInterop;
using Ember.Scripting.Compilation;
using Serilog;

namespace BlazorUI.Helpers
{
    public class MonacoEditorHelper
    {
        private readonly IScriptManagerBaseExtended _scriptManager;
        private readonly ScriptManager _eif;
        private readonly MonacoEditor _editor;
        private readonly ConsoleService _console;
        private readonly Popup _myPopup = default!;

        public bool IsBusy { get; private set; } = false;
        public string ScriptName { get; set; } = string.Empty;

        // If this is Guid.Empty, the helper knows it's a new script.
        public Guid CurrentScriptId { get; set; } = Guid.Empty;

        internal MonacoEditorHelper(IScriptManagerBaseExtended scriptManager, ScriptManager eif, MonacoEditor editor, ConsoleService console, Popup myPopup)
        {
            _scriptManager = scriptManager;
            _eif = eif;
            _editor = editor;
            _console = console;
            _myPopup = myPopup;
        }


        // Validates script, doesnt save doesnt compile
        public async Task<ValidationRecord?> Validate()
        {
            IsBusy = true;
            ValidationRecord? vali = null;
            string sourceCode = await _editor.GetValueAsync();
            try
            {
                vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
                _console.Log("Validation successfull!");
                _myPopup.CreateNormalPopup("Success!", "Validation successfull!");
            }
            catch (Exception e)
            {
                _console.LogException(e);
                _myPopup.CreateErrorPopup(e.GetType().Name, e.Message);
            }
            IsBusy = false;
            return vali;
        }
        public async Task Execute(string? methodName = null)
        {
            IsBusy = true;
            try
            {
                string sourceCode = await _editor.GetValueAsync();
                object ar = await _eif.ExecuteUnfinishedScriptBySourceCode(sourceCode, TestHelper.GetContext(), methodName: methodName);
                _console.Log(ar.ToString()!);
                _myPopup.CreateNormalPopup("Success", ar.ToString()!);
            }
            catch (Exception e)
            {
                _console.LogException(e);
                _myPopup.CreateErrorPopup(e.GetType().Name, e.Message);
            }


            IsBusy = false;
        }

        //compiles script, doesnt save doesnt perform validation, only compiles, can technically be ommitted
        public async Task Compile(string? sourceCode)
        {
            IsBusy = true;
            try
            {

                if (sourceCode == null)
                {
                    sourceCode = await _editor.GetValueAsync();
                }

                List<ScriptCompilationError> errors = EmberMethods.ParseCompilationErrors(await _scriptManager.GetCompilationErrors(sourceCode));

                var monacoMarkers = errors.Select(e => new MonacoMarker
                {
                    Message = $"{e.Id} - {e.Message}",
                    Severity = e.IsError ? 8 : 4, // 8 = Error, 4 = Warning
                    StartLineNumber = e.Line,
                    StartColumn = e.Column,
                    EndLineNumber = e.EndLine,
                    EndColumn = e.EndColumn
                }).ToList<object>();

                await _editor.SetErrorsAsync(monacoMarkers);
            }

            catch (Exception e)
            {
                _console.LogException(e);
                _myPopup.CreateErrorPopup(e.GetType().Name, e.Message);
            }
            IsBusy = false;
        }

        //saves script
        public async Task Save(Guid scriptId, string sourceCode)
        {
            try
            {
                IsBusy = true;

                var code = await _editor.GetValueAsync();
                await _scriptManager.UpdateScriptAndCompile(scriptId, code);
                IsBusy = false;
            }
            catch (Exception e)
            {
                _console.LogException(e);
                _myPopup.CreateErrorPopup(e.GetType().Name, e.Message);
            }
        }

        public async Task CreateAndInsert()
        {

            IsBusy = true;
            try
            {
                var code = await _editor.GetValueAsync();
                await _scriptManager.CreateScript(code);
            }
            catch (Exception e)
            {
                _console.LogException(e);
                _myPopup.CreateErrorPopup(e.GetType().Name, e.Message);
            }

            IsBusy = false;
        }

        public async Task SetErrorsInEditorAsync()
        {
            var code = await _editor.GetValueAsync();
            await _editor.ClearErrorsAsync();
            List<ScriptCompilationError> errors = EmberMethods.ParseCompilationErrors(await _scriptManager.GetCompilationErrors(code));
            var monacoMarkers = errors.Select(e => new MonacoMarker
            {
                Message = $"{e.Id} - {e.Message}",
                Severity = e.IsError ? 8 : 4, // 8 = Error, 4 = Warning
                StartLineNumber = e.Line,
                StartColumn = e.Column,
                EndLineNumber = e.EndLine,
                EndColumn = e.EndColumn
            }).ToList<object>();

            await _editor.SetErrorsAsync(monacoMarkers);
        }
    }
}
