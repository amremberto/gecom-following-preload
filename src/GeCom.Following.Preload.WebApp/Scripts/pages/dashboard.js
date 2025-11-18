/**
 * Theme: Adminto - Responsive Bootstrap 5 Admin Dashboard
 * Author: Coderthemes
 * Module/App: Dashboard
 */

export function loadDashboard() {
    console.log("Dashboard loaded");
}

/**
 * Copies text to clipboard using a safe fallback method.
 * @param {string} text - The text to copy to clipboard.
 */
export function copyToClipboard(text) {
    if (!text) {
        console.warn("No text provided to copy");
        return;
    }

    // Try modern clipboard API first
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text)
            .then(() => {
                console.log("Texto copiado al portapapeles");
            })
            .catch((err) => {
                console.error("Error al copiar con clipboard API:", err);
                // Fallback to traditional method
                fallbackCopyToClipboard(text);
            });
    } else {
        // Fallback to traditional method
        fallbackCopyToClipboard(text);
    }
}

/**
 * Fallback method to copy text to clipboard using a temporary input element.
 * @param {string} text - The text to copy to clipboard.
 */
function fallbackCopyToClipboard(text) {
    const textArea = document.createElement("textarea");
    textArea.value = text;
    textArea.style.position = "fixed";
    textArea.style.left = "-999999px";
    textArea.style.top = "-999999px";
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    
    try {
        const successful = document.execCommand('copy');
        if (successful) {
            console.log("Texto copiado al portapapeles (método tradicional)");
        } else {
            console.error("No se pudo copiar el texto");
        }
    } catch (err) {
        console.error("Error al copiar con método tradicional:", err);
    } finally {
        document.body.removeChild(textArea);
    }
}

// Mantener compatibilidad con código que usa window.loadDashboard
window.loadDashboard = loadDashboard;
window.copyToClipboard = copyToClipboard;
