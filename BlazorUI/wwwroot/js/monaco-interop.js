window.monacoEditor = {
    // Store dictionaries of instances and timers keyed by EditorId
    instances: {},
    typingTimers: {},

    initialize: function (elementId, initialValue, language, dotNetHelper) {
        // Clean up only the specific instance if it already exists
        if (window.monacoEditor.instances[elementId]) {
            window.monacoEditor.instances[elementId].dispose();
            delete window.monacoEditor.instances[elementId];
        }

        require(['vs/editor/editor.main'], function () {
            const container = document.getElementById(elementId);
            if (!container) return; // Guard

            const instance = monaco.editor.create(container, {
                value: initialValue || '',
                language: language || 'csharp',
                theme: 'vs-dark',
                showUnused: false,
                automaticLayout: true,
                fontSize: 14,
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                wordWrap: 'on',
                // --- ADD THIS LINE ---
                useShadowDOM: false 
            });

            instance.layout();

            // Save the instance into the dictionary
            window.monacoEditor.instances[elementId] = instance;

            instance.onDidChangeModelContent(function () {
                clearTimeout(window.monacoEditor.typingTimers[elementId]);

                window.monacoEditor.typingTimers[elementId] = setTimeout(function () {
                    dotNetHelper.invokeMethodAsync('HandleCodeChange');
                }, 600);
            });
        });
    },

    getValue: function (elementId) {
        return window.monacoEditor.instances[elementId]?.getValue() ?? '';
    },

    setErrors: function (elementId, errors) {
        const instance = window.monacoEditor.instances[elementId];
        if (!instance) return;

        const markers = errors.map(err => ({
            severity: err.severity,
            startLineNumber: err.startLineNumber,
            startColumn: err.startColumn,
            endLineNumber: err.endLineNumber,
            endColumn: err.endColumn,
            message: err.message
        }));

        monaco.editor.setModelMarkers(instance.getModel(), "csharp", markers);
    },

    clearErrors: function (elementId) {
        const instance = window.monacoEditor.instances[elementId];
        if (!instance) return;

        monaco.editor.setModelMarkers(instance.getModel(), "csharp", []);
    },

    setValue: function (elementId, value) {
        window.monacoEditor.instances[elementId]?.setValue(value);
    },
    
    layout: function (elementId) {
        const instance = window.monacoEditor.instances[elementId];
        if (instance) {
            instance.layout();
        }
    },

    // ---> ADD THE NEW FUNCTION HERE <---
    refreshTheme: function (elementId) {
        // Regenerates the dynamic <style> tags Blazor wiped out globally
        if (typeof monaco !== 'undefined') {
            monaco.editor.setTheme('vs-dark'); 
        }
        
        // Forces the specific editor to recalculate its dimensions
        const instance = window.monacoEditor.instances[elementId];
        if (instance) {
            instance.layout();
        }
    },

    dispose: function (elementId) {
        // Only dispose the editor belonging to the component being destroyed
        if (window.monacoEditor.instances[elementId]) {
            window.monacoEditor.instances[elementId].dispose();
            delete window.monacoEditor.instances[elementId];
        }
        clearTimeout(window.monacoEditor.typingTimers[elementId]);
        delete window.monacoEditor.typingTimers[elementId];
    }
};