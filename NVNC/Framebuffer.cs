// VncSharp - .NET VNC Client Library
// Copyright (C) 2008 David Humphrey
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
namespace NVNC
{
    /// <summary>
    /// Properties of a VNC Framebuffer, and its Pixel Format.
    /// </summary>
    public class Framebuffer
    {
        private string name;

        private int bpp;
        private int depth;
        private bool bigEndian;
        private bool trueColor;
        private int redMax;
        private int greenMax;
        private int blueMax;
        private int redShift;
        private int greenShift;
        private int blueShift;

        private readonly int width;
        private readonly int height;
        public readonly int[] pixels;	 // I'm reusing the same pixel buffer for all update rectangles.
        // Pixel values will always be 32-bits to match GDI representation


        /// <summary>
        /// Creates a new Framebuffer with (width x height) pixels.
        /// </summary>
        /// <param name="width">The width in pixels of the remote desktop.</param>
        /// <param name="height">The height in pixels of the remote desktop.</param>
        public Framebuffer(int width, int height)
        {
            this.width = width;
            this.height = height;

            // Cache the total size of the pixel array and initialize
            int pixelCount = width * height;
            pixels = new int[pixelCount];
        }

        /// <summary>
        /// An indexer to allow access to the internal pixel buffer.
        /// </summary>
        public int this[int index]
        {
            get
            {
                return pixels[index];
            }
            set
            {
                pixels[index] = value;
            }
        }

