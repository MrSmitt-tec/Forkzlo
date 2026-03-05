using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Zlo4NET.Core.Extensions;

internal static class ZBinaryReaderExtensions
{
	private static byte[] ReadReversedBytes(this BinaryReader binaryReader, int count)
	{
		return binaryReader.ReadBytes(count).Reverse().ToArray();
	}

	public static long BytesRemaining(this BinaryReader binaryReader)
	{
		return binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;
	}

	public static string ReadZString(this BinaryReader binaryReader)
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			char value;
			while (binaryReader.PeekChar() != -1 && (value = binaryReader.ReadChar()) > '\0')
			{
				stringBuilder.Append(value);
			}
		}
		catch (Exception)
		{
		}
		return stringBuilder.ToString();
	}

	public static void SkipZString(this BinaryReader binaryReader)
	{
		try
		{
			while (binaryReader.PeekChar() != -1 && binaryReader.ReadChar() > '\0')
			{
			}
		}
		catch (Exception)
		{
		}
	}

	public static void SkipBytes(this BinaryReader binaryReader, int numberOfBytes)
	{
		binaryReader.BaseStream.Seek(numberOfBytes, SeekOrigin.Current);
	}

	public static string ReadCountedString(this BinaryReader binaryReader, int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Append(binaryReader.ReadChar());
		}
		return stringBuilder.ToString();
	}

	public static uint ReadZUInt32(this BinaryReader binaryReader)
	{
		return BitConverter.ToUInt32(binaryReader.ReadReversedBytes(4), 0);
	}

	public static ulong ReadZUInt64(this BinaryReader binaryReader)
	{
		return BitConverter.ToUInt64(binaryReader.ReadReversedBytes(8), 0);
	}

	public static ushort ReadZUInt16(this BinaryReader binaryReader)
	{
		return BitConverter.ToUInt16(binaryReader.ReadReversedBytes(2), 0);
	}

	public static float ReadZFloat(this BinaryReader binaryReader)
	{
		return BitConverter.ToSingle(binaryReader.ReadReversedBytes(4), 0);
	}
}
