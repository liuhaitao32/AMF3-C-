using System.Collections;
using System.Collections.Generic;

public class ASObject : Dictionary<string, object>
{
	private string _typeName;

	public ASObject ()
	{
	}

	public ASObject (string typeName)
	{
		_typeName = typeName;
	}

	public ASObject (IDictionary<string, object> dictionary)
		: base (dictionary)
	{
	}

	public string TypeName
	{
		get { return _typeName; }
		set { _typeName = value; }
	}

	public bool IsTypedObject
	{
		get { return !string.IsNullOrEmpty (_typeName); }
	}

	public override string ToString ()
	{
		return "";
	}
}

