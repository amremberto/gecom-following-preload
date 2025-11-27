// PDF.js helper functions for Blazor
// Shared state
let pdfjsLib = null;
let currentDocument = null;

async function init() {
    if (!pdfjsLib) {
        // Load PDF.js from CDN
        const script = document.createElement('script');
        script.src = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js';
        script.async = true;
        await new Promise((resolve, reject) => {
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
        pdfjsLib = window.pdfjsLib;
        // Set worker
        pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
    }
    return pdfjsLib;
}

async function loadPdfFromUrl(url) {
    try {
        await init();
        
        if (!url || typeof url !== 'string') {
            throw new Error('PDF URL is invalid');
        }
        
        if (url.length === 0) {
            throw new Error('PDF URL is empty');
        }
        
        console.log('Loading PDF from URL:', url);
        
        // Load PDF document directly from URL
        // The proxy endpoint handles authentication server-side
        // Cookies/authentication are sent automatically for same-origin requests
        const loadingTask = pdfjsLib.getDocument({ 
            url: url,
            verbosity: 0, // Suppress PDF.js warnings
            httpHeaders: {
                'Accept': 'application/pdf'
            }
        });
        
        currentDocument = await loadingTask.promise;
        
        if (!currentDocument) {
            throw new Error('Failed to load PDF document: document is null');
        }
        
        if (currentDocument.numPages === 0) {
            throw new Error('PDF document has no pages');
        }
        
        console.log('PDF loaded successfully. Total pages:', currentDocument.numPages);
        
        return {
            totalPages: currentDocument.numPages
        };
    } catch (error) {
        console.error('Error loading PDF from URL:', error);
        console.error('Error details:', {
            name: error.name,
            message: error.message,
            stack: error.stack
        });
        
        // Provide more detailed error messages
        let errorMessage = 'Error al cargar el PDF';
        if (error.message) {
            errorMessage += ': ' + error.message;
        }
        
        // Check for specific error types
        if (error.name === 'InvalidPDFException') {
            errorMessage = 'El archivo PDF está corrupto o no es válido';
        } else if (error.name === 'MissingPDFException') {
            errorMessage = 'No se pudo encontrar el archivo PDF';
        } else if (error.name === 'UnexpectedResponseException') {
            errorMessage = 'Error de respuesta del servidor al cargar el PDF';
        } else if (error.message && error.message.includes('401')) {
            errorMessage = 'No autorizado para acceder al PDF. Por favor, inicie sesión nuevamente';
        } else if (error.message && error.message.includes('403')) {
            errorMessage = 'No tiene permisos para acceder a este PDF';
        } else if (error.message && error.message.includes('404')) {
            errorMessage = 'El archivo PDF no fue encontrado';
        } else if (error.message && error.message.includes('500')) {
            errorMessage = 'Error del servidor al cargar el PDF';
        }
        
        // Clean up on error
        if (currentDocument) {
            try {
                currentDocument.destroy();
            } catch (e) {
                console.error('Error destroying document:', e);
            }
            currentDocument = null;
        }
        
        // Throw error with improved message
        const improvedError = new Error(errorMessage);
        improvedError.name = error.name || 'PDFLoadError';
        throw improvedError;
    }
}

async function loadPdfFromBase64(base64String) {
    try {
        await init();
        
        if (!base64String || typeof base64String !== 'string') {
            throw new Error('PDF base64 string is invalid');
        }
        
        if (base64String.length === 0) {
            throw new Error('PDF base64 string is empty');
        }
        
        // Convert base64 to Uint8Array
        const binaryString = atob(base64String);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        
        if (bytes.length === 0) {
            throw new Error('PDF bytes array is empty after conversion');
        }
        
        // Load PDF document
        const loadingTask = pdfjsLib.getDocument({ 
            data: bytes,
            verbosity: 0 // Suppress PDF.js warnings
        });
        
        currentDocument = await loadingTask.promise;
        
        if (!currentDocument || currentDocument.numPages === 0) {
            throw new Error('PDF document has no pages');
        }
        
        return {
            totalPages: currentDocument.numPages
        };
    } catch (error) {
        console.error('Error loading PDF:', error);
        // Clean up on error
        if (currentDocument) {
            try {
                currentDocument.destroy();
            } catch (e) {
                // Ignore cleanup errors
            }
            currentDocument = null;
        }
        throw error;
    }
}

async function renderPage(canvasId, pageNumber, scale = 1.0) {
    try {
        if (!currentDocument) {
            throw new Error('PDF document not loaded');
        }

        if (pageNumber < 1 || pageNumber > currentDocument.numPages) {
            throw new Error(`Page number ${pageNumber} is out of range (1-${currentDocument.numPages})`);
        }

        const page = await currentDocument.getPage(pageNumber);
        const viewport = page.getViewport({ scale: scale });

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            throw new Error(`Canvas with id ${canvasId} not found`);
        }

        const context = canvas.getContext('2d');
        if (!context) {
            throw new Error('Could not get 2D context from canvas');
        }

        // Set canvas dimensions
        canvas.height = viewport.height;
        canvas.width = viewport.width;

        // Clear canvas before rendering
        context.clearRect(0, 0, canvas.width, canvas.height);

        const renderContext = {
            canvasContext: context,
            viewport: viewport
        };

        await page.render(renderContext).promise;
    } catch (error) {
        console.error('Error rendering PDF page:', error);
        throw error;
    }
}

function dispose() {
    if (currentDocument) {
        currentDocument.destroy();
        currentDocument = null;
    }
}

// Export functions for ES6 module
export { loadPdfFromUrl, loadPdfFromBase64, renderPage, dispose };

// Also expose on window for backward compatibility
window.pdfViewer = {
    loadPdfFromUrl,
    loadPdfFromBase64,
    renderPage,
    dispose
};

// Helper function to download PDF file from base64
window.downloadPdfFile = function(base64String, fileName) {
    try {
        // Convert base64 to binary
        const binaryString = atob(base64String);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        
        // Create blob
        const blob = new Blob([bytes], { type: 'application/pdf' });
        
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName || 'documento.pdf';
        document.body.appendChild(link);
        link.click();
        
        // Cleanup
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Error downloading PDF:', error);
        alert('Error al descargar el PDF: ' + error.message);
    }
};

// Helper function to download file from URL (same origin, cookies sent automatically)
window.downloadFileFromUrl = async function(url, fileName) {
    try {
        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include', // Include cookies for authentication
            headers: {
                'Accept': 'application/pdf'
            }
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Error ${response.status}: ${errorText || response.statusText}`);
        }

        const blob = await response.blob();
        
        // Create download link
        const downloadUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = fileName || 'documento.pdf';
        document.body.appendChild(link);
        link.click();
        
        // Cleanup
        document.body.removeChild(link);
        window.URL.revokeObjectURL(downloadUrl);
    } catch (error) {
        console.error('Error downloading file:', error);
        alert('Error al descargar el archivo: ' + error.message);
    }
};

