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
using System.Drawing;
using NVNC.Utils;

namespace NVNC.Encodings
{
    public class CoRRE : RreRectangle
    {
        public CoRRE(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle rectangle)
            : base(rfb, framebuffer, pixels, rectangle)
        {
            this.pixels = pixels;
        }

        public override void WriteData()
        {
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.X));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Y));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Width));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Height));

            rfb.WriteUInt32(Convert.ToUInt32(VncHost.Encoding.CoRreEncoding));
            rfb.WriteUInt32(Convert.ToUInt32(subrects.Length));

            WritePixel32(bgpixel);
            for (int i = 0; i < subrects.Length; i++)
            {
                WritePixel32(subrects[i].pixel);
                rfb.WriteByte(Convert.ToByte(subrects[i].x));
                rfb.WriteByte(Convert.ToByte(subrects[i].y));
                rfb.WriteByte(Convert.ToByte(subrects[i].w));
                rfb.WriteByte(Convert.ToByte(subrects[i].h));
            }
        }
    }
}
