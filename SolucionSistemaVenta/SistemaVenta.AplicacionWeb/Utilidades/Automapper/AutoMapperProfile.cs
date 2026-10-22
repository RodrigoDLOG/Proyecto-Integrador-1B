
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.Entity;
using System.Globalization;
using AutoMapper;

namespace SistemaVenta.AplicacionWeb.Utilidades.Automapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Rol
            CreateMap<Rol, VMRol>().ReverseMap();
            #endregion Rol

            #region Usuario
            CreateMap<Usuario, VMUsuario>()
                .ForMember(destino =>
                    destino.EsActivo,
                    opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)
                )
                .ForMember(destino =>
                    destino.NombreRol,
                    opt => opt.MapFrom(origen => origen.IdRolNavigation.Descripcion)
                );

            CreateMap<VMUsuario, Usuario>()
                .ForMember(destino =>
                    destino.EsActivo,
                    opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)
                )
                .ForMember(destino =>
                    destino.IdRolNavigation,
                    opt => opt.Ignore()
                    );

            #endregion

            #region Negocio
            CreateMap<Negocio, VMNegocio>()
                .ForMember(destino =>
                    destino.PorcentajeImpuesto,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.PorcentajeImpuesto.Value, new CultureInfo("es-MX")))
                );

            CreateMap<VMNegocio , Negocio>()
               .ForMember(destino =>
                   destino.PorcentajeImpuesto,
                   opt => opt.MapFrom(origen => Convert.ToDecimal(origen.PorcentajeImpuesto, new CultureInfo("es-MX")))
               );


            #endregion

            #region Categoria
            CreateMap<Categoria, VMCategoria>()
           .ForMember(destino =>
               destino.esActivo,
               opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)
           );

            CreateMap<VMCategoria, Categoria>()
            .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.esActivo == 1 ? true : false)
            );
            #endregion

            #region Producto

            CreateMap<Producto, VMProducto>()
                .ForMember(destino =>
                    destino.EsActivo,
                    opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)
                )
                .ForMember(destino =>
                    destino.NombreCategoria,
                    opt => opt.MapFrom(origen => origen.IdCategoriaNavigation.Descripcion)
                )
                .ForMember(destino =>
                    destino.Precio,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("es-MX")))
                )
                // Version Mexico
                .ForMember(destino =>
                    destino.NombreMedidaLocal,
                    opt => opt.MapFrom(origen => origen.IdMedidaLocalNavigation.Descripcion)
                )
                .ForMember(destino =>
                    destino.NombreClaveUnidad,
                    opt => opt.MapFrom(origen => origen.ClaveUnidadNavigation.Nombre)
                )
                .ForMember(destino =>
                    destino.DescripcionClaveProdServ,
                    opt => opt.MapFrom(origen => origen.ClaveProdServNavigation.Descripcion)
                )
                .ForMember(destino =>
                    destino.Descuento,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.Descuento ?? 0, new CultureInfo("es-MX")))
                )
                .ForMember(destino =>
                    destino.TasaImpuesto,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.TasaImpuesto ?? 0, new CultureInfo("es-MX")))
                );

            CreateMap<VMProducto, Producto >()
                .ForMember(destino =>
                    destino.EsActivo,
                    opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)
                )
                .ForMember(destino =>
                    destino.IdCategoriaNavigation,
                    opt => opt.Ignore()
                )
                .ForMember(destino =>
                    destino.Precio,
                    opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Precio, new CultureInfo("es-MX")))
                )
                // Version Mexico
                .ForMember(destino => destino.IdMedidaLocalNavigation, opt => opt.Ignore())
                .ForMember(destino => destino.ClaveUnidadNavigation, opt => opt.Ignore())
                .ForMember(destino => destino.ClaveProdServNavigation, opt => opt.Ignore())
                .ForMember(destino =>
                    destino.Descuento,
                    opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Descuento, new CultureInfo("es-MX")))
                )
                .ForMember(destino =>
                    destino.TasaImpuesto,
                    opt => opt.MapFrom(origen => Convert.ToDecimal(origen.TasaImpuesto, new CultureInfo("es-MX")))
                );

            #endregion

            #region Cliente
            CreateMap<Cliente, VMCliente>()
                .ForMember(destino =>
                    destino.EsActivo,
                    opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)
                );

            CreateMap<VMCliente, Cliente>()
                .ForMember(destino =>
                    destino.EsActivo,
                    opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)
                );
            #endregion

            #region Catalogos SAT
            // Todos los catalogos se mandan a la pantalla con la misma forma: Clave + Descripcion
            CreateMap<CRegimenFiscalSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CRegimenFiscal));
            CreateMap<CUsoCfdiSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CUsoCfdi));
            CreateMap<CFormaPagoSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CFormaPago));
            CreateMap<CMetodoPagoSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CMetodoPago));
            CreateMap<CTipoDeComprobanteSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CTipoDeComprobante));
            CreateMap<CObjetoImpSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CObjetoImpuesto));
            CreateMap<CImpuestoSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CImpuesto));
            CreateMap<CClaveUnidadSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CClaveUnidad))
                .ForMember(destino => destino.Descripcion, opt => opt.MapFrom(origen => origen.Nombre));
            CreateMap<CClaveProdServSat, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.CClaveProdServ));
            CreateMap<CMedidaLocal, VMCatalogo>()
                .ForMember(destino => destino.Clave, opt => opt.MapFrom(origen => origen.IdMedidaLocal.ToString()));
            #endregion

            #region TipoDocumentoVenta
            CreateMap<TipoDocumentoVenta, VMTipoDocumentoVenta>().ReverseMap();
            #endregion

            #region Venta
            CreateMap<Venta,VMVenta>()
                 .ForMember(destino =>
                    destino.TipoDocumentoVenta,
                    opt => opt.MapFrom(origen => origen.IdTipoDocumentoVentaNavigation.Descripcion)
                )
                 .ForMember(destino =>
                    destino.Usuario,
                    opt => opt.MapFrom(origen => origen.IdUsuarioNavigation.Nombre)
                )
                .ForMember(destino =>
                    destino.SubTotal,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.SubTotal.Value, new CultureInfo("es-MX")))
                )
                .ForMember(destino =>
                    destino.ImpuestoTotal,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.ImpuestoTotal.Value, new CultureInfo("es-MX")))
                )
                 .ForMember(destino =>
                    destino.Total,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("es-MX")))
                )
                 .ForMember(destino =>
                    destino.FechaRegistro,
                    opt => opt.MapFrom(origen => origen.FechaRegistro.Value.ToString("dd/MM/yyyy"))
                );

            CreateMap<VMVenta, Venta>()
              .ForMember(destino =>
                   destino.SubTotal,
                   opt => opt.MapFrom(origen => Convert.ToDecimal(origen.SubTotal, new CultureInfo("es-MX")))
               )
               .ForMember(destino =>
                   destino.ImpuestoTotal,
                   opt => opt.MapFrom(origen => Convert.ToDecimal(origen.ImpuestoTotal, new CultureInfo("es-MX")))
               )
                .ForMember(destino =>
                   destino.Total,
                   opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Total, new CultureInfo("es-MX")))
               );
            #endregion

            #region DetalleVenta
            CreateMap<DetalleVenta, VMDetalleVenta>()
                 .ForMember(destino =>
                    destino.Precio,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("es-MX")))
                )
                 .ForMember(destino =>
                    destino.Total,
                    opt => opt.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("es-MX")))
                );

            CreateMap< VMDetalleVenta,DetalleVenta>()
               .ForMember(destino =>
                  destino.Precio,
                  opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Precio, new CultureInfo("es-MX")))
              )
               .ForMember(destino =>
                  destino.Total,
                  opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Total, new CultureInfo("es-MX")))
              );

            CreateMap<DetalleVenta,VMReporteVenta>()
            .ForMember(destino =>
                  destino.FechaRegistro,
                  opt => opt.MapFrom(origen => origen.IdVentaNavigation.FechaRegistro.Value.ToString("dd/MM/yyyy"))
              )
            .ForMember(destino =>
                  destino.NumeroVenta,
                  opt => opt.MapFrom(origen => origen.IdVentaNavigation.NumeroVenta )
              )
            .ForMember(destino =>
                  destino.TipoDocumento,
                  opt => opt.MapFrom(origen => origen.IdVentaNavigation.IdTipoDocumentoVentaNavigation.Descripcion)
              )
            .ForMember(destino =>
                  destino.DocumentoCliente,
                  opt => opt.MapFrom(origen => origen.IdVentaNavigation.DocumentoCliente)
              )
             .ForMember(destino =>
                  destino.NombreCliente,
                  opt => opt.MapFrom(origen => origen.IdVentaNavigation.NombreCliente)
              )

              .ForMember(destino =>
                  destino.SubTotalVenta,
                  opt => opt.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.SubTotal.Value, new CultureInfo("es-MX")))
              )
              .ForMember(destino =>
                  destino.ImpuestoTotalVenta,
                  opt => opt.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.ImpuestoTotal.Value, new CultureInfo("es-MX")))
              )
               .ForMember(destino =>
                  destino.TotalVenta,
                  opt => opt.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.Total.Value, new CultureInfo("es-MX")))
              )
               .ForMember(destino =>
                  destino.Producto,
                  opt => opt.MapFrom(origen => origen.DescripcionProducto)
              )
               .ForMember(destino =>
                  destino.Precio,
                  opt => opt.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("es-MX")))
              )
                .ForMember(destino =>
                  destino.Total,
                  opt => opt.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("es-MX")))
              )
            ;

            #endregion

            #region Menu
            CreateMap<Menu, VMMenu>()
                 .ForMember(destino =>
                  destino.SubMenus,
                  opt => opt.MapFrom(origen => origen.InverseIdMenuPadreNavigation)
              );
            #endregion

        }

    }
}
