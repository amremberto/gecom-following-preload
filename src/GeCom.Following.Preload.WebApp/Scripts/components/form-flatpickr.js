/**
 * Theme: Adminto - Responsive Bootstrap 5 Admin Dashboard
 * Author: Coderthemes
 * Module/App: Flatpickr Date Picker Components
 * 
 * Note: initFlatpickrWithStrictValidation is defined in app.js and used globally.
 * This file only contains cleanup functions for the edit document modal.
 */

/**
 * Destroys Flatpickr instance for a date input
 * @param {string} selector - CSS selector for the input element
 */
window.destroyFlatpickr = function (selector) {
    try {
        var input = document.querySelector(selector);
        if (input && input._flatpickr) {
            input._flatpickr.destroy();
        }
    } catch (error) {
        console.error('Error al destruir Flatpickr:', error);
    }
};

/**
 * Cleans up all Flatpickr instances in the edit document modal
 */
window.cleanupFlatpickrInModal = function () {
    destroyFlatpickr('#fecha-factura-edit');
    destroyFlatpickr('#venc-caecai-edit');
};
