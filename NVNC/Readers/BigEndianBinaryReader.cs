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
using System.IO;

namespace NVNC.Readers
{
    /// <summary>
    /// BigEndianBinaryReader is a wrapper class used to read .NET integral types from a Big-Endian stream.  It inherits from BinaryReader and adds Big- to Little-Endian conversion.
    /// </summary>
    public sealed class BigEndianBinaryReader : BinaryReader
    {
        private byte[] buff = new byte[4];

        public BigEndianBinaryReader(Stream input)
            : base(input)
        { }

        public BigEndianBinaryReader(Stream input, System.Text.Encoding encoding)
            : base(input, encoding)
        { }

        // Since this is being used to communicate with an RFB host, only some of the overrides are provided below.

        public override ushort ReadUInt16()
        {
            FillBuff(2);
            return (ushort)(((uint)buff[1]) | ((uint)buff[0]) << 8);
        }

        public override short ReadInt16()
        {
            FillBuff(2);
            return (short)(buff[1] & 0xFF | buff[0] << 8);
        }

        public override uint ReadUInt32()
        {
            FillBuff(4);
            return (uint)(((uint)buff[3]) & 0xFF | ((uint)buff[2]) << 8 | ((uint)buff[1]) << 16 | ((uint)buff[0]) << 24);
        }

        public override int ReadInt32()
        {
            FillBuff(4);
            return Convert.ToInt32(buff[3] | buff[2] << 8 | buff[1] << 16 | buff[0] << 24);
        }

        private void FillBuff(int totalBytes)
        {
            int bytesRead = 0;
            int n = 0;
            do
            {
                n = BaseStream.Read(buff, bytesRead, totalBytes - bytesRead);
                if (n == 0)
                    throw new IOException("Unable to read next byte(s).");

                bytesRead += n;
            } while (bytesRead < totalBytes);
        }
    }
}