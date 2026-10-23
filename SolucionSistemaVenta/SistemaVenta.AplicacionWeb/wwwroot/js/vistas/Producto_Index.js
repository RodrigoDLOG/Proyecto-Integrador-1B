

const MODELO_BASE = {
    idProducto: 0,
    codigoBarra: "",
    marca: "",
    nombre: "",
    idCategoria: 0,
    stock: 0,
    urlImagen: "",
    precio:0,
    esActivo: 1,
    // Version Mexico: valores por defecto de un producto nuevo (pieza, IVA 16 %)
    idMedidaLocal: 0,
    claveUnidad: "H87",
    nombreClaveUnidad: "Pieza",
    claveProdServ: "",
    descripcionClaveProdServ: "",
    descuento: "0",
    objetoImp: "02",
    claveImpuesto: "002",
    tasaImpuesto: "16",
}



let tablaData;

$(document).ready(function () {

    fetch("/Categoria/Lista")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            console.log(responseJson)
            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#cboCategoria").append(
                        $("<option>").val(item.idCategoria).text(item.descripcion)
                    )
                })
            }
        })

    // Version Mexico: catalogos chicos para los combos
    llenarCombo("/Catalogo/MedidasLocales", "#cboMedidaLocal", false)
    llenarCombo("/Catalogo/ObjetosImpuesto", "#cboObjetoImp", true)
    llenarCombo("/Catalogo/Impuestos", "#cboImpuesto", true)

    // Catalogos grandes del SAT: se buscan mientras se escribe
    buscadorSat("#cboClaveUnidad", "/Catalogo/BuscarClavesUnidad", "Buscar unidad: pieza, H87, caja...")
    buscadorSat("#cboClaveProdServ", "/Catalogo/BuscarClavesProdServ", "Buscar clave: monitor, teclado, 43211902...")


    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": '/Producto/Lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idProducto", "visible": false, "searchable": false },
            {
                "data": "urlImagen", render: function (data) {
                    return `<img style="height:60px" src=${data} class="rounded mx-auto d-block"/>`
                }
            },
            // render.text(): el texto se muestra como texto, no como HTML (evita que un nombre con <script> se ejecute)
            { "data": "codigoBarra", render: $.fn.dataTable.render.text() },
            { "data": "marca", render: $.fn.dataTable.render.text() },
            { "data": "descripcion", render: $.fn.dataTable.render.text() },
            { "data": "nombreCategoria", render: $.fn.dataTable.render.text() },
            { "data": "stock" },
            { "data": "precio" },
            { "data": "descuento" },
            { "data": "claveProdServ", render: $.fn.dataTable.render.text() },
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
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Productos',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6, 7, 8, 9]
                }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });

})


function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idProducto)

    $("#txtCodigoBarra").val(modelo.codigoBarra)
    $("#txtMarca").val(modelo.marca)
    $("#txtDescripcion").val(modelo.descripcion)
    $("#cboCategoria").val(modelo.idCategoria == 0 ? $("#cboCategoria option:first").val() : modelo.idCategoria)
    $("#txtStock").val(modelo.stock)
    $("#txtPrecio").val(modelo.precio)
    $("#cboEstado").val(modelo.esActivo)
    $("#txtImagen").val("")
    $("#imgProducto").attr("src", modelo.urlImagen)

    // Version Mexico
    $("#txtDescuento").val(modelo.descuento)
    $("#cboMedidaLocal").val(modelo.idMedidaLocal == 0 ? $("#cboMedidaLocal option:first").val() : modelo.idMedidaLocal)
    seleccionarSat("#cboClaveUnidad", modelo.claveUnidad, modelo.nombreClaveUnidad)
    seleccionarSat("#cboClaveProdServ", modelo.claveProdServ, modelo.descripcionClaveProdServ)
    $("#cboObjetoImp").val(modelo.objetoImp)
    $("#cboImpuesto").val(modelo.claveImpuesto ?? "002")
    $("#txtTasaImpuesto").val(modelo.tasaImpuesto)
    habilitarImpuesto()


    $("#modalData").modal("show")
}


// Llena un <select> con un catalogo: "02 - Si objeto de impuesto"
function llenarCombo(url, selector, mostrarClave) {
    fetch(url)
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            responseJson.forEach((item) => {
                $(selector).append(
                    $("<option>").val(item.clave).text(mostrarClave ? `${item.clave} - ${item.descripcion}` : item.descripcion)
                )
            })
        })
}

