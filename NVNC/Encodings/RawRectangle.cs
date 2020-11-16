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
    /// <summary>
    /// Implementation of Raw encoding. 
    /// </summary>
    public sealed class RawRectangle : EncodedRectangle
    {
        private int[] pixels;
        public RawRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }
        
        public override void Encode()
        {
            /*
            bytes = PixelGrabber.GrabPixels(bmp, PixelFormat.Format32bppArgb);
            for (int i = 0; i < pixels.Length; i++)
                framebuffer[i] = pixels[i];
             */
            if(bytes == null)
                bytes = PixelGrabber.GrabPixels(pixels, new Rectangle(0,0,rectangle.Width, rectangle.Height), framebuffer);

        }
        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUInt32(Convert.ToUInt32(VncHost.Encoding.RawEncoding));
            rfb.Write(bytes);

            /*  Very slow, not practically usable
            for (int i = 0; i < framebuffer.pixels.Length; i++)
                pwriter.WritePixel(framebuffer[i]);
            */
        }
    }
}
