const MODELO_BASE = {
    idCliente: 0,
    nombre: "",
    correo: "",
    rfc: "",
    domicilioFiscalReceptor: "",
    regimenFiscalReceptor: "",
    esActivo: 1,
}

// clave -> descripcion del regimen, para mostrar "616 - Sin obligaciones fiscales" en la tabla
let regimenes = {};
let tablaData;

$(document).ready(function () {

    // Primero el catalogo de regimenes (llena el combo y la tabla lo usa para mostrar la descripcion)
    fetch("/Catalogo/RegimenesFiscales")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            responseJson.forEach((item) => {
                regimenes[item.clave] = item.descripcion
                $("#cboRegimenFiscal").append(
                    $("<option>").val(item.clave).text(`${item.clave} - ${item.descripcion}`)
                )
            })
        })
        .catch(() => toastr.error("", "No se pudo cargar el catalogo de regimenes fiscales"))
        .finally(() => cargarTabla())

})


function cargarTabla() {

    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": '/Cliente/Lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idCliente", "visible": false, "searchable": false },
            // render.text(): el texto se muestra como texto, no como HTML (evita que un nombre con <script> se ejecute)
            { "data": "rfc", render: $.fn.dataTable.render.text() },
            { "data": "nombre", render: $.fn.dataTable.render.text() },
            { "data": "correo", render: $.fn.dataTable.render.text() },
            { "data": "domicilioFiscalReceptor", render: $.fn.dataTable.render.text() },
            {
                "data": "regimenFiscalReceptor", render: function (data) {
                    return $("<span>").text(`${data} - ${regimenes[data] ?? ""}`).html()
                }
            },
            {
                "data": "esActivo", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">No Activo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[2, "asc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Clientes',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
}


function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idCliente)
    $("#txtRfc").val(modelo.rfc)
    $("#txtNombre").val(modelo.nombre)
    $("#txtCorreo").val(modelo.correo)
    $("#txtCodigoPostal").val(modelo.domicilioFiscalReceptor)
    $("#cboRegimenFiscal").val(modelo.regimenFiscalReceptor == "" ? $("#cboRegimenFiscal option:first").val() : modelo.regimenFiscalReceptor)
    $("#cboEstado").val(modelo.esActivo)

    $("#modalData").modal("show")
}

$("#btnNuevo").click(function () {
    mostrarModal()
})


$("#btnGuardar").click(function () {

    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "")

    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo : "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje)
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus()
        return;
    }

    // RFC de 12 (persona moral) o 13 (persona fisica). El servidor vuelve a revisar todo.
    const rfc = $("#txtRfc").val().trim().toUpperCase()
    if (rfc.length != 12 && rfc.length != 13) {
        toastr.warning("", "El RFC debe tener 12 o 13 caracteres")
        $("#txtRfc").focus()
        return;
    }

    const modelo = structuredClone(MODELO_BASE);
    modelo["idCliente"] = parseInt($("#txtId").val())
    modelo["rfc"] = rfc
    modelo["nombre"] = $("#txtNombre").val()
    modelo["correo"] = $("#txtCorreo").val()
    modelo["domicilioFiscalReceptor"] = $("#txtCodigoPostal").val()
    modelo["regimenFiscalReceptor"] = $("#cboRegimenFiscal").val()
    modelo["esActivo"] = parseInt($("#cboEstado").val())

    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idCliente == 0) {

        fetch("/Cliente/Crear", {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(modelo)
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJson => {

                if (responseJson.estado) {

                    tablaData.row.add(responseJson.objeto).draw(false)
                    $("#modalData").modal("hide")
                    swal("Listo!", "El cliente fue creado", "success")
                } else {
                    swal("Los sentimos", responseJson.mensaje, "error")
                }
            })
    } else {
        fetch("/Cliente/Editar", {
            method: "PUT",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(modelo)
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJson => {

                if (responseJson.estado) {

                    tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                    filaSeleccionada = null;
                    $("#modalData").modal("hide")
                    swal("Listo!", "El cliente fue modificado", "success")
                } else {
                    swal("Los sentimos", responseJson.mensaje, "error")
                }
            })
    }
})


let filaSeleccionada;
$("#tbdata tbody").on("click", ".btn-editar", function () {

    if ($(this).closest("tr").hasClass("child")) {
        filaSeleccionada = $(this).closest("tr").prev();
    } else {
        filaSeleccionada = $(this).closest("tr");
    }

    const data = tablaData.row(filaSeleccionada).data();

    mostrarModal(data);
})


$("#tbdata tbody").on("click", ".btn-eliminar", function () {

    let fila;
    if ($(this).closest("tr").hasClass("child")) {
        fila = $(this).closest("tr").prev();
    } else {
        fila = $(this).closest("tr");
    }

    const data = tablaData.row(fila).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar el cliente "${data.nombre}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                $(".showSweetAlert").LoadingOverlay("show");

                fetch(`/Cliente/Eliminar?IdCliente=${data.idCliente}`, {
                    method: "DELETE"
                })
                    .then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide");
                        return response.ok ? response.json() : Promise.reject(response);
                    })
                    .then(responseJson => {

                        if (responseJson.estado) {

                            tablaData.row(fila).remove().draw()

                            swal("Listo!", "El cliente fue eliminado", "success")
                        } else {
                            swal("Los sentimos", responseJson.mensaje, "error")
                        }
                    })
            }
        }
    )
})
