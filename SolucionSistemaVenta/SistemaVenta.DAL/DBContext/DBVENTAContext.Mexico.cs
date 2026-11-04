using Microsoft.EntityFrameworkCore;
using SistemaVenta.Entity;

namespace SistemaVenta.DAL.DBContext;

// Version Mexico (diapositivas 18 a 29): tablas y columnas nuevas (BaseDatos\03, 04 y 05).
// Va en un archivo aparte para no mezclarlo con lo que genero el Scaffold del tutorial:
// DBVENTAContext es "partial" y llama a OnModelCreatingPartial al final de OnModelCreating.
public partial class DBVENTAContext
{
    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<CMedidaLocal> CMedidaLocals { get; set; }

    public virtual DbSet<CRegimenFiscalSat> CRegimenFiscalSats { get; set; }

    public virtual DbSet<CUsoCfdiSat> CUsoCfdiSats { get; set; }

    public virtual DbSet<CFormaPagoSat> CFormaPagoSats { get; set; }

    public virtual DbSet<CMetodoPagoSat> CMetodoPagoSats { get; set; }

    public virtual DbSet<CTipoDeComprobanteSat> CTipoDeComprobanteSats { get; set; }

    public virtual DbSet<CObjetoImpSat> CObjetoImpSats { get; set; }

    public virtual DbSet<CImpuestoSat> CImpuestoSats { get; set; }

    public virtual DbSet<CClaveUnidadSat> CClaveUnidadSats { get; set; }

    public virtual DbSet<CClaveProdServSat> CClaveProdServSats { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        //________________________ columnas nuevas en tablas del tutorial ________________________

        modelBuilder.Entity<Negocio>(entity =>
        {
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("rfc");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("codigoPostal");
            entity.Property(e => e.RegimenFiscal)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("regimenFiscal");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(e => e.IdMedidaLocal).HasColumnName("idMedidaLocal");
            entity.Property(e => e.ClaveUnidad)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("claveUnidad");
            entity.Property(e => e.ClaveProdServ)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("claveProdServ");
            entity.Property(e => e.Descuento)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("descuento");
            entity.Property(e => e.ObjetoImp)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("objetoImp");
            entity.Property(e => e.ClaveImpuesto)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("claveImpuesto");
            entity.Property(e => e.TasaImpuesto)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("tasaImpuesto");

            // Para mostrar la descripcion de cada clave (ej. "H87 - Pieza")
            entity.HasOne(d => d.IdMedidaLocalNavigation).WithMany()
                .HasForeignKey(d => d.IdMedidaLocal)
                .HasConstraintName("FK_Producto_MedidaLocal");
            entity.HasOne(d => d.ClaveUnidadNavigation).WithMany()
                .HasForeignKey(d => d.ClaveUnidad)
                .HasConstraintName("FK_Producto_ClaveUnidad");
            entity.HasOne(d => d.ClaveProdServNavigation).WithMany()
                .HasForeignKey(d => d.ClaveProdServ)
                .HasConstraintName("FK_Producto_ClaveProdServ");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.DescuentoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("descuentoTotal");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.Property(e => e.PorcentajeDescuento)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("porcentajeDescuento");
            entity.Property(e => e.Descuento)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("descuento");
            entity.Property(e => e.SubTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subTotal");
            entity.Property(e => e.TasaImpuesto)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("tasaImpuesto");
            entity.Property(e => e.Impuesto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("impuesto");
            entity.Property(e => e.ClaveProdServ)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("claveProdServ");
            entity.Property(e => e.ClaveUnidad)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("claveUnidad");
            entity.Property(e => e.Unidad)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("unidad");
            entity.Property(e => e.ObjetoImp)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("objetoImp");
            entity.Property(e => e.ClaveImpuesto)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("claveImpuesto");
        });

        //________________________ tablas nuevas ________________________

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.ToTable("Clientes");

            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("rfc");
            entity.Property(e => e.DomicilioFiscalReceptor)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("domicilioFiscalReceptor");
            entity.Property(e => e.RegimenFiscalReceptor)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("regimenFiscalReceptor");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura);

            entity.ToTable("Factura");

            entity.Property(e => e.IdFactura).HasColumnName("idFactura");
            entity.Property(e => e.IdVenta).HasColumnName("idVenta");
            entity.Property(e => e.UsoCfdi)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("usoCFDI");
            entity.Property(e => e.RegimenFiscalReceptor)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("regimenFiscalReceptor");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("formaPago");
            entity.Property(e => e.TipoComprobante)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("tipoComprobante");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("metodoPago");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("codigoPostal");
            entity.Property(e => e.Uuid)
                .HasMaxLength(36)
                .IsUnicode(false)
                .HasColumnName("uuid");
            entity.Property(e => e.Folio)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("folio");
            entity.Property(e => e.FechaTimbrado)
                .HasColumnType("datetime")
                .HasColumnName("fechaTimbrado");
            entity.Property(e => e.XmlTimbrado).HasColumnName("xmlTimbrado");
            entity.Property(e => e.PdfTimbrado).HasColumnName("pdfTimbrado");
            entity.Property(e => e.MensajeError)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("mensajeError");
            entity.Property(e => e.Intentos).HasColumnName("intentos");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
        });

