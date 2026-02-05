using SkiaSharp;
using Svg.Skia;

namespace AppTiemposV3.Api.Utilidades
{
    public static class SvgBase64ToPngBytes
    {

        public static byte[] ConvertSvgToPng(string base64Svg, int width = 128, int height = 128)
        {
            // quitar prefijo
            string? base64 = base64Svg.Substring(base64Svg.IndexOf(",") + 1);
            byte[]? svgBytes = Convert.FromBase64String(base64);

            using SKSvg? svg = new SKSvg();
            svg.Load(new MemoryStream(svgBytes));

            SKPicture? picture = svg.Picture;

            using SKBitmap? bitmap = new SKBitmap(width, height);
            using SKCanvas? canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            float scaleX = width / picture!.CullRect.Width;
            float scaleY = height / picture.CullRect.Height;
            canvas.Scale(scaleX, scaleY);

            canvas.DrawPicture(picture);
            canvas.Flush();

            using SKImage? image = SKImage.FromBitmap(bitmap);
            using SKData? data = image.Encode(SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }
    }
}
