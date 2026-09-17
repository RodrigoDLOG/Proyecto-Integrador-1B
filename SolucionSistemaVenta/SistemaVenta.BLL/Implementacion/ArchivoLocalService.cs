using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.BLL.Implementacion
{
    // Sustituye a FireBaseService del tutorial: guarda en wwwroot/archivos/{carpeta}/{nombre}
    // y devuelve la URL relativa que sirve UseStaticFiles.
    public class ArchivoLocalService : IArchivoService
    {
        private readonly string _rutaWebRoot;

        public ArchivoLocalService(string rutaWebRoot)
        {
            _rutaWebRoot = rutaWebRoot;
        }

        public async Task<string> SubirStorage(Stream streamArchivo, string carpetaDestino, string nombreArchivo)
        {
            try
            {
                string carpeta = Path.Combine(_rutaWebRoot, "archivos", carpetaDestino);
                Directory.CreateDirectory(carpeta);

                using (FileStream destino = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.Create))
                {
                    if (streamArchivo.CanSeek) streamArchivo.Position = 0;
                    await streamArchivo.CopyToAsync(destino);
                }

                return $"/archivos/{carpetaDestino}/{nombreArchivo}";
            }
            catch
            {
                return "";
            }
        }

        public Task<bool> EliminarStorage(string carpetaDestino, string nombreArchivo)
        {
            try
            {
                string ruta = Path.Combine(_rutaWebRoot, "archivos", carpetaDestino, nombreArchivo);
                if (File.Exists(ruta))
                    File.Delete(ruta);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
