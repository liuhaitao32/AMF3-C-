using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;

public  class DataOutput //: IDataOutput
{
	//		private AMFWriter _amfWriter;
	private MemoryStream _amfWriter;
	private ObjectEncoding _objectEncoding;

	private Dictionary<Object, int> _amf0ObjectReferences;
	private Dictionary<Object, int> _objectReferences;
	private Dictionary<Object, int> _stringReferences;
	private Dictionary<Object,int>_classDefinitionReferences;
//	static CopyOnWriteDictionary<string, AMFClassDefinition> classDefinitions;
	public DataOutput (MemoryStream amfWriter)
	{
		_amfWriter = amfWriter;
		_objectEncoding = ObjectEncoding.AMF3;

		_amf0ObjectReferences = new Dictionary<Object, int> (5);
		_objectReferences = new Dictionary<Object, int> (5);
		_stringReferences = new Dictionary<Object, int> (5);
		_classDefinitionReferences = new Dictionary<Object, int> ();
	}
	public void dispose(){
		if (_amf0ObjectReferences != null) {
			_amf0ObjectReferences.Clear ();
		}
		if (_objectReferences != null) {
			_objectReferences.Clear ();
		}
		if (_stringReferences != null) {
			_stringReferences.Clear ();
		}
		if (_classDefinitionReferences != null) {
			_classDefinitionReferences.Clear ();
		}
		_amf0ObjectReferences = null;
		_objectReferences = null;
		_stringReferences = null;
		_classDefinitionReferences = null;
		_amfWriter = null;
	}
	public ObjectEncoding ObjectEncoding
	{
		get { return _objectEncoding; }
		set { _objectEncoding = value; }
	}

	bool _useLegacyCollection = true;
	bool _useLegacyThrowable = true;

	public bool UseLegacyCollection
	{
		get{ return _useLegacyCollection; }
		set{ _useLegacyCollection = value; }
	}

	public bool UseLegacyThrowable
	{
		get { return _useLegacyThrowable; }
		set { _useLegacyThrowable = value; }
	}

	//	#region IDataOutput Members

	public void WriteBoolean (bool value)
	{
//			_amfWriter.WriteBoolean(value);
		WriteByte ((byte)(value ? AMF3TypeCode.BooleanTrue : AMF3TypeCode.BooleanFalse));
	}

	public void WriteByte (byte value)
	{
		_amfWriter.WriteByte (value);
	}

	public void WriteBytes (byte[] bytes, int offset, int length)
	{
		for (int i = offset; i < offset + length; i++)
			_amfWriter.WriteByte (bytes [i]);
	}

	public void WriteDouble (double value)
	{
//			_amfWriter.WriteDouble(value);
		byte[] bytes = BitConverter.GetBytes (value);
		WriteBigEndian (bytes);
	}

	private void WriteBigEndian (byte[] bytes)
	{
		if (bytes == null)
			return;
		for (int i = bytes.Length - 1; i >= 0; i--)
		{
			WriteByte (bytes [i]);
		}
	}

	public void WriteFloat (float value)
	{
//			_amfWriter.WriteFloat(value);

		byte[] bytes = BitConverter.GetBytes (value);			
		WriteBigEndian (bytes);
	}

	public void WriteInt (int value)
	{
//			_amfWriter.WriteInt32(value);
		WriteInt32 (value);
	}

	public void WriteInt32 (int value)
	{
		byte[] bytes = BitConverter.GetBytes (value);
		WriteBigEndian (bytes);
	}

	private void WriteAMF3IntegerData (int value)
	{
		value &= 0x1fffffff;
		if (value < 0x80)
			this.WriteByte ((byte)value);
		else if (value < 0x4000)
		{
			this.WriteByte ((byte)(value >> 7 & 0x7f | 0x80));
			this.WriteByte ((byte)(value & 0x7f));
		}
		else if (value < 0x200000)
		{
			this.WriteByte ((byte)(value >> 14 & 0x7f | 0x80));
			this.WriteByte ((byte)(value >> 7 & 0x7f | 0x80));
			this.WriteByte ((byte)(value & 0x7f));
		}
		else
		{
			this.WriteByte ((byte)(value >> 22 & 0x7f | 0x80));
			this.WriteByte ((byte)(value >> 15 & 0x7f | 0x80));
			this.WriteByte ((byte)(value >> 8 & 0x7f | 0x80));
			this.WriteByte ((byte)(value & 0xff));
		}
	}