        /// <summary>
        /// The Width of the Framebuffer, measured in Pixels.
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// The Height of the Framebuffer, measured in Pixels.
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Gets a Rectangle object constructed out of the Width and Height for the Framebuffer.  Used as a convenience in other classes.
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(0, 0, width, height);
            }
        }

        /// <summary>
        /// The number of Bits Per Pixel for the Framebuffer--one of 8, 24, or 32.
        /// </summary>
        public int BitsPerPixel
        {
            get
            {
                return bpp;
            }
            set
            {
                if (value == 32 || value == 16 || value == 8)
                    bpp = value;
                else throw new ArgumentException("Wrong value for BitsPerPixel");
            }
        }

        /// <summary>
        /// The Color Depth of the Framebuffer.
        /// The default value is 24.
        /// </summary>
        public int Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = value;
            }
        }

        /// <summary>
        /// Indicates whether the remote host uses Big- or Little-Endian order when sending multi-byte values.
        /// </summary>
        public bool BigEndian
        {
            get
            {
                return bigEndian;
            }
            set
            {
                bigEndian = value;
            }
        }

        /// <summary>
        /// Indicates whether the remote host supports True Color.
        /// The default value is true.
        /// </summary>
        public bool TrueColor
        {
            get
            {
                return trueColor;
            }
            set
            {
                trueColor = value;
            }
        }

        /// <summary>
        /// The maximum value for Red in a pixel's color value.
        /// The default value is 0xFF.
        /// </summary>
        public int RedMax
        {
            get
            {
                return redMax;
            }
            set
            {
                redMax = value;
            }
        }

        /// <summary>
        /// The maximum value for Green in a pixel's color value.
        /// The default value is 0xFF.
        /// </summary>
        public int GreenMax
        {
            get
            {
                return greenMax;
            }
            set
            {
                greenMax = value;
            }
        }

        /// <summary>
        /// The maximum value for Blue in a pixel's color value.
        /// The default value is 0xFF.
        /// </summary>
        public int BlueMax
        {
            get
            {
                return blueMax;
            }
            set
            {
                blueMax = value;
            }
        }

        /// <summary>
        /// The number of bits to shift pixel values in order to obtain Red values.
        /// The default value is 16.
        /// </summary>
        public int RedShift
        {
            get
            {
                return redShift;
            }
            set
            {
                redShift = value;
            }
        }

        /// <summary>
        /// The number of bits to shift pixel values in order to obtain Green values.
        /// The default value is 8.
        /// </summary>
        public int GreenShift
        {
            get
            {
                return greenShift;
            }
            set
            {
                greenShift = value;
            }
        }

        /// <summary>
        /// The number of bits to shift pixel values in order to obtain Blue values.
        /// The default value is 0.
        /// </summary>
        public int BlueShift
        {
            get
            {
                return blueShift;
            }
            set
            {
                blueShift = value;
            }
        }

        /// <summary>
        /// The name of the remote destkop, if any.  Must be non-null.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if a null string is used when setting DesktopName.</exception>
        public string DesktopName
        {
            get
            {
                return name;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("DesktopName");
                name = value;
            }
        }

        /// <summary>
        /// When communicating with the VNC Server, bytes are used to represent many of the values above.  However, internally it is easier to use Integers.  This method provides a translation between the two worlds.
        /// </summary>
        /// <returns>A byte array of 16 bytes containing the properties of the framebuffer in a format ready for transmission to the VNC server.</returns>
        public byte[] ToPixelFormat()
        {
            byte[] b = new byte[16];

            b[0] = Convert.ToByte(bpp);
            b[1] = Convert.ToByte(depth);
            b[2] = Convert.ToByte(bigEndian ? 1 : 0);
            b[3] = Convert.ToByte(trueColor ? 1 : 0);
            b[4] = Convert.ToByte((redMax >> 8) & 0xff);
            b[5] = Convert.ToByte(redMax & 0xff);
            b[6] = Convert.ToByte((greenMax >> 8) & 0xff);
            b[7] = Convert.ToByte(greenMax & 0xff);
            b[8] = Convert.ToByte((blueMax >> 8) & 0xff);
            b[9] = Convert.ToByte(blueMax & 0xff);
            b[10] = Convert.ToByte(redShift);
            b[11] = Convert.ToByte(greenShift);
            b[12] = Convert.ToByte(blueShift);
            // plus 3 bytes padding = 16 bytes

            return b;
        }

        /// <summary>
        /// Given the dimensions and 16-byte PIXEL_FORMAT record from the VNC Host, deserialize this into a Framebuffer object.
        /// </summary>
        /// <param name="b">The 16-byte PIXEL_FORMAT record.</param>
        /// <param name="width">The width in pixels of the remote desktop.</param>
        /// <param name="height">The height in pixles of the remote desktop.</param>
        /// <returns>Returns a Framebuffer object matching the specification of b[].</returns>
        public static Framebuffer FromPixelFormat(byte[] b, int width, int height)
        {
            if (b.Length != 16)
                throw new ArgumentException("Length of b must be 16 bytes.");

            Framebuffer buffer = new Framebuffer(width, height);

            buffer.BitsPerPixel = Convert.ToInt32(b[0]);
            buffer.Depth = Convert.ToInt32(b[1]);
            buffer.BigEndian = (b[2] != 0);
            buffer.TrueColor = (b[3] != 0);
            buffer.RedMax = Convert.ToInt32(b[5] | b[4] << 8);
            buffer.GreenMax = Convert.ToInt32(b[7] | b[6] << 8);
            buffer.BlueMax = Convert.ToInt32(b[9] | b[8] << 8);
            buffer.RedShift = Convert.ToInt32(b[10]);
            buffer.GreenShift = Convert.ToInt32(b[11]);
            buffer.BlueShift = Convert.ToInt32(b[12]);
            // Last 3 bytes are padding, ignore									

            return buffer;
        }
        /// <summary>
        /// Prints the Framebuffer data to the console.
        /// </summary>
        public void Print()
        {
            Console.WriteLine("BitsPerPixel: " + BitsPerPixel);
            Console.WriteLine("Depth: " + Depth);
            Console.WriteLine("BigEndian: " + BigEndian);
            Console.WriteLine("TrueColor: " + TrueColor);
            Console.WriteLine("RedMax: " + RedMax);
            Console.WriteLine("GreenMax: " + GreenMax);
            Console.WriteLine("BlueMax: " + BlueMax);
            Console.WriteLine("RedShift: " + RedShift);
            Console.WriteLine("GreenShift: " + GreenShift);
            Console.WriteLine("BlueShift: " + BlueShift);
        }

        public int TranslatePixel(int pixel)
        {
            if (redFix != -1)
            {
                return
                    ((pixel & redMask) >> redFix << redShift) |
                    ((pixel & greenMask) >> greenFix << greenShift) |
                    ((pixel & blueMask) >> blueFix << blueShift);
            }
            return pixel;
        }

        private int redMask = -1, greenMask = -1, blueMask = -1;
        private int redFix = -1, greenFix = -1, blueFix = -1;

        private static int FixColorModel(int max1, int max2, int mask)
        {
            int fix = 0;
            for (; fix < 8; fix++)
            {
                if (max1 == max2)
                    break;

                max1 >>= 1;
            }

            while ((mask & 1) == 0)
            {
                fix++;
                mask >>= 1;
            }

            return fix;
        }
    }
}
