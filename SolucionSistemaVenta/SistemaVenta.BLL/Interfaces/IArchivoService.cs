namespace SistemaVenta.BLL.Interfaces
{
    // Sustituye a IFireBaseService del tutorial: los archivos se guardan en wwwroot/archivos.
    public interface IArchivoService
    {
        Task<string> SubirStorage(Stream streamArchivo, string carpetaDestino, string nombreArchivo);
        Task<bool> EliminarStorage(string carpetaDestino, string nombreArchivo);
    }
}