        modelBuilder.Entity<CMedidaLocal>(entity =>
        {
            entity.HasKey(e => e.IdMedidaLocal);

            entity.ToTable("c_Medida_Local");

            entity.Property(e => e.IdMedidaLocal).HasColumnName("idMedidaLocal");
            entity.Property(e => e.ClaveMedida)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("claveMedida");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
        });

        //________________________ catalogos del SAT (llave = clave del SAT) ________________________

        modelBuilder.Entity<CRegimenFiscalSat>(entity =>
        {
            entity.HasKey(e => e.CRegimenFiscal);
            entity.ToTable("c_RegimenFiscal");
            entity.Property(e => e.CRegimenFiscal).HasMaxLength(3).IsUnicode(false).HasColumnName("c_RegimenFiscal");
            entity.Property(e => e.Descripcion).HasMaxLength(150).HasColumnName("descripcion");
            entity.Property(e => e.Fisica).HasMaxLength(2).HasColumnName("fisica");
            entity.Property(e => e.Moral).HasMaxLength(2).HasColumnName("moral");
        });

        modelBuilder.Entity<CUsoCfdiSat>(entity =>
        {
            entity.HasKey(e => e.CUsoCfdi);
            entity.ToTable("c_UsoCFDI");
            entity.Property(e => e.CUsoCfdi).HasMaxLength(4).IsUnicode(false).HasColumnName("c_UsoCFDI");
            entity.Property(e => e.Descripcion).HasMaxLength(150).HasColumnName("Descripcion");
            entity.Property(e => e.AplicaPersonaFisica).HasMaxLength(2).HasColumnName("Aplica_persona_fisica");
            entity.Property(e => e.AplicaPersonaMoral).HasMaxLength(2).HasColumnName("Aplica_persona_moral");
            entity.Property(e => e.RegimenFiscalReceptor).HasMaxLength(200).IsUnicode(false).HasColumnName("Regimen_fiscal_receptor");
        });

        modelBuilder.Entity<CFormaPagoSat>(entity =>
        {
            entity.HasKey(e => e.CFormaPago);
            entity.ToTable("c_FormaPago_SAT");
            entity.Property(e => e.CFormaPago).HasMaxLength(2).IsUnicode(false).HasColumnName("c_FormaPago");
            entity.Property(e => e.Descripcion).HasMaxLength(100).HasColumnName("descripcion");
            entity.Property(e => e.Bancarizado).HasMaxLength(10).HasColumnName("bancarizado");
        });

        modelBuilder.Entity<CMetodoPagoSat>(entity =>
        {
            entity.HasKey(e => e.CMetodoPago);
            entity.ToTable("c_MetodoPago_SAT");
            entity.Property(e => e.CMetodoPago).HasMaxLength(3).IsUnicode(false).HasColumnName("c_MetodoPago");
            entity.Property(e => e.Descripcion).HasMaxLength(100).HasColumnName("descripcion");
        });

        modelBuilder.Entity<CTipoDeComprobanteSat>(entity =>
        {
            entity.HasKey(e => e.CTipoDeComprobante);
            entity.ToTable("c_TipoDeComprobante_SAT");
            entity.Property(e => e.CTipoDeComprobante).HasMaxLength(1).IsUnicode(false).HasColumnName("c_TipoDeComprobante");
            entity.Property(e => e.Descripcion).HasMaxLength(50).HasColumnName("descripcion");
        });

        modelBuilder.Entity<CObjetoImpSat>(entity =>
        {
            entity.HasKey(e => e.CObjetoImpuesto);
            entity.ToTable("c_ObjetoImp_SAT");
            entity.Property(e => e.CObjetoImpuesto).HasMaxLength(2).IsUnicode(false).HasColumnName("c_ObjetoImpuesto");
            entity.Property(e => e.Descripcion).HasMaxLength(200).HasColumnName("descripcion");
        });

        modelBuilder.Entity<CImpuestoSat>(entity =>
        {
            entity.HasKey(e => e.CImpuesto);
            entity.ToTable("c_Impuesto_SAT");
            entity.Property(e => e.CImpuesto).HasMaxLength(3).IsUnicode(false).HasColumnName("c_Impuesto");
            entity.Property(e => e.Descripcion).HasMaxLength(10).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.Retencion).HasMaxLength(2).HasColumnName("retencion");
            entity.Property(e => e.Traslado).HasMaxLength(2).HasColumnName("traslado");
        });

        modelBuilder.Entity<CClaveUnidadSat>(entity =>
        {
            entity.HasKey(e => e.CClaveUnidad);
            entity.ToTable("c_ClaveUnidad_SAT");
            entity.Property(e => e.CClaveUnidad).HasMaxLength(3).IsUnicode(false).HasColumnName("c_ClaveUnidad");
            entity.Property(e => e.Nombre).HasMaxLength(200).HasColumnName("nombre");
            entity.Property(e => e.Simbolo).HasMaxLength(50).HasColumnName("simbolo");
        });

        modelBuilder.Entity<CClaveProdServSat>(entity =>
        {
            entity.HasKey(e => e.CClaveProdServ);
            entity.ToTable("c_ClaveProdServ_SAT");
            entity.Property(e => e.CClaveProdServ).HasMaxLength(8).IsUnicode(false).HasColumnName("c_ClaveProdServ");
            entity.Property(e => e.Descripcion).HasMaxLength(300).HasColumnName("Descripcion");
            entity.Property(e => e.PalabrasSimilares).HasColumnName("Palabras_similares");
        });
    }
}
