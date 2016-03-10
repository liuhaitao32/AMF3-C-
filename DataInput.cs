using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;

public class DataInput //: IDataInput
{
	private MemoryStream _amfReader;
	private ObjectEncoding _objectEncoding;
	private List<Object> _objectReferences;
	private List<Object> _stringReferences;
	private List<AMFClassDefinition> _classDefinitions;

	public DataInput (MemoryStream amfReader)
	{
		_amfReader = amfReader;
		_objectEncoding = ObjectEncoding.AMF3;
		_objectReferences = new List<Object> (15);
		_stringReferences = new List<Object> (15);
		_classDefinitions = new List<AMFClassDefinition> (2);
	}
	public void dispose(){
		if (_objectReferences != null) {
			_objectReferences.Clear ();
		}
		if (_stringReferences != null) {
			_stringReferences.Clear ();
		}
		if (_classDefinitions != null) {
			_classDefinitions.Clear ();
		}
		_objectReferences = null;
		_stringReferences = null;
		_classDefinitions = null;
		_amfReader = null;
	}
	public ObjectEncoding ObjectEncoding
	{
		get { return _objectEncoding; }
		set { _objectEncoding = value; }
	}

	public bool ReadBoolean ()
	{
		//			return _amfReader.ReadBoolean();
		return (_amfReader.ReadByte () == AMF3TypeCode.BooleanTrue);
	}

	public byte ReadByte ()
	{
		return (byte)_amfReader.ReadByte ();
	}

	public void ReadBytes (byte[] bytes, uint offset, uint length)
	{
		_amfReader.Read (bytes, (int)offset, (int)length);
	}

	public byte[] ReadBytes (uint c)
	{
		byte[] a = new byte[c];
		for (uint i = 0; i < c; i++)
		{
			a [i] = ReadByte ();
		}
		return a;
	}

	public double ReadDouble ()
	{
		byte[] bytes = ReadBytes (8);
		byte[] reverse = new byte[8];
		//Grab the bytes in reverse order 
		for (int i = 7, j = 0; i >= 0; i--, j++)
		{
			reverse [j] = bytes [i];
		}
		double value = BitConverter.ToDouble (reverse, 0);
		return value;
	}

	public float ReadFloat ()
	{
		byte[] bytes = ReadBytes (4);
		byte[] invertedBytes = new byte[4];
		//Grab the bytes in reverse order from the backwards index
		for (int i = 3, j = 0; i >= 0; i--, j++)
		{
			invertedBytes [j] = bytes [i];
		}
		float value = BitConverter.ToSingle (invertedBytes, 0);
		return value;
	}

	public int ReadInt ()
	{
		return ReadInt32 ();
	}

	public int ReadInt32 ()
	{
		byte[] bytes = ReadBytes (4);
		int value = (int)((bytes [0] << 24) | (bytes [1] << 16) | (bytes [2] << 8) | bytes [3]);
		return value;
	}

	public object ReadObject ()
	{
		object obj = null;
//            if( _objectEncoding == ObjectEncoding.AMF0 )
//                obj = _amfReader.ReadData();
//            if (_objectEncoding == ObjectEncoding.AMF3)
		obj = ReadAMF3Data ();
		return obj;
	}

	public short ReadShort ()
	{
		return ReadInt16 ();//_amfReader.ReadInt16();
	}

	public short ReadInt16 ()
	{
		//Read the next 2 bytes, shift and add.
		byte[] bytes = ReadBytes (2);
		return (short)((bytes [0] << 8) | bytes [1]);
	}

	public byte ReadUnsignedByte ()
	{
		return 0;//_amfReader.ReadByte();
	}

	public uint ReadUnsignedInt ()
	{
		return (uint)0;//_amfReader.ReadInt32();
	}

	public ushort ReadUnsignedShort ()
	{
		return 0;//_amfReader.ReadUInt16();
	}

	public string ReadUTF (int length)
	{
		if (length == 0)
			return string.Empty;
		UTF8Encoding utf8 = new UTF8Encoding (false, true);
		byte[] encodedBytes = ReadBytes ((uint)length);
//		#if !(NET_1_1)
		string decodedString = utf8.GetString (encodedBytes, 0, encodedBytes.Length);
//		#else
//		string decodedString = utf8.GetString(encodedBytes);
//		#endif
		return decodedString;
	}

	public string ReadString ()
	{
		//Read the next 2 bytes, shift and add.
		int length = ReadUInt16 ();
		return ReadUTF (length);
	}

