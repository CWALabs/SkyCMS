/**
 * Custom Code Snippets for SkyCMS
 * Provides quick access to common patterns
 */

const skyCmsSnippets = {
    html: [
        {
            label: 'ccms-editable',
            insertText: '<div data-ccms-ceid="${1:region-name}" contenteditable="true">\n\t$0\n</div>',
            documentation: 'Creates a SkyCMS editable region',
            detail: 'SkyCMS Editable Region'
        },
        {
            label: 'hero-section',
            insertText: `<section class="hero-section" style="background-image: url('\${1:image.jpg}'); min-height: 400px; background-size: cover; background-position: center;">
    <div class="container h-100 d-flex align-items-center">
        <div class="text-white">
            <h1>\${2:Hero Title}</h1>
            <p class="lead">\${3:Hero description}</p>
            <a href="\${4:#}" class="btn btn-primary btn-lg">$0</a>
        </div>
    </div>
</section>`,
            documentation: 'Hero section with background image',
            detail: 'Hero Section Template'
        },
        {
            label: 'card-grid',
            insertText: `<div class="row g-4">
    <div class="col-md-4">
        <div class="card">
            <img src="\${1:image.jpg}" class="card-img-top" alt="\${2:Alt text}">
            <div class="card-body">
                <h5 class="card-title">\${3:Card Title}</h5>
                <p class="card-text">\${4:Card description}</p>
                <a href="\${5:#}" class="btn btn-primary">$0</a>
            </div>
        </div>
    </div>
</div>`,
            documentation: 'Responsive card grid layout',
            detail: 'Bootstrap Card Grid'
        },
        {
            label: 'navbar',
            insertText: `<nav class="navbar navbar-expand-lg navbar-\${1:light} bg-\${1:light}">
    <div class="container">
        <a class="navbar-brand" href="/">\${2:Brand Name}</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
            <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
            <ul class="navbar-nav ms-auto">
                <li class="nav-item"><a class="nav-link" href="\${3:#}">$0</a></li>
            </ul>
        </div>
    </div>
</nav>`,
            documentation: 'Responsive Bootstrap navbar',
            detail: 'Bootstrap Navbar'
        }
    ],
    css: [
        {
            label: 'responsive-font',
            insertText: 'font-size: clamp(${1:1rem}, ${2:2vw}, ${3:2rem});',
            documentation: 'Responsive font size using clamp',
            detail: 'Responsive Typography'
        },
        {
            label: 'smooth-scroll',
            insertText: 'scroll-behavior: smooth;\noverflow-y: auto;',
            documentation: 'Smooth scrolling behavior',
            detail: 'Smooth Scroll'
        },
        {
            label: 'gradient-bg',
            insertText: 'background: linear-gradient(${1:135deg}, ${2:#667eea} 0%, ${3:#764ba2} 100%);',
            documentation: 'Gradient background',
            detail: 'Gradient Background'
        }
    ],
    javascript: [
        {
            label: 'ajax-get',
            insertText: `fetch('\${1:/api/endpoint}')
    .then(response => response.json())
    .then(data => {
        $0
    })
    .catch(error => console.error('Error:', error));`,
            documentation: 'Fetch API GET request',
            detail: 'AJAX GET Request'
        },
        {
            label: 'ajax-post',
            insertText: `fetch('\${1:/api/endpoint}', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    },
    body: JSON.stringify(\${2:data})
})
    .then(response => response.json())
    .then(data => {
        $0
    })
    .catch(error => console.error('Error:', error));`,
            documentation: 'Fetch API POST request',
            detail: 'AJAX POST Request'
        }
    ]
};

function registerSkyCmsSnippets() {
    Object.keys(skyCmsSnippets).forEach(language => {
        monaco.languages.registerCompletionItemProvider(language, {
            provideCompletionItems: function(model, position) {
                const word = model.getWordUntilPosition(position);
                const range = {
                    startLineNumber: position.lineNumber,
                    endLineNumber: position.lineNumber,
                    startColumn: word.startColumn,
                    endColumn: word.endColumn
                };
                
                return {
                    suggestions: skyCmsSnippets[language].map(snippet => ({
                        label: snippet.label,
                        kind: monaco.languages.CompletionItemKind.Snippet,
                        insertText: snippet.insertText,
                        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                        documentation: snippet.documentation,
                        detail: snippet.detail,
                        range: range
                    }))
                };
            }
        });
    });
}