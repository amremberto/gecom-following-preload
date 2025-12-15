/**
 * Theme: Adminto - Responsive Bootstrap 5 Admin Dashboard
 * Author: Coderthemes
 * Module/App: Flatpickr Date Picker Components
 */

/**
 * Initializes Flatpickr with strict validation for a date input
 * @param {string} selector - CSS selector for the input element
 * @param {object} options - Flatpickr options (defaultDate, etc.)
 */
window.initFlatpickrWithStrictValidation = function (selector, options) {
    if (typeof flatpickr === 'undefined') {
        console.error('Flatpickr is not loaded.');
        return;
    }

    try {
        var input = document.querySelector(selector);
        if (!input) {
            console.warn('Input element not found:', selector);
            return;
        }

        // Destroy existing Flatpickr instance if it exists
        if (input._flatpickr) {
            input._flatpickr.destroy();
        }

        // Default options
        var defaultOptions = {
            dateFormat: 'd/m/Y',
            allowInput: true,
            locale: 'es',
            // Strict validation: only allow valid dates
            onReady: function (selectedDates, dateStr, instance) {
                // Add custom validation
                instance.input.addEventListener('blur', function () {
                    if (this.value && !instance.selectedDates.length) {
                        // Invalid date entered, clear it
                        this.value = '';
                        this.setCustomValidity('Ingrese una fecha válida');
                        this.classList.add('is-invalid');
                    } else {
                        this.setCustomValidity('');
                        this.classList.remove('is-invalid');
                        if (this.value) {
                            this.classList.add('is-valid');
                        }
                    }
                });
            },
            onChange: function (selectedDates, dateStr, instance) {
                if (selectedDates.length > 0) {
                    instance.input.setCustomValidity('');
                    instance.input.classList.remove('is-invalid');
                    instance.input.classList.add('is-valid');
                }
            }
        };

        // Merge with provided options
        var flatpickrOptions = Object.assign({}, defaultOptions, options);

        // Initialize Flatpickr
        flatpickr(input, flatpickrOptions);
    } catch (error) {
        console.error('Error al inicializar Flatpickr:', error);
    }
};

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
