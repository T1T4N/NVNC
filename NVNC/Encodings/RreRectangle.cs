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
using System.Collections.Generic;
using NVNC.Utils;

namespace NVNC.Encodings
{
    /// <summary>
    /// Implementation of RRE encoding.
    /// </summary>
    public class RreRectangle : EncodedRectangle
    {
        protected int[] pixels;
        public RreRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        protected internal int bgpixel;
        protected internal SubRect[] subrects;

        protected internal class SubRect
        {
            public int pixel;
            public ushort x;
            public ushort y;
            public ushort w;
            public ushort h;
        }

        public override unsafe void Encode()
        {
            int x = 0;//rectangle.X;
            int y = 0;//rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            SubRect subrect;
            List<SubRect> vector = new List<SubRect>();

            int currentPixel;
            int runningX, runningY;
            int firstX = 0;
            int secondX = 0;
            bgpixel = GetBackground(pixels, w, x, y, w, h);

            fixed (int* px = pixels)
            {
                for (int currentY = y; currentY < h; currentY++)
                {
                    int line = currentY * w;
                    for (int currentX = x; currentX < w; currentX++)
                    {
                        if (*(px + (line + currentX)) != bgpixel)
                        {
                            currentPixel = *(px + (line + currentX));
                            int firstY = currentY - 1;
                            bool firstYflag = true;
                            for (runningY = currentY; runningY < h; runningY++)
                            {
                                int segment = runningY * w;
                                if ((*(px + (segment + currentX))) != currentPixel)
                                    break;
                                runningX = currentX;
                                while ((runningX < w) && (*(px + (segment + runningX)) == currentPixel))
                                    runningX++;
                                runningX--;
                                if (runningY == currentY)
                                    secondX = firstX = runningX;
                                if (runningX < secondX)
                                    secondX = runningX;
                                if (firstYflag && (runningX >= firstX))
                                    firstY++;
                                else
                                    firstYflag = false;
                            }
                            int secondY = runningY - 1;

                            int firstW = firstX - currentX + 1;
                            int firstH = firstY - currentY + 1;
                            int secondW = secondX - currentX + 1;
                            int secondH = secondY - currentY + 1;

                            subrect = new SubRect();
                            subrect.pixel = currentPixel;
                            subrect.x = (ushort)currentX;
                            subrect.y = (ushort)currentY;

                            if ((firstW * firstH) > (secondW * secondH))
                            {
                                subrect.w = (ushort)firstW;
                                subrect.h = (ushort)firstH;
                            }
                            else
                            {
                                subrect.w = (ushort)secondW;
                                subrect.h = (ushort)secondH;
                            }
                            vector.Add(subrect);

                            for (runningY = subrect.y; runningY < (subrect.y + subrect.h); runningY++)
                                for (runningX = subrect.x; runningX < (subrect.x + subrect.w); runningX++)
                                    *(px + (runningY * w + runningX)) = bgpixel;
                        }
                    }
                }
            }
            subrects = vector.ToArray();
        }
        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUint32(Convert.ToUInt32(VncHost.Encoding.RreEncoding));
            rfb.WriteUInt32(Convert.ToUInt32(subrects.Length));
            WritePixel32(bgpixel);

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                for (int i = 0; i < subrects.Length; i++)
                {
                    byte[] data = PixelGrabber.GrabBytes(subrects[i].pixel, framebuffer);

                    //This is how BigEndianBinaryWriter writes short values :)
                    byte[] x = Flip(BitConverter.GetBytes(subrects[i].x));
                    byte[] y = Flip(BitConverter.GetBytes(subrects[i].y));
                    byte[] w = Flip(BitConverter.GetBytes(subrects[i].w));
                    byte[] h = Flip(BitConverter.GetBytes(subrects[i].h));

                    ms.Write(data, 0, data.Length);
                    ms.Write(x, 0, x.Length);
                    ms.Write(y, 0, y.Length);
                    ms.Write(w, 0, w.Length);
                    ms.Write(h, 0, h.Length);
                }
                rfb.Write(ms.ToArray());
            }
        }
        private byte[] Flip(byte[] b)
        {
            // Given an array of bytes, flip and write to underlying stream
            Array.Reverse(b);
            return b;
        }
    }
}