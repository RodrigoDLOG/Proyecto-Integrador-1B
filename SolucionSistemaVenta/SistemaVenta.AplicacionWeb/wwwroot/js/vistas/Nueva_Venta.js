
const RFC_PUBLICO_GENERAL = "XAXX010101000";
let SimboloMoneda = "$";

$(document).ready(function () {


    fetch("/Venta/ListaTipoDocumentoVenta")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboTipoDocumentoVenta").append(
                        $("<option>").val(item.idTipoDocumentoVenta).text(item.descripcion)
                    )
                })
            }
        })


    // Del negocio solo se ocupa el simbolo de moneda; el impuesto ahora viene en cada producto
    fetch("/Negocio/Obtener")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                SimboloMoneda = responseJson.objeto.simboloMoneda
                $(".etiqueta-moneda").each(function () {
                    $(this).text(`${$(this).text()} - ${SimboloMoneda}`)
                })
            }
        })

    // Si no se elige cliente, la venta es a Publico En General (diapositiva 27)
    cargarClientePorRfc(RFC_PUBLICO_GENERAL)

    $("#cboBuscarProducto").select2({
        ajax: {
            url: "/Venta/ObtenerProductos",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    busqueda: params.term
                };
            },
            processResults: function (data,) {

                return {
                    results: data.map((item) => (
                        {
                            id: item.idProducto,
                            text: item.descripcion,

                            marca: item.marca,
                            categoria : item.nombreCategoria,
                            urlImagen: item.urlImagen,
                            precio : parseFloat(item.precio),
                            // Version Mexico: descuento y tasa del producto (en %)
                            descuento: parseFloat(item.descuento),
                            tasaImpuesto: item.objetoImp == "01" ? 0 : parseFloat(item.tasaImpuesto),
                            stock: item.stock
                        }
                    ))
                };
            }
        },
        language: "es",
        placeholder: 'Buscar Producto...',
        minimumInputLength: 1,
        templateResult: formatoResultados
    });



})

function formatoResultados(data) {

    //esto es por defecto, ya que muestra el "buscando..."
    if (data.loading)
        return data.text;

    var contenedor = $(
        `<table width="100%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;width:60px;margin-right:10px" src="${data.urlImagen}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.marca}</p>
                    <p style="margin:2px">${data.text}</p>
                </td>
            </tr>
         </table>`
    );

    return contenedor;
}

$(document).on("select2:open", function () {
    document.querySelector(".select2-search__field").focus();
})


//________________________ Cliente por RFC (diapositivas 26 y 27) ________________________

function ponerCliente(cliente) {
    $("#txtIdCliente").val(cliente.idCliente)
    $("#txtRfcCliente").val(cliente.rfc)
    $("#txtNombreCliente").val(cliente.nombre)
}

function cargarClientePorRfc(rfc) {
    fetch(`/Cliente/ObtenerPorRfc?rfc=${encodeURIComponent(rfc)}`)
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                ponerCliente(responseJson.objeto)
            } else {
                toastr.warning("", responseJson.mensaje)
                if (rfc != RFC_PUBLICO_GENERAL)
                    cargarClientePorRfc(RFC_PUBLICO_GENERAL)
            }
        })
}

// Captura: al presionar Enter o salir del campo se busca el RFC escrito
$("#txtRfcCliente").on("keydown", function (e) {
    if (e.key == "Enter") $(this).blur()
})
$("#txtRfcCliente").on("change", function () {
    const rfc = $(this).val().trim().toUpperCase()
    cargarClientePorRfc(rfc == "" ? RFC_PUBLICO_GENERAL : rfc)
})

// Busqueda: ventana con todos los clientes activos para elegir uno
let ClientesActivos = [];
$("#btnBuscarCliente").click(function () {
    fetch("/Cliente/Lista")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            ClientesActivos = responseJson.data.filter(c => c.esActivo == 1)
            $("#txtFiltroCliente").val("")
            mostrarClientes()
            $("#modalClientes").modal("show")
        })
})

// "publico" debe encontrar "Público": se comparan sin acentos y en mayusculas
function sinAcentos(texto) {
    return texto.normalize("NFD").replace(/[̀-ͯ]/g, "").toUpperCase()
}

function mostrarClientes() {
    const filtro = sinAcentos($("#txtFiltroCliente").val().trim())
    $("#tbClientes tbody").html("")

    ClientesActivos
        .filter(c => c.rfc.includes(filtro) || sinAcentos(c.nombre).includes(filtro))
        .forEach((cliente) => {
            $("#tbClientes tbody").append(
                $("<tr>").append(
                    $("<td>").text(cliente.rfc),
                    $("<td>").text(cliente.nombre),
                    $("<td>").append(
                        $("<button>").addClass("btn btn-success btn-sm btn-elegir-cliente").text("Elegir").data("cliente", cliente)
                    )
                )
            )
        })
}

$("#txtFiltroCliente").on("input", mostrarClientes)

$("#tbClientes tbody").on("click", ".btn-elegir-cliente", function () {
    ponerCliente($(this).data("cliente"))
    $("#modalClientes").modal("hide")
})


//________________________ Productos ________________________

