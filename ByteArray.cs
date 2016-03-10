using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class CompressionAlgorithm
{
	public const string Deflate = "deflate";
	public const string Zlib = "zlib";
}

public class ByteArray
{
	private MemoryStream _memoryStream;
	private DataOutput _dataOutput;
	private DataInput _dataInput;
	private ObjectEncoding _objectEncoding;

	public ByteArray ()
	{
		_memoryStream = new MemoryStream ();
		_dataOutput = new DataOutput (_memoryStream);
		_dataInput = new DataInput (_memoryStream);
		_objectEncoding = ObjectEncoding.AMF3;
	}

	public ByteArray (MemoryStream ms)
	{
		_memoryStream = ms;

		_dataOutput = new DataOutput (_memoryStream);
		_dataInput = new DataInput (_memoryStream);
		_objectEncoding = ObjectEncoding.AMF3;
	}

	public ByteArray (byte[] buffer)
	{
		_memoryStream = new MemoryStream ();
		_memoryStream.Write (buffer, 0, buffer.Length);
		_memoryStream.Position = 0;

		_dataOutput = new DataOutput (_memoryStream);
		_dataInput = new DataInput (_memoryStream);
		_objectEncoding = ObjectEncoding.AMF3;
	}
	public void dispose(){
		if (_dataOutput != null) {
			_dataOutput.dispose ();
		}
		if (_dataInput != null) {
			_dataInput.dispose ();
		}
		if (_memoryStream != null) {
			_memoryStream.Close ();
			_memoryStream.Dispose ();
		}
		_dataOutput = null;
		_dataInput = null;
		_memoryStream = null;
	}
	public uint Length
	{
		get
		{ 
			return (uint)_memoryStream.Length;
		}
	}

	public uint Position
	{
		get { return (uint)_memoryStream.Position; }
		set{ _memoryStream.Position = value; }
	}

	public uint BytesAvailable
	{
		get { return Length - Position; }
	}

	public ObjectEncoding ObjectEncoding
	{
		get { return _objectEncoding; }
		set
		{ 
			_objectEncoding = value;
			_dataInput.ObjectEncoding = value;
			_dataOutput.ObjectEncoding = value;
		}
	}

	public byte[] GetBuffer ()
	{
		return _memoryStream.GetBuffer ();
	}

	public byte[] ToArray ()
	{
		return _memoryStream.ToArray ();
	}

	public MemoryStream MemoryStream{ get { return _memoryStream; } }

	public bool ReadBoolean ()
	{
		return _dataInput.ReadBoolean ();
	}

	public byte ReadByte ()
	{
		return _dataInput.ReadByte ();
	}

	public void ReadBytes (byte[] bytes, uint offset, uint length)
	{
		_dataInput.ReadBytes (bytes, offset, length);
	}

	public void ReadBytes (ByteArray bytes, uint offset, uint length)
	{
		uint tmp = bytes.Position;
		int count = (int)(length != 0 ? length : BytesAvailable);
		for (int i = 0; i < count; i++)
		{
			bytes._memoryStream.Position = i + offset;
			bytes._memoryStream.WriteByte (ReadByte ());
		}
		bytes.Position = tmp;
	}

	public void ReadBytes (ByteArray bytes)
	{
		ReadBytes (bytes, 0, 0);
	}

	public double ReadDouble ()
	{
		return _dataInput.ReadDouble ();
	}

	public float ReadFloat ()
	{
		return _dataInput.ReadFloat ();
	}

	public int ReadInt ()
	{
		return _dataInput.ReadInt ();
	}

	public object ReadObject ()
	{
		return _dataInput.ReadObject ();
	}

	public short ReadShort ()
	{
		return _dataInput.ReadShort ();
	}

	public byte ReadUnsignedByte ()
	{
		return _dataInput.ReadUnsignedByte ();
	}

	public uint ReadUnsignedInt ()
	{
		return _dataInput.ReadUnsignedInt ();
	}

	public ushort ReadUnsignedShort ()
	{
		return _dataInput.ReadUnsignedShort ();
	}

	public string ReadUTF ()
	{
		return _dataInput.ReadString ();
	}

	public string ReadUTFBytes (uint length)
	{
		return _dataInput.ReadUTFBytes (length);
	}

	public void WriteBoolean (bool value)
	{
		_dataOutput.WriteBoolean (value);
	}

	public void WriteByte (byte value)
	{
		_dataOutput.WriteByte (value);
	}

	public void WriteBytes (byte[] bytes, int offset, int length)
	{
		_dataOutput.WriteBytes (bytes, offset, length);
	}

	public void WriteDouble (double value)
	{
		_dataOutput.WriteDouble (value);
	}

	public void WriteFloat (float value)
	{
		_dataOutput.WriteFloat (value);
	}

	public void WriteInt (int value)
	{
		_dataOutput.WriteInt (value);
	}

	public void WriteObject (object value)
	{
		_dataOutput.WriteObject (value);
	}

	public void WriteShort (short value)
	{
		_dataOutput.WriteShort (value);
	}

	public void WriteUnsignedInt (uint value)
	{
		_dataOutput.WriteUnsignedInt (value);
	}

	public void WriteUTF (string value)
	{
		_dataOutput.WriteUTF (value);
	}

	public void WriteUTFBytes (string value)
	{
		_dataOutput.WriteUTFBytes (value);
	}


