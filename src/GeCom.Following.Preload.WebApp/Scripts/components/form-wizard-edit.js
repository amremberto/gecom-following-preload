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
            if (input.type === 'number') {
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
}

/**
 * Validates a single field
 * @param {HTMLElement} input - The input element to validate
 * @returns {boolean} True if valid, false otherwise
 */
function validateField(input) {
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
 * Sets up validation listeners for form fields
 * @param {HTMLElement} form - The form element
 * @param {boolean} isPendingPreload - Whether the document is in "Pendiente Precarga" status
 * @param {HTMLElement} wizardElement - The wizard container element
 */
function setupFormValidation(form, isPendingPreload, wizardElement) {
    if (!form) return;

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
                    if (isPendingPreload) {
                        updateTabsState(wizardElement, isPendingPreload);
                    }
                }, 10);
            });
        } else {
            // For regular inputs and selects without SELECT2
            input.addEventListener('blur', function () {
                validateField(this);
            });

            // Validate on input change
            input.addEventListener('input', function () {
                validateField(this);
                if (isPendingPreload) {
                    updateTabsState(wizardElement, isPendingPreload);
                }
            });

            // Validate on change (for selects)
            input.addEventListener('change', function () {
                validateField(this);
                if (isPendingPreload) {
                    updateTabsState(wizardElement, isPendingPreload);
                }
            });
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
                        if (isPendingPreload) {
                            updateTabsState(wizardElement, isPendingPreload);
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
 * Sets up navigation restrictions for "Pendiente Precarga" status
 * @param {HTMLElement} wizardElement - The wizard container element
 * @param {boolean} isPendingPreload - Whether the document is in "Pendiente Precarga" status
 * @param {DotNetObjectReference} dotNetReference - Reference to the C# component for showing toast
 */
function setupNavigationRestrictions(wizardElement, isPendingPreload, dotNetReference) {
    if (!isPendingPreload) return;

    var validationMessage = 'Por favor debe completar todos los campos requeridos del documento para poder avanzar.';

    // Initially disable tabs after the first one
    updateTabsState(wizardElement, isPendingPreload);

    // Prevent navigation to other tabs when clicking on disabled tabs
    var tabLinks = wizardElement.querySelectorAll('.nav-link');
    tabLinks.forEach(function (link, index) {
        if (index > 0) {
            // Use capture phase to intercept before Bootstrap handles it
            link.addEventListener('click', function (e) {
                if (isPendingPreload && !validateFirstTabFields()) {
                    e.preventDefault();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    showValidationToast(dotNetReference, validationMessage);
                    return false;
                }
            }, true);

            // Also intercept Bootstrap tab events
            link.addEventListener('show.bs.tab', function (e) {
                if (isPendingPreload && !validateFirstTabFields()) {
                    e.preventDefault();
                    showValidationToast(dotNetReference, validationMessage);
                    return false;
                }
            });
        }
    });

    // Intercept wizard navigation buttons
    var nextButton = wizardElement.querySelector('.next a');
    if (nextButton) {
        // Use capture phase to intercept before wizard handles it
        nextButton.addEventListener('click', function (e) {
            var activeTab = wizardElement.querySelector('.nav-link.active');
            if (activeTab && activeTab.getAttribute('href') === '#document-properties-step') {
                if (!validateFirstTabFields()) {
                    e.preventDefault();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    showValidationToast(dotNetReference, validationMessage);
                    return false;
                }
            }
        }, true);
    }

    // Intercept Bootstrap tab show event globally for this wizard
    wizardElement.addEventListener('show.bs.tab', function (e) {
        if (isPendingPreload) {
            var targetTab = e.target;
            var targetHref = targetTab.getAttribute('href');

            // If trying to navigate away from first tab
            if (targetHref !== '#document-properties-step') {
                if (!validateFirstTabFields()) {
                    e.preventDefault();
                    showValidationToast(dotNetReference, validationMessage);
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
 */
window.initEditDocumentWizard = function (isPendingPreload, dotNetReference) {
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
            // Create new wizard instance with validation
            var wizard = new Wizard('#edit-document-wizard', {
                validate: true
            });

            // Set up navigation restrictions if document is in "Pendiente Precarga" status
            setupNavigationRestrictions(wizardElement, isPendingPreload, dotNetReference);

            // Set up form validation
            var form = document.getElementById('documentPropertiesForm');
            if (form) {
                setupFormValidation(form, isPendingPreload, wizardElement);
            }

            // Initial validation and tabs state update after a delay to ensure SELECT2 and Flatpickr are initialized
            setTimeout(function () {
                if (isPendingPreload) {
                    updateTabsState(wizardElement, isPendingPreload);
                }
            }, 600);

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
            }
        }
    }
};
