using Microsoft.AspNetCore.Http;

namespace AppTiemposV3.Api.Utilidades
{
    public static class ConvertByteToFormFile
    {
        public static IFormFile ConvertByteArrayToIFormFile(byte[] byteArray, string fileName, string contentType)
        {
            MemoryStream? stream = new MemoryStream(byteArray);

            // IFormFile(Stream baseStream, long baseStreamOffset, long length, string name, string fileName)
            FormFile? formFile = new FormFile(stream, 0, byteArray.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return formFile;
        }
    }
}
