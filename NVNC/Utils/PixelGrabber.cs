// NVNC - .NET VNC Server Library
// Copyright (C) 2014 T!T@N
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NVNC.Utils
{
    /// <summary>
    /// A clone of Java's PixelGrabber class.
    /// </summary>
    public static unsafe class PixelGrabber
    {
        /// <summary>
        /// Creates a screen capture in a bitmap format. The currently used method in EncodedRectangleFactory.
        /// </summary>
        /// <param name="r">The rectangle from the screen that we should take a screenshot from.</param>
        /// <returns>A bitmap containing the image data of our screenshot. The return value is null only if a problem occured.</returns>
        public static Bitmap CreateScreenCapture(Rectangle r)
        {
            
            try
            {
                Stopwatch t = Stopwatch.StartNew();
                int width = r.Width;
                int height = r.Height;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);
                g.CopyFromScreen(r.X, r.Y, 0, 0, new Size(width, height));

                t.Stop();
                Trace.WriteLine("Screen capture done in: " + t.ElapsedMilliseconds + "ms");
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                System.Threading.Thread.Sleep(200);
                try
                {
                    int width = r.Width;
                    int height = r.Height;
                    Bitmap bitmap = new Bitmap(width, height);
                    Graphics g = Graphics.FromImage(bitmap);
                    g.CopyFromScreen(r.X, r.Y, 0, 0, new Size(width, height));
                }
                catch (Exception)
                {
                    return null;
                }
                return null;
            }
        }
        /*
        public static byte[] BitmapToPng(Bitmap bmp)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                bmp.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
        public static Image BitmapToPng(Image bmp)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                bmp.Save(stream, ImageFormat.Png);
                return new Bitmap(stream);
            }
        }
        */
        /// <summary>
        /// An alternate method of creating a screenshot.
        /// </summary>
        /// <param name="x">The X coordinate of the Rectangle of our screenshot</param>
        /// <param name="y">The Y coordinate of the Rectangle of our screenshot</param>
        /// <param name="w">The width of the Rectangle of our screenshot</param>
        /// <param name="h">The height of the Rectangle of our screenshot</param>
        /// <returns></returns>
        public static Bitmap CreateScreenCapture(int x, int y, int w, int h)
        {
            Bitmap bitmap = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(x, y, 0, 0, new Size(w, h));
            return bitmap;
        }
        /// <summary>
        /// Converts a bitmap to a byte array of it's pixel data.
        /// </summary>
        /// <param name="bmp">The bitmap that should be converted to a byte array.</param>
        /// <param name="pf">The pixel format that should be used to do the conversion.</param>
        /// <returns>A byte array containing the pixel data of the bitmap.</returns>
        public static byte[] GrabPixels(Bitmap bmp, PixelFormat pf)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(new Point(0,0), bmp.Size),
                ImageLockMode.ReadOnly,
                pf);
            // number of bytes in the bitmap
            int byteCount = bData.Stride * bmp.Height;
            byte[] bmpBytes = new byte[byteCount];

            // Copy the locked bytes from memory
            Marshal.Copy(bData.Scan0, bmpBytes, 0, byteCount);

            // don't forget to unlock the bitmap!!
            bmp.UnlockBits(bData);

            return bmpBytes;
        }

        /// <summary>
        /// Converts an array of pixels represented as integers into a byte array of pixel data.
        /// </summary>
        /// <param name="pixels">The pixel array represented as integers.</param>
        /// <param name="rectangle">A sub-rectangle of the pixels which we should extract</param>
        /// <param name="pf">The pixel format that should be used.</param>
        /// <returns></returns>
        public static byte[] GrabPixels(int[] pixels, Rectangle rectangle, PixelFormat pf)
        {
            // Encode as bytes
            int x = rectangle.X;
            int y = rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            byte[] bytes = null;

            int b = 0;
            int i = 0;
            int s = 0;
            int pixel;
            int size = w*h;
            int scanline = w;
            int jump = scanline - w;
            int offsetX = x;
            int offsetY = y;

            int p = (y - offsetY) * w + x - offsetX;

            switch (pf)
            {
                case (PixelFormat.Format32bppArgb | PixelFormat.Format32bppRgb | PixelFormat.Format32bppPArgb):
                    bytes = new byte[size << 2];
                    for (; i < size; i++, s++, p++)
                    {
                        if (s == w)
                        {
                            s = 0;
                            p += jump;
                        }
                        //pixel = framebuffer.TranslatePixel(pixels[p]);
                        pixel = pixels[p];
                        bytes[b++] = (byte)(pixel & 0xFF); //B
                        bytes[b++] = (byte)((pixel >> 8) & 0xFF); //G
                        bytes[b++] = (byte)((pixel >> 16) & 0xFF); //R
                        bytes[b++] = (byte)((pixel >> 24) & 0xFF); //A
                    }
                    break;
                case (PixelFormat.Format16bppRgb565 | PixelFormat.Format16bppRgb555 | PixelFormat.Format16bppGrayScale | PixelFormat.Format16bppArgb1555):
                    bytes = new byte[size << 1];
                    for (; i < size; i++, s++, p++)
                    {
                        if (s == w)
                        {
                            s = 0;
                            p += jump;
                        }
                        pixel = pixels[p];
                        bytes[b++] = (byte)(pixel & 0xFF); //B
                        bytes[b++] = (byte)((pixel >> 8) & 0xFF); //G
                    }
                    break;
                case (PixelFormat.Format8bppIndexed):
                    bytes = new byte[size];
                    for (; i < size; i++, s++, p++)
                    {
                        if (s == w)
                        {
                            s = 0;
                            p += jump;
                        }
                        bytes[i] = (byte)pixels[p]; //B
                    }
                    break;
            }
            return bytes;

        }
        /// <summary>
        /// Converts an array of pixels represented as integers into a byte array of pixel data.
        /// </summary>
        /// <param name="pixels">The pixel array represented as integers.</param>
        /// <param name="rectangle">A sub-rectangle of the pixels which we should extract</param>
        /// <param name="fb">The Framebuffer that should be used.</param>
        /// <returns></returns>
        public static byte[] GrabPixels(int[] pixels, Rectangle rectangle, Framebuffer fb)
        {
            // Encode as bytes
            int x = rectangle.X;
            int y = rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            byte[] bytes = null;

            int b = 0;
            int i = 0;
            int s = 0;
            int pixel;
            int size = w*h;
            int scanline = w;
            int offsetX = x;
            int offsetY = y;
            int jump = scanline - w;
            int p = (y - offsetY) * w + x - offsetX;

            switch (fb.BitsPerPixel)
            {
                case 32:
                    bytes = new byte[size << 2];
                    for (; i < size; i++, s++, p++)
                    {
                        if (s == w)
                        {
                            s = 0;
                            p += jump;
                        }
                        int tmp = pixels[p];
                        pixel = fb.TranslatePixel(tmp);
                        //pixel = pixels[p];

                        bytes[b++] = (byte)(pixel & 0xFF); //B
                        bytes[b++] = (byte)((pixel >> 8) & 0xFF); //G
                        bytes[b++] = (byte)((pixel >> 16) & 0xFF); //R
                        bytes[b++] = (byte)((pixel >> 24) & 0xFF); //A
                    }
                    break;
                case 24:
                    bytes = new byte[size << 2];
                    for (; i < size; i++, s++, p++)
                    {
                        if (s == w)
                        {
                            s = 0;
                            p += jump;
                        }
                        pixel = fb.TranslatePixel(pixels[p]);
                        bytes[b++] = (byte)(pixel & 0xFF); //B
                        bytes[b++] = (byte)((pixel >> 8) & 0xFF); //G
                        bytes[b++] = (byte)((pixel >> 16) & 0xFF); //R
                    }
                    break;
                case 16:
                    bytes = new byte[size << 1];
                    for (; i < size; i++, s++, p++)
                    {
                        if (s == w)
                        {
                            s = 0;
                            p += jump;
                        }
                        pixel = fb.TranslatePixel(pixels[p]);
                        bytes[b++] = (byte)(pixel & 0xFF); //B
                        bytes[b++] = (byte)((pixel >> 8) & 0xFF); //G
                    }
                    break;
                case 8:
                    bytes = new byte[size];
                    for (; i < size; i++, s++, p++)
                    {
                        if (s == w)
                        {
                            s = 0;
                            p += jump;
                        }
                        bytes[i] = (byte)fb.TranslatePixel(pixels[p]); //B
                    }
                    break;
            }
            return bytes;

        }
        
        /// <summary>
        /// Converts an integer pixel into a byte array
        /// </summary>
        /// <param name="pixel">The pixel represented as an integer value</param>
        /// <param name="fb">The framebuffer that should be used</param>
        /// <returns>A byte array containing the pixels BGRA data.</returns>
        public static byte[] GrabBytes(int pixel, Framebuffer fb)
        {
            int b = 0;
            byte[] bytes = null;
            switch (fb.BitsPerPixel)
            {
                case 32:
                    bytes = new byte[4];
                    bytes[b++] = (byte)(pixel & 0xFF);          //B
                    bytes[b++] = (byte)((pixel >> 8) & 0xFF);   //G
                    bytes[b++] = (byte)((pixel >> 16) & 0xFF);  //R
                    bytes[b++] = (byte)((pixel >> 24) & 0xFF);  //A
                    break;
                case 16:
                    bytes = new byte[2];
                    bytes[b++] = (byte)(pixel & 0xFF);          //B
                    bytes[b++] = (byte)((pixel >> 8) & 0xFF);   //G
                    break;
                case 8:
                    bytes = new byte[1];
                    bytes[b++] = (byte)(pixel & 0xFF);          //B
                    break;
            }
            return bytes;
        }
        /// <summary>
        /// Extracts the pixels consisted in a bitmap into an integer array
        /// </summary>
        /// <param name="img">The bitmap whose pixels should be converted to an integer array</param>
        /// <returns>An integer array of the bitmap's pixel data</returns>
        public static int[] GrabPixels(Bitmap img)
        {
            int[] array = new int[img.Width * img.Height];
            BitmapData bmp = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat);
            unsafe
            {
                int PixelSize = 4;

                for (int y = 0; y < bmp.Height; y++)
                {
                    byte* row = (byte*)bmp.Scan0 + (y * bmp.Stride);
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int a = Convert.ToInt32(row[(x * PixelSize) + 3]); //A
                        int r = Convert.ToInt32(row[(x * PixelSize) + 2]); //R
                        int g = Convert.ToInt32(row[(x * PixelSize) + 1]); //G
                        int b = Convert.ToInt32(row[(x * PixelSize)]); //B

                        Color c = Color.FromArgb(a, r, g, b);
                        int val = c.ToArgb();
                        array[y * bmp.Width + x] = val;
                    }
                }

            }
            img.UnlockBits(bmp);
            return array;
        }
        /// <summary>
        /// Extracts the pixels consisted in a bitmap from the specified rectangle into an integer array
        /// </summary>
        /// <param name="img">The bitmap whose pixels should be converted to an integer array</param>
        /// <param name="x">The X coordinate of the Rectangle</param>
        /// <param name="y">The Y coordinate of the Rectangle</param>
        /// <param name="w">The width of the Rectangle</param>
        /// <param name="h">The height of the Rectangle</param>
        /// <param name="pf">The pixel format that should be used</param>
        /// <returns></returns>
        public static int[]  GrabPixels(Bitmap img, int x, int y, int w, int h, PixelFormat pf)
        {
            int[] array = new int[w * h];
            BitmapData bmp = img.LockBits(new Rectangle(x, y, w, h), ImageLockMode.ReadOnly, pf);
            unsafe
            {
                int PixelSize = 4;

                for (int j = 0; j < h; j++)
                {
                    byte* row = (byte*)bmp.Scan0 + (j * bmp.Stride);
                    for (int i = 0; i < w; i++)
                    {
                        int a = Convert.ToInt32(row[(i * PixelSize) + 3]);
                        int r = Convert.ToInt32(row[(i * PixelSize) + 2]);
                        int g = Convert.ToInt32(row[(i * PixelSize) + 1]);
                        int b = Convert.ToInt32(row[(i * PixelSize)]);

                        Color c = Color.FromArgb(a, r, g, b);
                        int val = c.ToArgb();
                        array[j * w + i] = val;
                    }
                }

            }
            img.UnlockBits(bmp);
            return array;
        }
        public static int[] CopyPixels(int[] pixels, int scanline, int x, int y, int w, int h)
        {
            int size = w * h;
            int[] ourPixels = new int[size];
            int jump = scanline - w;
            int s = 0;
            int p = y * scanline + x;
            Trace.WriteLine("Data offset: " + p);
            for (int i = 0; i < size; i++, s++, p++)
            {
                if (s == w)
                {
                    s = 0;
                    p += jump;
                }
                ourPixels[i] = pixels[p];
            }
            Trace.WriteLine("Data end: " + --p);
            return ourPixels;
        }
        /// <summary>
        /// Converts a byte array of pixel data into a bitmap with specified width and height
        /// </summary>
        /// <param name="data">The byte array of pixel data</param>
        /// <param name="w">The width of the bitmap</param>
        /// <param name="h">The height of the bitmap</param>
        /// <param name="pf">The pixel format that should be used</param>
        /// <returns>A bitmap containing the pixel data</returns>
        public static Bitmap GrabImage(byte[] data, int w, int h, PixelFormat pf)
        {
            int m = 0;
            switch (pf)
            {
                case (PixelFormat.Format32bppRgb | PixelFormat.Format32bppPArgb | PixelFormat.Format32bppArgb):
                    m = 4;
                    break;
                case PixelFormat.Format24bppRgb:
                    m = 3;
                    break;
                case (PixelFormat.Format16bppRgb565 | PixelFormat.Format16bppRgb555 | PixelFormat.Format16bppArgb1555 | PixelFormat.Format16bppGrayScale):
                    m = 2;
                    break;
            }
            fixed (byte* ptr = data)
            {
                Bitmap image = new Bitmap(w, h, m * w, pf, new IntPtr(ptr));
                return image;
            }
        }
        /// <summary>
        /// Converts an integer array of pixel data in to a bitmap with the specified width and height.
        /// </summary>
        /// <param name="w">The width of the bitmap.</param>
        /// <param name="h">The height of the bitmap.</param>
        /// <param name="data">The integer array of pixel data</param>
        /// <returns>A bitmap containing the pixel data</returns>
        public static Bitmap GrabImage(int w, int h, int[] data)
        {
            /*Color[,] r = new Color[w, h];
            for (int i = 0; i < data.Length; i++)
                r[i % w, i / w] = Color.FromArgb(data[i]); // Copy over into a Color structure
            */

            Bitmap ret = new Bitmap(w, h);
            BitmapData bmd = ret.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, ret.PixelFormat);
            int PixelSize = 4;
            for (int y = 0; y < bmd.Height; y++)
            {
                byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                for (int x = 0; x < bmd.Width; x++)
                {
                    Color c = Color.FromArgb(data[(y * bmd.Width) + x]);
                    row[(x * PixelSize) + 3] = c.A;
                    row[(x * PixelSize) + 2] = c.R;
                    row[(x * PixelSize) + 1] = c.G;
                    row[(x * PixelSize)] = c.B;
                }
            }
            ret.UnlockBits(bmd);
            return ret;
        }
        /// <summary>
        /// Gets the 32-bit ARGB value of the pixel.
        /// </summary>
        /// <param name="bmp">The bitmap containing the pixel.</param>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <returns></returns>
        public static int GetRGB(Bitmap bmp, int x, int y)
        {
            return bmp.GetPixel(x, y).ToArgb();
        }

        /// <summary>
        /// Extracts a subimage from the given bitmap.
        /// </summary>
        /// <param name="bmp">The bitmap from which we should extract a subimage.</param>
        /// <param name="rect">The position and size of the subimage represented as a Rectangle.</param>
        /// <returns>A bitmap subimage with the specified location and size.</returns>
        public static Bitmap GetSubImage(Bitmap bmp, Rectangle rect)
        {
            try
            {
                int[] data = GrabPixels(bmp, rect.X, rect.Y, rect.Width, rect.Height, bmp.PixelFormat);
                return GrabImage(rect.Width, rect.Height, data);
            }
            catch (Exception)
            {
                return bmp;
            }
        }
        /// <summary>
        /// An alternative function for extracting a subimage from a given bitmap
        /// </summary>
        /// <param name="bmp">The bitmap from which we should extract a subimage.</param>
        /// <param name="rect">The position and size of the subimage represented as a Rectangle.</param>
        /// <returns>A bitmap subimage with the specified location and size.</returns>
        public static Bitmap GetSubImage2(Bitmap bmp, Rectangle rect)
        {
            Bitmap cropped = bmp.Clone(rect, bmp.PixelFormat);
            return cropped;
        }

        public static Rectangle AlignRectangle(Rectangle paramRectangle, int dw, int dh)
        {
            int i = paramRectangle.X % 16;
            if (i != 0)
                paramRectangle.X -= i;
            i = paramRectangle.Y % 16;
            if (i != 0)
                paramRectangle.Y -= i;
            i = paramRectangle.Width % 16;
            if (i != 0)
            {
                paramRectangle.Width = (paramRectangle.Width - i + 16);
                if (paramRectangle.X + paramRectangle.Width > dw)
                    paramRectangle.Width = (dw - paramRectangle.X);
            }
            i = paramRectangle.Height % 16;
            if (i != 0)
            {
                paramRectangle.Height = (paramRectangle.Height - i + 16);
                if (paramRectangle.Y + paramRectangle.Height > dh)
                    paramRectangle.Height = (dh - paramRectangle.Y);
            }
            return paramRectangle;
        }
        /// <summary>
        /// Checks if two bitmaps have different pixels in an area.
        /// </summary>
        /// <param name="paramBitmap1">First bitmap to be compared.</param>
        /// <param name="paramBitmap2">Second bitmap to be compared.</param>
        /// <param name="paramRectangle">A rectangle area where we should check for changes</param>
        /// <returns>True if there is a change in at least one pixel, otherwise false</returns>
        public static bool IsChangeArea(Bitmap paramBitmap1, Bitmap paramBitmap2, Rectangle paramRectangle)
        {
            int[] data1 = GrabPixels(paramBitmap1, paramRectangle.X, paramRectangle.Y, paramRectangle.Width, paramRectangle.Height, paramBitmap1.PixelFormat);
            int[] data2 = GrabPixels(paramBitmap2, paramRectangle.X, paramRectangle.Y, paramRectangle.Width, paramRectangle.Height, paramBitmap2.PixelFormat);

            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Gets the rectangle area where two bitmaps have different pixels.
        /// </summary>
        /// <param name="bmp1">First bitmap to be compared.</param>
        /// <param name="bmp2">Second bitmap to be compared.</param>
        /// <param name="rect">A rectangle area where we should check for different pixels</param>
        /// <returns>A rectangle where the different pixels are located.</returns>
        public static Rectangle GetChangeArea(Bitmap bmp1, Bitmap bmp2, Rectangle rect)
        {
            bool first_x = false;
            int minx1 = -1, maxx2 = -1, miny1 = -1, maxy2 = -1;
            int w = bmp1.Width;
            int h = bmp1.Height;
            int[] pixels1 = GrabPixels(bmp1);
            int[] pixels2 = GrabPixels(bmp2);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (pixels2[i + j * w] != pixels1[i + j * w])
                    {
                        if (!first_x) 
                            minx1 = i; miny1 = j; maxx2 = i; maxy2 = j; first_x = true; 

                        if (minx1 > i) minx1 = i;
                        if (miny1 > j) miny1 = j;
                        if (maxx2 < i) maxx2 = i;
                        if (maxy2 < j) maxy2 = j;
                        // System.out.println(i +"x"+ j);
                    }
                }
            }
            //if (minx1 == maxx2 && maxy2 == miny1)
            //Console.WriteLine("Single pixel modified");
            //else
            //{
            //Console.WriteLine("Modified part (rectangle): " + minx1 + "," + miny1 + "<->" + maxx2 + "," + maxy2);
            return new Rectangle(minx1, miny1, maxx2, maxy2);
            //}
            //return Rectangle.Empty;
        }
    }
}
