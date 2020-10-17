// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Drawing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 图像扩展方法。
    /// </summary>
    public static class ImageExtension
    {
        /// <summary>
        /// 生成缩略图。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="width">缩略图宽度。</param>
        /// <param name="height">缩略图高度。</param>
        /// <param name="style">缩略图样式。</param>
        /// <returns></returns>
        public static Image Thumb(this Image image, int width, int height, ThumbnailStyle style = ThumbnailStyle.Stretch)
        {
            if (image.Width < width && image.Height < height)
            {
                return image;
            }

            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                switch (style)
                {
                    case ThumbnailStyle.Stretch:
                        g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                        break;
                    case ThumbnailStyle.Zoom:
                        g.DrawImage(image, GetZoomRectangle(image.Width, image.Height, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                        break;
                    case ThumbnailStyle.Center:
                        g.DrawImage(image, new Rectangle(0, 0, width, height), GetCenterRectangle(image.Width, image.Height, width, height), GraphicsUnit.Pixel);
                        break;
                }
            }

            return bitmap;
        }

        /// <summary>
        /// 等比例缩放图像。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="scale">缩放的比例。</param>
        /// <returns></returns>
        public static Image Zoom(this Image image, float scale)
        {
            var width = (int)(image.Width * scale);
            var height = (int)(image.Height * scale);

            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        /// <summary>
        /// 使用限制的范围大小来缩放图像。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="width">限制的宽度。</param>
        /// <param name="height">限制的高度。</param>
        /// <returns></returns>
        public static Image Zoom(this Image image, int width, int height)
        {
            var rect = GetZoomRectangle(image.Width, image.Height, width, height);

            var bitmap = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(image, new Rectangle(0, 0, rect.Width, rect.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        /// <summary>
        /// 压缩图像，压缩后图像的质量的大小都会降低。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="quality">压缩质量，取值为 0-100。</param>
        /// <param name="format">压缩的格式。</param>
        /// <returns></returns>
        public static Image Compress(this Image image, int quality, CompressFormat format = CompressFormat.JPEG)
        {
            using (var memory = new MemoryStream())
            {
                if (image.Compress(memory, quality, format))
                {
                    return Image.FromStream(memory);
                }
            }

            return image;
        }

        /// <summary>
        /// 压缩图像到流容器，压缩后图像的质量的大小都会降低。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="stream">流容器。</param>
        /// <param name="quality">压缩质量，取值为 0-100。</param>
        /// <param name="format">压缩的格式。</param>
        /// <returns></returns>
        public static bool Compress(this Image image, Stream stream, int quality, CompressFormat format = CompressFormat.JPEG)
        {
            var ep = new EncoderParameters();
            var qy = new long[1];
            qy[0] = quality;

            var eParam = new EncoderParameter(Encoder.Quality, qy);
            ep.Param[0] = eParam;

            var arrayICI = ImageCodecInfo.GetImageEncoders();
            var jpegICIinfo = arrayICI.FirstOrDefault(s => s.FormatDescription.Equals(format.ToString()));

            if (jpegICIinfo != null)
            {
                image.Save(stream, jpegICIinfo, ep);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 保存到流容器。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="format">输出的图片格式。</param>
        /// <returns></returns>
        public static Stream SaveToStream(this Image image, ImageFormat format = null)
        {
            var stream = new MemoryStream();
            image.Save(stream, format ?? ImageFormat.Png);
            return stream;
        }

        /// <summary>
        /// 获取居中显示的绘制矩形。
        /// </summary>
        /// <param name="imgWidth"></param>
        /// <param name="imgHeight"></param>
        /// <param name="thuWidth"></param>
        /// <param name="thuHeight"></param>
        /// <returns></returns>
        private static Rectangle GetCenterRectangle(int imgWidth, int imgHeight, int thuWidth, int thuHeight)
        {
            var x = (imgWidth - thuWidth) / 2;
            var y = (imgHeight - thuHeight) / 2;
            return new Rectangle(x, y, thuWidth, thuHeight);
        }

        /// <summary>
        /// 获取等比例缩放的绘制矩形。
        /// </summary>
        /// <param name="imgWidth"></param>
        /// <param name="imgHeight"></param>
        /// <param name="thuWidth"></param>
        /// <param name="thuHeight"></param>
        /// <returns></returns>
        private static Rectangle GetZoomRectangle(int imgWidth, int imgHeight, int thuWidth, int thuHeight)
        {
            int sw;
            int sh;

            if (imgHeight > thuHeight || imgWidth > thuWidth)
            {
                if ((imgWidth * thuHeight) > (imgHeight * thuWidth))
                {
                    sw = thuWidth;
                    sh = (thuWidth * imgHeight) / imgWidth;
                }
                else
                {
                    sh = thuHeight;
                    sw = (imgWidth * thuHeight) / imgHeight;
                }
            }
            else
            {
                sw = imgHeight;
                sh = imgWidth;
            }

            return new Rectangle((thuWidth - sw) / 2, (thuHeight - sh) / 2, sw, sh);
        }
    }
}