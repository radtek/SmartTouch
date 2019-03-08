using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;

namespace LandmarkIT.Enterprise.Utilities.Common
{
    public static class ImageProcessor
    {
        public static Bitmap CreateImage(Uri imageUri, int width, int height)
        {
            var request = WebRequest.Create(imageUri);
            var sourceImage = default(Image);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                sourceImage = Bitmap.FromStream(stream);
            }

            return CreateImage(sourceImage, width, height);
        }
        public static Bitmap CreateImage(Image sourceImage, int width, int height)
        {
            var image = new Bitmap(width, height);

            using (var gr = Graphics.FromImage(image))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(sourceImage, new Rectangle(0, 0, width, height));
            }
            
            return image;
        }
    }
}
