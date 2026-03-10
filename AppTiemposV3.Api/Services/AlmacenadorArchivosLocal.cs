using AppTiemposV3.Api.Services.Interfaces;

namespace AppTiemposV3.Api.Services
{
    public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
    {
        private readonly IWebHostEnvironment _env;

        public AlmacenadorArchivosLocal(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> ObtenerBase64(string rutaCompleta)
        {
            try
            {
                if (!File.Exists(rutaCompleta))
                {
                    throw new FileNotFoundException($"No se encontró el archivo en la ruta: {rutaCompleta}");
                }

                byte[] archivoBytes = await File.ReadAllBytesAsync(rutaCompleta);

                string base64String = Convert.ToBase64String(archivoBytes);

                string extension = Path.GetExtension(rutaCompleta).Replace(".", "").ToLower();
                string mimeType = extension switch
                {
                    "png" => "image/png",
                    "jpg" or "jpeg" => "image/jpeg",
                    "pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };

                return $"data:{mimeType};base64,{base64String}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener Base64: {ex.Message}");
                throw;
            }
        }

        public async Task<string> Almacenar(string contenedor, IFormFile archivo)
        {
            try
            {
                if (string.IsNullOrEmpty(_env.WebRootPath))
                {
                    throw new Exception("WebRootPath no está configurado. Verificá Program.cs y que exista la carpeta wwwroot.");
                }

                string nombreArchivo = $"{archivo.FileName}";
                string folder = Path.Combine(_env.WebRootPath, contenedor);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string ruta = Path.Combine(folder, nombreArchivo);

                using (MemoryStream? ms = new MemoryStream())
                {
                    await archivo.CopyToAsync(ms);
                    Byte[] contenido = ms.ToArray();
                    await File.WriteAllBytesAsync(ruta, contenido);
                }

                return ruta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Almacenar: {ex.Message}");
                throw;
            }
        }

        public Task Borrar(string ruta, string contenedor)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return Task.CompletedTask;
            }

            string nombreArchivo = Path.GetFileName(ruta);
            string directorioArchivo = Path.Combine(_env.WebRootPath, contenedor, nombreArchivo);

            if (File.Exists(directorioArchivo))
            {
                File.Delete(directorioArchivo);
            }

            return Task.CompletedTask;
        }
    }
}
