using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Bang.Unity.Editor {

public class AttributeInfo
{
	public readonly object Attribute;
	public readonly IEnumerable<PublicMemberInfo> MemberInfos;

	public AttributeInfo(object attribute, IEnumerable<PublicMemberInfo> memberInfos)
	{
		Attribute = attribute;
		MemberInfos = memberInfos;
	}
}

public class PublicMemberInfo
{
	public readonly Type Type;
	public readonly string Name;
	public readonly AttributeInfo[] Attributes;

	public MemberInfo MemberInfo => _fieldInfo != null ? _fieldInfo : _propertyInfo;

	private readonly FieldInfo _fieldInfo;
	private readonly PropertyInfo _propertyInfo;

	public PublicMemberInfo(FieldInfo info)
	{
		_fieldInfo = info;
		Type = _fieldInfo.FieldType;
		Name = _fieldInfo.Name;
		Attributes = GetAttributes(_fieldInfo.GetCustomAttributes(false));
	}

	public PublicMemberInfo(PropertyInfo info)
	{
		_propertyInfo = info;
		Type = _propertyInfo.PropertyType;
		Name = _propertyInfo.Name;
		Attributes = GetAttributes(_propertyInfo.GetCustomAttributes(false));
	}

	public object GetValue(object obj) => _fieldInfo != null
		? _fieldInfo.GetValue(obj)
		: _propertyInfo.GetValue(obj, null);

	public void SetValue(object obj, object value)
	{
		if (_fieldInfo != null)
			_fieldInfo.SetValue(obj, value);
		else
			_propertyInfo.SetValue(obj, value);
	}

	public bool HasAttribute< T >() where T : Attribute {
		foreach ( var attributeInfo in Attributes ) {
			if ( attributeInfo.Attribute.GetType() == typeof( T ) ) {
				return true;
			}
		}

		return false;
	}

	static AttributeInfo[] GetAttributes(IEnumerable<object> attributes) => attributes
																			.Select(attr => new AttributeInfo(attr, attr.GetType().GetPublicMemberInfos()))
																			.ToArray();
}


public static class PublicMemberInfoExtension
{
	public static PublicMemberInfo[] GetPublicMemberInfos(this Type type)
	{
		const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

		var fieldInfos = type.GetFields(bindingFlags)
							 .Select(info => new PublicMemberInfo(info));

		var propertyInfos = type.GetProperties(bindingFlags)
								.Where(info => info.CanRead && info.CanWrite && info.GetIndexParameters().Length == 0)
								.Select(info => new PublicMemberInfo(info));

		return fieldInfos.Concat(propertyInfos).ToArray();
	}

	public static object PublicMemberClone(this object obj)
	{
		var clone = Activator.CreateInstance(obj.GetType());
		CopyPublicMemberValues(obj, clone);
		return clone;
	}

	public static T PublicMemberClone<T>(this object obj) where T : new()
	{
		var clone = new T();
		CopyPublicMemberValues(obj, clone);
		return clone;
	}

	public static void CopyPublicMemberValues(this object source, object target)
	{
		foreach (var info in source.GetType().GetPublicMemberInfos())
			info.SetValue(target, info.GetValue(source));
	}
}

}