let ProductosParaVenta = [];
$("#cboBuscarProducto").on("select2:select", function (e) {
    const data = e.params.data;

    let producto_encontrado = ProductosParaVenta.filter(p => p.idProducto == data.id)
    if (producto_encontrado.length > 0) {
        $("#cboBuscarProducto").val("").trigger("change")
        toastr.warning("", "El producto ya fue agregado")
        return false
    }

    swal({
        title: data.marca,
        text: data.text,
        imageUrl: data.urlImagen,
        type:"input",
        showCancelButton: true,
        closeOnConfirm: false,
        inputPlaceholder: "Ingrese Cantidad"
    },
        function (valor) {

            if (valor === false) return false;

            if (valor === "") {
                toastr.warning("", "Necesita ingresar la cantidad")
                return false;
            }
            const cantidad = parseInt(valor)
            if (isNaN(cantidad) || cantidad <= 0) {
                toastr.warning("", "Debe ingresar un numero mayor a 0")
                return false;
            }
            if (cantidad > data.stock) {
                toastr.warning("", `Solo hay ${data.stock} en stock`)
                return false;
            }

            let producto = {
                idProducto: data.id,
                marcaProducto: data.marca,
                descripcionProducto: data.text,
                categoriaProducto: data.categoria,
                cantidad: cantidad,
                precio: data.precio,
                porcentajeDescuento: data.descuento,
                tasaImpuesto: data.tasaImpuesto
            }
            calcularProducto(producto)

            ProductosParaVenta.push(producto)

            mostrarProducto_Precios();
            $("#cboBuscarProducto").val("").trigger("change")
            swal.close()
        }
    )

})

// Mismo calculo que CalculoVenta.cs (diapositiva 27); aqui es solo para mostrarlo.
// Ejemplo: 1 x 5000 con 15 % -> descuento 750, subtotal 4250, IVA 680, total 4930
function redondear(valor) {
    return Math.round((valor + Number.EPSILON) * 100) / 100
}

function calcularProducto(item) {
    const importe = redondear(item.cantidad * item.precio)
    item.descuento = redondear(importe * item.porcentajeDescuento / 100)
    item.subTotal = redondear(importe - item.descuento)
    item.impuesto = redondear(item.subTotal * item.tasaImpuesto / 100)
    item.total = redondear(item.subTotal + item.impuesto)
}

function mostrarProducto_Precios() {

    let descuento = 0;
    let subtotal = 0;
    let impuesto = 0;
    let total = 0;

    $("#tbProducto tbody").html("")

    ProductosParaVenta.forEach((item) => {

        descuento += item.descuento
        subtotal += item.subTotal
        impuesto += item.impuesto
        total += item.total

        $("#tbProducto tbody").append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-eliminar btn-sm").append(
                        $("<i>").addClass("fas fa-trash-alt")
                    ).data("idProducto",item.idProducto)
                ),
                $("<td>").text(item.descripcionProducto),
                $("<td>").addClass("text-right").text(item.cantidad),
                $("<td>").addClass("text-right").text(item.precio.toFixed(2)),
                $("<td>").addClass("text-right").text(`${item.descuento.toFixed(2)} (${item.porcentajeDescuento}%)`),
                $("<td>").addClass("text-right").text(item.subTotal.toFixed(2)),
                $("<td>").addClass("text-right").text(`${item.impuesto.toFixed(2)} (${item.tasaImpuesto}%)`),
                $("<td>").addClass("text-right").text(item.total.toFixed(2))
            )
        )
    })

    $("#txtImporte").val((subtotal + descuento).toFixed(2))
    $("#txtDescuento").val(descuento.toFixed(2))
    $("#txtSubTotal").val(subtotal.toFixed(2))
    $("#txtIGV").val(impuesto.toFixed(2))
    $("#txtTotal").val(total.toFixed(2))
}

$(document).on("click", "button.btn-eliminar", function () {

    const _idproducto = $(this).data("idProducto")

    ProductosParaVenta = ProductosParaVenta.filter(p => p.idProducto != _idproducto);

    mostrarProducto_Precios();
})


$("#btnTerminarVenta").click(function () {

    if (ProductosParaVenta.length < 1) {
        toastr.warning("", "Debe ingresar productos")
        return;
    }

    // Solo se manda que se vende y a quien; el servidor calcula precios, descuentos e impuestos
    const venta = {
        idTipoDocumentoVenta: $("#cboTipoDocumentoVenta").val(),
        idCliente: parseInt($("#txtIdCliente").val()),
        DetalleVenta: ProductosParaVenta.map(p => ({ idProducto: p.idProducto, cantidad: p.cantidad }))
    }

    $("#btnTerminarVenta").LoadingOverlay("show");

    fetch("/Venta/RegistrarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(venta)
    })
        .then(response => {
            $("#btnTerminarVenta").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                ProductosParaVenta = [];
                mostrarProducto_Precios();

                cargarClientePorRfc(RFC_PUBLICO_GENERAL)
                $("#cboTipoDocumentoVenta").val($("#cboTipoDocumentoVenta option:first").val())

                swal("Registrado!", `Numero Venta : ${responseJson.objeto.numeroVenta}`, "success")
            } else {
                swal("Lo sentimos!", responseJson.mensaje || "No se pudo registrar la venta", "error")
            }
        })

})
