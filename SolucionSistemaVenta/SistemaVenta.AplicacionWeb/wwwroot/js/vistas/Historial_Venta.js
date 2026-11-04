
const VISTA_BUSQUEDA = {

    busquedaFecha: () => {

        $("#txtFechaInicio").val("")
        $("#txtFechaFin").val("")
        $("#txtNumeroVenta").val("")

        $(".busqueda-fecha").show()
        $(".busqueda-venta").hide()
    }, busquedaVenta: () => {

        $("#txtFechaInicio").val("")
        $("#txtFechaFin").val("")
        $("#txtNumeroVenta").val("")

        $(".busqueda-fecha").hide()
        $(".busqueda-venta").show()
    }
}

$(document).ready(function () {
    VISTA_BUSQUEDA["busquedaFecha"]()

    $.datepicker.setDefaults($.datepicker.regional["es"])

    $("#txtFechaInicio").datepicker({dateFormat : "dd/mm/yy"})
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" })

    precargarUltimos30Dias()
})

$("#cboBuscarPor").change(function () {

    if ($("#cboBuscarPor").val() == "fecha") {
        VISTA_BUSQUEDA["busquedaFecha"]()
    } else {
        VISTA_BUSQUEDA["busquedaVenta"]()
    }

})


$("#btnBuscar").click(function () {

    if ($("#cboBuscarPor").val() == "fecha") {

        if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
            toastr.warning("", "Debe ingresar fecha inicio y fin")
            return;
        }
    } else {

        if ($("#txtNumeroVenta").val().trim() == "") {
            toastr.warning("", "Debe ingresar el numero de venta")
            return;
        }
    }

    let numeroVenta = $("#txtNumeroVenta").val()
    let fechaInicio = $("#txtFechaInicio").val()
    let fechaFin = $("#txtFechaFin").val()


    $(".card-body").find("div.row").LoadingOverlay("show");

    fetch(`/Venta/Historial?numeroVenta=${numeroVenta}&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
        .then(response => {
            $(".card-body").find("div.row").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            // La tabla se vuelve a armar en cada busqueda con los controles estandar (diapositiva 15)
            if ($.fn.DataTable.isDataTable("#tbventa"))
                $("#tbventa").DataTable().destroy()

            $("#tbventa tbody").html("");

            if (responseJson.length > 0) {

                responseJson.forEach((venta) => {

                    $("#tbventa tbody").append(
                        $("<tr>").append(
                            $("<td>").text(venta.fechaRegistro),
                            $("<td>").text(venta.numeroVenta),
                            $("<td>").text(venta.tipoDocumentoVenta),
                            $("<td>").text(venta.documentoCliente),
                            $("<td>").text(venta.nombreCliente),
                            $("<td>").text(venta.total),
                            $("<td>").append(
                                $("<button>").addClass("btn btn-info btn-sm").append(
                                    $("<i>").addClass("fas fa-eye")
                                ).data("venta", venta),
                                botonSolicitarFactura(venta)
                            )
                        )
                    )

                })

            }

            $("#tbventa").DataTable({
                order: [[1, "desc"]],
                columnDefs: [{ orderable: false, targets: 6 }],
                dom: "Bfrtip",
                buttons: [
                    {
                        text: 'Exportar Excel',
                        extend: 'excelHtml5',
                        title: '',
                        filename: 'Historial Ventas',
                        exportOptions: { columns: [0, 1, 2, 3, 4, 5] }
                    }, 'pageLength'
                ],
                language: {
                    url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
                },
            });

        })
        .catch(() => {
            $(".card-body").find("div.row").LoadingOverlay("hide");
            toastr.error("", "No se pudo consultar el historial")
        })

})

$("#tbventa tbody").on("click", ".btn-info", function () {

    let d = $(this).data("venta")

    $("#txtFechaRegistro").val(d.fechaRegistro)
    $("#txtNumVenta").val(d.numeroVenta)
    $("#txtUsuarioRegistro").val(d.usuario)
    $("#txtTipoDocumento").val(d.tipoDocumentoVenta)
    $("#txtDocumentoCliente").val(d.documentoCliente)
    $("#txtNombreCliente").val(d.nombreCliente)
    $("#txtDescuento").val(d.descuentoTotal)
    $("#txtSubTotal").val(d.subTotal)
    $("#txtIGV").val(d.impuestoTotal)
    $("#txtTotal").val(d.total)


    $("#tbProductos tbody").html("");

    // Version Mexico (diapositiva 26): descuento e impuesto por producto
    d.detalleVenta.forEach((item) => {

        $("#tbProductos tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descripcionProducto),
                $("<td>").addClass("text-right").text(item.cantidad),
                $("<td>").addClass("text-right").text(item.precio),
                $("<td>").addClass("text-right").text(`${item.descuento} (${parseFloat(item.porcentajeDescuento)}%)`),
                $("<td>").addClass("text-right").text(item.subTotal),
                $("<td>").addClass("text-right").text(`${item.impuesto} (${parseFloat(item.tasaImpuesto)}%)`),
                $("<td>").addClass("text-right").text(item.total),
            )
        )

    })

    $("#linkImprimir").attr("href",`/Venta/MostrarPDFVenta?numeroVenta=${d.numeroVenta}`)

    $("#modalData").modal("show");

})

// Regla del profesor (slide 15): al entrar, la pantalla muestra un listado.
// Se precargan los ultimos 30 dias y se dispara la busqueda.
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
    $("#btnBuscar").click()
}

// ______________________ Version Mexico: Solicitar Factura (diapositivas 28 y 29) ______________________

let facturaActual = null    // lo que sugiere el servidor para la venta que se va a facturar

function llenarCatalogo(idCombo, url) {
    fetch(url)
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(lista => {
            $(idCombo).html("")
            lista.forEach(item => $(idCombo).append($("<option>").val(item.clave).text(`${item.clave} - ${item.descripcion}`)))
        })
}

$(document).ready(function () {
    llenarCatalogo("#cboUsoCfdi", "/Catalogo/UsosCfdi")
    llenarCatalogo("#cboRegimenFiscal", "/Catalogo/RegimenesFiscales")
    llenarCatalogo("#cboFormaPago", "/Catalogo/FormasPago")
    llenarCatalogo("#cboTipoComprobante", "/Catalogo/TiposComprobante")
    llenarCatalogo("#cboMetodoPago", "/Catalogo/MetodosPago")
})

// En cada renglon. Deshabilitado si ya se facturo, o si la venta es de antes de la Version Mexico
function botonSolicitarFactura(venta) {
    const sinCliente = !venta.idCliente
    return $("<button>").addClass("btn btn-warning btn-sm btn-factura ml-1").text("Solicitar Factura")
        .prop("disabled", !!venta.uuid || sinCliente)
        .attr("title", venta.uuid ? `Facturada. UUID: ${venta.uuid}` : sinCliente ? "Venta anterior a la Version Mexico: no tiene cliente con RFC" : "")
        .data("venta", venta)
}

// Al abrir el detalle de una venta se muestra el estado de su factura y sus descargas
$("#modalData").on("show.bs.modal", function () {
    $("#linkFacturaXml, #linkFacturaPdf").addClass("d-none")
    $("#txtEstadoFactura").removeClass("text-danger text-success").text("")

    const numeroVenta = $("#txtNumVenta").val()

    fetch(`/Venta/ObtenerFactura?numeroVenta=${numeroVenta}`)
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            // Se cerro y se abrio otra venta antes de que llegara la respuesta
            if ($("#txtNumVenta").val() !== numeroVenta) return

            if (!responseJson.estado) {
                $("#txtEstadoFactura").text(responseJson.mensaje)
                return
            }

            const factura = responseJson.objeto
            if (factura.uuid) {
                $("#linkFacturaXml").attr("href", `/Venta/DescargarFacturaXml?numeroVenta=${numeroVenta}`).removeClass("d-none")
                $("#linkFacturaPdf").attr("href", `/Venta/MostrarFacturaPdf?numeroVenta=${numeroVenta}`).removeClass("d-none")
                $("#txtEstadoFactura").addClass("text-success").text(`Facturada. UUID: ${factura.uuid}`)
            } else if (factura.mensajeError) {
                $("#txtEstadoFactura").addClass("text-danger").text(`Ultimo intento: ${factura.mensajeError}`)
            }
        })
        .catch(() => $("#txtEstadoFactura").addClass("text-danger").text("No se pudo consultar la factura"))
})

// Abre la ventana con los 6 datos, ya llenos con lo que sugiere el servidor
$("#tbventa tbody").on("click", ".btn-factura", function () {
    const venta = $(this).data("venta")

    fetch(`/Venta/ObtenerFactura?numeroVenta=${venta.numeroVenta}`)
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            if (!responseJson.estado) {
                swal("Lo sentimos!", responseJson.mensaje, "error")
                return
            }

            facturaActual = responseJson.objeto
            $("#txtFacturaNumVenta").text(venta.numeroVenta)
            $("#txtFacturaCliente").text(`${venta.documentoCliente} - ${venta.nombreCliente}`)
            $("#txtFacturaError").text(facturaActual.mensajeError ? `Ultimo intento: ${facturaActual.mensajeError}` : "")
            $("#cboUsoCfdi").val(facturaActual.usoCfdi)
            $("#cboRegimenFiscal").val(facturaActual.regimenFiscalReceptor)
            $("#cboFormaPago").val(facturaActual.formaPago)
            $("#cboTipoComprobante").val(facturaActual.tipoComprobante)
            $("#cboMetodoPago").val(facturaActual.metodoPago)
            $("#txtFacturaCodigoPostal").val(facturaActual.codigoPostal)
            $("#modalFactura").modal("show")
        })
        .catch(() => swal("Lo sentimos!", "No se pudo comunicar con el servidor", "error"))
})

$("#btnTimbrar").click(function () {
    const modelo = {
        numeroVenta: facturaActual.numeroVenta,
        usoCfdi: $("#cboUsoCfdi").val(),
        regimenFiscalReceptor: $("#cboRegimenFiscal").val(),
        formaPago: $("#cboFormaPago").val(),
        tipoComprobante: $("#cboTipoComprobante").val(),
        metodoPago: $("#cboMetodoPago").val(),
        codigoPostal: $("#txtFacturaCodigoPostal").val().trim()
    }

    if (!/^[0-9]{5}$/.test(modelo.codigoPostal)) {
        toastr.warning("", "El codigo postal debe tener 5 digitos")
        return
    }

    // Deshabilitado mientras responde, para no pedir dos veces la misma factura
    $("#btnTimbrar").prop("disabled", true)
    $("#modalFactura").find("div.modal-content").LoadingOverlay("show")

    fetch("/Venta/SolicitarFactura", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(modelo)
    })
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            $("#btnTimbrar").prop("disabled", false)
            $("#modalFactura").find("div.modal-content").LoadingOverlay("hide")

            if (responseJson.estado) {
                $("#modalFactura").modal("hide")
                swal("Factura timbrada!", `UUID: ${responseJson.objeto.uuid}`, "success")
                $("#btnBuscar").click()     // para que su boton salga ya deshabilitado
            } else {
                swal("No se pudo facturar", responseJson.mensaje || "Intente de nuevo", "error")
                $("#txtFacturaError").text(responseJson.mensaje ? `Ultimo intento: ${responseJson.mensaje}` : "")
            }
        })
        .catch(() => {
            $("#btnTimbrar").prop("disabled", false)
            $("#modalFactura").find("div.modal-content").LoadingOverlay("hide")
            swal("Lo sentimos!", "No se pudo comunicar con el servidor", "error")
        })
})
