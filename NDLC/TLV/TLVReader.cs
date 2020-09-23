﻿using NBitcoin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NDLC.TLV
{
	public class TLVReader
	{
		byte[] buffer = new byte[32];
		public TLVReader(Stream stream)
		{
			Stream = stream;
		}

		public Stream Stream { get; }
		public ulong ReadU64()
		{
			CanRead(8);
			Stream.Read(buffer, 0, 8);
			return Utils.ToUInt64(buffer, false);
		}

		public void ReadBytes(Span<byte> output)
		{
			CanRead(output.Length);
			if (Stream.Read(output) != output.Length)
				throw new EndOfStreamException();
		}

		private void CanRead(int len)
		{
			if (Stream.Position + len  > Stream.Length)
				throw new EndOfStreamException();
		}

		public uint ReadU32()
		{
			CanRead(4);
			Stream.Read(buffer, 0, 4);
			return Utils.ToUInt32(buffer, false);
		}
		public ushort ReadU16()
		{
			CanRead(2);
			Stream.Read(buffer, 0, 2);
			return (ushort)(buffer[1] + (buffer[0] << 8));
		}
		public byte ReadByte()
		{
			CanRead(1);
			var b = Stream.ReadByte();
			return (byte)b;
		}
		public ulong ReadBigSize()
		{
			CanRead(1);
			var b = Stream.ReadByte();
			if (b < 0xfd)
				return (byte)b;
			if (b == 0xfd)
			{
				CanRead(2);
				Stream.Read(buffer, 0, 2);
				return (((ulong)buffer[0] << 8) + buffer[1]);
			}
			if (b == 0xfe)
			{
				CanRead(4);
				Stream.Read(buffer, 0, 4);
				return Utils.ToUInt32(buffer, false);
			}
			else
			{
				CanRead(8);
				Stream.Read(buffer, 0, 8);
				return Utils.ToUInt64(buffer, false);
			}
		}

		public uint256 ReadUInt256()
		{
			Span<byte> buf = stackalloc byte[32];
			if (Stream.Read(buf) != 32)
				throw new EndOfStreamException();
			return new uint256(buf);
		}

		public TLVRecordReader StartReadRecord()
		{
			return new TLVRecordReader(this);
		}
	}
}
