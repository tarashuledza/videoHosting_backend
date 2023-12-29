using VideoHosting_Backend.Models.Video;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using MySqlX.XDevAPI.Common;

namespace VideoHosting_Backend.Services
{
    public class ByteConverterService
    {
        public static byte[] ConvertFileToByteAsync(IFormFile file)
        {
            byte[] data;

            using (var stream = new MemoryStream())
            {
                file.CopyToAsync(stream);
                data = stream.ToArray();
            }
            return data;
        }

        public static byte[] ConvertByteToImage(byte[] image)
        {
            using (var stream = new MemoryStream(image))
            {
                var imageData = System.Drawing.Image.FromStream(stream);
                return ImageToByteArray(imageData);
            }
        }

        public static byte[] ImageToByteArray(System.Drawing.Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
    }
}
