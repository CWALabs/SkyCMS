/**
 * Diff Viewer for Version Comparison
 * Shows side-by-side comparison of different versions
 */

function showDiffEditor(originalContent, modifiedContent) {
    // Create diff editor container
    const diffContainer = document.createElement('div');
    diffContainer.id = 'diff-editor-container';
    diffContainer.className = 'diff-editor-modal';
    diffContainer.innerHTML = `
        <div class="diff-editor-header">
            <h5>Version Comparison</h5>
            <button id="closeDiffEditor" class="btn btn-sm btn-secondary">Close</button>
        </div>
        <div id="diff-editor" style="height: 600px;"></div>
    `;
    
    document.body.appendChild(diffContainer);
    
    // Create diff editor
    const diffEditor = monaco.editor.createDiffEditor(document.getElementById('diff-editor'), {
        enableSplitViewResizing: true,
        renderSideBySide: true,
        readOnly: true,
        automaticLayout: true
    });
    
    diffEditor.setModel({
        original: monaco.editor.createModel(originalContent, 'html'),
        modified: monaco.editor.createModel(modifiedContent, 'html')
    });
    
    // Close button handler
    document.getElementById('closeDiffEditor').addEventListener('click', function() {
        diffEditor.dispose();
        diffContainer.remove();
    });
}