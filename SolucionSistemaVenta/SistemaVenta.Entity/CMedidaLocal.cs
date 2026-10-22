using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

// Tabla c_Medida_Local (diapositiva 23): unidad de medida propia de la empresa
public partial class CMedidaLocal
{
    public int IdMedidaLocal { get; set; }

    public string? Descripcion { get; set; }

    public bool? EsActivo { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
