using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class ProductoService : IProductoService
    {
        private readonly IGenericRepository<Producto> _repositorio;
        private readonly IArchivoService _archivoService;

        public ProductoService(IGenericRepository<Producto> repositorio,
            IArchivoService archivoService)
        {
            _repositorio = repositorio;
            _archivoService = archivoService;
        }

        public async Task<List<Producto>> Lista()
        {
            IQueryable<Producto> query = await _repositorio.Consultar();
            return IncluirRelaciones(query).ToList();
        }

        public async Task<Producto> Crear(Producto entidad, Stream? imagen = null, string NombreImagen = "")
        {
            ValidarDatosSat(entidad);

            Producto producto_existe = await _repositorio.Obtener(p => p.CodigoBarra == entidad.CodigoBarra);

            if (producto_existe != null)
                throw new TaskCanceledException("El codigo de barra ya existe");

            try
            {
                entidad.NombreImagen = NombreImagen;
                if (imagen != null)
                {
                    string urlImage = await _archivoService.SubirStorage(imagen, "producto", NombreImagen);
                    entidad.UrlImagen = urlImage;
                }

                Producto producto_creado = await _repositorio.Crear(entidad);

                if (producto_creado.IdProducto == 0)
                    throw new TaskCanceledException("No se pudo crear el producto");

                IQueryable<Producto> query = await _repositorio.Consultar(p => p.IdProducto == producto_creado.IdProducto);

                producto_creado = IncluirRelaciones(query).First();

                return producto_creado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Producto> Editar(Producto entidad, Stream? imagen = null, string NombreImagen = "")
        {
            ValidarDatosSat(entidad);

            Producto producto_existe = await _repositorio.Obtener(p => p.CodigoBarra == entidad.CodigoBarra && p.IdProducto != entidad.IdProducto);

            if (producto_existe != null)
                throw new TaskCanceledException("El codigo de barra ya existe");

            try
            {
                IQueryable<Producto> queryProducto = await _repositorio.Consultar(p => p.IdProducto == entidad.IdProducto);

                Producto producto_para_editar = queryProducto.First();

                producto_para_editar.CodigoBarra = entidad.CodigoBarra;
                producto_para_editar.Marca = entidad.Marca;
                producto_para_editar.Descripcion = entidad.Descripcion;
                producto_para_editar.IdCategoria = entidad.IdCategoria;
                producto_para_editar.Stock = entidad.Stock;
                producto_para_editar.Precio = entidad.Precio;
                producto_para_editar.EsActivo = entidad.EsActivo;

                // Version Mexico (diapositiva 22)
                producto_para_editar.IdMedidaLocal = entidad.IdMedidaLocal;
                producto_para_editar.ClaveUnidad = entidad.ClaveUnidad;
                producto_para_editar.ClaveProdServ = entidad.ClaveProdServ;
                producto_para_editar.Descuento = entidad.Descuento;
                producto_para_editar.ObjetoImp = entidad.ObjetoImp;
                producto_para_editar.ClaveImpuesto = entidad.ClaveImpuesto;
                producto_para_editar.TasaImpuesto = entidad.TasaImpuesto;

                if (producto_para_editar.NombreImagen == "")
                {
                    producto_para_editar.NombreImagen = NombreImagen;
                }

                if (imagen != null)
                {
                    string urlImagen = await _archivoService.SubirStorage(imagen, "producto", producto_para_editar.NombreImagen);
                    producto_para_editar.UrlImagen = urlImagen;
                }

                bool respuesta = await _repositorio.Editar(producto_para_editar);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar el producto");

                Producto producto_editado = IncluirRelaciones(queryProducto).First();

                return producto_editado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idProducto)
        {
            try
            {
                Producto producto_encontrado = await _repositorio.Obtener(p => p.IdProducto == idProducto);

                if (producto_encontrado == null)
                    throw new TaskCanceledException("El producto no existe");

                string nombreImagen = producto_encontrado.NombreImagen;

                bool respuesta = await _repositorio.Eliminar(producto_encontrado);

                if (respuesta)
                    await _archivoService.EliminarStorage("producto", nombreImagen);

                return true;
            }
            catch
            {
                throw;
            }
        }

        // Categoria y las descripciones de las claves SAT que se muestran en pantalla
        private static IQueryable<Producto> IncluirRelaciones(IQueryable<Producto> query)
        {
            return query.Include(p => p.IdCategoriaNavigation)
                        .Include(p => p.IdMedidaLocalNavigation)
                        .Include(p => p.ClaveUnidadNavigation)
                        .Include(p => p.ClaveProdServNavigation);
        }

        // Lo que necesita el producto para poder facturarse (diapositivas 22 a 25)
        private static void ValidarDatosSat(Producto entidad)
        {
            if (entidad.Precio == null || entidad.Precio <= 0)
                throw new TaskCanceledException("El precio unitario debe ser mayor a 0");
            if (entidad.IdMedidaLocal == null || entidad.IdMedidaLocal == 0)
                throw new TaskCanceledException("Seleccione la unidad de medida de la empresa");
            if (string.IsNullOrWhiteSpace(entidad.ClaveUnidad))
                throw new TaskCanceledException("Seleccione la unidad de medida del SAT");
            if (string.IsNullOrWhiteSpace(entidad.ClaveProdServ))
                throw new TaskCanceledException("Seleccione la clave de producto del SAT");
            if (string.IsNullOrWhiteSpace(entidad.ObjetoImp))
                throw new TaskCanceledException("Seleccione el objeto del impuesto");

            entidad.Descuento ??= 0;
            if (entidad.Descuento < 0 || entidad.Descuento > 100)
                throw new TaskCanceledException("El descuento debe estar entre 0 y 100 %");

            // 01 = no objeto de impuesto: no lleva impuesto ni tasa
            if (entidad.ObjetoImp == "01")
            {
                entidad.ClaveImpuesto = null;
                entidad.TasaImpuesto = 0;
                return;
            }

            if (string.IsNullOrWhiteSpace(entidad.ClaveImpuesto))
                throw new TaskCanceledException("Seleccione el impuesto");
            if (entidad.TasaImpuesto == null || entidad.TasaImpuesto < 0 || entidad.TasaImpuesto > 100)
                throw new TaskCanceledException("La tasa del impuesto debe estar entre 0 y 100 %");
        }
    }
}
