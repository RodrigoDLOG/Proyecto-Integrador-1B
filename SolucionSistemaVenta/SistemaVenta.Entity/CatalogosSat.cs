using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

// Catalogos del SAT para CFDI 4.0 (tablas c_..._SAT). Se llenan con 03_catalogos_sat.sql
// y 04_catalogo_claveprodserv.sql, generados del Excel que dio el profesor.
// La llave de cada uno es la clave del SAT (texto), no un numero consecutivo.

public partial class CRegimenFiscalSat
{
    public string CRegimenFiscal { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Fisica { get; set; }

    public string? Moral { get; set; }
}

public partial class CUsoCfdiSat
{
    public string CUsoCfdi { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? AplicaPersonaFisica { get; set; }

    public string? AplicaPersonaMoral { get; set; }

    public string? RegimenFiscalReceptor { get; set; }
}

public partial class CFormaPagoSat
{
    public string CFormaPago { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Bancarizado { get; set; }
}

public partial class CMetodoPagoSat
{
    public string CMetodoPago { get; set; } = null!;

    public string? Descripcion { get; set; }
}

public partial class CTipoDeComprobanteSat
{
    public string CTipoDeComprobante { get; set; } = null!;

    public string? Descripcion { get; set; }
}

public partial class CObjetoImpSat
{
    public string CObjetoImpuesto { get; set; } = null!;

    public string? Descripcion { get; set; }
}

public partial class CImpuestoSat
{
    public string CImpuesto { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Retencion { get; set; }

    public string? Traslado { get; set; }
}

public partial class CClaveUnidadSat
{
    public string CClaveUnidad { get; set; } = null!;

    public string? Nombre { get; set; }

    public string? Simbolo { get; set; }
}

public partial class CClaveProdServSat
{
    public string CClaveProdServ { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? PalabrasSimilares { get; set; }
}
