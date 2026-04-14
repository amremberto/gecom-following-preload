// Overrides for Pending documents flow.
(function () {
    if (typeof window === "undefined") {
        return;
    }

    var originalInitEditDocumentWizard = window.initEditDocumentWizard;
    if (typeof originalInitEditDocumentWizard !== "function") {
        return;
    }

    window.initEditDocumentWizard = function (isPendingPreload, dotNetReference, docId) {
        originalInitEditDocumentWizard(isPendingPreload, dotNetReference, docId);

        setTimeout(function () {
            var wizardElement = document.getElementById("edit-document-wizard");
            if (!wizardElement) {
                return;
            }

            var saveButton = wizardElement.querySelector(".wizard .last a");
            if (!saveButton) {
                return;
            }

            saveButton.onclick = async function (e) {
                e.preventDefault();

                var selectedDocId = wizardElement.getAttribute("data-doc-id")
                    ? parseInt(wizardElement.getAttribute("data-doc-id"), 10)
                    : 0;

                if (!selectedDocId) {
                    if (typeof showValidationToast === "function") {
                        showValidationToast(dotNetReference, "Error: No se pudo obtener el ID del documento.");
                    }
                    return false;
                }

                if (!dotNetReference || typeof dotNetReference.invokeMethodAsync !== "function") {
                    if (typeof showValidationToast === "function") {
                        showValidationToast(dotNetReference, "Error: No se pudo comunicar con el servidor.");
                    }
                    return false;
                }

                var isConfirmed = false;
                try {
                    isConfirmed = await dotNetReference.invokeMethodAsync(
                        "ShowConfirmationDialog",
                        "Si esta seguro de enviar el documento a Mesa de Entrada?"
                    );
                } catch (error) {
                    console.error("Error showing confirmation dialog:", error);
                    isConfirmed = false;
                }

                if (!isConfirmed) {
                    return false;
                }

                try {
                    var result = await dotNetReference.invokeMethodAsync("ConfirmDocumentFromWizard", selectedDocId);
                    if (result && result.success) {
                        var modalElement = document.getElementById("edit-document-modal");
                        if (modalElement && typeof bootstrap !== "undefined") {
                            var modalInstance = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
                            modalInstance.hide();
                        }
                    } else if (typeof showValidationToast === "function") {
                        showValidationToast(
                            dotNetReference,
                            result && result.message ? result.message : "No se pudo confirmar el documento."
                        );
                    }
                } catch (error) {
                    console.error("Error confirming document from wizard:", error);
                    if (typeof showValidationToast === "function") {
                        showValidationToast(dotNetReference, "Error al confirmar el documento.");
                    }
                }

                return false;
            };
        }, 250);
    };
})();
