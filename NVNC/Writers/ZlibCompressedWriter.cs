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
using System.IO;
using ComponentAce.Compression.Libs.zlib;

namespace NVNC.Writers
{
    /// <summary>
    /// A BinaryWriter that uses the Zlib algorithm to write compressed data to a Stream.
    /// I have overrided only the necessary methods used by the ZRLE and Zlib encoding.
    /// </summary>
    public sealed class ZlibCompressedWriter : BinaryWriter
    {
        /// <summary>
        /// A temporary MemoryStream to hold the compressed data.
        /// </summary>
        private MemoryStream zMemoryStream;
        private ZOutputStream zCompressStream;

        /// <summary>
        /// CompressedWriter is used to write the compressed bytes from zMemoryStream to uncompressedStream.
        /// </summary>
        private BinaryWriter compressedWriter;

        /// <summary>
        /// BigWriter is used to write the number of bytes in a BigEndian format.
        /// </summary>
        private BigEndianBinaryWriter bigWriter;

        public int Level { get; private set; }

        /// <summary>
        /// Writes compressed data to the given stream.
        /// </summary>
        /// <param name="uncompressedStream">A stream where the compressed data should be written.</param>
        /// <param name="level">The Zlib compression level that should be used. Default is Z_BEST_COMPRESSION = 9.</param>
        public ZlibCompressedWriter(Stream uncompressedStream, int level = 9)
            : base(uncompressedStream)
        {
            /* Since we need to write the number of compressed bytes that we are going to send first,
             * We cannot directly write to the uncompressedStream.
             * We first write the compressed data to zMemoryStream, and after that we write the data from it to the uncompressedStream
             * using CompressedWriter.
             */

            zMemoryStream = new MemoryStream();
            zCompressStream = new ZOutputStream(zMemoryStream, level)
            {
                //The VNC Protocol uses Z_SYNC_FLUSH as a Flush Mode
                FlushMode = zlibConst.Z_SYNC_FLUSH
            };
            Level = level;
            compressedWriter = new BinaryWriter(uncompressedStream);
            bigWriter = new BigEndianBinaryWriter(uncompressedStream);
        }
        public override void Write(byte[] buffer, int index, int count)
        {
            //Seek to the beginning of the MemoryStream before writing
            //So stream capacity won't increase, and eventually throw an OutOfMemory exception
            zMemoryStream.Seek(0, SeekOrigin.Begin);

            zCompressStream.Write(buffer, index, count);
            long cPos = zMemoryStream.Position;
            int len = Convert.ToInt32(cPos - 0);

            bigWriter.Write(len);

            zMemoryStream.Position = 0;

            byte[] buff = new byte[len];
            zMemoryStream.Read(buff, 0, len);
            compressedWriter.Write(buff);

            Trace.WriteLine("Compressed data length: " + len);
        }
        public override void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }
        public override void Write(byte value)
        {
            byte[] b = new byte[1];
            b[0] = value;
            Write(b, 0, 1);
        }
    }
}
