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

    setValue: function (value) {
        window.monacoEditor._instance?.setValue(value);
    },

    dispose: function () {
        window.monacoEditor._instance?.dispose();
        window.monacoEditor._instance = null;
    }
};
