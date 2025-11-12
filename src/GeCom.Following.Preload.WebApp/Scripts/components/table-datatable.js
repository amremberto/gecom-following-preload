/**
 * Theme: Adminto - Responsive Bootstrap 5 Admin Dashboard
 * Author: Coderthemes
 * Module/App: Data tables
 */

window.loadDataTable = function (tableId) {

    // Check if jQuery is loaded
    if (typeof jQuery === 'undefined') {
        console.error('jQuery is not loaded. DataTable requires jQuery.');
        return;
    }

    try {
        // Scroll Vertical Datatable
        $('#' + tableId).DataTable({
            scrollX: true,
            fixedColumns: { leftColumns: 1 }, // <-- Fija la primera columna
            language: {
                url: 'vendor/datatables.net-plugins/i18n/es-AR.json',
                paginate: {
                    first: '<i class="ti ti-chevrons-left"></i>',
                    previous: '<i class="ti ti-chevron-left"></i>',
                    next: '<i class="ti ti-chevron-right"></i>',
                    last: '<i class="ti ti-chevrons-right"></i>'
                },
                lengthMenu: "Mostrar _MENU_ registros por página",
                info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
                infoEmpty: "Mostrando 0 a 0 de 0 registros",
                infoFiltered: "(filtrado de _MAX_ registros totales)",
                search: "Buscar:",
                zeroRecords: "No se encontraron registros coincidentes"
            },
            pageLength: 10,
            lengthMenu: [[10, 25, 50], [10, 25, 50]],
            drawCallback: function () {
                $(".dataTables_paginate > .pagination").addClass("pagination-rounded");
            },
        });

        $(".dataTables_length select").addClass("form-select form-select-sm");
        $(".dataTables_length label").addClass("form-label");
    } catch (error) {
        console.error('Error al inicializar DataTable:', error);
    }
};

window.destroyDataTable = function (tableId) {
    if (typeof $ === 'undefined') {
        console.error('jQuery no está cargado');
        return;
    }

    try {
        const table = $('#' + tableId).DataTable();
        if (table) {
            table.destroy();
        }
    } catch (error) {
        console.error('Error al destruir DataTable:', error);
    }
};
