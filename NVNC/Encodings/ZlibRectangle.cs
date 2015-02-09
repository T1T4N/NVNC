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
    public sealed class ZlibRectangle : EncodedRectangle
    {
        private int[] pixels;
        public ZlibRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        public override void Encode()
        {
            if (bytes == null)
                bytes = PixelGrabber.GrabPixels(pixels, new Rectangle(0,0,rectangle.Width, rectangle.Height), framebuffer);
        }
        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUInt32(Convert.ToUInt32(VncHost.Encoding.ZlibEncoding));
            Console.WriteLine("ZLib uncompressed bytes size: " + bytes.Length);
            
            //ZlibRectangle exclusively uses a ZlibWriter to compress the bytes
            rfb.ZlibWriter.Write(bytes);
            rfb.ZlibWriter.Flush();
        }
        private int Adler32(byte[] arr)
        {
            const uint a32mod = 65521;
            uint s1 = 1, s2 = 0;
            foreach (byte b in arr)
            {
                s1 = (s1 + b) % a32mod;
                s2 = (s2 + s1) % a32mod;
            }
            return unchecked(Convert.ToInt32((s2 << 16) + s1));
        }

    }
}