	public void Compress ()
	{
		Compress (CompressionAlgorithm.Zlib);
	}

	public void Deflate ()
	{
		Compress (CompressionAlgorithm.Deflate);
	}

	public void Compress (string algorithm)
	{
//		ValidationUtils.ArgumentConditionTrue(algorithm == CompressionAlgorithm.Deflate || algorithm == CompressionAlgorithm.Zlib, "algorithm", "Invalid parameter");
//		#if SILVERLIGHT
//		throw new NotSupportedException();
//		#else
//		if (algorithm == CompressionAlgorithm.Deflate)
//		{
//			byte[] buffer = _memoryStream.ToArray();
//			MemoryStream ms = new MemoryStream();
//			DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Compress, true);
//			deflateStream.Write(buffer, 0, buffer.Length);
//			deflateStream.Close();
//			_memoryStream.Close();
//			_memoryStream = ms;
//			AMFReader amfReader = new AMFReader(_memoryStream);
//			AMFWriter amfWriter = new AMFWriter(_memoryStream);
//			_dataOutput = new DataOutput(amfWriter);
//			_dataInput = new DataInput(amfReader);
//		}
//		if (algorithm == CompressionAlgorithm.Zlib)
//		{
//			byte[] buffer = _memoryStream.ToArray();
//			MemoryStream ms = new MemoryStream();
//			ZlibStream zlibStream = new ZlibStream(ms, CompressionMode.Compress, true);
//			zlibStream.Write(buffer, 0, buffer.Length);
//			zlibStream.Flush();
//			zlibStream.Close();
//			zlibStream.Dispose();
//			_memoryStream.Close();
//			_memoryStream = ms;
//			AMFReader amfReader = new AMFReader(_memoryStream);
//			AMFWriter amfWriter = new AMFWriter(_memoryStream);
//			_dataOutput = new DataOutput(amfWriter);
//			_dataInput = new DataInput(amfReader);
//		}
//		#endif
	}

	/// <summary>
	/// Decompresses the byte array using the deflate compression algorithm. The byte array must have been compressed using the same algorithm.
	/// </summary>
	/// <remarks>
	/// After the call, the position property is set to 0.
	/// </remarks>
	public void Inflate ()
	{
		Uncompress (CompressionAlgorithm.Deflate);
	}

	/// <summary>
	/// Decompresses the byte array. The byte array must have been previously compressed with the Compress() method.
	/// </summary>
	public void Uncompress ()
	{
		Uncompress (CompressionAlgorithm.Zlib);
	}

	/// <summary>
	/// Decompresses the byte array. The byte array must have been previously compressed with the Compress() method.
	/// </summary>
	/// <param name="algorithm">The compression algorithm to use when decompressing. This must be the same compression algorithm used to compress the data. Valid values are defined as constants in the CompressionAlgorithm class. The default is to use zlib format.</param>
	public void Uncompress (string algorithm)
	{
//		ValidationUtils.ArgumentConditionTrue(algorithm == CompressionAlgorithm.Deflate || algorithm == CompressionAlgorithm.Zlib, "algorithm", "Invalid parameter");
//		#if SILVERLIGHT
//		throw new NotSupportedException();
//		#else
//		if (algorithm == CompressionAlgorithm.Zlib)
//		{
//			//The zlib format is specified by RFC 1950. Zlib also uses deflate, plus 2 or 6 header bytes, and a 4 byte checksum at the end. 
//			//The first 2 bytes indicate the compression method and flags. If the dictionary flag is set, then 4 additional bytes will follow.
//			//Preset dictionaries aren't very common and we don't support them
//			Position = 0;
//			ZlibStream deflateStream = new ZlibStream(_memoryStream, CompressionMode.Decompress, false);
//			MemoryStream ms = new MemoryStream();
//			byte[] buffer = new byte[1024];
//			// Chop off the first two bytes
//			//int b = _memoryStream.ReadByte();
//			//b = _memoryStream.ReadByte();
//			while (true)
//			{
//				int readCount = deflateStream.Read(buffer, 0, buffer.Length);
//				if (readCount > 0)
//					ms.Write(buffer, 0, readCount);
//				else
//					break;
//			}
//			deflateStream.Close();
//			_memoryStream.Close();
//			_memoryStream.Dispose();
//			_memoryStream = ms;
//			_memoryStream.Position = 0;
//		}
//		if (algorithm == CompressionAlgorithm.Deflate)
//		{
//			Position = 0;
//			DeflateStream deflateStream = new DeflateStream(_memoryStream, CompressionMode.Decompress, false);
//			MemoryStream ms = new MemoryStream();
//			byte[] buffer = new byte[1024];
//			while (true)
//			{
//				int readCount = deflateStream.Read(buffer, 0, buffer.Length);
//				if (readCount > 0)
//					ms.Write(buffer, 0, readCount);
//				else
//					break;
//			}
//			deflateStream.Close();
//			_memoryStream.Close();
//			_memoryStream.Dispose();
//			_memoryStream = ms;
//			_memoryStream.Position = 0;
//		}
//		AMFReader amfReader = new AMFReader(_memoryStream);
//		AMFWriter amfWriter = new AMFWriter(_memoryStream);
//		_dataOutput = new DataOutput(amfWriter);
//		_dataInput = new DataInput(amfReader);
//		#endif
	}


	public override string ToString ()
	{
		return "";
	}
}