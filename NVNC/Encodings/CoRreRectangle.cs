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
    /// Implementation of CoRRE encoding.
    /// </summary>
    public sealed class CoRreRectangle : EncodedRectangle
    {
        private int[] pixels;
        public CoRreRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        public CoRRE[] rects;

        public override void Encode()
        {
            int x = 0;//rectangle.X;
            int y = 0;//rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            CoRRE rect;
            List<CoRRE> vector = new List<CoRRE>();


            if ((w <= 0xFF) && (h <= 0xFF))
            {
                rect = new CoRRE(rfb, framebuffer, pixels, rectangle);
                rect.Encode();
                vector.Add(rect);
            }
            else
            {
                int currentW, currentH;
                for (int currentY = 0; currentY < h; currentY += 0xFF)
                {
                    for (int currentX = 0; currentX < w; currentX += 0xFF)
                    {
                        try
                        {
                            currentW = w - currentX;
                            currentH = h - currentY;

                            if (currentW > 0xFF)
                                currentW = 0xFF;
                            if (currentH > 0xFF)
                                currentH = 0xFF;
                            Rectangle2 rc = new Rectangle2(x + currentX, y + currentY, currentW, currentH);
                            rect = new CoRRE(rfb, framebuffer, pixels, rc);

                            //problem ... WHY ?
                            rect.Encode();
                            vector.Add(rect);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Console.ReadLine();
                        }
                    }
                }
            }

            rects = vector.ToArray();
            //count = rects.length;
        }

        public override void WriteData()
        {
            Console.WriteLine(rects.Length);
            foreach (CoRRE r in rects)
                r.WriteData();
        }
    }
}