	public void WriteObject (object value)
	{
//		if (_objectEncoding == ObjectEncoding.AMF0)
//                _amfWriter.WriteData(ObjectEncoding.AMF0, value);
//		if (_objectEncoding == ObjectEncoding.AMF3) {
//                _amfWriter.WriteAMF3Data(value);
		WriteAMF3Data (value);
//		}
	}

	public void WriteShort (short value)
	{
//			_amfWriter.WriteShort(value);
		byte[] bytes = BitConverter.GetBytes ((ushort)value);
		WriteBigEndian (bytes);
	}

	public void WriteUnsignedInt (uint value)
	{
//			_amfWriter.WriteInt32((int)value);
	}

	public void WriteUTF (string value)
	{
//			_amfWriter.WriteUTF(value);
		UTF8Encoding utf8Encoding = new UTF8Encoding ();
		int byteCount = utf8Encoding.GetByteCount (value);
		byte[] buffer = utf8Encoding.GetBytes (value);
		this.WriteShort ((short)byteCount);
		if (buffer.Length > 0)
		{
//			this.WriteBytes (buffer, 0, buffer.Length);
			Write (buffer);
		}
	}

	public void WriteUTFBytes (string value)
	{
//			_amfWriter.WriteUTFBytes(value);
	}

	//	#endregion
	public void WriteAMF3Bool (bool value)
	{
		WriteByte ((byte)(value ? AMF3TypeCode.BooleanTrue : AMF3TypeCode.BooleanFalse));
	}

	public void WriteAMF3Data (object data)
	{
		if (data == null)
		{
			WriteAMF3Null ();
			return;
		}
//		if(data is DBNull )
//		{
//			WriteAMF3Null();
//			return;
//		}
		Type type = data.GetType ();
//
		if (type == typeof(string))
		{
			WriteAMF3String (data as string);
		}
		else if (type == typeof(bool))
		{
			WriteAMF3Bool ((bool)data);
		}
		else if (type == typeof(Char))
		{
			WriteAMF3String (new String ((char)data, 1));
		}
		else if (
			(type == typeof(Byte)) ||
			(type == typeof(Int16)) ||
			(type == typeof(UInt16)) ||
			(type == typeof(Int32)) ||
			(type == typeof(UInt32)))
		{
			WriteAMF3Int (Convert.ToInt32 (data));
		}
		else if (
			(type == typeof(Int64)) ||
			(type == typeof(UInt64)) ||
			(type == typeof(Double)) ||
			(type == typeof(Decimal)) ||
			(type == typeof(Single)))
		{
			WriteAMF3Double (Convert.ToDouble (data));
		}
		else
		{
			AMF3ObjectWriter (data);
		}
	}

	public void WriteAMF3String (string value)
	{
		WriteByte (AMF3TypeCode.String);
		WriteAMF3UTF (value);
	}

	public void WriteAMF3Null ()
	{
		//Write the null code (0x1) to the output stream.
		WriteByte (AMF3TypeCode.Null);
	}

	public void WriteAMF3UTF (string value)
	{
		if (value == string.Empty)
		{
			WriteAMF3IntegerData (1);
		}
		else
		{
			if (!_stringReferences.ContainsKey (value))
			{
				_stringReferences.Add (value, _stringReferences.Count);
				UTF8Encoding utf8Encoding = new UTF8Encoding ();
				int byteCount = utf8Encoding.GetByteCount (value);
				int handle = byteCount;
				handle = handle << 1;
				handle = handle | 1;
				WriteAMF3IntegerData (handle);
				byte[] buffer = utf8Encoding.GetBytes (value);
				if (buffer.Length > 0)
					Write (buffer);
			}
			else
			{
				int handle = (int)_stringReferences [value];
				handle = handle << 1;
				WriteAMF3IntegerData (handle);
			}
		}
	}