	public ushort ReadUInt16 ()
	{
		//Read the next 2 bytes, shift and add.
		byte[] bytes = ReadBytes (2);
		return (ushort)(((bytes [0] & 0xff) << 8) | (bytes [1] & 0xff));
	}

	public string ReadUTFBytes (uint length)
	{
		return "";//_amfReader.ReadUTF((int)length);
	}

	//	#endregion
	public object ReadAMF3Data ()
	{
		byte typeCode = this.ReadByte ();
		return this.ReadAMF3Data (typeCode);
	}

	public object ReadAMF3Data (byte typeMarker)
	{
		//		return AmfTypeTable[3][typeMarker].ReadData(this);
		if ((typeMarker == AMF3TypeCode.Undefined) || (typeMarker == AMF3TypeCode.Null))
		{
			return null;
		}
		else if ((typeMarker == AMF3TypeCode.BooleanFalse))
		{
			return false;
		}
		else if ((typeMarker == AMF3TypeCode.BooleanTrue))
		{
			return true;
		}
		else if ((typeMarker == AMF3TypeCode.Integer))
		{
			return ReadAMF3Int ();
		}
		else if ((typeMarker == AMF3TypeCode.Number))
		{
			return ReadDouble ();
		}
		else if ((typeMarker == AMF3TypeCode.String))
		{
			return ReadAMF3String ();
		}
		else if ((typeMarker == AMF3TypeCode.DateTime))
		{
			return ReadAMF3Date ();
		}
		else if ((typeMarker == AMF3TypeCode.Array))
		{
			return ReadAMF3Array ();
		}
		else if ((typeMarker == AMF3TypeCode.Object))
		{
			return ReadAMF3Object ();//ReadAMF3Object();
		}
		return null;
	}

	public int ReadAMF3Int ()
	{
		int intData = ReadAMF3IntegerData ();
		return intData;
	}

	public int ReadAMF3IntegerData ()
	{
		int acc = ReadByte ();
		int tmp;
		if (acc < 128)
			return acc;
		else
		{
			acc = (acc & 0x7f) << 7;
			tmp = this.ReadByte ();
			if (tmp < 128)
				acc = acc | tmp;
			else
			{
				acc = (acc | tmp & 0x7f) << 7;
				tmp = this.ReadByte ();
				if (tmp < 128)
					acc = acc | tmp;
				else
				{
					acc = (acc | tmp & 0x7f) << 8;
					tmp = this.ReadByte ();
					acc = acc | tmp;
				}
			}
		}

		int mask = 1 << 28; // mask
		int r = -(acc & mask) | acc;
		return r;

		//s = 32 - 29;
		//r = (x << s) >> s;
	}

	public string ReadAMF3String ()
	{
		int handle = ReadAMF3IntegerData ();
		bool inline = ((handle & 1) != 0);
		handle = handle >> 1;
		if (inline)
		{
			int length = handle;
			if (length == 0)
				return string.Empty;
			string str = ReadUTF (length);
			AddStringReference (str);
			return str;
		}
		else
		{
			return ReadStringReference (handle);
		}
	}

	public void AddStringReference (string str)
	{
		_stringReferences.Add (str);
	}

	public string ReadStringReference (int index)
	{
		return _stringReferences [index] as string;
	}

	public object ReadAMF3Array ()
	{
		int handle = ReadAMF3IntegerData ();
		bool inline = ((handle & 1) != 0);
		handle = handle >> 1;
		if (inline)
		{
			Dictionary<string, object> hashtable = null;

			string key = ReadAMF3String ();
			object value = null;
			while (key != null && key != string.Empty)
			{
				if (hashtable == null)
				{

					hashtable = new Dictionary<string, object> ();

					AddAMF3ObjectReference (hashtable);
				}
				value = ReadAMF3Data ();
				hashtable.Add (key.ToString (), value);
				key = ReadAMF3String ();
			}
			//Not an associative array
			if (hashtable == null)
			{
				object[] array = new object[handle];
				byte typeCode = 0;
				AddAMF3ObjectReference (array);
				for (int i = 0; i < handle; i++)
				{
					//Grab the type for each element.
					typeCode = this.ReadByte ();
					value = ReadAMF3Data (typeCode);
					array [i] = value;
				}
				return array;
			}
			else
			{
				for (int i = 0; i < handle; i++)
				{
					value = ReadAMF3Data ();
					hashtable.Add (i.ToString (), value);
				}
				return hashtable;
			}
		}
		else
		{
			return ReadAMF3ObjectReference (handle);
		}
	}

	public void AddAMF3ObjectReference (object instance)
	{
		_objectReferences.Add (instance);
	}

