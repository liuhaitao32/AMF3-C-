using System.Collections;
using System.Collections.Generic;

public class AMFClassDefinition
{
	private string _className;
	private AMFClassMember[] _members;
	private bool _externalizable;
	private bool _dynamic;

	public static AMFClassMember[] EmptyClassMembers = new AMFClassMember[0];

	public AMFClassDefinition (string className, AMFClassMember[] members, bool externalizable, bool dynamic)
	{
		_className = className;
		_members = members;
		_externalizable = externalizable;
		_dynamic = dynamic;
	}

	public string ClassName{ get { return _className; } }

	public int MemberCount
	{ 
		get
		{
			if (_members == null)
				return 0;
			return _members.Length; 
		} 
	}

	public AMFClassMember[] Members { get { return _members; } }

	public bool IsExternalizable{ get { return _externalizable; } }

	public bool IsDynamic{ get { return _dynamic; } }

	public bool IsTypedObject{ get { return (_className != null && _className != string.Empty); } }
}

public class ObjectFactory
{

	public static Dictionary<string,object> CreateInstance ()
	{
		return new Dictionary<string,object> ();
	}

	public static Dictionary<string,object> CreateInstance (string name)
	{
		return new Dictionary<string,object> ();
	}
}

