/**
 * Collaborative Editing Features
 * Shows who else is editing and their cursor positions
 */

var collaborators = new Map();

function showCollaboratorCursor(userId, userName, position, color) {
    const decorationId = `cursor-${userId}`;
    
    // Remove old decoration
    if (collaborators.has(decorationId)) {
        const oldDecorations = collaborators.get(decorationId);
        editor.deltaDecorations(oldDecorations, []);
    }
    
    // Add new cursor decoration
    const decorations = editor.deltaDecorations([], [
        {
            range: new monaco.Range(position.lineNumber, position.column, position.lineNumber, position.column),
            options: {
                className: 'collaborator-cursor',
                glyphMarginClassName: 'collaborator-glyph',
                afterContentClassName: 'collaborator-label',
                after: {
                    content: userName,
                    inlineClassName: 'collaborator-name',
                    inlineClassNameAffectsLetterSpacing: true
                },
                stickiness: monaco.editor.TrackedRangeStickiness.NeverGrowsWhenTypingAtEdges
            }
        }
    ]);
    
    collaborators.set(decorationId, decorations);
}

function removeCollaboratorCursor(userId) {
    const decorationId = `cursor-${userId}`;
    if (collaborators.has(decorationId)) {
        const decorations = collaborators.get(decorationId);
        editor.deltaDecorations(decorations, []);
        collaborators.delete(decorationId);
    }
}