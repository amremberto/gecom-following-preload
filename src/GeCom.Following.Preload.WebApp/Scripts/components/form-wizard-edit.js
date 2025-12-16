/**
 * Theme: Adminto - Responsive Bootstrap 5 Admin Dashboard
 * Author: Coderthemes
 * Module/App: Form Wizard with Validation for Edit Document Modal
 */

/**
 * Validates all required fields in the first tab of the document properties form
 * @returns {boolean} True if all required fields are valid, false otherwise
 */
function validateFirstTabFields() {
    var form = document.getElementById('documentPropertiesForm');
    if (!form) return false;

    var isValid = true;
    var requiredInputs = form.querySelectorAll('input[required], select[required]');

    requiredInputs.forEach(function (input) {
        // Skip validation for hidden or disabled fields
        if (input.offsetParent === null || input.disabled) {
            return;
        }

        // For SELECT2 selects, get value from SELECT2
        if (input.tagName === 'SELECT' && typeof $ !== 'undefined' && $(input).data('select2')) {
            var value = $(input).val();
            if (!value || value === '' || value === null) {
                isValid = false;
                input.classList.add('is-invalid');
                input.classList.remove('is-valid');
            } else {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
            }
        } else {
            // For regular inputs
            var value = input.value;
            
            // Special validation for CAE/CAI field - must have exactly 14 digits
            if (input.id === 'caecai-edit') {
                // Remove non-digit characters for validation
                var digitsOnly = value.replace(/\D/g, '');
                if (!value || value.trim() === '' || digitsOnly.length !== 14) {
                    isValid = false;
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                    if (input.nextElementSibling && input.nextElementSibling.classList.contains('invalid-feedback')) {
                        input.nextElementSibling.textContent = 'El nro. CAE/CAI debe tener exactamente 14 dígitos.';
                    }
                } else {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                }
            }
            // Special validation for Punto de Venta field - must have maximum 5 digits
            else if (input.id === 'punto-venta-edit') {
                // Remove non-digit characters for validation
                var digitsOnly = value.replace(/\D/g, '');
                if (!value || value.trim() === '' || digitsOnly.length === 0) {
                    isValid = false;
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                    if (input.nextElementSibling && input.nextElementSibling.classList.contains('invalid-feedback')) {
                        input.nextElementSibling.textContent = 'Por favor ingrese el punto de venta.';
                    }
                } else if (digitsOnly.length > 5) {
                    isValid = false;
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                    if (input.nextElementSibling && input.nextElementSibling.classList.contains('invalid-feedback')) {
                        input.nextElementSibling.textContent = 'El punto de venta no debe tener más de 5 dígitos.';
                    }
                } else {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                }
            }
            // Special validation for Número de Comprobante field - must have maximum 8 digits
            else if (input.id === 'numero-comprobante-edit') {
                // Remove non-digit characters for validation
                var digitsOnly = value.replace(/\D/g, '');
                if (!value || value.trim() === '' || digitsOnly.length === 0) {
                    isValid = false;
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                    if (input.nextElementSibling && input.nextElementSibling.classList.contains('invalid-feedback')) {
                        input.nextElementSibling.textContent = 'Por favor ingrese el número de comprobante.';
                    }
                } else if (digitsOnly.length > 8) {
                    isValid = false;
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                    if (input.nextElementSibling && input.nextElementSibling.classList.contains('invalid-feedback')) {
                        input.nextElementSibling.textContent = 'El número de comprobante no debe tener más de 8 dígitos.';
                    }
                } else {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                }
            }
            // Special validation for date fields using Flatpickr
            else if (input.id === 'fecha-factura-edit' || input.id === 'venc-caecai-edit') {
                // Check if Flatpickr instance exists
                var flatpickrInput = input.id === 'fecha-factura-edit' 
                    ? document.querySelector('#fecha-factura-edit')
                    : document.querySelector('#venc-caecai-edit');
                
                if (flatpickrInput && flatpickrInput._flatpickr) {
                    var altInput = flatpickrInput._flatpickr.altInput;
                    if (altInput) {
                        var dateValue = altInput.value;
                        if (!dateValue || dateValue.trim() === '') {
                            isValid = false;
                            altInput.classList.add('is-invalid');
                            altInput.classList.remove('is-valid');
                        } else {
                            // Validate date constraints
                            var dateValid = true;
                            
                            // Validate fecha factura - cannot be greater than today
                            if (input.id === 'fecha-factura-edit') {
                                if (flatpickrInput._flatpickr.selectedDates.length > 0) {
                                    var fechaFacturaDate = flatpickrInput._flatpickr.selectedDates[0];
                                    var today = new Date();
                                    today.setHours(0, 0, 0, 0);
                                    fechaFacturaDate.setHours(0, 0, 0, 0);
                                    if (fechaFacturaDate > today) {
                                        dateValid = false;
                                        altInput.setCustomValidity('La fecha de factura no puede ser mayor al día actual.');
                                    } else {
                                        altInput.setCustomValidity('');
                                    }
                                }
                            }
                            
                            // Validate vencimiento CAE/CAI - cannot be before fecha factura
                            if (input.id === 'venc-caecai-edit') {
                                var fechaFacturaInput = document.querySelector('#fecha-factura-edit');
                                if (fechaFacturaInput && fechaFacturaInput._flatpickr && fechaFacturaInput._flatpickr.selectedDates.length > 0) {
                                    var fechaFacturaDate = fechaFacturaInput._flatpickr.selectedDates[0];
                                    if (flatpickrInput._flatpickr.selectedDates.length > 0) {
                                        var vencCaecaiDate = flatpickrInput._flatpickr.selectedDates[0];
                                        fechaFacturaDate.setHours(0, 0, 0, 0);
                                        vencCaecaiDate.setHours(0, 0, 0, 0);
                                        if (vencCaecaiDate < fechaFacturaDate) {
                                            dateValid = false;
                                            altInput.setCustomValidity('La fecha Venc. CAE/CAI no puede ser anterior a la fecha de factura.');
                                        } else {
                                            altInput.setCustomValidity('');
                                        }
                                    }
                                }
                            }
                            
                            if (!dateValid || altInput.classList.contains('is-invalid')) {
                                isValid = false;
                                altInput.classList.add('is-invalid');
                                altInput.classList.remove('is-valid');
                            } else {
                                altInput.classList.remove('is-invalid');
                                altInput.classList.add('is-valid');
                            }
                        }
                    }
                } else {
                    // Fallback validation if Flatpickr is not initialized
                    if (!value || value.trim() === '') {
                        isValid = false;
                        input.classList.add('is-invalid');
                        input.classList.remove('is-valid');
                    } else {
                        input.classList.remove('is-invalid');
                        input.classList.add('is-valid');
                    }
                }
            }
            else if (input.type === 'number') {
                if (!value || value === '' || parseFloat(value) <= 0) {
                    isValid = false;
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                } else {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                }
            } else {
                if (!value || value.trim() === '') {
                    isValid = false;
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                } else {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                }
            }
        }
    });

    return isValid;
}

