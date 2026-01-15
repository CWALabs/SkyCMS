// Shared GUID/UUID v4 generator for Cosmos CMS
(function (global) {
    function ccmsGenerateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    // Preserve existing implementations but prefer the shared one
    if (!global.ccmsGenerateGuid) global.ccmsGenerateGuid = ccmsGenerateGuid;
    if (!global.ccms__generateGUID) global.ccms__generateGUID = ccmsGenerateGuid;
    if (!global.ccms___generateGUID) global.ccms___generateGUID = ccmsGenerateGuid;
    if (!global.generateGUID) global.generateGUID = ccmsGenerateGuid;
})(typeof window !== 'undefined' ? window : globalThis);