	public void WriteAMF3Int (int value)
	{
		if (value >= -268435456 && value <= 268435455)//check valid range for 29bits
		{
			WriteByte (AMF3TypeCode.Integer);
			WriteAMF3IntegerData (value);
		}
		else
		{
			//overflow condition would occur upon int conversion
			WriteAMF3Double ((double)value);
		}
	}

	public void WriteAMF3Double (double value)
	{
		WriteByte (AMF3TypeCode.Number);
		//long tmp = BitConverter.DoubleToInt64Bits( double.Parse(value.ToString()) );
		WriteDouble (value);
	}

	public void Write (byte[] buffer)
	{
		if (buffer != null && buffer.Length > 0)
		{
			this.WriteBytes (buffer, 0, buffer.Length);
		}
	}

	public void WriteAMF3Array (Array value)
	{
//		if (_amf0ObjectReferences.ContainsKey(value))
//		{
//			WriteReference(value);
//			return;
//		}

		if (!_objectReferences.ContainsKey (value))
		{
			_objectReferences.Add (value, _objectReferences.Count);
			int handle = value.Length;
			handle = handle << 1;
			handle = handle | 1;
			WriteAMF3IntegerData (handle);
			WriteAMF3UTF (string.Empty);//hash name
			for (int i = 0; i < value.Length; i++)
			{
				WriteAMF3Data (value.GetValue (i));
			}
		}
		else
		{
			int handle = (int)_objectReferences [value];
			handle = handle << 1;
			WriteAMF3IntegerData (handle);
		}
	}

	public void WriteAMF3Array (IList value)
	{
		if (!_objectReferences.ContainsKey (value))
		{
			_objectReferences.Add (value, _objectReferences.Count);
			int handle = value.Count;
			handle = handle << 1;
			handle = handle | 1;
			WriteAMF3IntegerData (handle);
			WriteAMF3UTF (string.Empty);//hash name
			for (int i = 0; i < value.Count; i++)
			{
				WriteAMF3Data (value [i]);
			}
		}
		else
		{
			int handle = (int)_objectReferences [value];
			handle = handle << 1;
			WriteAMF3IntegerData (handle);
		}
	}

	public void WriteAMF3AssociativeArray (IDictionary value)
	{
		if (!_objectReferences.ContainsKey (value))
		{
			_objectReferences.Add (value, _objectReferences.Count);
			WriteAMF3IntegerData (1);
			foreach (DictionaryEntry entry in value)
			{
				WriteAMF3UTF (entry.Key.ToString ());
				WriteAMF3Data (entry.Value);
			}
			WriteAMF3UTF (string.Empty);
		}
		else
		{
			int handle = (int)_objectReferences [value];
			handle = handle << 1;
			WriteAMF3IntegerData (handle);
		}
	}

