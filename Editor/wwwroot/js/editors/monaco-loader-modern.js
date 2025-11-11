/**
 * Modern Monaco Editor Loader for Cosmos CMS
 * Replaces unreliable AMD loader with Promise-based loading
 */

class MonacoLoader {
    constructor() {
        this.loaded = false;
        this.loading = false;
        this.loadPromise = null;
        this.monaco = null;
    }

    async load(config = {}) {
        if (this.loaded && this.monaco) {
            return this.monaco;
        }

        if (this.loading) {
            return this.loadPromise;
        }

        this.loading = true;
        
        // Configure worker paths BEFORE loading Monaco
        this._configureWorkerPaths(config.basePath || '/lib/monaco-editor/min');
        
        this.loadPromise = this._loadMonaco(config);
        
        try {
            this.monaco = await this.loadPromise;
            this.loaded = true;
            window.monaco = this.monaco; // Backward compatibility
            return this.monaco;
        } catch (error) {
            this.loading = false;
            console.error('Monaco loading failed:', error);
            throw new Error(`Failed to load Monaco Editor: ${error.message}`);
        }
    }

    _configureWorkerPaths(basePath) {
        // Set up the Monaco environment to properly resolve worker paths
        window.MonacoEnvironment = {
            getWorkerUrl: function(moduleId, label) {
                // Use relative paths from the base path
                if (label === 'json') {
                    return `${basePath}/vs/language/json/json.worker.js`;
                }
                if (label === 'css' || label === 'scss' || label === 'less') {
                    return `${basePath}/vs/language/css/css.worker.js`;
                }
                if (label === 'html' || label === 'handlebars' || label === 'razor') {
                    return `${basePath}/vs/language/html/html.worker.js`;
                }
                if (label === 'typescript' || label === 'javascript') {
                    return `${basePath}/vs/language/typescript/ts.worker.js`;
                }
                // Default editor worker
                return `${basePath}/vs/editor/editor.worker.js`;
            }
        };
    }

    async _loadMonaco(config) {
        return new Promise((resolve, reject) => {
            const basePath = config.basePath || '/lib/monaco-editor/min';
            const timeout = config.timeout || 10000;
            
            // Set timeout for loading
            const timeoutId = setTimeout(() => {
                reject(new Error('Monaco Editor loading timeout'));
            }, timeout);

            // Check if loader already exists
            if (window.require?.config) {
                clearTimeout(timeoutId);
                this._requireEditor(basePath, resolve, reject);
                return;
            }

            // Create and load the AMD loader
            const loaderScript = document.createElement('script');
            loaderScript.src = `${basePath}/vs/loader.js`;
            loaderScript.async = true;
            
            loaderScript.onload = () => {
                clearTimeout(timeoutId);
                this._requireEditor(basePath, resolve, reject);
            };

            loaderScript.onerror = () => {
                clearTimeout(timeoutId);
                reject(new Error('Failed to load Monaco loader script'));
            };

            document.head.appendChild(loaderScript);
        });
    }

    _requireEditor(basePath, resolve, reject) {
        try {
            window.require.config({
                paths: { vs: `${basePath}/vs` },
                'vs/nls': { availableLanguages: { '*': '' } }
            });

            window.require(['vs/editor/editor.main'], (monacoModule) => {
                // Monaco AMD loader doesn't always pass monaco as parameter
                // Access it from window object instead
                const monaco = window.monaco;
                
                if (monaco && monaco.editor) {
                    resolve(monaco);
                } else {
                    reject(new Error('Monaco editor.main loaded but monaco.editor is undefined. Check if Monaco files are properly deployed.'));
                }
            }, (error) => {
                reject(error);
            });
        } catch (error) {
            reject(error);
        }
    }

    async createEditor(container, options = {}) {
        const monaco = await this.load();
        
        if (!container) {
            throw new Error('Editor container element not found');
        }

        if (!monaco || !monaco.editor) {
            throw new Error('Monaco editor API not properly loaded. Ensure Monaco Editor files are correctly deployed.');
        }

        const defaultOptions = {
            theme: 'vs-dark',
            automaticLayout: true,
            minimap: { enabled: true },
            scrollBeyondLastLine: false,
            fontSize: 14,
            wordWrap: 'on',
            formatOnPaste: true,
            formatOnType: true,
            tabSize: 2,
            insertSpaces: true
        };

        return monaco.editor.create(container, {
            ...defaultOptions,
            ...options
        });
    }

    dispose() {
        if (this.monaco && window.monaco) {
            this.monaco = null;
            this.loaded = false;
        }
    }
}

// Export singleton instance
const monacoLoader = new MonacoLoader();
window.monacoLoader = monacoLoader; // Backward compatibility