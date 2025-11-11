/**
 * Code Validation and Linting
 * Provides real-time HTML/CSS/JS validation
 */

function setupCodeValidation() {
    // HTML validation
    monaco.languages.registerDocumentFormattingEditProvider('html', {
        provideDocumentFormattingEdits: function(model, options, token) {
            // Use html-beautify or similar
            const formatted = html_beautify(model.getValue(), {
                indent_size: 2,
                indent_char: ' ',
                max_preserve_newlines: 2,
                preserve_newlines: true,
                end_with_newline: true
            });
            
            return [{
                range: model.getFullModelRange(),
                text: formatted
            }];
        }
    });
    
    // CSS validation
    monaco.languages.css.cssDefaults.setDiagnosticsOptions({
        validate: true,
        lint: {
            compatibleVendorPrefixes: 'warning',
            vendorPrefix: 'warning',
            duplicateProperties: 'warning',
            emptyRules: 'warning',
            importStatement: 'warning',
            zeroUnits: 'warning',
            fontFaceProperties: 'warning',
            hexColorLength: 'warning',
            argumentsInColorFunction: 'warning',
            unknownProperties: 'warning',
            ieHack: 'warning',
            unknownVendorSpecificProperties: 'warning',
            propertyIgnoredDueToDisplay: 'warning',
            important: 'warning',
            float: 'warning',
            idSelector: 'warning'
        }
    });
    
    // JavaScript validation
    monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
        noSemanticValidation: false,
        noSyntaxValidation: false
    });
    
    monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
        target: monaco.languages.typescript.ScriptTarget.ES2020,
        allowNonTsExtensions: true,
        allowJs: true
    });
}

// Custom HTML validation for SkyCMS-specific requirements
function validateSkyCmsHtml(model) {
    const markers = [];
    const content = model.getValue();
    
    // Check for nested editable regions
    const editableRegionRegex = /data-ccms-ceid/g;
    const matches = content.match(editableRegionRegex) || [];
    
    if (matches.length > 0) {
        // Validate no nesting
        const parser = new DOMParser();
        const doc = parser.parseFromString(content, 'text/html');
        const editableElements = doc.querySelectorAll('[data-ccms-ceid]');
        
        editableElements.forEach(elem => {
            if (elem.querySelector('[data-ccms-ceid]')) {
                markers.push({
                    severity: monaco.MarkerSeverity.Error,
                    message: 'Nested editable regions are not allowed',
                    startLineNumber: 1,
                    startColumn: 1,
                    endLineNumber: 1,
                    endColumn: 1
                });
            }
        });
    }
    
    monaco.editor.setModelMarkers(model, 'skycms-validator', markers);
}