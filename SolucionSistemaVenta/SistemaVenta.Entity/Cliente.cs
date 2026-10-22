using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

// Tabla Cliente (diapositiva 21)
public partial class Cliente
{
    public int IdCliente { get; set; }

    public string? Nombre { get; set; }

    public string? Correo { get; set; }

    public string? Rfc { get; set; }

    public string? DomicilioFiscalReceptor { get; set; }

    public string? RegimenFiscalReceptor { get; set; }

    public bool? EsActivo { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