/**
 * Updates the state of tabs (enabled/disabled) based on validation
 * @param {HTMLElement} wizardElement - The wizard container element
 * @param {boolean} isPendingPreload - Whether the document is in "Pendiente Precarga" status
 */
function updateTabsState(wizardElement, isPendingPreload) {
    if (!isPendingPreload) return;

    var isValid = validateFirstTabFields();
    var navLinks = wizardElement.querySelectorAll('.nav-link');

    for (var i = 1; i < navLinks.length; i++) {
        if (isValid) {
            navLinks[i].classList.remove('disabled');
            navLinks[i].style.pointerEvents = '';
            navLinks[i].style.opacity = '';
            navLinks[i].style.cursor = '';
        } else {
            navLinks[i].classList.add('disabled');
            navLinks[i].style.pointerEvents = 'none';
            navLinks[i].style.opacity = '0.5';
            navLinks[i].style.cursor = 'not-allowed';
        }
    }
    
    // Update warning alert visibility
    updateWarningAlertVisibility(isValid);
}

/**
 * Updates the visibility of the first tab warning alert
 * @param {boolean} isValid - Whether all required fields are valid
 */
function updateWarningAlertVisibility(isValid) {
    var warningAlert = document.getElementById('first-tab-warning-alert');
    if (warningAlert) {
        if (isValid) {
            // Hide the alert if all fields are valid
            warningAlert.style.display = 'none';
        } else {
            // Show the alert if any field is invalid
            warningAlert.style.display = 'block';
        }
    }
}

/**
 * Validates the CAE/CAI field - must have exactly 14 digits
 * @param {HTMLElement} input - The input element to validate
 * @returns {boolean} True if valid, false otherwise
 */
function validateCaecaiField(input) {
    if (!input || input.id !== 'caecai-edit') {
        return true;
    }

    var value = input.value || '';
    // Remove non-digit characters for validation
    var digitsOnly = value.replace(/\D/g, '');
    
    if (!value || value.trim() === '') {
        if (input.hasAttribute('required')) {
            input.setCustomValidity('Por favor ingrese el número de CAE / CAI.');
            input.classList.add('is-invalid');
            input.classList.remove('is-valid');
            return false;
        }
        input.setCustomValidity('');
        input.classList.remove('is-invalid');
        input.classList.remove('is-valid');
        return true;
    }

    if (digitsOnly.length !== 14) {
        input.setCustomValidity('El nro. CAE/CAI debe tener exactamente 14 dígitos.');
        input.classList.add('is-invalid');
        input.classList.remove('is-valid');
        // Update invalid-feedback message
        var feedback = input.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = 'El nro. CAE/CAI debe tener exactamente 14 dígitos.';
        }
        return false;
    }

    input.setCustomValidity('');
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    return true;
}

/**
 * Validates the Punto de Venta field - must have maximum 5 digits
 * @param {HTMLElement} input - The input element to validate
 * @returns {boolean} True if valid, false otherwise
 */
function validatePuntoDeVentaField(input) {
    if (!input || input.id !== 'punto-venta-edit') {
        return true;
    }

    var value = input.value || '';
    // Remove non-digit characters for validation
    var digitsOnly = value.replace(/\D/g, '');
    
    if (!value || value.trim() === '' || digitsOnly.length === 0) {
        if (input.hasAttribute('required')) {
            input.setCustomValidity('Por favor ingrese el punto de venta.');
            input.classList.add('is-invalid');
            input.classList.remove('is-valid');
            // Update invalid-feedback message
            var feedback = input.nextElementSibling;
            if (feedback && feedback.classList.contains('invalid-feedback')) {
                feedback.textContent = 'Por favor ingrese el punto de venta.';
            }
            return false;
        }
        input.setCustomValidity('');
        input.classList.remove('is-invalid');
        input.classList.remove('is-valid');
        return true;
    }

    if (digitsOnly.length > 5) {
        input.setCustomValidity('El punto de venta no debe tener más de 5 dígitos.');
        input.classList.add('is-invalid');
        input.classList.remove('is-valid');
        // Update invalid-feedback message
        var feedback = input.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = 'El punto de venta no debe tener más de 5 dígitos.';
        }
        return false;
    }

    input.setCustomValidity('');
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    return true;
}

