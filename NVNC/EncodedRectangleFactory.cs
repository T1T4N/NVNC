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
using NVNC.Encodings;
using NVNC.Utils;
using NVNC.Utils.ScreenTree;

namespace NVNC
{
    /// <summary>
    /// Factory class used to create derived EncodedRectangle objects at runtime for sending to the VNC Client.
    /// </summary>
    public class EncodedRectangleFactory
    {
        VncHost rfb;
        Framebuffer framebuffer;

        /// <summary>
        /// Creates an instance of the EncodedRectangleFactory using the connected RfbProtocol object and associated Framebuffer object.
        /// </summary>
        /// <param name="rfb">An RfbProtocol object that will be passed to any created EncodedRectangle objects.  Must be non-null, already initialized, and connected.</param>
        /// <param name="framebuffer">A Framebuffer object which will be used by any created EncodedRectangle objects in order to decode and draw rectangles locally.</param>
        public EncodedRectangleFactory(VncHost rfb, Framebuffer framebuffer)
        {
            System.Diagnostics.Debug.Assert(rfb != null, "RfbProtocol object must be non-null");
            System.Diagnostics.Debug.Assert(framebuffer != null, "Framebuffer object must be non-null");

            this.rfb = rfb;
            this.framebuffer = framebuffer;
        }

        
        public EncodedRectangle Build(QuadNode node, VncHost.Encoding encoding)
        {
            int[] pixels = node.NodeData;
            EncodedRectangle e;
            switch (encoding)
            {
                case VncHost.Encoding.RawEncoding:
                    e = new RawRectangle(rfb, framebuffer, pixels, node.Bounds);
                    break;
                case VncHost.Encoding.CopyRectEncoding:
                    e = new CopyRectRectangle(rfb, framebuffer, node.Bounds);
                    break;
                case VncHost.Encoding.RreEncoding:
                    e = new RreRectangle(rfb, framebuffer, pixels, node.Bounds);
                    break;
                case VncHost.Encoding.CoRreEncoding:
                    e = new CoRreRectangle(rfb, framebuffer, pixels, node.Bounds);
                    break;
                case VncHost.Encoding.HextileEncoding:
                    e = new HextileRectangle(rfb, framebuffer, pixels, node.Bounds);
                    break;
                case VncHost.Encoding.ZrleEncoding:
                    e = new ZrleRectangle(rfb, framebuffer, pixels, node.Bounds);
                    break;
                case VncHost.Encoding.ZlibEncoding:
                    e = new ZlibRectangle(rfb, framebuffer, pixels, node.Bounds);
                    break;
                default:
                    // Sanity check
                    throw new Exception("Unsupported Encoding Format received: " + encoding + ".");
            }
            return e;
        }
        
        /// <summary>
        /// Creates an object type derived from EncodedRectangle, based on the value of encoding.
        /// </summary>
        /// <param name="rectangle">A node object from the Screen Handler defining the bounds of the rectangle and the pixel data. IT SHOULD BE CONSIDERED LOCALLY AGAINST pixels param, not globally against the screen size</param>
        /// <param name="encoding">An Integer indicating the encoding type to be used for this rectangle.  Used to determine the type of EncodedRectangle to create.</param>
        /// <returns></returns>
        public EncodedRectangle Build(Rectangle rectangle, VncHost.Encoding encoding)
        {
            Bitmap bmp = PixelGrabber.CreateScreenCapture(rectangle);
            int[] pixels = PixelGrabber.GrabPixels(bmp, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, bmp.PixelFormat);
            return Build(rectangle, pixels, encoding);
        }
        public EncodedRectangle Build(Rectangle rectangle, int[] pixels, VncHost.Encoding encoding)
        {
            EncodedRectangle e;
            switch (encoding)
            {
                case VncHost.Encoding.RawEncoding:
                    e = new RawRectangle(rfb, framebuffer, pixels, rectangle);
                    break;
                case VncHost.Encoding.CopyRectEncoding:
                    e = new CopyRectRectangle(rfb, framebuffer, rectangle);
                    break;
                case VncHost.Encoding.RreEncoding:
                    e = new RreRectangle(rfb, framebuffer, pixels, rectangle);
                    break;
                case VncHost.Encoding.CoRreEncoding:
                    e = new CoRreRectangle(rfb, framebuffer, pixels, rectangle);
                    break;
                case VncHost.Encoding.HextileEncoding:
                    e = new HextileRectangle(rfb, framebuffer, pixels, rectangle);
                    break;
                case VncHost.Encoding.ZrleEncoding:
                    e = new ZrleRectangle(rfb, framebuffer, pixels, rectangle);
                    break;
                case VncHost.Encoding.ZlibEncoding:
                    e = new ZlibRectangle(rfb, framebuffer, pixels, rectangle);
                    break;
                default:
                    // Sanity check
                    throw new Exception("Unsupported Encoding Format received: " + encoding + ".");
            }
            return e;
        }

    }
}
