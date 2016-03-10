using System.Collections;
using System.Collections.Generic;

public class AMFClassMember
{
	private string _name;
	private int _bindingFlags;
	private int _memberType;
	private object[] _customAttributes;

	public AMFClassMember (string name, int bindingFlags, int memberType, object[] customAttributes)
	{
		_name = name;
		_bindingFlags = bindingFlags;
		_memberType = memberType;
		_customAttributes = customAttributes;
	}

	public string Name
	{
		get { return _name; }
	}

	public int BindingFlags
	{
		get { return _bindingFlags; }
	}

	public int MemberType
	{
		get { return _memberType; }
	}

	public object[] CustomAttributes
	{
		get { return _customAttributes; }
	}

}