	public void AMF3ObjectWriter (object data)
	{
		IList list = data as IList;
		if (list != null)
		{
			WriteByte (AMF3TypeCode.Array);
			WriteAMF3Array (list);
			return;
		}

		IDictionary dictionary = data as IDictionary;
		if (dictionary != null)
		{
			if (dictionary.Count == 0) {
				WriteByte(AMF3TypeCode.Object);
				WriteAMF3Object(data);
			}
			else{
				WriteByte (AMF3TypeCode.Array);
				WriteAMF3AssociativeArray (dictionary);
			}
			return;
		}
	}
	public void WriteAMF3Object(object value)
	{
		
		if (!_objectReferences.ContainsKey(value))
		{
			_objectReferences.Add(value, _objectReferences.Count);

			//AMFClassDefinition classDefinition = GetClassDefinition(value);
			Dictionary<object,object> classDefinition = new Dictionary<object, object>();
			if (classDefinition == null)
			{
				//Something went wrong in our reflection?
//				string msg = __Res.GetString(__Res.Fluorine_Fatal, "serializing " + value.GetType().FullName);
//				#if !SILVERLIGHT
//				if (log.IsFatalEnabled)
//					log.Fatal(msg);
//				#endif
//				System.Diagnostics.Debug.Assert(false, msg);
//				return;
			}
			if (_classDefinitionReferences.ContainsKey(classDefinition))
			{
				//Existing class-def
				int handle = (int)_classDefinitionReferences[classDefinition];//handle = classRef 0 1
				handle = handle << 2;
				handle = handle | 1;
				WriteAMF3IntegerData(handle);
			}
			else
			{//inline class-def

				//classDefinition = CreateClassDefinition(value);
				_classDefinitionReferences.Add(classDefinition, _classDefinitionReferences.Count);
				//handle = memberCount dynamic externalizable 1 1
				//int handle = classDefinition.MemberCount;
				int handle = classDefinition.Count;
				handle = handle << 1;
//				handle = handle | (classDefinition.IsDynamic ? 1 : 0);
				handle = handle | (false ? 1 : 0);
				handle = handle << 1;
//				handle = handle | (classDefinition.IsExternalizable ? 1 : 0);
				handle = handle | (false ? 1 : 0);
				handle = handle << 2;
				handle = handle | 3;
				WriteAMF3IntegerData(handle);
//				WriteAMF3UTF(classDefinition.ClassName);
				WriteAMF3UTF(string.Empty);
//				for(int i = 0; i < classDefinition.MemberCount; i++)
//				for(int i = 0; i < classDefinition.Count; i++)
//				{
//					string key = classDefinition.Members[i].Name;
//					string key = classDefinition.Keys[i];
//					WriteAMF3UTF(key);
//				}
			}
			//write inline object
//			if( classDefinition.IsExternalizable )
			if( true )
			{
//				if( value is IExternalizable )
//				{
//					IExternalizable externalizable = value as IExternalizable;
//					DataOutput dataOutput = new DataOutput(this);
//					externalizable.WriteExternal(dataOutput);
//				}
//				else
//					throw new FluorineException(__Res.GetString(__Res.Externalizable_CastFail,classDefinition.ClassName));
			}
			else
			{
//				Type type = value.GetType();
//				IObjectProxy proxy = ObjectProxyRegistry.Instance.GetObjectProxy(type);

//				for(int i = 0; i < classDefinition.MemberCount; i++)
//				{
//					//object memberValue = GetMember(value, classDefinition.Members[i]);
////					object memberValue = proxy.GetValue(value, classDefinition.Members[i]);
//					Dictionary<object,object> obj = new Dictionary<object, object>();
//					WriteAMF3Data(obj);
//				}
//				if(classDefinition.IsDynamic)
				if(false)
				{
					IDictionary dictionary = value as IDictionary;
					foreach(DictionaryEntry entry in dictionary)
					{
						WriteAMF3UTF(entry.Key.ToString());
						WriteAMF3Data(entry.Value);
					}
					WriteAMF3UTF(string.Empty);
				}
			}
		}
		else
		{
			//handle = objectRef 0
			int handle = (int)_objectReferences[value];
			handle = handle << 1;
			WriteAMF3IntegerData(handle);
		}
	}
//	private AMFClassDefinition GetClassDefinition(object obj)
//	{
//		AMFClassDefinition classDefinition = null;
//		Dictionary<object,object> classDefinition = null;
//		if (obj is ASObject)
//		{
//			ASObject asObject = obj as ASObject;
//			if (asObject.IsTypedObject)
////				classDefinitions.TryGetValue(asObject.TypeName, out classDefinition);
//			if (classDefinition != null)
//				return classDefinition;
//
//			IObjectProxy proxy = ObjectProxyRegistry.Instance.GetObjectProxy(typeof(ASObject));
//			classDefinition = proxy.GetClassDefinition(obj);
//			if (asObject.IsTypedObject)
//			{
//				//Only typed ASObject class definitions are cached.
//				classDefinitions[asObject.TypeName] = classDefinition;
//			}
//			return classDefinition;
//		}
//		else
//		{
//			string typeName = obj.GetType().FullName;
//			if( !classDefinitions.TryGetValue(typeName, out classDefinition))
//			{
//				IObjectProxy proxy = ObjectProxyRegistry.Instance.GetObjectProxy(obj.GetType());
//				classDefinition = proxy.GetClassDefinition(obj);
//				classDefinitions[typeName] = classDefinition;
//			}
//			return classDefinition;
//		}
//	}
}
