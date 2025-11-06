/**
 * Advanced Editor Features
 * Code folding, minimap, breadcrumbs, etc.
 */

function configureAdvancedFeatures(editor) {
    editor.updateOptions({
        // Code folding
        folding: true,
        foldingStrategy: 'indentation',
        showFoldingControls: 'always',
        
        // Minimap
        minimap: {
            enabled: true,
            side: 'right',
            showSlider: 'always',
            renderCharacters: false,
            maxColumn: 120
        },
        
        // Breadcrumbs
        breadcrumbs: {
            enabled: true
        },
        
        // Scrolling
        smoothScrolling: true,
        mouseWheelZoom: true,
        
        // Code lens
        codeLens: true,
        
        // Suggestions
        quickSuggestions: {
            other: true,
            comments: false,
            strings: true
        },
        suggestOnTriggerCharacters: true,
        acceptSuggestionOnCommitCharacter: true,
        acceptSuggestionOnEnter: 'on',
        
        // Word wrap
        wordWrap: 'on',
        wordWrapColumn: 120,
        wrappingIndent: 'indent',
        
        // Indentation
        tabSize: 2,
        insertSpaces: true,
        detectIndentation: true,
        
        // Formatting
        formatOnPaste: true,
        formatOnType: true,
        
        // Line numbers
        lineNumbers: 'on',
        lineNumbersMinChars: 3,
        
        // Scrollbar
        scrollbar: {
            vertical: 'auto',
            horizontal: 'auto',
            useShadows: true,
            verticalHasArrows: false,
            horizontalHasArrows: false,
            verticalScrollbarSize: 10,
            horizontalScrollbarSize: 10
        },
        
        // Find widget
        find: {
            seedSearchStringFromSelection: true,
            autoFindInSelection: 'never',
            addExtraSpaceOnTop: true
        },
        
        // Links
        links: true,
        
        // Context menu
        contextmenu: true,
        
        // Matching brackets
        matchBrackets: 'always',
        
        // Render whitespace
        renderWhitespace: 'selection',
        
        // Cursor
        cursorBlinking: 'smooth',
        cursorSmoothCaretAnimation: true
    });
}

// Toggle minimap
function toggleMinimap() {
    const currentState = editor.getOption(monaco.editor.EditorOption.minimap).enabled;
    editor.updateOptions({
        minimap: { enabled: !currentState }
    });
    localStorage.setItem('monaco-minimap-enabled', !currentState);
}

// Toggle word wrap
function toggleWordWrap() {
    const currentWrap = editor.getOption(monaco.editor.EditorOption.wordWrap);
    editor.updateOptions({
        wordWrap: currentWrap === 'on' ? 'off' : 'on'
    });
}