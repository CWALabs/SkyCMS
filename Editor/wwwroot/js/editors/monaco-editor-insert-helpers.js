/**
 * Monaco Editor Insert Helpers
 * Functions for inserting links, images, and files into the Monaco editor
 * Used by Editor, Layouts, and Templates views
 * Version: 1.0.0
 */

/**
 * Opens the page picker modal for inserting page links
 */
//function openPickPageModal() {
//    if (typeof editor === "undefined" || editor === null) {
//        alert("Error: Could not open code editor.");
//        return;
//    }
    
//    // Clear and reset form fields
//    $("#combobox").val("");
//    $("#inputLinkText").val("");
//    $("#switchNewWindow").prop('checked', false);
//    $("#inputLinkCss").val("");
//    $("#inputLinkStyle").val("");
    
//    if (typeof pickPageModal !== 'undefined' && pickPageModal) {
//        pickPageModal.show();
//    } else {
//        console.error("pickPageModal is not defined");
//    }
//}

/**
 * Opens the file picker modal for inserting file links
 */
//function openInsertFileLinkModel() {
//    if (typeof editor === "undefined" || editor === null) {
//        alert("Error: Could not open code editor.");
//        return;
//    }
    
//    if (typeof openSelectFileModal === 'function') {
//        openSelectFileModal("file");
//    } else {
//        console.error("openSelectFileModal function not defined");
//    }
//}

/**
 * Opens the image picker modal for inserting images
 */
//function openInsertImageModel() {
//    if (typeof editor === "undefined" || editor === null) {
//        alert("Error: Could not open code editor.");
//        return;
//    }
    
//    if (typeof openSelectFileModal === 'function') {
//        openSelectFileModal("image");
//    } else {
//        console.error("openSelectFileModal function not defined");
//    }
//}

/**
 * Inserts a page link into the editor
 * Uses data from the page picker modal
 */
//function insertPageLink() {
//    const inputLinkText = $("#inputLinkText").val();
    
//    if (typeof (inputLinkText) === "undefined" || inputLinkText === null || inputLinkText === "") {
//        $("#inputLinkTextError").show();
//        return false;
//    }
    
//    if (typeof pickPageModal !== 'undefined' && pickPageModal) {
//        pickPageModal.hide();
//    }
    
//    if (typeof selectedAnchorData === 'undefined' || !selectedAnchorData) {
//        console.error("selectedAnchorData is not defined");
//        return false;
//    }
    
//    const link = "<a href='/" + selectedAnchorData.url + "'>" + inputLinkText + "</a>";
    
//    // Insert text into Monaco editor
//    if (editor && typeof editor.trigger === 'function') {
//        editor.trigger('keyboard', 'type', { text: link });
//    }
//}

/**
 * Inserts a file link into the editor
 * @param {string} path - The file path to insert
 */
//function insertFileLink(path) {
//    if (typeof clearFileMgrPaths === 'function') {
//        clearFileMgrPaths();
//    }
    
//    if (typeof fileBaseUrl === 'undefined') {
//        console.error("fileBaseUrl is not defined");
//        return;
//    }
    
//    const url = fileBaseUrl + "/" + path;
//    const link = "<a href='" + url + "'>" + path + "</a>";
    
//    // Insert text into Monaco editor
//    if (editor && typeof editor.trigger === 'function') {
//        editor.trigger('keyboard', 'type', { text: link });
//    }
//}

/**
 * Inserts an image tag into the editor
 * @param {string} path - The image path to insert
 */
//function insertImage(path) {
//    if (typeof clearFileMgrPaths === 'function') {
//        clearFileMgrPaths();
//    }
    
//    if (typeof fileBaseUrl === 'undefined') {
//        console.error("fileBaseUrl is not defined");
//        return;
//    }
    
//    const url = fileBaseUrl + "/" + path;
//    const image = "<img src='" + url + "' />";
    
//    // Insert text into Monaco editor
//    if (editor && typeof editor.trigger === 'function') {
//        editor.trigger('keyboard', 'type', { text: image });
//    }
//}

/**
 * Setup event handlers for insert buttons
 * Call this in $(document).ready()
 */
//function setupInsertButtonHandlers() {
//    $("#btnOpenLink").off('click').on('click', function (e) {
//        e.preventDefault();
//        openPickPageModal();
//    });
    
//    $("#btnOpenInsertFileLink").off('click').on('click', function (e) {
//        e.preventDefault();
//        openInsertFileLinkModel();
//    });
    
//    $("#btnOpenInsertImage").off('click').on('click', function (e) {
//        e.preventDefault();
//        openInsertImageModel();
//    });
//}