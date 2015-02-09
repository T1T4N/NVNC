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
using NVNC.Utils;

namespace NVNC.Encodings
{
    /// <summary>
    /// Implementation of ZRLE encoding, as well as drawing support. See RFB Protocol document v. 3.8 section 6.6.5.
    /// </summary>
    public sealed class ZrleRectangle : EncodedRectangle
    {
        private const int TILE_WIDTH = 64;
        private const int TILE_HEIGHT = 64;
        private int[] pixels;
        public ZrleRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        public override void Encode()
        {
            int x = 0;//rectangle.X;
            int y = 0;//rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            Trace.WriteLine("Landed at ZRLE start!");

            //int rawDataSize = w * h * (framebuffer.BitsPerPixel / 8);
            //byte[] data = new byte[rawDataSize];
            
            //Bitmap bmp = PixelGrabber.GrabImage(rectangle.Width, rectangle.Height, pixels);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                for (int currentY = y; currentY < y + h; currentY += TILE_HEIGHT)
                {
                    int tileH = TILE_HEIGHT;
                    tileH = Math.Min(tileH, y + h - currentY);
                    for (int currentX = x; currentX < x + w; currentX += TILE_WIDTH)
                    {
                        int tileW = TILE_WIDTH;
                        tileW = Math.Min(tileW, x + w - currentX);

                        byte subencoding = (rectangle.IsSolidColor) ? (byte)1 : (byte)0;
                        ms.WriteByte(subencoding);

                        if (subencoding == 0)
                        {
                            int[] pixelz = PixelGrabber.CopyPixels(pixels, w, currentX, currentY, tileW, tileH);
                            for (int i = 0; i < pixelz.Length; ++i)
                            {
                                int b = 0;

                                //The CPixel structure (Compressed Pixel) has 3 bytes, opposed to the normal pixel which has 4.
                                int pixel = pixelz[i];
                                byte[] pbytes = new byte[3];

                                pbytes[b++] = (byte) (pixel & 0xFF);
                                pbytes[b++] = (byte) ((pixel >> 8) & 0xFF);
                                pbytes[b++] = (byte) ((pixel >> 16) & 0xFF);
                                //bytes[b++] = (byte)((pixel >> 24) & 0xFF);

                                ms.Write(pbytes, 0, pbytes.Length);
                            }
                        }
                        else
                        {
                            int b = 0;
                            int pixel = rectangle.SolidColor;
                            byte[] pbytes = new byte[3];

                            pbytes[b++] = (byte)(pixel & 0xFF);
                            pbytes[b++] = (byte)((pixel >> 8) & 0xFF);
                            pbytes[b++] = (byte)((pixel >> 16) & 0xFF);
                            //bytes[b++] = (byte)((pixel >> 24) & 0xFF);

                            ms.Write(pbytes, 0, pbytes.Length);
                        }
                    }
                }
                byte[] uncompressed = ms.ToArray();
                bytes = uncompressed;
            }
        }

        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUint32(Convert.ToUInt32(VncHost.Encoding.ZrleEncoding));

            //ZrleRectangle exclusively uses a ZlibWriter to compress the bytes
            rfb.ZlibWriter.Write(bytes);
            rfb.ZlibWriter.Flush();
        }
    }
}