/**
 * Validates the Número de Comprobante field - must have maximum 8 digits
 * @param {HTMLElement} input - The input element to validate
 * @returns {boolean} True if valid, false otherwise
 */
function validateNumeroComprobanteField(input) {
    if (!input || input.id !== 'numero-comprobante-edit') {
        return true;
    }

    var value = input.value || '';
    // Remove non-digit characters for validation
    var digitsOnly = value.replace(/\D/g, '');
    
    if (!value || value.trim() === '' || digitsOnly.length === 0) {
        if (input.hasAttribute('required')) {
            input.setCustomValidity('Por favor ingrese el número de comprobante.');
            input.classList.add('is-invalid');
            input.classList.remove('is-valid');
            // Update invalid-feedback message
            var feedback = input.nextElementSibling;
            if (feedback && feedback.classList.contains('invalid-feedback')) {
                feedback.textContent = 'Por favor ingrese el número de comprobante.';
            }
            return false;
        }
        input.setCustomValidity('');
        input.classList.remove('is-invalid');
        input.classList.remove('is-valid');
        return true;
    }

    if (digitsOnly.length > 8) {
        input.setCustomValidity('El número de comprobante no debe tener más de 8 dígitos.');
        input.classList.add('is-invalid');
        input.classList.remove('is-valid');
        // Update invalid-feedback message
        var feedback = input.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = 'El número de comprobante no debe tener más de 8 dígitos.';
        }
        return false;
    }

    input.setCustomValidity('');
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    return true;
}

/**
 * Validates a single field
 * @param {HTMLElement} input - The input element to validate
 * @returns {boolean} True if valid, false otherwise
 */
function validateField(input) {
    // Special validation for CAE/CAI field
    if (input.id === 'caecai-edit') {
        return validateCaecaiField(input);
    }
    
    // Special validation for Punto de Venta field
    if (input.id === 'punto-venta-edit') {
        return validatePuntoDeVentaField(input);
    }
    
    // Special validation for Número de Comprobante field
    if (input.id === 'numero-comprobante-edit') {
        return validateNumeroComprobanteField(input);
    }
    
    if (input.hasAttribute('required')) {
        if (input.type === 'text' || input.type === 'number') {
            if (!input.value || input.value.trim() === '') {
                input.setCustomValidity('Este campo es obligatorio');
                input.classList.add('is-invalid');
                input.classList.remove('is-valid');
                return false;
            } else {
                input.setCustomValidity('');
                input.classList.remove('is-invalid');
                if (input.checkValidity()) {
                    input.classList.add('is-valid');
                }
                return true;
            }
        } else if (input.tagName === 'SELECT') {
            if (!input.value || input.value === '') {
                input.setCustomValidity('Por favor seleccione una opción');
                input.classList.add('is-invalid');
                input.classList.remove('is-valid');
                return false;
            } else {
                input.setCustomValidity('');
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
                return true;
            }
        }
    }
    return true;
}

/**
 * Sets up validation listeners for form fields and change detection
 * @param {HTMLElement} form - The form element
 * @param {boolean} isPendingPreload - Whether the document is in "Pendiente Precarga" status
 * @param {HTMLElement} wizardElement - The wizard container element
 * @param {Function} getInitialState - Function that returns the initial state of form fields for change detection
 */
