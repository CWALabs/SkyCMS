/**
 * Monaco Editor Core Functions
 * Shared across Editor, FileManager, Layouts, and Templates
 * Version: 1.0.0
 */

// Core variables (initialized in each view)
var editor;
var fieldId;
var ccmsEditorIsLocked = false;
var editFields; // Set by each view via JSON serialization

/**
 * Saves current editor content and switches to new field
 * @param {string} id - The field name to switch to
 */
function saveExistingLoadNewEditor(id) {
    if (editor !== null && typeof editor !== "undefined") {
        $("#" + fieldId).val(editor.getValue());
    }
    createAndLoadEditor(id);
}

/**
 * Gets Monaco editor language mode from EditorMode enum
 * @param {number} editorModeEnum - The editor mode enumeration value
 * @returns {string} Monaco editor language identifier
 */
function getMonacoLanguageMode(editorModeEnum) {
    switch (editorModeEnum) {
        case 0: return "javascript";
        case 1: return "html";
        case 2: return "css";
        case 3: return "xml";
        case 4: return "json";
        default: return "html";
    }
}

/**
 * Creates and loads Monaco Editor for a given field
 * Can be overridden in each view if custom behavior needed
 * @param {string} id - The field name to create editor for
 */
function createAndLoadEditor(id) {
    // Dispose of existing editor
    if (editor !== null && typeof editor !== "undefined") {
        if (editor.getModel()) {
            editor.getModel().dispose();
        }
        $("#msgBox").html("Loading...");
        editor.dispose();
        editor = null;
    }

    fieldId = null;
    $("#EditingField").val(null);

    // Configure Monaco AMD loader
    require.config({ paths: { 'vs': window.location.origin + '/lib/monaco-editor/min/vs' } });

    require(['vs/editor/editor.main'], function () {
        fieldId = id;
        var fieldInfo = editFields.find(o => o.FieldName === id);
        fieldId = fieldInfo.FieldId;

        var mode = getMonacoLanguageMode(fieldInfo.EditorMode);

        $("#EditingField").val(fieldId);
        var hiddenInput = $("#" + fieldId);
        var code = hiddenInput.val();

        // Allow views to customize before creation
        if (typeof beforeEditorCreate === 'function') {
            beforeEditorCreate(fieldInfo);
        }

        // Create Monaco editor instance
        editor = monaco.editor.create(document.getElementById('editspace'), {
            language: mode,
            theme: "vs-dark",
            value: code,
            readOnly: window.editorReadOnly || false,
            automaticLayout: true
        });

        // Update UI
        $("#msgBox").html("");
        $("#spinLoading").hide();
        $("#btnSavingStatus").show();

        // Allow views to run custom logic after creation
        if (typeof afterEditorCreate === 'function') {
            afterEditorCreate(fieldInfo);
        }
    });
}

/**
 * Setup keyboard shortcuts (Ctrl+S / Cmd+S to save)
 */
function setupKeyboardShortcuts() {
    $(window).bind('keydown', function (event) {
        // Check if read-only mode is enabled
        if (window.editorReadOnly) {
            return;
        }

        if (event.ctrlKey || event.metaKey) {
            switch (String.fromCharCode(event.which).toLowerCase()) {
                case 's':
                    event.preventDefault();
                    $("#frmSave").submit();
                    break;
            }
        }
    });
}

/**
 * Setup auto-save on keyup with debouncing
 * @param {number} delay - Milliseconds to wait before auto-saving (default: 1500)
 */
function setupAutoSave(delay) {
    var timeout;
    delay = delay || 1500;
    
    $(window).bind('keyup', function (event) {
        // Check if read-only mode is enabled
        if (window.editorReadOnly) {
            return;
        }

        // Clear the timeout if it has already been set
        clearTimeout(timeout);
        
        // Make a new timeout set to go off after specified delay
        timeout = setTimeout(function () {
            if (typeof getAutoSave === 'function' && getAutoSave()) {
                $("#frmSave").submit();
            }
        }, delay);
    });
}

/**
 * Setup tab switching between editor fields
 */
function setupTabSwitching() {
    $("[data-ccms-fieldname]").click(function (event) {
        var name = $(event.target).attr("data-ccms-fieldname");
        $("[data-ccms-fieldname]").removeClass("active");
        saveExistingLoadNewEditor(name);
        $(event.target).addClass("active");
    });
}

/**
 * Setup form submit handler
 */
function setupFormSubmit() {
    $("#frmSave").submit(function (e) {
        e.preventDefault();
        if (typeof saveChanges === 'function') {
            saveChanges();
        } else {
            console.error("saveChanges function not defined");
        }
    });
}

/**
 * Initialize common editor functionality
 * @param {object} options - Configuration options
 * @param {boolean} options.readOnly - Enable read-only mode
 * @param {string} options.initialField - Initial field ID to load
 * @param {number} options.autoSaveDelay - Auto-save delay in milliseconds
 */
function initializeMonacoEditor(options) {
    options = options || {};
    
    // Set global read-only flag
    window.editorReadOnly = options.readOnly || false;
    
    // Setup core functionality
    setupFormSubmit();
    setupKeyboardShortcuts();
    setupTabSwitching();
    setupAutoSave(options.autoSaveDelay || 1500);
    
    // Add editor container class
    $("body").addClass("cwps-editor-container");
    
    // Load first field if specified
    if (options.initialField) {
        var fieldInfo = editFields.find(o => o.FieldId === options.initialField);
        if (fieldInfo) {
            createAndLoadEditor(fieldInfo.FieldName);
            
            // Optionally refresh locks after a delay
            if (options.enableLocking !== false) {
                setTimeout(function () {
                    if (typeof ccmsSendSignal === 'function') {
                        //ccmsSendSignal("NotifyRoomOfLock");
                    }
                }, 2000);
            }
        }
    }
}

/**
 * Legacy function for compatibility
 * @deprecated Use saveExistingLoadNewEditor instead
 */
function btnSelectField(e) {
    var fieldName = e.target.text();
    saveExistingLoadNewEditor(e.id);
}