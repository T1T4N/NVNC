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

namespace NVNC.Writers
{
    /// <summary>
    /// BigEndianBinaryWriter is a wrapper class used to write .NET integral types in Big-Endian order to a stream.  It inherits from BinaryWriter and adds Little-to-Big-Endian conversion.
    /// </summary>
    public sealed class BigEndianBinaryWriter : BinaryWriter
    {
        public BigEndianBinaryWriter(Stream input)
            : base(input)
        { }

        public BigEndianBinaryWriter(Stream input, System.Text.Encoding encoding)
            : base(input, encoding)
        { }

        public override void Write(ushort value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(short value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(uint value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(int value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(ulong value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(long value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        private void FlipAndWrite(byte[] b)
        {
            // Given an array of bytes, flip and write to underlying stream
            Array.Reverse(b);
            base.Write(b);
        }
    }
}