// Select2 que busca en el servidor (igual que "Buscar Producto..." de Nueva Venta)
function buscadorSat(selector, url, placeholder) {
    $(selector).select2({
        ajax: {
            url: url,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { busqueda: params.term };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => ({ id: item.clave, text: `${item.clave} - ${item.descripcion}` }))
                };
            }
        },
        dropdownParent: $("#modalData"),   // sin esto no se puede escribir dentro del modal
        language: "es",
        placeholder: placeholder,
        minimumInputLength: 2
    });
}

// Pone en el Select2 la clave que ya tiene el producto (al editar)
function seleccionarSat(selector, clave, descripcion) {
    $(selector).empty()
    if (clave) {
        $(selector).append(new Option(`${clave} - ${descripcion ?? ""}`, clave, true, true))
    }
    $(selector).trigger("change")
}

// 01 = No objeto de impuesto: el producto no lleva impuesto ni tasa
function habilitarImpuesto() {
    const sinImpuesto = $("#cboObjetoImp").val() == "01"
    $("#cboImpuesto").prop("disabled", sinImpuesto)
    $("#txtTasaImpuesto").prop("disabled", sinImpuesto)
}

$("#cboObjetoImp").change(habilitarImpuesto)

// Con jQuery 3.6 Select2 no pone el cursor en su buscador al abrirse (igual que en Nueva Venta)
$(document).on("select2:open", function () {
    document.querySelector(".select2-container--open .select2-search__field")?.focus();
})



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

    // Numeros con punto decimal (7.5). Con coma, "7,5" se leeria como 75.
    const numeros = [["#txtPrecio", "Precio Unitario"], ["#txtDescuento", "Descuento"], ["#txtTasaImpuesto", "Tasa"]]
    for (const [id, nombre] of numeros) {
        if ($(id).prop("disabled")) continue
        $(id).val($(id).val().trim())
        if (!/^\d+(\.\d{1,2})?$/.test($(id).val())) {
            toastr.warning("", `${nombre}: escriba un numero con punto decimal, por ejemplo 7.5`)
            $(id).focus()
            return;
        }
    }

    if (!$("#cboClaveUnidad").val()) {
        toastr.warning("", "Seleccione la unidad de medida del SAT")
        return;
    }
    if (!$("#cboClaveProdServ").val()) {
        toastr.warning("", "Seleccione la clave de producto del SAT")
        return;
    }


    const modelo = structuredClone(MODELO_BASE);
    modelo["idProducto"] = parseInt($("#txtId").val())
    modelo["codigoBarra"] = $("#txtCodigoBarra").val()
    modelo["marca"] = $("#txtMarca").val()
    modelo["descripcion"] = $("#txtDescripcion").val()
    modelo["idCategoria"] = $("#cboCategoria").val()
    modelo["stock"] = $("#txtStock").val()
    modelo["precio"] = $("#txtPrecio").val()
    modelo["esActivo"] = $("#cboEstado").val()
    // Version Mexico
    modelo["descuento"] = $("#txtDescuento").val()
    modelo["idMedidaLocal"] = parseInt($("#cboMedidaLocal").val())
    modelo["claveUnidad"] = $("#cboClaveUnidad").val()
    modelo["claveProdServ"] = $("#cboClaveProdServ").val()
    modelo["objetoImp"] = $("#cboObjetoImp").val()
    modelo["claveImpuesto"] = $("#cboImpuesto").val()
    modelo["tasaImpuesto"] = $("#txtTasaImpuesto").val() || "0"

    const inputFoto = document.getElementById("txtImagen")

    const formData = new FormData();

    formData.append("imagen", inputFoto.files[0])
    formData.append("modelo", JSON.stringify(modelo))

    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idProducto == 0) {

        fetch("/Producto/Crear", {
            method: "POST",
            body: formData
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJson => {

                if (responseJson.estado) {

                    tablaData.row.add(responseJson.objeto).draw(false)
                    $("#modalData").modal("hide")
                    swal("Listo!", "El producto fue creado", "success")
                } else {
                    swal("Los sentimos", responseJson.mensaje, "error")
                }
            })
    } else {
        fetch("/Producto/Editar", {
            method: "PUT",
            body: formData
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
                    swal("Listo!", "El producto fue modificado", "success")
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
        text: `Eliminar el producto "${data.descripcion}"`,
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

                fetch(`/Producto/Eliminar?IdProducto=${data.idProducto}`, {
                    method: "DELETE"
                })
                    .then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide");
                        return response.ok ? response.json() : Promise.reject(response);
                    })
                    .then(responseJson => {

                        if (responseJson.estado) {

                            tablaData.row(fila).remove().draw()

                            swal("Listo!", "El producto fue eliminado", "success")
                        } else {
                            swal("Los sentimos", responseJson.mensaje, "error")
                        }
                    })


            }
        }
    )


})