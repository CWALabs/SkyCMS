/**
 * Monaco Editor IntelliSense Configuration
 * Provides context-aware auto-completion for HTML, CSS, and JavaScript
 */

function setupIntelliSense() {
    // HTML IntelliSense for common Bootstrap classes
    monaco.languages.registerCompletionItemProvider('html', {
        provideCompletionItems: function(model, position) {
            var word = model.getWordUntilPosition(position);
            var range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn
            };
            
            var suggestions = [
                // Bootstrap Components
                {
                    label: 'bs-container',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '<div class="container">\n\t$0\n</div>',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Bootstrap container',
                    range: range
                },
                {
                    label: 'bs-row',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '<div class="row">\n\t$0\n</div>',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Bootstrap row',
                    range: range
                },
                {
                    label: 'bs-col',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '<div class="col-md-${1:6}">\n\t$0\n</div>',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Bootstrap column',
                    range: range
                },
                // Common HTML5 elements
                {
                    label: 'article',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '<article>\n\t<h2>${1:Title}</h2>\n\t<p>$0</p>\n</article>',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'HTML5 article element',
                    range: range
                },
                {
                    label: 'section',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '<section id="${1:section-id}">\n\t<h2>${2:Section Title}</h2>\n\t$0\n</section>',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'HTML5 section element',
                    range: range
                }
            ];
            
            return { suggestions: suggestions };
        }
    });
    
    // CSS IntelliSense for common patterns
    monaco.languages.registerCompletionItemProvider('css', {
        provideCompletionItems: function(model, position) {
            var suggestions = [
                {
                    label: 'flex-center',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: 'display: flex;\njustify-content: center;\nalign-items: center;',
                    documentation: 'Flexbox centering',
                },
                {
                    label: 'grid-responsive',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: 'display: grid;\ngrid-template-columns: repeat(auto-fit, minmax(${1:250px}, 1fr));\ngap: ${2:1rem};',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Responsive CSS Grid',
                }
            ];
            return { suggestions: suggestions };
        }
    });
}