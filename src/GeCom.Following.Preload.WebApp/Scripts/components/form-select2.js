/**
 * Theme: Adminto - Responsive Bootstrap 5 Admin Dashboard
 * Author: Coderthemes
 * Module/App: SELECT2 Form Components
 */

/**
 * Initializes SELECT2 for a select element
 * @param {string} selector - jQuery selector for the select element
 * @param {object} options - SELECT2 options (placeholder, allowClear, width, dropdownParent)
 * @param {string} selectedValue - Optional selected value to set after initialization
 */
window.initSelect2 = function (selector, options, selectedValue) {
    if (typeof $ === 'undefined') {
        console.error('jQuery is not loaded. SELECT2 requires jQuery.');
        return;
    }

    try {
        var $select = $(selector);
        if ($select.length === 0) {
            console.warn('Select element not found:', selector);
            return;
        }

        // Destroy existing SELECT2 instance if it exists
        if ($select.data('select2')) {
            $select.select2('destroy');
        }

        // Default options
        var defaultOptions = {
            placeholder: 'Seleccione una opción',
            allowClear: true,
            width: '100%',
            language: 'es'
        };

        // Merge with provided options
        var select2Options = Object.assign({}, defaultOptions, options);

        // Initialize SELECT2
        $select.select2(select2Options);

        // Set the selected value AFTER SELECT2 initialization
        if (selectedValue) {
            $select.val(selectedValue).trigger('change.select2');
        } else {
            // Try to get value from data attribute
            var dataValue = $select.attr('data-selected-value');
            if (dataValue) {
                $select.val(dataValue).trigger('change.select2');
            }
        }
    } catch (error) {
        console.error('Error al inicializar SELECT2:', error);
    }
};

/**
 * Destroys SELECT2 instance for a select element
 * @param {string} selector - jQuery selector for the select element
 */
window.destroySelect2 = function (selector) {
    if (typeof $ === 'undefined') {
        console.error('jQuery is not loaded');
        return;
    }

    try {
        var $select = $(selector);
        if ($select.length > 0 && $select.data('select2')) {
            $select.select2('destroy');
        }
    } catch (error) {
        console.error('Error al destruir SELECT2:', error);
    }
};

/**
 * Initializes SELECT2 for currency dropdown in edit document modal
 * @param {string} selectedValue - Optional selected currency code
 */
window.initCurrencySelect2 = function (selectedValue) {
    initSelect2('#currency-select-edit', {
        placeholder: 'Seleccione una moneda',
        allowClear: true,
        width: '100%',
        dropdownParent: $('#edit-document-modal')
    }, selectedValue);
};

/**
 * Initializes SELECT2 for document type dropdown in edit document modal
 * @param {string} selectedValue - Optional selected document type ID
 */
window.initDocumentTypeSelect2 = function (selectedValue) {
    initSelect2('#document-type-select-edit', {
        placeholder: 'Seleccione un tipo de documento',
        allowClear: true,
        width: '100%',
        dropdownParent: $('#edit-document-modal')
    }, selectedValue);
};

/**
 * Initializes SELECT2 for society dropdown in edit document modal
 * @param {string} selectedValue - Optional selected society CUIT
 */
window.initSocietySelect2 = function (selectedValue) {
    initSelect2('#society-select-edit', {
        placeholder: 'Seleccione un cliente',
        allowClear: true,
        width: '100%',
        dropdownParent: $('#edit-document-modal')
    }, selectedValue);
};

/**
 * Cleans up all SELECT2 instances in the edit document modal
 */
window.cleanupSelect2InModal = function () {
    destroySelect2('#currency-select-edit');
    destroySelect2('#document-type-select-edit');
    destroySelect2('#society-select-edit');
};
