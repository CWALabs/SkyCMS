/**
 * Custom Monaco Editor Themes
 * Provides multiple theme options for user preference
 */

const editorThemes = {
    'vs-dark-custom': {
        base: 'vs-dark',
        inherit: true,
        rules: [
            { token: 'comment', foreground: '6A9955', fontStyle: 'italic' },
            { token: 'keyword', foreground: '569CD6' },
            { token: 'string', foreground: 'CE9178' },
            { token: 'number', foreground: 'B5CEA8' },
            { token: 'tag', foreground: '4EC9B0' },
            { token: 'attribute.name', foreground: '9CDCFE' },
            { token: 'attribute.value', foreground: 'CE9178' }
        ],
        colors: {
            'editor.background': '#1E1E1E',
            'editor.foreground': '#D4D4D4',
            'editor.lineHighlightBackground': '#2A2A2A',
            'editorCursor.foreground': '#AEAFAD',
            'editor.selectionBackground': '#264F78',
            'editor.inactiveSelectionBackground': '#3A3D41'
        }
    },
    'monokai': {
        base: 'vs-dark',
        inherit: true,
        rules: [
            { token: 'comment', foreground: '75715E' },
            { token: 'keyword', foreground: 'F92672' },
            { token: 'string', foreground: 'E6DB74' },
            { token: 'number', foreground: 'AE81FF' },
            { token: 'tag', foreground: 'F92672' },
            { token: 'attribute.name', foreground: 'A6E22E' }
        ],
        colors: {
            'editor.background': '#272822',
            'editor.foreground': '#F8F8F2',
            'editor.lineHighlightBackground': '#3E3D32',
            'editorCursor.foreground': '#F8F8F0'
        }
    },
    'github-light': {
        base: 'vs',
        inherit: true,
        rules: [
            { token: 'comment', foreground: '6A737D' },
            { token: 'keyword', foreground: 'D73A49' },
            { token: 'string', foreground: '032F62' },
            { token: 'number', foreground: '005CC5' },
            { token: 'tag', foreground: '22863A' }
        ],
        colors: {
            'editor.background': '#FFFFFF',
            'editor.foreground': '#24292E',
            'editor.lineHighlightBackground': '#F6F8FA'
        }
    }
};

function initializeThemes() {
    Object.keys(editorThemes).forEach(themeName => {
        monaco.editor.defineTheme(themeName, editorThemes[themeName]);
    });
}

function changeEditorTheme(themeName) {
    if (editor) {
        monaco.editor.setTheme(themeName);
        localStorage.setItem('monaco-theme-preference', themeName);
    }
}

function loadThemePreference() {
    const savedTheme = localStorage.getItem('monaco-theme-preference');
    return savedTheme || 'vs-dark';
}