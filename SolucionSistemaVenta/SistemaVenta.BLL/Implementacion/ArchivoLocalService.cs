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

        // Lo que se sube queda publico en wwwroot: solo se aceptan imagenes.
        // Se revisa el CONTENIDO, no el nombre: al editar se reutiliza el nombre viejo (ej. 007_monitor.jpg)
        // y cualquier archivo renombrado a .jpg pasaria. Cada formato empieza con una firma fija.
        private static bool EsImagen(Stream archivo)
        {
            byte[] inicio = new byte[12];
            if (archivo.CanSeek) archivo.Position = 0;
            int leidos = archivo.Read(inicio, 0, inicio.Length);
            if (archivo.CanSeek) archivo.Position = 0;
            if (leidos < 12) return false;

            bool jpg = inicio[0] == 0xFF && inicio[1] == 0xD8 && inicio[2] == 0xFF;
            bool png = inicio[0] == 0x89 && inicio[1] == 0x50 && inicio[2] == 0x4E && inicio[3] == 0x47;
            bool gif = inicio[0] == 0x47 && inicio[1] == 0x49 && inicio[2] == 0x46 && inicio[3] == 0x38;
            bool webp = inicio[0] == 0x52 && inicio[1] == 0x49 && inicio[2] == 0x46 && inicio[3] == 0x46   // RIFF
                     && inicio[8] == 0x57 && inicio[9] == 0x45 && inicio[10] == 0x42 && inicio[11] == 0x50; // WEBP
            return jpg || png || gif || webp;
        }

        public async Task<string> SubirStorage(Stream streamArchivo, string carpetaDestino, string nombreArchivo)
        {
            if (!EsImagen(streamArchivo))
                throw new TaskCanceledException("Solo se permiten imagenes jpg, png, gif o webp");

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
