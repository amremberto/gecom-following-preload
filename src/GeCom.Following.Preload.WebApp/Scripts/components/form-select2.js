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

        // Helper function to apply validation classes to SELECT2
        var applyValidationClasses = function($selectElement) {
            var selectElement = $selectElement[0];
            
            // Get SELECT2 container using SELECT2 API (most reliable method)
            var $container = null;
            if ($selectElement.data('select2')) {
                var select2Data = $selectElement.data('select2');
                if (select2Data && select2Data.$container && select2Data.$container.length > 0) {
                    $container = select2Data.$container;
                }
            }
            
            // Fallback: try to find container by ID or next to select
            if (!$container || $container.length === 0) {
                var selectId = selectElement.id || '';
                if (selectId) {
                    // SELECT2 creates container with ID based on select ID
                    $container = $('#select2-' + selectId + '-container');
                }
                // If still not found, try next sibling
                if (!$container || $container.length === 0) {
                    $container = $selectElement.next('.select2-container');
                }
                // Last resort: search in parent
                if (!$container || $container.length === 0) {
                    $container = $selectElement.parent().find('.select2-container').first();
                }
            }
            
            // Remove both validation classes first from both SELECT and container
            $selectElement.removeClass('is-valid is-invalid');
            if ($container && $container.length > 0) {
                $container.removeClass('is-valid is-invalid');
            }
            
            // Check if field is required
            if (selectElement.hasAttribute('required')) {
                var value = $selectElement.val();
                if (value && value !== '' && value !== null) {
                    // Valid value selected - apply to both SELECT and container
                    $selectElement.addClass('is-valid');
                    $selectElement.removeClass('is-invalid');
                    if ($container && $container.length > 0) {
                        $container.addClass('is-valid');
                        $container.removeClass('is-invalid');
                    }
                    selectElement.setCustomValidity('');
                } else {
                    // No value selected (invalid for required field)
                    $selectElement.addClass('is-invalid');
                    $selectElement.removeClass('is-valid');
                    if ($container && $container.length > 0) {
                        $container.addClass('is-invalid');
                        $container.removeClass('is-valid');
                    }
                    selectElement.setCustomValidity('Por favor seleccione una opción');
                }
            } else {
                // Not required, but if has value, mark as valid
                var value = $selectElement.val();
                if (value && value !== '' && value !== null) {
                    $selectElement.addClass('is-valid');
                    if ($container && $container.length > 0) {
                        $container.addClass('is-valid');
                    }
                }
            }
        };

        // Add validation listeners for SELECT2
        // Apply Bootstrap validation classes when value changes
        $select.on('select2:select select2:clear', function () {
            var $selectElement = $(this);
            setTimeout(function () {
                applyValidationClasses($selectElement);
            }, 10);
        });

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
        
        // Validate initial value after a delay to ensure SELECT2 is fully initialized
        // SELECT2 needs time to create the container DOM element
        setTimeout(function () {
            applyValidationClasses($select);
        }, 300);
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
