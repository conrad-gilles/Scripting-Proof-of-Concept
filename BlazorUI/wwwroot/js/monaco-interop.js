window.monacoEditor = {
    _instance: null,
    _typingTimer: null, // Timer for debouncing

    initialize: function (elementId, initialValue, language, dotNetHelper) {
        if (window.monacoEditor._instance) {
            window.monacoEditor._instance.dispose();
            window.monacoEditor._instance = null;
        }

        require(['vs/editor/editor.main'], function () {
            const container = document.getElementById(elementId);
            if (!container) return; // Guard: Blazor may have removed it already

            window.monacoEditor._instance = monaco.editor.create(container, {
                value: initialValue || '',
                language: language || 'csharp',
                theme: 'vs-dark',
                automaticLayout: true,
                fontSize: 14,
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                wordWrap: 'on'
            });

            // Force a layout pass so the editor measures itself correctly
            window.monacoEditor._instance.layout();
            window.monacoEditor._instance.onDidChangeModelContent(function () {
                // Clear the previous timer if the user is still typing
                clearTimeout(window.monacoEditor._typingTimer);

                // Set a new timer for 1 second (1000ms)
                window.monacoEditor._typingTimer = setTimeout(function () {
                    // Call the C# HandleCodeChange method
                    dotNetHelper.invokeMethodAsync('HandleCodeChange');
                }, 500);
            });
        });
    },

    getValue: function () {
        return window.monacoEditor._instance?.getValue() ?? '';
    },

    setErrors: function (errors) {
        // 1. Change .instance to ._instance
        if (!window.monacoEditor._instance) return;

        const markers = errors.map(err => ({
            severity: err.severity,
            startLineNumber: err.startLineNumber,
            startColumn: err.startColumn,
            endLineNumber: err.endLineNumber,
            endColumn: err.endColumn,
            message: err.message
        }));

        // 2. Change .instance to ._instance here as well
        const model = window.monacoEditor._instance.getModel();
        monaco.editor.setModelMarkers(model, "csharp", markers);
    },



    clearErrors: function () {
        if (!window.monacoEditor._instance) return;
        const model = window.monacoEditor._instance.getModel();
        monaco.editor.setModelMarkers(model, "csharp", []);
    },

    setValue: function (value) {
        window.monacoEditor._instance?.setValue(value);
    },

    dispose: function () {
        window.monacoEditor._instance?.dispose();
        window.monacoEditor._instance = null;
    }
};
