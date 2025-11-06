/**
 * Enhanced Find & Replace
 * Advanced search functionality with regex and multi-file support
 */

function setupAdvancedSearch() {
    // Add custom find widget actions
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyH, function() {
        editor.trigger('', 'actions.find');
    });
    
    // Replace all in selection
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyMod.Alt | monaco.KeyCode.Enter, function() {
        editor.trigger('', 'editor.action.replaceAll');
    });
}

// Multi-cursor support
function addCursorAtSearchResults(searchTerm) {
    const model = editor.getModel();
    const matches = model.findMatches(searchTerm, false, false, true, null, true);
    
    const selections = matches.map(match => 
        new monaco.Selection(
            match.range.startLineNumber,
            match.range.startColumn,
            match.range.endLineNumber,
            match.range.endColumn
        )
    );
    
    editor.setSelections(selections);
}