	public object ReadAMF3ObjectReference (int index)
	{
		return _objectReferences [index];
	}


	/**/
	public object ReadAMF3Object ()
	{
		int handle = ReadAMF3IntegerData ();
		bool inline = ((handle & 1) != 0);
		handle = handle >> 1;
		if (!inline)
		{
			//An object reference
			return ReadAMF3ObjectReference (handle);
		}
		else
		{
			AMFClassDefinition classDefinition = ReadClassDefinition (handle);
			object obj = ReadAMF3Object (classDefinition);
			return obj;
		}
	}

	public object ReadAMF3Object (AMFClassDefinition classDefinition)
	{
		Dictionary<string,object> instance = null;
		if (!string.IsNullOrEmpty (classDefinition.ClassName))
			instance = ObjectFactory.CreateInstance (classDefinition.ClassName);
		else
			instance = ObjectFactory.CreateInstance ();//new ASObject();
		if (instance == null)
		{
			instance = ObjectFactory.CreateInstance ();//new ASObject(classDefinition.ClassName);
		}
		AddAMF3ObjectReference (instance);
		if (classDefinition.IsExternalizable)
		{
//			if (instance is IExternalizable)
//			{
//				IExternalizable externalizable = instance as IExternalizable;
//				DataInput dataInput = new DataInput(this);
//				externalizable.ReadExternal(dataInput);
//			}
//			else
//			{
//				string msg = __Res.GetString(__Res.Externalizable_CastFail, instance.GetType().FullName);
//				throw new FluorineException(msg);
//			}
		}
		else
		{
			object value = null;
			string key = "";
			for (int i = 0; i < classDefinition.MemberCount; i++)
			{
				key = classDefinition.Members [i].Name;
				value = ReadAMF3Data ();
//				SetMember(instance, key, value);
				instance.Add (key, value);//key = value;
			}
			if (classDefinition.IsDynamic)
			{
				key = ReadAMF3String ();
				while (key != null && key != string.Empty)
				{
					value = ReadAMF3Data ();
//					SetMember(instance, key, value);
					instance.Add (key, value);
					key = ReadAMF3String ();
				}
			}
		}
		return instance;
	}

	public AMFClassDefinition ReadClassDefinition (int handle)
	{
		AMFClassDefinition classDefinition = null;
		//an inline object
		bool inlineClassDef = ((handle & 1) != 0);
		handle = handle >> 1;
		if (inlineClassDef)
		{
			//inline class-def
			string typeIdentifier = ReadAMF3String ();
			//flags that identify the way the object is serialized/deserialized
			bool externalizable = ((handle & 1) != 0);
			handle = handle >> 1;
			bool dynamic = ((handle & 1) != 0);
			handle = handle >> 1;

			AMFClassMember[] members = new AMFClassMember[handle];
			string name = "";
			AMFClassMember classMember = null;
			for (int i = 0; i < handle; i++)
			{
				name = ReadAMF3String ();
				classMember = new AMFClassMember (name, 0x0, 0x40, null);
				members [i] = classMember;
			}
			classDefinition = new AMFClassDefinition (typeIdentifier, members, externalizable, dynamic);
			AddClassReference (classDefinition);
		}
		else
		{
			classDefinition = ReadClassReference (handle);
		}
		return classDefinition;
	}

	private void AddClassReference (AMFClassDefinition classDefinition)
	{
		_classDefinitions.Add (classDefinition);
	}

	private AMFClassDefinition ReadClassReference (int index)
	{
		return _classDefinitions [index] as AMFClassDefinition;
	}

	public DateTime ReadAMF3Date ()
	{
		int handle = ReadAMF3IntegerData ();
		bool inline = ((handle & 1) != 0);
		handle = handle >> 1;
		if (inline)
		{
			double milliseconds = this.ReadDouble ();
			DateTime start = new DateTime (1970, 1, 1, 0, 0, 0);

			DateTime date = start.AddMilliseconds (milliseconds);
//			#if !(NET_1_1)
			date = DateTime.SpecifyKind (date, DateTimeKind.Utc);
//			#endif
//			switch(FluorineConfiguration.Instance.TimezoneCompensation)
//			{
//			case TimezoneCompensation.None:
//				//No conversion by default
//				break;
//			case TimezoneCompensation.Auto:
//				//Not applicable for AMF3
//				break;
//			case TimezoneCompensation.Server:
//				//Convert to local time
//				date = date.ToLocalTime();
//				break;
//			}
			AddAMF3ObjectReference (date);
			return date;
		}
		else
		{
			return (DateTime)ReadAMF3ObjectReference (handle);
		}
	}
	/**/
}
