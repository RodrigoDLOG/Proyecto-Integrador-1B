using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IGenericRepository<Categoria> _repositorio;
        private readonly IGenericRepository<Producto> _repositorioProducto;

        // Pedimos DOS contratos: uno para categorias y otro para productos.
        // Dependencia.cs entrega un GenericRepository<Producto> gracias al registro con <> vacio.
        public CategoriaService(IGenericRepository<Categoria> repositorio, IGenericRepository<Producto> repositorioProducto)
        {
            _repositorio = repositorio;
            _repositorioProducto = repositorioProducto;
        }

        public async Task<List<Categoria>> Lista()
        {
            IQueryable<Categoria> query = await _repositorio.Consultar();
            return query.ToList();
        }

        public async Task<Categoria> Crear(Categoria entidad)
        {
            try
            {
                Categoria categoria_creada = await _repositorio.Crear(entidad);
                if (categoria_creada.IdCategoria == 0)
                    throw new TaskCanceledException("No se pudo crear la categoria");

                return categoria_creada;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Categoria> Editar(Categoria entidad)
        {
            try
            {
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == entidad.IdCategoria);
                categoria_encontrada.Descripcion = entidad.Descripcion;
                categoria_encontrada.EsActivo = entidad.EsActivo;

                bool respuesta = await _repositorio.Editar(categoria_encontrada);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar la categoria");

                return categoria_encontrada;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idCategoria)
        {
            try
            {
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == idCategoria);

                if (categoria_encontrada == null)
                    throw new TaskCanceledException("La categoria no existe");

                // La tabla Producto tiene llave foranea hacia Categoria: SQL no deja borrar una
                // categoria con productos. Se revisa antes para dar un mensaje claro.
                IQueryable<Producto> productos = await _repositorioProducto.Consultar(p => p.IdCategoria == idCategoria);
                if (productos.Any())
                    throw new TaskCanceledException("No se puede eliminar: la categoria tiene productos asignados");

                bool respuesta = await _repositorio.Eliminar(categoria_encontrada);

                return respuesta;
            }
            catch
            {
                throw;
            }
        }
    }
}
