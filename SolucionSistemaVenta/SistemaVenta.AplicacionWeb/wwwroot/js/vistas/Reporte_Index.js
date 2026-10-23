// Regla del profesor (slide 15): al entrar, la pantalla muestra un listado.
// Se precargan los ultimos 30 dias en los inputs y la tabla carga con ese rango.
function fechaDDMMYYYY(d) {
    const dd = String(d.getDate()).padStart(2, "0")
    const mm = String(d.getMonth() + 1).padStart(2, "0")
    return `${dd}/${mm}/${d.getFullYear()}`
}
function precargarUltimos30Dias() {
    const hoy = new Date()
    const hace30 = new Date(); hace30.setDate(hoy.getDate() - 30)
    $("#txtFechaInicio").val(fechaDDMMYYYY(hace30))
    $("#txtFechaFin").val(fechaDDMMYYYY(hoy))
    return `/Reporte/ReporteVenta?fechaInicio=${$("#txtFechaInicio").val()}&fechaFin=${$("#txtFechaFin").val()}`
}

﻿let tablaData;
$(document).ready(function () {

    $.datepicker.setDefaults($.datepicker.regional["es"])

    $("#txtFechaInicio").datepicker({ dateFormat: "dd/mm/yy" })
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" })

    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": precargarUltimos30Dias(),
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "fechaRegistro" },
            { "data": "numeroVenta" },
            { "data": "tipoDocumento" },
            // render.text(): el texto se muestra como texto, no como HTML
            { "data": "documentoCliente", render: $.fn.dataTable.render.text() },
            { "data": "nombreCliente", render: $.fn.dataTable.render.text() },
            { "data": "subTotalVenta" },
            { "data": "impuestoTotalVenta" },
            { "data": "totalVenta" },
            { "data": "producto", render: $.fn.dataTable.render.text() },
            { "data": "cantidad" },
            { "data": "precio" },
            { "data": "descuento" },
            { "data": "impuesto" },
            { "data": "total" },
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Ventas'
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });


})

$("#btnBuscar").click(function () {

    if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
        toastr.warning("", "Debe ingresar fecha inicio y fin")
        return;
    }

    let fechaInicio = $("#txtFechaInicio").val().trim();
    let fechaFin = $("#txtFechaFin").val().trim();

    let nueva_url = `/Reporte/ReporteVenta?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`;

    tablaData.ajax.url(nueva_url).load();


})
