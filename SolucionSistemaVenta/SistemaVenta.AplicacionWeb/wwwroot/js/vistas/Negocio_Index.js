
$(document).ready(function () {


    $(".card-body").LoadingOverlay("show");

    // Version Mexico: primero el catalogo de regimenes para el combo, despues los datos del negocio
    fetch("/Catalogo/RegimenesFiscales")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            responseJson.forEach((item) => {
                $("#cboRegimenFiscal").append(
                    $("<option>").val(item.clave).text(`${item.clave} - ${item.descripcion}`)
                )
            })
            return fetch("/Negocio/Obtener")
        })
        .then(response => {
            $(".card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                const d = responseJson.objeto

                $("#txtRfc").val(d.rfc)
                $("#txtRazonSocial").val(d.nombre)
                $("#txtCorreo").val(d.correo)
                $("#txtDireccion").val(d.direccion)
                $("#txTelefono").val(d.telefono)
                $("#txtCodigoPostal").val(d.codigoPostal)
                $("#cboRegimenFiscal").val(d.regimenFiscal)
                $("#txtSimboloMoneda").val(d.simboloMoneda)
                $("#imgLogo").attr("src", d.urlLogo)


            } else {
                swal("Los sentimos", responseJson.mensaje, "error")

            }
        })


})


$("#btnGuardarCambios").click(function () {

    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "")

    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo : "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje)
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus()
        return;
    }

    const modelo = {
        rfc: $("#txtRfc").val().trim().toUpperCase(),
        nombre: $("#txtRazonSocial").val(),
        correo: $("#txtCorreo").val(),
        direccion: $("#txtDireccion").val(),
        telefono: $("#txTelefono").val(),
        codigoPostal: $("#txtCodigoPostal").val(),
        regimenFiscal: $("#cboRegimenFiscal").val(),
        simboloMoneda: $("#txtSimboloMoneda").val()
    }

    const inputLogo = document.getElementById("txtLogo")

    const formData = new FormData()

    formData.append("logo", inputLogo.files[0])
    formData.append("modelo",JSON.stringify(modelo))


    $(".card-body").LoadingOverlay("show");


    fetch("/Negocio/GuardarCambios", {
        method: "POST",
        body: formData
        })
        .then(response => {
            $(".card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                const d = responseJson.objeto

                $("#imgLogo").attr("src",d.urlLogo)
                swal("Listo!", "Los datos del negocio fueron guardados", "success")

            } else {
                swal("Los sentimos", responseJson.mensaje, "error")

            }
        })


})
