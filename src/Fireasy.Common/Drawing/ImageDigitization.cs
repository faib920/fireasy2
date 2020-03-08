// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Fireasy.Common.Drawing
{
    /// <summary>
    /// 图像数字化处理器。
    /// </summary>
    public static class ImageDigitization
    {
        /// <summary>
        /// 将图像进行反相处理。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <returns></returns>
        public static unsafe Image Invert(Image image)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            bitmap = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            byte* p = (byte*)(void*)scan0;
            var nOffset = stride - image.Width * 3;
            var nWidth = image.Width * 3;
            for (var y = 0; y < image.Height; ++y)
            {
                for (var x = 0; x < nWidth; ++x)
                {
                    p[0] = (byte)(255 - p[0]);
                    ++p;
                }
                p += nOffset;
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        /// <summary>
        /// 将图像进行灰度处理。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <returns></returns>
        public static unsafe Image Gray(Image image)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            bitmap = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            byte* p = (byte*)(void*)scan0;
            var nOffset = stride - image.Width * 3;
            byte red, green, blue;
            for (var y = 0; y < image.Height; ++y)
            {
                for (var x = 0; x < image.Width; ++x)
                {
                    blue = p[0];
                    green = p[1];
                    red = p[2];
                    p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);
                    p += 3;
                }
                p += nOffset;
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        /// <summary>
        /// 调整图像的亮度。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="brightness">亮度。</param>
        /// <returns></returns>
        public static unsafe Image Brightness(Image image, byte brightness)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            bitmap = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            var nVal = 0;
            byte* p = (byte*)(void*)scan0;
            int nOffset = stride - image.Width * 3;
            int nWidth = image.Width * 3;
            for (var y = 0; y < image.Height; ++y)
            {
                for (var x = 0; x < nWidth; ++x)
                {
                    nVal = (int)(p[0] + brightness);
                    if (nVal < 0) nVal = 0;
                    if (nVal > 255) nVal = 255;
                    p[0] = (byte)nVal;
                    ++p;
                }
                p += nOffset;
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        /// <summary>
        /// 对图像进行加噪处理。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="level">噪点的强度。</param>
        /// <param name="step">噪点的间隔。</param>
        /// <returns></returns>
        public static unsafe Image Noise(Image image, int level, int step)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            bitmap = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            byte* p = (byte*)(void*)scan0;
            var nOffset = stride - image.Width * 3;
            int blue = 0, green = 0, red = 0;
            var rad = new Random();
            for (var y = 0; y < image.Height; ++y)
            {
                for (var x = 0; x < image.Width; ++x)
                {
                    if (y % step == 0 && x % step == 0)
                    {
                        var mLev = rad.Next(2 * level) - level;
                        blue = p[0] + mLev;
                        green = p[1] + mLev;
                        red = p[2] + mLev;
                        if (blue < 0) blue = 0;
                        else if (blue > 255) blue = 255;
                        if (green < 0) green = 0;
                        else if (green > 255) green = 255;
                        if (red < 0) red = 0;
                        else if (red > 255) red = 255;
                        p[0] = (byte)blue;
                        p[1] = (byte)green;
                        p[2] = (byte)red;
                    }
                    p += 3;
                }
                p += nOffset;
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        /// <summary>
        /// 对图像进行马赛克处理。
        /// </summary>
        /// <param name="image">源图像。</param>
        /// <param name="pixelSize">方块的大小。</param>
        /// <returns></returns>
        public static unsafe Image Mosaic(Image image, int pixelSize)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            bitmap = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            byte* p = (byte*)(void*)scan0;
            var nOffset = stride - image.Width * 3;
            int blue = 0, green = 0, red = 0;
            var rad = new Random();
            for (var y = 0; y < image.Height; ++y)
            {
                for (var x = 0; x < image.Width; ++x)
                {
                    if (y % pixelSize == 0)
                    {
                        if (x % pixelSize == 0)
                        {
                            blue = p[0]; green = p[1]; red = p[2];
                        }
                        else
                        {
                            p[0] = (byte)blue;
                            p[1] = (byte)green;
                            p[2] = (byte)red;
                        }
                    }
                    else
                    {
                        byte* pTemp = p - bmData.Stride;

                        p[0] = (byte)pTemp[0];
                        p[1] = (byte)pTemp[1];
                        p[2] = (byte)pTemp[2];
                    }
                    p += 3;
                }
                p += nOffset;
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        /// <summary>
        /// 对图像进行二值化处理。
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static unsafe Image Binaryzation(Image image)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            bitmap = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            byte* p = (byte*)(void*)scan0;

            int means = GetThreshold(p, image.Height * stride);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width * 3; x += 3)
                {
                    if (p[y * stride + x + 2] > means)
                    {
                        p[y * stride + x]
                            = p[y * stride + x + 1]
                            = p[y * stride + x + 2]
                            = 255;
                    }
                    else
                    {
                        p[y * stride + x]
                            = p[y * stride + x + 1]
                            = p[y * stride + x + 2]
                            = 0;
                    }
                }
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        /// <summary>
        /// 对图像进行锐化处理。
        /// </summary>
        /// <param name="image"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static unsafe Image Sharpen(Image image, double v)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            var dstBmp = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var dstData = dstBmp.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte* pIn = (byte*)(void*)bmData.Scan0;
            byte* pOut = (byte*)(void*)dstData.Scan0;
            byte* p;
            var stride = bmData.Stride;
            var nOffset = stride - image.Width * 3;

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    //边缘八个点像素不变
                    if (x == 0 || x == image.Width - 1 || y == 0 || y == image.Height - 1)
                    {
                        pOut[0] = pIn[0];
                        pOut[1] = pIn[1];
                        pOut[2] = pIn[2];
                    }
                    else
                    {
                        int r0, r1, r2, r3, r4, r5, r6, r7, r8;
                        int g1, g2, g3, g4, g5, g6, g7, g8, g0;
                        int b1, b2, b3, b4, b5, b6, b7, b8, b0;
                        double vR, vG, vB;

                        //左上
                        p = pIn - stride - 3;
                        r1 = p[2];
                        g1 = p[1];
                        b1 = p[0];

                        //正上
                        p = pIn - stride;
                        r2 = p[2];
                        g2 = p[1];
                        b2 = p[0];

                        //右上
                        p = pIn - stride + 3;
                        r3 = p[2];
                        g3 = p[1];
                        b3 = p[0];

                        //左
                        p = pIn - 3;
                        r4 = p[2];
                        g4 = p[1];
                        b4 = p[0];

                        //右
                        p = pIn + 3;
                        r5 = p[2];
                        g5 = p[1];
                        b5 = p[0];

                        //左下
                        p = pIn + stride - 3;
                        r6 = p[2];
                        g6 = p[1];
                        b6 = p[0];

                        //正下
                        p = pIn + stride;
                        r7 = p[2];
                        g7 = p[1];
                        b7 = p[0];

                        // 右下 
                        p = pIn + stride + 3;
                        r8 = p[2];
                        g8 = p[1];
                        b8 = p[0];

                        //中心点
                        p = pIn;
                        r0 = p[2];
                        g0 = p[1];
                        b0 = p[0];

                        vR = (double)r0 - (double)(r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8) / 8;
                        vG = (double)g0 - (double)(g1 + g2 + g3 + g4 + g5 + g6 + g7 + g8) / 8;
                        vB = (double)b0 - (double)(b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8) / 8;

                        vR = r0 + vR * v;
                        vG = g0 + vG * v;
                        vB = b0 + vB * v;

                        vR = AdjustByte(vR);
                        vG = AdjustByte(vG);
                        vB = AdjustByte(vB);

                        pOut[0] = (byte)vB;
                        pOut[1] = (byte)vG;
                        pOut[2] = (byte)vR;

                    }
                    pIn += 3;
                    pOut += 3;
                }
                pIn += nOffset;
                pOut += nOffset;
            }

            bitmap.UnlockBits(bmData);
            dstBmp.UnlockBits(dstData);

            return dstBmp;
        }

        /// <summary>
        /// 调整图像的对比度。
        /// </summary>
        /// <param name="image"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static unsafe Image Contrast(Image image, double v)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            bitmap = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            byte* p = (byte*)(void*)scan0;
            var nOffset = stride - image.Width * 3;
            var nWidth = image.Width * 3;
            int temp;
            for (var y = 0; y < image.Height; ++y)
            {
                for (var x = 0; x < nWidth; ++x)
                {
                    temp = (int)((p[0] - 127) * v + 127);
                    temp = (temp > 255) ? 255 : temp < 0 ? 0 : temp;
                    p[0] = (byte)temp;
                    p++;
                }
                p += nOffset;
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        /// <summary>
        /// 对图像进行浮雕处理。
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static unsafe Image Relief(Image image)
        {
            if (!(image is Bitmap bitmap))
            {
                return image;
            }

            var dstBmp = Clone(bitmap);

            var bmData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var dstData = dstBmp.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte* pIn = (byte*)(void*)bmData.Scan0;
            byte* pOut = (byte*)(void*)dstData.Scan0;
            byte* p;
            var stride = bmData.Stride;
            var nOffset = stride - image.Width * 3;

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    //边缘八个点像素不变
                    if (x == 0 || x == image.Width - 1 || y == 0 || y == image.Height - 1)
                    {
                        pOut[0] = pIn[0];
                        pOut[1] = pIn[1];
                        pOut[2] = pIn[2];
                    }
                    else
                    {
                        int r0, r1;
                        int g1, g0;
                        int b1, b0;
                        double vR, vG, vB;
                        //右
                        p = pIn - 3;
                        r1 = p[2];
                        g1 = p[1];
                        b1 = p[0];

                        //中心点
                        p = pIn;
                        r0 = p[2];
                        g0 = p[1];
                        b0 = p[0];
                        //使用模板
                        vR = Math.Abs(r0 - r1 + 128);
                        vG = Math.Abs((g0 - g1 + 128));
                        vB = Math.Abs((b0 - b1 + 128));

                        vR = AdjustByte(vR);
                        vG = AdjustByte(vG);
                        vB = AdjustByte(vB);

                        pOut[0] = (byte)vB;
                        pOut[1] = (byte)vG;
                        pOut[2] = (byte)vR;
                    }
                    pIn += 3;
                    pOut += 3;
                }
                pIn += nOffset;
                pOut += nOffset;
            }

            bitmap.UnlockBits(bmData);
            dstBmp.UnlockBits(dstData);

            return dstBmp;
        }

        /// <summary>
        /// 图像二值化，获取阀值。
        /// </summary>
        /// <param name="inPixels"></param>
        /// <param name="length">height * Stride</param>
        /// <returns></returns>
        private static unsafe int GetThreshold(byte* inPixels, int length)
        {
            var inithreshold = 127;
            var finalthreshold = 0;
            var temp = new List<int>();
            for (var index = 0; index < length; index += 3)
            {
                temp.Add(inPixels[index + 2]);
            }

            var sub1 = new List<int>();
            var sub2 = new List<int>();
            int means1 = 0, means2 = 0;

            while (finalthreshold != inithreshold)
            {
                finalthreshold = inithreshold;
                for (var i = 0; i < temp.Count; i++)
                {
                    if (temp[i] <= inithreshold)
                    {
                        sub1.Add(temp[i]);
                    }
                    else
                    {
                        sub2.Add(temp[i]);
                    }
                }

                means1 = (int)sub1.Average();
                means2 = (int)sub2.Average();
                sub1.Clear();
                sub2.Clear();
                inithreshold = (means1 + means2) / 2;
            }

            return finalthreshold;
        }

        /// <summary>
        /// 克隆一个副本。
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static Bitmap Clone(Bitmap bitmap)
        {
            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
        }

        private static double AdjustByte(double d)
        {
            if (d > 0)
            {
                return Math.Min(255, d);
            }
            else
            {
                return Math.Max(0, d);
            }
        }
    }
}