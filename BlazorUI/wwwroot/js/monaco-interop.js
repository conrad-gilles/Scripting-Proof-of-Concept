window.monacoEditor = {
    _instance: null,

    initialize: function (elementId, initialValue, language) {
        require(['vs/editor/editor.main'], function () {
            window.monacoEditor._instance = monaco.editor.create(
                document.getElementById(elementId), {
                    value: initialValue || '',
                    language: language || 'csharp',
                    theme: 'vs-dark',
                    automaticLayout: true,   // resizes with container
                    fontSize: 14,
                    minimap: { enabled: false },
                    scrollBeyondLastLine: false,
                    wordWrap: 'on'
                }
            );
        });
    },

    getValue: function () {
        return window.monacoEditor._instance?.getValue() ?? '';
    },

        // ADD THIS NEW FUNCTION:
    setErrors: function (errors) {
        if (!window.monacoEditor._instance) return;
        
        // Map the errors coming from C# to Monaco's marker format
        const markers = errors.map(err => ({
            severity: monaco.MarkerSeverity.Error,
            startLineNumber: err.line,
            startColumn: err.column,
            endLineNumber: err.line,
            endColumn: err.column + 5, // Just highlighting 5 chars for the squiggle
            message: err.message
        }));

        // Apply the red squiggly lines
        const model = window.monacoEditor._instance.getModel();
        monaco.editor.setModelMarkers(model, "csharp", markers);
    },

    // ADD THIS TO CLEAR ERRORS:
    clearErrors: function() {
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