function setupFormValidation(form, isPendingPreload, wizardElement, getInitialState) {
    if (!form) return;

    // Function to update tab restrictions based on form changes and validation
    function updateTabRestrictionsOnChange() {
        var initialState = getInitialState ? getInitialState() : null;
        var isValid = validateFirstTabFields();
        var hasChanges = initialState ? hasFormChanged(form, initialState) : false;
        var tabLinks = wizardElement.querySelectorAll('.nav-link');
        
        // Block tabs if required fields are not valid (regardless of status or changes)
        if (!isValid) {
            tabLinks.forEach(function (link, index) {
                if (index > 0) {
                    // Disable direct clicking on tabs when required fields are not valid
                    link.style.pointerEvents = 'none';
                    link.style.opacity = '0.5';
                    link.style.cursor = 'not-allowed';
                }
            });
        }
        // Block tabs if form has changes (only when not pending preload, as pending preload has its own validation)
        else if (hasChanges && !isPendingPreload) {
            tabLinks.forEach(function (link, index) {
                if (index > 0) {
                    // Disable direct clicking on tabs when there are changes
                    link.style.pointerEvents = 'none';
                    link.style.opacity = '0.5';
                    link.style.cursor = 'not-allowed';
                }
            });
        } 
        // Re-enable tabs if fields are valid and no changes (or if pending preload and valid)
        else {
            tabLinks.forEach(function (link, index) {
                if (index > 0) {
                    // Only re-enable if not pending preload, or if pending preload and valid
                    if (!isPendingPreload || (isPendingPreload && isValid)) {
                        link.style.pointerEvents = '';
                        link.style.opacity = '';
                        link.style.cursor = '';
                    }
                }
            });
        }
    }

    // Validate all required fields on initialization
    var requiredInputs = form.querySelectorAll('input[required], select[required]');
    requiredInputs.forEach(function (input) {
        // Validate on initialization
        validateField(input);

        // For SELECT2 selects, listen to SELECT2 events without interfering
        if (input.tagName === 'SELECT' && typeof $ !== 'undefined' && $(input).data('select2')) {
            $(input).on('select2:select select2:clear', function () {
                // Small delay to ensure SELECT2 has updated the native select value
                var selectElement = this;
                setTimeout(function () {
                    validateField(selectElement);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        // Still update warning alert visibility even if not pending preload
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                }, 10);
            });
        } else {
            // Special handling for CAE/CAI field - validate 14 digits
            if (input.id === 'caecai-edit') {
                // Only allow digits
                input.addEventListener('input', function (e) {
                    // Remove non-digit characters
                    var value = this.value.replace(/\D/g, '');
                    // Limit to 14 digits
                    if (value.length > 14) {
                        value = value.substring(0, 14);
                    }
                    this.value = value;
                    validateCaecaiField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                input.addEventListener('blur', function () {
                    validateCaecaiField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                // Prevent non-digit characters on keypress
                input.addEventListener('keypress', function (e) {
                    var char = String.fromCharCode(e.which);
                    if (!/[0-9]/.test(char)) {
                        e.preventDefault();
                    }
                });
            }
            // Special handling for Punto de Venta field - validate maximum 5 digits
            else if (input.id === 'punto-venta-edit') {
                // Only allow digits
                input.addEventListener('input', function (e) {
                    // Remove non-digit characters
                    var value = this.value.replace(/\D/g, '');
                    // Limit to 5 digits
                    if (value.length > 5) {
                        value = value.substring(0, 5);
                    }
                    this.value = value;
                    validatePuntoDeVentaField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                input.addEventListener('blur', function () {
                    validatePuntoDeVentaField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                // Prevent non-digit characters on keypress
                input.addEventListener('keypress', function (e) {
                    var char = String.fromCharCode(e.which);
                    if (!/[0-9]/.test(char)) {
                        e.preventDefault();
                    }
                });
            }
            // Special handling for Número de Comprobante field - validate maximum 8 digits
            else if (input.id === 'numero-comprobante-edit') {
                // Only allow digits
                input.addEventListener('input', function (e) {
                    // Remove non-digit characters
                    var value = this.value.replace(/\D/g, '');
                    // Limit to 8 digits
                    if (value.length > 8) {
                        value = value.substring(0, 8);
                    }
                    this.value = value;
                    validateNumeroComprobanteField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                input.addEventListener('blur', function () {
                    validateNumeroComprobanteField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                // Prevent non-digit characters on keypress
                input.addEventListener('keypress', function (e) {
                    var char = String.fromCharCode(e.which);
                    if (!/[0-9]/.test(char)) {
                        e.preventDefault();
                    }
                });
            }
            // Special handling for date fields - validate date constraints
            else if (input.id === 'fecha-factura-edit' || input.id === 'venc-caecai-edit') {
                // Date fields are handled by Flatpickr, but we add listeners to the altInput
                setTimeout(function() {
                    var flatpickrInput = input.id === 'fecha-factura-edit' 
                        ? document.querySelector('#fecha-factura-edit')
                        : document.querySelector('#venc-caecai-edit');
                    
                    if (flatpickrInput && flatpickrInput._flatpickr && flatpickrInput._flatpickr.altInput) {
                        var altInput = flatpickrInput._flatpickr.altInput;
                        
                        altInput.addEventListener('blur', function () {
                            validateFirstTabFields();
                            updateTabRestrictionsOnChange();
                            if (isPendingPreload) {
                                updateTabsState(wizardElement, isPendingPreload);
                            } else {
                                var isValid = validateFirstTabFields();
                                updateWarningAlertVisibility(isValid);
                            }
                        });

                        // Listen for changes in fecha-factura to update venc-caecai minDate
                        if (input.id === 'fecha-factura-edit') {
                            flatpickrInput._flatpickr.config.onChange.push(function(selectedDates) {
                                var vencInput = document.querySelector('#venc-caecai-edit');
                                if (vencInput && vencInput._flatpickr && selectedDates.length > 0) {
                                    vencInput._flatpickr.set('minDate', selectedDates[0]);
                                    // Re-validate venc-caecai field
                                    validateFirstTabFields();
                                    updateTabRestrictionsOnChange();
                                    if (isPendingPreload) {
                                        updateTabsState(wizardElement, isPendingPreload);
                                    } else {
                                        var isValid = validateFirstTabFields();
                                        updateWarningAlertVisibility(isValid);
                                    }
                                }
                            });
                        }
                    }
                }, 500);
            }
            // For regular inputs and selects without SELECT2
            else {
                input.addEventListener('blur', function () {
                    validateField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                // Validate on input change
                input.addEventListener('input', function () {
                    validateField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });

                // Validate on change (for selects)
                input.addEventListener('change', function () {
                    validateField(this);
                    updateTabRestrictionsOnChange();
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    } else {
                        var isValid = validateFirstTabFields();
                        updateWarningAlertVisibility(isValid);
                    }
                });
            }
        }
    });

    // Also listen for SELECT2 initialization after wizard is created
    // This handles cases where SELECT2 is initialized after the wizard
    setTimeout(function () {
        form.querySelectorAll('select[required]').forEach(function (selectElement) {
            var $select = typeof $ !== 'undefined' ? $(selectElement) : null;
            if ($select && $select.data('select2')) {
                // Only add validation listener, don't interfere with SELECT2's native change event
                $select.on('select2:select select2:clear', function () {
                    // Small delay to ensure SELECT2 has updated the native select value
                    setTimeout(function () {
                        validateField(selectElement);
                        updateTabRestrictionsOnChange();
                        if (isPendingPreload) {
                            updateTabsState(wizardElement, isPendingPreload);
                        } else {
                            var isValid = validateFirstTabFields();
                            updateWarningAlertVisibility(isValid);
                        }
                    }, 10);
                });
            }
        });
    }, 500);
}

/**
 * Shows a validation toast message using DotNet interop
 * @param {DotNetObjectReference} dotNetReference - Reference to the C# component
 * @param {string} message - The message to display
 */
function showValidationToast(dotNetReference, message) {
    if (dotNetReference && typeof dotNetReference.invokeMethodAsync === 'function') {
        dotNetReference.invokeMethodAsync('ShowValidationToast', message).catch(function (error) {
            console.error('Error al mostrar toast:', error);
            // Fallback a alert si falla el toast
            alert(message);
        });
    } else {
        // Fallback a alert si no hay referencia DotNet
        alert(message);
    }
}

/**
 * Gets form data for updating document
 * @param {HTMLElement} form - The form element
 * @returns {Object} Object containing form field values
 */
function getFormData(form) {
    if (!form) return {};

    var formData = {};
    var inputs = form.querySelectorAll('input, select, textarea');

    inputs.forEach(function (input) {
        var fieldId = input.id || input.name;
        if (!fieldId) return;

        // For SELECT2 selects, get value from SELECT2
        if (input.tagName === 'SELECT' && typeof $ !== 'undefined' && $(input).data('select2')) {
            formData[fieldId] = $(input).val() || '';
        }
        // For Flatpickr date fields, get value from altInput and convert to yyyy-MM-dd format
        else if (input.id === 'fecha-factura-edit' || input.id === 'venc-caecai-edit') {
            if (input._flatpickr && input._flatpickr.selectedDates && input._flatpickr.selectedDates.length > 0) {
                var date = input._flatpickr.selectedDates[0];
                var year = date.getFullYear();
                var month = String(date.getMonth() + 1).padStart(2, '0');
                var day = String(date.getDate()).padStart(2, '0');
                formData[fieldId] = year + '-' + month + '-' + day;
            } else {
                formData[fieldId] = '';
            }
        }
        // For regular inputs
        else {
            formData[fieldId] = input.value || '';
        }
    });

    // Map field IDs to request parameter names
    var result = {
        docId: 0, // Will be set from document
        sociedadCuit: formData['society-select-edit'] || '',
        tipoDocId: formData['document-type-select-edit'] ? parseInt(formData['document-type-select-edit']) : null,
        puntoDeVenta: formData['punto-venta-edit'] || '',
        numeroComprobante: formData['numero-comprobante-edit'] || '',
        fechaEmisionComprobante: formData['fecha-factura-edit'] || null,
        moneda: formData['currency-select-edit'] || '',
        montoBruto: formData['monto-bruto-edit'] ? parseFloat(formData['monto-bruto-edit']) : null,
        caecai: formData['caecai-edit'] || '',
        vencimientoCaecai: formData['venc-caecai-edit'] || null
    };

    return result;
}

/**
 * Stores the initial state of form fields for change detection
 * @param {HTMLElement} form - The form element
 * @returns {Object} Object containing initial field values
 */
function saveInitialFormState(form) {
    if (!form) return {};

    var initialState = {};
    var inputs = form.querySelectorAll('input, select, textarea');

    inputs.forEach(function (input) {
        var fieldId = input.id || input.name;
        if (!fieldId) return;

        // For SELECT2 selects, get value from SELECT2
        if (input.tagName === 'SELECT' && typeof $ !== 'undefined' && $(input).data('select2')) {
            initialState[fieldId] = $(input).val() || '';
        }
        // For Flatpickr date fields, get value from altInput
        else if (input.id === 'fecha-factura-edit' || input.id === 'venc-caecai-edit') {
            if (input._flatpickr && input._flatpickr.altInput) {
                initialState[fieldId] = input._flatpickr.altInput.value || '';
            } else {
                initialState[fieldId] = input.value || '';
            }
        }
        // For regular inputs
        else {
            initialState[fieldId] = input.value || '';
        }
    });

    return initialState;
}

/**
 * Detects if any form field has changed compared to initial state
 * @param {HTMLElement} form - The form element
 * @param {Object} initialState - The initial state of fields
 * @returns {boolean} True if any field has changed, false otherwise
 */
function hasFormChanged(form, initialState) {
    if (!form || !initialState) return false;

    var inputs = form.querySelectorAll('input, select, textarea');

    for (var i = 0; i < inputs.length; i++) {
        var input = inputs[i];
        var fieldId = input.id || input.name;
        if (!fieldId || !initialState.hasOwnProperty(fieldId)) continue;

        var currentValue = '';
        var initialValue = initialState[fieldId] || '';

        // For SELECT2 selects, get value from SELECT2
        if (input.tagName === 'SELECT' && typeof $ !== 'undefined' && $(input).data('select2')) {
            currentValue = $(input).val() || '';
        }
        // For Flatpickr date fields, get value from altInput
        else if (input.id === 'fecha-factura-edit' || input.id === 'venc-caecai-edit') {
            if (input._flatpickr && input._flatpickr.altInput) {
                currentValue = input._flatpickr.altInput.value || '';
            } else {
                currentValue = input.value || '';
            }
        }
        // For regular inputs
        else {
            currentValue = input.value || '';
        }

        // Compare values (normalize empty strings and null/undefined)
        var normalizedCurrent = (currentValue || '').toString().trim();
        var normalizedInitial = (initialValue || '').toString().trim();

        if (normalizedCurrent !== normalizedInitial) {
            return true;
        }
    }

    return false;
}

/**
 * Sets up navigation restrictions for "Pendiente Precarga" status and when form has changes
 * @param {HTMLElement} wizardElement - The wizard container element
 * @param {boolean} isPendingPreload - Whether the document is in "Pendiente Precarga" status
 * @param {DotNetObjectReference} dotNetReference - Reference to the C# component for showing toast
 * @param {Function} getInitialState - Function that returns the initial state of form fields for change detection
 * @param {Function} updateInitialState - Function to update the initial state
 */
function setupNavigationRestrictions(wizardElement, isPendingPreload, dotNetReference, getInitialState, updateInitialState) {
    var validationMessage = 'Por favor debe completar todos los campos requeridos del documento para poder avanzar.';
    var changeMessage = 'Debe usar el botón "Siguiente" para avanzar cuando hay cambios en los campos.';

    // Initially disable tabs after the first one and update warning alert visibility
    if (isPendingPreload) {
        updateTabsState(wizardElement, isPendingPreload);
    }
    
    // Also check initial state for warning alert (in case not pending preload)
    if (!isPendingPreload) {
        var isValid = validateFirstTabFields();
        updateWarningAlertVisibility(isValid);
    }

    // Function to check if navigation should be blocked
    function shouldBlockNavigation() {
        var form = document.getElementById('documentPropertiesForm');
        if (!form) return false;

        // Block if pending preload and fields are not valid
        if (isPendingPreload && !validateFirstTabFields()) {
            return true;
        }

        // Block if form has changes (only for tabs after the first one)
        var initialState = getInitialState ? getInitialState() : null;
        if (initialState && hasFormChanged(form, initialState)) {
            return true;
        }

        return false;
    }

    // Flag to track if navigation is coming from "Siguiente" button
    var isNavigatingFromNextButton = false;

    // Prevent navigation to other tabs when clicking on disabled tabs or when form has changes
    var tabLinks = wizardElement.querySelectorAll('.nav-link');
    tabLinks.forEach(function (link, index) {
        if (index > 0) {
            // Use capture phase to intercept before Bootstrap handles it
            link.addEventListener('click', function (e) {
                var form = document.getElementById('documentPropertiesForm');
                if (!form) return;

                // Always block if required fields are not valid (regardless of status)
                if (!validateFirstTabFields()) {
                    e.preventDefault();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    showValidationToast(dotNetReference, validationMessage);
                    return false;
                }

                // Block navigation if form has changes (only when clicking on tab titles, not from "Siguiente" button)
                if (!isNavigatingFromNextButton) {
                    var initialState = getInitialState ? getInitialState() : null;
                    if (initialState && hasFormChanged(form, initialState)) {
                        e.preventDefault();
                        e.stopPropagation();
                        e.stopImmediatePropagation();
                        showValidationToast(dotNetReference, changeMessage);
                        return false;
                    }
                }
            }, true);

            // Also intercept Bootstrap tab events
            link.addEventListener('show.bs.tab', function (e) {
                var form = document.getElementById('documentPropertiesForm');
                if (!form) return;

                // Always block if required fields are not valid (regardless of status)
                if (!validateFirstTabFields()) {
                    e.preventDefault();
                    showValidationToast(dotNetReference, validationMessage);
                    return false;
                }

                // Block navigation if form has changes (only when clicking on tab titles, not from "Siguiente" button)
                if (!isNavigatingFromNextButton) {
                    var initialState = getInitialState ? getInitialState() : null;
                    if (initialState && hasFormChanged(form, initialState)) {
                        e.preventDefault();
                        showValidationToast(dotNetReference, changeMessage);
                        return false;
                    }
                }
            });
        }
    });

    // Intercept wizard navigation buttons
    var nextButton = wizardElement.querySelector('.next a');
    if (nextButton) {
        // Use capture phase to intercept before wizard handles it
        nextButton.addEventListener('click', async function (e) {
            var activeTab = wizardElement.querySelector('.nav-link.active');
            var activeTabHref = activeTab ? activeTab.getAttribute('href') : null;
            var form = document.getElementById('documentPropertiesForm');
            if (!form) return;

            // Only check for changes and validate when on the first tab (properties)
            if (activeTabHref === '#document-properties-step') {
                // Always validate required fields before allowing navigation
                if (!validateFirstTabFields()) {
                    e.preventDefault();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    showValidationToast(dotNetReference, validationMessage);
                    return false;
                }

                // Check if form has changes
                var initialState = getInitialState ? getInitialState() : null;
                var hasChanges = initialState ? hasFormChanged(form, initialState) : false;

                if (hasChanges) {
                    // Prevent default navigation
                    e.preventDefault();
                    e.stopPropagation();
                    e.stopImmediatePropagation();

                    // Show confirmation dialog using C# component
                    var confirmed = false;
                    if (dotNetReference && typeof dotNetReference.invokeMethodAsync === 'function') {
                        try {
                            confirmed = await dotNetReference.invokeMethodAsync('ShowConfirmationDialog',
                                'Ha realizado cambios en los campos del documento. ¿Desea guardar estos cambios antes de continuar?\n\nPresione "Aceptar" para guardar y continuar, o "Cancelar" para permanecer en esta pestaña.');
                        } catch (error) {
                            console.error('Error al mostrar diálogo de confirmación:', error);
                            // Fallback a confirm nativo si falla
                            confirmed = confirm('Ha realizado cambios en los campos del documento. ¿Desea guardar estos cambios antes de continuar?\n\nPresione "Aceptar" para guardar y continuar, o "Cancelar" para permanecer en esta pestaña.');
                        }
                    } else {
                        // Fallback a confirm nativo si no hay referencia DotNet
                        confirmed = confirm('Ha realizado cambios en los campos del documento. ¿Desea guardar estos cambios antes de continuar?\n\nPresione "Aceptar" para guardar y continuar, o "Cancelar" para permanecer en esta pestaña.');
                    }

                    if (confirmed) {
                        // User confirmed - update document
                        try {
                            // Get form values
                            var formData = getFormData(form);
                            
                            // Get docId from wizard element data attribute or from C# method
                            var docId = wizardElement.getAttribute('data-doc-id') ? 
                                parseInt(wizardElement.getAttribute('data-doc-id')) : 0;
                            
                            if (!docId) {
                                showValidationToast(dotNetReference, 'Error: No se pudo obtener el ID del documento.');
                                return false;
                            }
                            
                            // Call C# method to update document
                            if (dotNetReference && typeof dotNetReference.invokeMethodAsync === 'function') {
                                var result = await dotNetReference.invokeMethodAsync('UpdateDocumentFromWizard',
                                    docId,
                                    formData.sociedadCuit,
                                    formData.tipoDocId,
                                    formData.puntoDeVenta,
                                    formData.numeroComprobante,
                                    formData.fechaEmisionComprobante,
                                    formData.moneda,
                                    formData.montoBruto,
                                    formData.caecai,
                                    formData.vencimientoCaecai);

                                if (result && result.success) {
                                    // Update successful - update initial state and allow navigation
                                    // Wait a bit for form to update from C# (StateHasChanged)
                                    setTimeout(function() {
                                        // Save new initial state after update
                                        // Wait for DOM to update with new values from C#
                                        setTimeout(function() {
                                            var updatedState = saveInitialFormState(form);
                                            
                                            // Update the initial state using the update function
                                            if (updateInitialState) {
                                                updateInitialState(updatedState);
                                            }

                                            // Show success message
                                            showValidationToast(dotNetReference, result.message || 'Documento actualizado exitosamente.');

                                            // Set flag to allow navigation
                                            isNavigatingFromNextButton = true;
                                            
                                            // Trigger wizard navigation programmatically
                                            setTimeout(function() {
                                                // Find next tab and activate it
                                                var currentTabIndex = Array.from(wizardElement.querySelectorAll('.nav-link')).indexOf(activeTab);
                                                var nextTabLink = wizardElement.querySelectorAll('.nav-link')[currentTabIndex + 1];
                                                if (nextTabLink) {
                                                    // Use Bootstrap tab API to switch
                                                    var nextTab = new bootstrap.Tab(nextTabLink);
                                                    nextTab.show();
                                                }
                                                
                                                // Reset flag after navigation completes
                                                setTimeout(function() {
                                                    isNavigatingFromNextButton = false;
                                                }, 200);
                                            }, 100);
                                        }, 500); // Wait for C# StateHasChanged to update DOM
                                    }, 100);
                                } else {
                                    // Update failed - show error and prevent navigation
                                    showValidationToast(dotNetReference, result.message || 'Error al actualizar el documento. No se pudo avanzar.');
                                }
                            } else {
                                showValidationToast(dotNetReference, 'Error: No se pudo comunicar con el servidor para actualizar el documento.');
                            }
                        } catch (error) {
                            console.error('Error al actualizar documento:', error);
                            showValidationToast(dotNetReference, 'Error al actualizar el documento: ' + (error.message || 'Error desconocido'));
                        }
                    }
                    // If user cancelled, do nothing (stay on current tab)
                    return false;
                } else {
                    // No changes - allow normal navigation
                    isNavigatingFromNextButton = true;
                    
                    // Reset flag after a short delay to allow the tab change to complete
                    setTimeout(function() {
                        isNavigatingFromNextButton = false;
                    }, 100);
                }
            } else {
                // Not on first tab - allow normal navigation without checking for changes
                isNavigatingFromNextButton = true;
                
                // Reset flag after a short delay to allow the tab change to complete
                setTimeout(function() {
                    isNavigatingFromNextButton = false;
                }, 100);
            }
        }, true);
    }

    // Intercept Bootstrap tab show event globally for this wizard
    wizardElement.addEventListener('show.bs.tab', function (e) {
        var form = document.getElementById('documentPropertiesForm');
        if (!form) return;

        var targetTab = e.target;
        var targetHref = targetTab.getAttribute('href');

        // If trying to navigate away from first tab
        if (targetHref !== '#document-properties-step') {
            // Always block if required fields are not valid (regardless of status)
            if (!validateFirstTabFields()) {
                e.preventDefault();
                showValidationToast(dotNetReference, validationMessage);
                return false;
            }

            // Block navigation if form has changes (only when clicking on tab titles, not from "Siguiente" button)
            if (!isNavigatingFromNextButton) {
                var initialState = getInitialState ? getInitialState() : null;
                if (initialState && hasFormChanged(form, initialState)) {
                    e.preventDefault();
                    showValidationToast(dotNetReference, changeMessage);
                    return false;
                }
            }
        }
    });
}

/**
 * Initializes the edit document wizard with validation
 * @param {boolean} isPendingPreload - Whether the document is in "Pendiente Precarga" status
 * @param {DotNetObjectReference} dotNetReference - Reference to the C# component for showing toast
 * @param {number} docId - The document ID
 */
window.initEditDocumentWizard = function (isPendingPreload, dotNetReference, docId) {
    // Store initial state in a variable accessible to all functions
    // Use an object to allow updating the reference
    var initialStateContainer = { value: null };
    
    // Function to update the initial state (accessible from nested functions)
    var updateInitialState = function(newState) {
        initialStateContainer.value = newState;
    };

    function initWizard() {
        var wizardElement = document.getElementById('edit-document-wizard');
        if (!wizardElement) {
            console.warn('Wizard element not found');
            return false;
        }

        // Check if Wizard class is available
        if (typeof Wizard === 'undefined') {
            console.warn('Wizard class not found, retrying...');
            return false;
        }

        try {
            // Store docId in wizard element for later use
            if (docId) {
                wizardElement.setAttribute('data-doc-id', docId.toString());
            }

            // Create new wizard instance with validation
            var wizard = new Wizard('#edit-document-wizard', {
                validate: true
            });

            // Get form element
            var form = document.getElementById('documentPropertiesForm');

            // Set up navigation restrictions (initialState will be set later)
            // Pass both getter and setter functions
            setupNavigationRestrictions(wizardElement, isPendingPreload, dotNetReference, 
                function() {
                    return initialStateContainer.value;
                },
                updateInitialState
            );

            // Set up form validation (initialState will be set later)
            if (form) {
                setupFormValidation(form, isPendingPreload, wizardElement, function() {
                    return initialStateContainer.value;
                });
            }

            // Initial validation and tabs state update after a delay to ensure SELECT2 and Flatpickr are initialized
            setTimeout(function () {
                // Save initial state now that SELECT2 and Flatpickr are initialized
                if (form) {
                    initialStateContainer.value = saveInitialFormState(form);
                    
                    // Update tab restrictions based on initial validation state
                    // This will block tabs if required fields are not complete
                    var isValid = validateFirstTabFields();
                    var tabLinks = wizardElement.querySelectorAll('.nav-link');
                    
                    // Block tabs if required fields are not valid (regardless of status)
                    if (!isValid) {
                        tabLinks.forEach(function (link, index) {
                            if (index > 0) {
                                link.style.pointerEvents = 'none';
                                link.style.opacity = '0.5';
                                link.style.cursor = 'not-allowed';
                            }
                        });
                    }
                }

                if (isPendingPreload) {
                    updateTabsState(wizardElement, isPendingPreload);
                } else {
                    // Check initial state for warning alert visibility even if not pending preload
                    var isValid = validateFirstTabFields();
                    updateWarningAlertVisibility(isValid);
                }
                // Update save button visibility on initialization
                updateSaveButtonVisibility(wizardElement);
            }, 800);

            // Set up event listener to update save button visibility when tab changes
            setupSaveButtonVisibility(wizardElement);

            console.log('Edit document wizard initialized successfully');
            return true;
        } catch (e) {
            console.error('Error initializing wizard:', e);
            return false;
        }
    }

    // Try to initialize immediately
    if (!initWizard()) {
        // If Wizard class is not available, wait a bit and try again
        var attempts = 0;
        var maxAttempts = 10;
        var interval = setInterval(function () {
            attempts++;
            if (initWizard() || attempts >= maxAttempts) {
                clearInterval(interval);
                if (attempts >= maxAttempts && typeof Wizard === 'undefined') {
                    console.error('Wizard class not found after multiple attempts');
                }
            }
        }, 100);
    }
};

/**
 * Updates the visibility of the save button based on the current active tab
 * @param {HTMLElement} wizardElement - The wizard container element
 */
function updateSaveButtonVisibility(wizardElement) {
    if (!wizardElement) return;

    var saveButtonContainer = wizardElement.querySelector('.wizard .last');
    if (!saveButtonContainer) return;

    // Get the active tab
    var activeTab = wizardElement.querySelector('.nav-link.active');
    if (!activeTab) {
        // Hide save button if no active tab
        saveButtonContainer.style.display = 'none';
        return;
    }

    var activeTabHref = activeTab.getAttribute('href');
    
    // Show save button only on the last step (notes-step)
    if (activeTabHref === '#notes-step') {
        saveButtonContainer.style.display = '';
    } else {
        saveButtonContainer.style.display = 'none';
    }
}

/**
 * Sets up event listeners to update save button visibility when tabs change
 * @param {HTMLElement} wizardElement - The wizard container element
 */
function setupSaveButtonVisibility(wizardElement) {
    if (!wizardElement) return;

    // Listen for Bootstrap tab show events
    wizardElement.addEventListener('shown.bs.tab', function (e) {
        updateSaveButtonVisibility(wizardElement);
    });

    // Also listen for tab click events as fallback
    var tabLinks = wizardElement.querySelectorAll('.nav-link');
    tabLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            // Use setTimeout to ensure the tab has changed before updating visibility
            setTimeout(function () {
                updateSaveButtonVisibility(wizardElement);
            }, 100);
        });
    });
}

/**
 * Resets the wizard to the first step
 */
window.resetEditDocumentWizard = function () {
    var wizardElement = document.getElementById('edit-document-wizard');
    if (wizardElement) {
        var firstTab = wizardElement.querySelector('.nav-link');
        if (firstTab) {
            var firstTabPane = document.querySelector(firstTab.getAttribute('href'));
            if (firstTabPane) {
                // Reset all tabs
                wizardElement.querySelectorAll('.nav-link').forEach(function (link) {
                    link.classList.remove('active');
                });
                wizardElement.querySelectorAll('.tab-pane').forEach(function (pane) {
                    pane.classList.remove('show', 'active');
                });
                // Activate first tab
                firstTab.classList.add('active');
                firstTabPane.classList.add('show', 'active');
                
                // Update save button visibility after reset
                updateSaveButtonVisibility(wizardElement);
            }
        }
    }
};
