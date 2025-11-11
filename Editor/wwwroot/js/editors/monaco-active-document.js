/**
 * Monaco Editor Active Document Tracking
 * Tracks and displays the currently active document/field in the editor
 */

class MonacoActiveDocumentTracker {
    constructor() {
        this.currentDocument = null;
        this.listeners = [];
    }

    /**
     * Set the active document
     * @param {Object} documentInfo - Information about the active document
     * @param {string} documentInfo.fieldName - Name of the field being edited
     * @param {string} documentInfo.language - Programming language
     * @param {string} documentInfo.title - Page/article title
     * @param {number} documentInfo.articleNumber - Article ID
     * @param {boolean} documentInfo.isDirty - Whether document has unsaved changes
     */
    setActiveDocument(documentInfo) {
        this.currentDocument = {
            ...documentInfo,
            timestamp: new Date()
        };
        
        this.notifyListeners();
        this.updateUI();
    }

    /**
     * Get current active document info
     */
    getActiveDocument() {
        return this.currentDocument;
    }

    /**
     * Mark document as dirty (has unsaved changes)
     */
    markDirty() {
        if (this.currentDocument) {
            this.currentDocument.isDirty = true;
            this.updateUI();
        }
    }

    /**
     * Mark document as clean (saved)
     */
    markClean() {
        if (this.currentDocument) {
            this.currentDocument.isDirty = false;
            this.updateUI();
        }
    }

    /**
     * Add listener for document changes
     */
    addListener(callback) {
        this.listeners.push(callback);
    }

    /**
     * Notify all listeners of document change
     */
    notifyListeners() {
        this.listeners.forEach(listener => {
            try {
                listener(this.currentDocument);
            } catch (error) {
                console.error('Error in active document listener:', error);
            }
        });
    }

    /**
     * Update UI to reflect active document
     */
    updateUI() {
        if (!this.currentDocument) return;

        try {
            // Update breadcrumb
            this.updateBreadcrumb();
            
            // Update tab highlighting
            this.highlightActiveTab();
            
            // Update status indicators
            this.updateStatusIndicators();
        } catch (error) {
            console.error('Error updating active document UI:', error);
        }
    }

    /**
     * Update breadcrumb navigation
     */
    updateBreadcrumb() {
        const breadcrumb = document.getElementById('activeDocumentBreadcrumb');
        if (!breadcrumb) return;

        const { title, fieldName, isDirty } = this.currentDocument;
        const dirtyIndicator = isDirty ? '<span class="text-warning ms-1">●</span>' : '';
        
        breadcrumb.innerHTML = `
            <span class="text-muted">${title || 'Untitled'}</span>
            <span class="text-muted mx-2">/</span>
            <span class="text-light fw-bold">${fieldName || 'Unknown'}</span>
            ${dirtyIndicator}
        `;
    }

    /**
     * Highlight the active tab
     */
    highlightActiveTab() {
        const { fieldName } = this.currentDocument;
        if (!fieldName) return;
        
        // Remove active class from all tabs
        document.querySelectorAll('.code-tabs .nav-link').forEach(tab => {
            tab.classList.remove('active');
        });
        
        // Add active class to current tab
        const activeTab = document.querySelector(`.code-tabs .nav-link[data-ccms-fieldname="${fieldName}"]`);
        if (activeTab) {
            activeTab.classList.add('active');
        }
    }

    /**
     * Update status indicators
     */
    updateStatusIndicators() {
        const { language, isDirty } = this.currentDocument;
        
        // Update language indicator - safely handle language value
        const langIndicator = document.getElementById('activeDocumentLanguage');
        if (langIndicator) {
            let displayLanguage = '-';
            
            if (language) {
                // Handle if language is a string
                if (typeof language === 'string') {
                    displayLanguage = language.toUpperCase();
                } 
                // Handle if language is an object with an id property
                else if (typeof language === 'object' && language.id) {
                    displayLanguage = language.id.toUpperCase();
                }
                // Handle other cases
                else {
                    displayLanguage = String(language).toUpperCase();
                }
            }
            
            langIndicator.textContent = displayLanguage;
        }
        
        // Update dirty indicator in save button
        if (isDirty) {
            const saveBtn = document.getElementById('btnSaveChanges');
            if (saveBtn && !saveBtn.querySelector('.dirty-indicator')) {
                const indicator = document.createElement('span');
                indicator.className = 'dirty-indicator text-warning ms-1';
                indicator.textContent = '●';
                const anchor = saveBtn.querySelector('a');
                if (anchor) {
                    anchor.appendChild(indicator);
                }
            }
        } else {
            const indicator = document.querySelector('#btnSaveChanges .dirty-indicator');
            if (indicator) {
                indicator.remove();
            }
        }
    }
}

// Create global instance
window.activeDocumentTracker = new MonacoActiveDocumentTracker();