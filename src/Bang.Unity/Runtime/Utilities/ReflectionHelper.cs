using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Bang.Unity.Utilities {

public static class ReflectionHelper {
	
	public const BindingFlags FLAGS_ALL = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
	public const BindingFlags FLAGS_ALL_DECLARED = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

	
	private static List<Type>? _allTypesInAllAssemblies = null;
	
	private static readonly HashSet<string> _ignoredAssemblies = new () { "Bang.Generator" };

	static ReflectionHelper() {
		FlushMem();
	}

#if UNITY_EDITOR
	[UnityEditor.Callbacks.DidReloadScripts]
#endif
	public static void FlushMem() {
		_loadedAssemblies = null;
		_allTypes = null;
		_tempArgs = new object[1];
		_typesMap = new Dictionary<string, Type>();
		_subTypesMap = new Dictionary<Type, Type[]>();
		_typeFriendlyName = new Dictionary<Type, string>();
		_typeFriendlyNameCompileSafe = new Dictionary<Type, string>();
		_typeConstructors = new Dictionary<Type, ConstructorInfo[]>();
		_typeMethods = new Dictionary<Type, MethodInfo[]>();
		_typeFields = new Dictionary<Type, FieldInfo[]>();
		_typeProperties = new Dictionary<Type, PropertyInfo[]>();
		_typeEvents = new Dictionary<Type, EventInfo[]>();
		// _typeAttributes = new Dictionary<Type, object[]>();
		_memberAttributes = new Dictionary<MemberInfo, object[]>();
		_genericArgsTypeCache = new Dictionary<Type, Type[]>();
		_genericArgsMathodCache = new Dictionary<MethodInfo, Type[]>();
	}
	
	/// <summary>
	/// Gets all types that implement  <typeparamref name="T"/>, including <typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static IEnumerable<Type> GetAllImplementationsOf<T>()
	{
		return GetAllImplementationsOf(typeof(T));
	}

	public static IEnumerable<Type> GetAllImplementationsOf(Type type)
	{
		var types = SafeGetAllTypesInAllAssemblies()
					.Where(p => !p.IsInterface && !p.IsAbstract && type.IsAssignableFrom(p)
								// && !Attribute.IsDefined(p, typeof(HideInEditorAttribute))
								)
					.OrderBy(o => o.Name);

		return types;
	}

	private static readonly CacheDictionary< Type, IEnumerable< Type > > _cachedTypesWithAttributes = new ( 12 );
	public static IEnumerable<Type> GetAllTypesWithAttributeDefined<T>() => GetAllTypesWithAttributeDefined(typeof(T));

	public static IEnumerable<Type> GetAllTypesWithAttributeDefined(Type t)
	{
		if (_cachedTypesWithAttributes.TryGetValue(t, out var result))
		{
			return result;
		}

		result = SafeGetAllTypesInAllAssemblies().Where(p => Attribute.IsDefined(p, t));
		_cachedTypesWithAttributes[t] = result;

		return result;
	}

	public static IEnumerable<Type> GetAllTypesWithAttributeDefinedOfType<T>(Type ofType)
	{
		return GetAllTypesWithAttributeDefinedOfType(typeof(T), ofType);
	}
	
	public static IEnumerable<Type> GetAllTypesWithAttributeDefinedOfType(Type attributeType, Type ofType)
	{
		return SafeGetAllTypesInAllAssemblies()
			.Where(p => Attribute.IsDefined(p, attributeType) && ofType.IsAssignableFrom(p));
	}

	public static List<Type> SafeGetAllTypesInAllAssemblies()
	{
		if (_allTypesInAllAssemblies is not null)
		{
			return _allTypesInAllAssemblies;
		}

		var allTypes = new List< Type >();
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			string? assemblyName = assembly.GetName().Name;
			if (assemblyName is null || assemblyName.StartsWith("System"))
			{
				continue;
			}

			if (_ignoredAssemblies.Contains(assemblyName))
			{
				continue;
			}

			foreach (Type type in assembly.GetTypes())
			{
				allTypes.Add(type);
			}
		}

		_allTypesInAllAssemblies = allTypes;
		return _allTypesInAllAssemblies;
	}
	
	private static Dictionary<Type, Type[]> _subTypesMap;
	
	///<summary>Get a collection of types assignable to provided type, excluding Abstract types</summary>
	public static Type[] GetImplementationsOf(Type baseType) {

		Type[] result = null;
		if ( _subTypesMap.TryGetValue(baseType, out result) ) {
			return result;
		}

		var temp = new List<Type>();
		var allTypes = GetAllTypes(false);
		for ( var i = 0; i < allTypes.Length; i++ ) {
			var type = allTypes[i];
			if ( baseType.RTIsAssignableFrom(type) && !type.RTIsAbstract() ) {
				temp.Add(type);
			}
		}
		return _subTypesMap[baseType] = temp.ToArray();
	}

	///----------------------------------------------------------------------------------------------

	private static object[] _tempArgs;
	
	///<summary>Returns an object[] with a single element, that can for example be used as method invocation args</summary>
	public static object[] SingleTempArgsArray(object arg) {
		_tempArgs[0] = arg;
		return _tempArgs;
	}

	///----------------------------------------------------------------------------------------------

	
	public static Type RTReflectedOrDeclaredType(this MemberInfo member) {
		return member.ReflectedType != null ? member.ReflectedType : member.DeclaringType;
	}
	
	public static bool RTIsAssignableFrom(this Type type, Type other) {
		return type.IsAssignableFrom(other);
	}
	
	public static bool RTIsAssignableTo(this Type type, Type other) {
		return other.RTIsAssignableFrom(type);
	}

	public static bool RTIsAbstract(this Type type) {
		return type.IsAbstract;
	}

	public static bool RTIsValueType(this Type type) {
		return type.IsValueType;
	}

	public static bool RTIsArray(this Type type) {
		return type.IsArray;
	}

	public static bool RTIsInterface(this Type type) {
		return type.IsInterface;
	}

	public static bool RTIsSubclassOf(this Type type, Type other) {
		return type.IsSubclassOf(other);
	}
	
	public static Type RTMakeGenericType(this Type type, params Type[] typeArgs) {
		return type.MakeGenericType(typeArgs);
	}
	
	///----------------------------------------------------------------------------------------------

    public static ConstructorInfo RTGetDefaultConstructor(this Type type) {
        var ctors = type.RTGetConstructors();
        for ( var i = 0; i < ctors.Length; i++ ) {
            if ( ctors[i].GetParameters().Length == 0 ) {
                return ctors[i];
            }
        }
        return null;
    }

    public static ConstructorInfo RTGetConstructor(this Type type, Type[] paramTypes) {
        var ctors = type.RTGetConstructors();
        for ( var i = 0; i < ctors.Length; i++ ) {
            var ctor = ctors[i];
            var parameters = ctor.GetParameters();
            if ( parameters.Length != paramTypes.Length ) {
                continue;
            }
            var sequenceEquals = true;
            for ( var j = 0; j < parameters.Length; j++ ) {
                if ( parameters[j].ParameterType != paramTypes[j] ) {
                    sequenceEquals = false;
                    break;
                }
            }
            if ( sequenceEquals ) {
                return ctor;
            }
        }
        return null;
    }

    ///----------------------------------------------------------------------------------------------

    //Utility used bellow
    private static bool MemberResolvedFromDeserializeAttribute(MemberInfo member, string targetName) {
        var att = member.RTGetAttribute<Serialization.DeserializeFromAttribute>(true);
        return att != null && att.previousTypeFullName == targetName;
    }

    public static MethodInfo RTGetMethod(this Type type, string name) {
        var methods = type.RTGetMethods();
        for ( var i = 0; i < methods.Length; i++ ) {
            var m = methods[i];
            if ( m.Name == name || MemberResolvedFromDeserializeAttribute(m, name) ) {
                return m;
            }
        }
        Debug.LogError(string.Format("Method with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()));
        return null;
    }

    public static MethodInfo RTGetMethod(this Type type, string name, Type[] paramTypes, Type returnType = null, Type[] genericArgumentTypes = null) {
        var methods = type.RTGetMethods();
        for ( var i = 0; i < methods.Length; i++ ) {
            var m = methods[i];

            if ( m.Name == name || MemberResolvedFromDeserializeAttribute(m, name) ) {

                if ( genericArgumentTypes != null && !m.IsGenericMethod ) {
                    continue;
                }

                var parameters = m.GetParameters();
                if ( parameters.Length != paramTypes.Length ) {
                    continue;
                }

                if ( genericArgumentTypes != null ) {
                    m = m.MakeGenericMethod(genericArgumentTypes);
                    parameters = m.GetParameters();
                }

                if ( returnType != null && m.ReturnType != returnType ) {
                    continue;
                }

                var sequenceEquals = true;
                for ( var j = 0; j < parameters.Length; j++ ) {
                    if ( parameters[j].ParameterType != paramTypes[j] ) {
                        sequenceEquals = false;
                        break;
                    }
                }
                if ( sequenceEquals ) {
                    return m;
                }
            }
        }
        Debug.LogError(string.Format("Method with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()));
        return null;
    }

    public static FieldInfo RTGetField(this Type type, string name, bool includePrivateBase = false) {
        var current = type;
        while ( current != null ) {

            var fields = current.RTGetFields();
            for ( var i = 0; i < fields.Length; i++ ) {
                var f = fields[i];
                if ( f.Name == name || MemberResolvedFromDeserializeAttribute(f, name) ) {
                    return f;
                }
            }

            if ( !includePrivateBase ) {
                break;
            }

            current = current.BaseType;
        }

		Debug.LogError(string.Format("Field with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()));
        return null;
    }

    public static PropertyInfo RTGetProperty(this Type type, string name) {
        var props = type.RTGetProperties();
        for ( var i = 0; i < props.Length; i++ ) {
            var p = props[i];
            if ( p.Name == name || MemberResolvedFromDeserializeAttribute(p, name) ) {
                return p;
            }
        }
		Debug.LogError(string.Format("Property with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()));
        return null;
    }

    ///<summary>returns either field or property member info </summary>
    public static MemberInfo RTGetFieldOrProp(this Type type, string name) {
        var fields = type.RTGetFields();
        for ( var i = 0; i < fields.Length; i++ ) {
            var f = fields[i];
            if ( f.Name == name || MemberResolvedFromDeserializeAttribute(f, name) ) {
                return f;
            }
        }
        var props = type.RTGetProperties();
        for ( var i = 0; i < props.Length; i++ ) {
            var p = props[i];
            if ( p.Name == name || MemberResolvedFromDeserializeAttribute(p, name) ) {
                return p;
            }
        }
		Debug.LogError(string.Format("Field Or Property with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()));
        return null;
    }

    public static EventInfo RTGetEvent(this Type type, string name) {
        var events = type.RTGetEvents();
        for ( var i = 0; i < events.Length; i++ ) {
            var e = events[i];
            if ( e.Name == name || MemberResolvedFromDeserializeAttribute(e, name) ) {
                return e;
            }
        }
        Debug.LogError(string.Format("Event with name '{0}' on type '{1}', could not be resolved.", name, type.FriendlyName()));
        return null;
    }

    ///<summary>return field or property value</summary>
    public static object RTGetFieldOrPropValue(this MemberInfo member, object instance, int index = -1) {
        if ( member is FieldInfo ) { return ( member as FieldInfo ).GetValue(instance); }
        if ( member is PropertyInfo ) { return ( member as PropertyInfo ).GetValue(instance, index == -1 ? null : SingleTempArgsArray(index)); }
        return null;
    }

    //set field or property value
    public static void RTSetFieldOrPropValue(this MemberInfo member, object instance, object value, int index = -1) {
        if ( member is FieldInfo ) { ( member as FieldInfo ).SetValue(instance, value); }
        if ( member is PropertyInfo ) { ( member as PropertyInfo ).SetValue(instance, value, index == -1 ? null : SingleTempArgsArray(index)); }
    }

    ///----------------------------------------------------------------------------------------------

	
	///----------------------------------------------------------------------------------------------

	private static Dictionary<Type, ConstructorInfo[]> _typeConstructors;
	private static Dictionary<Type, MethodInfo[]> _typeMethods;

	public static ConstructorInfo[] RTGetConstructors(this Type type) {
		ConstructorInfo[] constructors;
		if ( !_typeConstructors.TryGetValue(type, out constructors) ) {
			constructors = type.GetConstructors(FLAGS_ALL);
			_typeConstructors[type] = constructors;
		}

		return constructors;
	}

	public static MethodInfo[] RTGetMethods(this Type type) {
		MethodInfo[] methods;
		if ( !_typeMethods.TryGetValue(type, out methods) ) {
			methods = type.GetMethods(FLAGS_ALL);
			_typeMethods[type] = methods;
		}

		return methods;
	}
	
	private static Dictionary<Type, FieldInfo[]> _typeFields;

	public static FieldInfo[] RTGetFields(this Type type) {
		FieldInfo[] fields;
		if ( !_typeFields.TryGetValue(type, out fields) ) {
			fields = type.GetFields(FLAGS_ALL);
			_typeFields[type] = fields;
		}

		return fields;
	}
	
	private static Dictionary<Type, PropertyInfo[]> _typeProperties;

	public static PropertyInfo[] RTGetProperties(this Type type) {
		PropertyInfo[] properties;
		if ( !_typeProperties.TryGetValue(type, out properties) ) {
			properties = type.GetProperties(FLAGS_ALL);
			_typeProperties[type] = properties;
		}

		return properties;
	}
	
	private static Dictionary<Type, EventInfo[]> _typeEvents;

	public static EventInfo[] RTGetEvents(this Type type) {
		EventInfo[] events;
		if ( !_typeEvents.TryGetValue(type, out events) ) {
			events = type.GetEvents(FLAGS_ALL);
			_typeEvents[type] = events;
		}

		return events;
	}

	///----------------------------------------------------------------------------------------------
	
	
	private static Dictionary<Type, Type[]> _genericArgsTypeCache;
	private static Dictionary<MethodInfo, Type[]> _genericArgsMathodCache;
	
	public static Type[] RTGetGenericArguments(this Type type) {
		Type[] result = null;
		if ( _genericArgsTypeCache.TryGetValue(type, out result) ) {
			return result;
		}
		return _genericArgsTypeCache[type] = result = type.GetGenericArguments();
	}

	public static Type[] RTGetGenericArguments(this MethodInfo method) {
		Type[] result = null;
		if ( _genericArgsMathodCache.TryGetValue(method, out result) ) {
			return result;
		}
		return _genericArgsMathodCache[method] = result = method.GetGenericArguments();
	}
	
	///<summary>Is attribute defined?</summary>
	public static bool RTIsDefined<T>(this Type type, bool inherited) where T : Attribute { return type.RTIsDefined(typeof(T), inherited); }
	public static bool RTIsDefined(this Type type, Type attributeType, bool inherited) {
		return type.IsDefined(attributeType, inherited);
		// return inherited ? type.RTGetAttribute(attributeType, inherited) != null : type.IsDefined(attributeType, false);
	}
	
	///<summary>Get attribute from type of type T</summary>
    public static T RTGetAttribute<T>(this Type type, bool inherited) where T : Attribute { return (T)type.RTGetAttribute(typeof(T), inherited); }
    public static Attribute RTGetAttribute(this Type type, Type attributeType, bool inherited) {
        return type.GetCustomAttribute(attributeType, inherited);
        // object[] attributes = RTGetAllAttributes(type);
        // if ( attributes != null ) {
        //     for ( var i = 0; i < attributes.Length; i++ ) {
        //         var att = (Attribute)attributes[i];
        //         var attType = att.GetType();
        //         if ( attType.RTIsAssignableTo(attributeType) ) {
        //             if ( inherited || type.IsDefined(attType, false) ) {
        //                 return att;
        //             }
        //         }
        //     }
        // }
        // return null;
    }
	
	///<summary>Is the field read only?</summary>
	public static bool IsReadOnly(this FieldInfo field) {
		return field.IsInitOnly || field.IsLiteral;
	}

	///<summary>Is the field a Constant?</summary>
	public static bool IsConstant(this FieldInfo field) {
		return field.IsReadOnly() && field.IsStatic;
	}

	///<summary>Quicky to get if an event info is static.</summary>
	public static bool IsStatic(this EventInfo info) {
		var m = info.GetAddMethod();
		return m != null ? m.IsStatic : false;
	}

	///<summary>Quicky to get if a property info is static.</summary>
	public static bool IsStatic(this PropertyInfo info) {
		var m = info.GetGetMethod();
		return m != null ? m.IsStatic : false;
	}

	///<summary>Is the parameter provided a params array?</summary>
	public static bool IsParams(this ParameterInfo parameter, ParameterInfo[] parameters) {
		return parameter.Position == parameters.Length - 1 && parameter.IsDefined(typeof(ParamArrayAttribute), false);
	}
	
    ///------------------------------------------

	private static Dictionary<MemberInfo, object[]> _memberAttributes;
	
    ///<summary>Get all attributes from member including inherited</summary>
    public static object[] RTGetAllAttributes(this MemberInfo member) {
        object[] attributes;
        if ( !_memberAttributes.TryGetValue(member, out attributes) ) {
            attributes = member.GetCustomAttributes(true);
            _memberAttributes[member] = attributes;
        }
        return attributes;
    }

    ///<summary>Is attribute defined?</summary>
    public static bool RTIsDefined<T>(this MemberInfo member, bool inherited) where T : Attribute { return member.RTIsDefined(typeof(T), inherited); }
    public static bool RTIsDefined(this MemberInfo member, Type attributeType, bool inherited) {
        return member.IsDefined(attributeType, inherited);
        // return inherited ? member.RTGetAttribute(attributeType, inherited) != null : member.IsDefined(attributeType, false);
    }

    ///<summary>Get attribute from member of type T</summary>
    public static T RTGetAttribute<T>(this MemberInfo member, bool inherited) where T : Attribute { return (T)member.RTGetAttribute(typeof(T), inherited); }
    public static Attribute RTGetAttribute(this MemberInfo member, Type attributeType, bool inherited) {
        return member.GetCustomAttribute(attributeType, inherited);
        // object[] attributes = RTGetAllAttributes(member);
        // for ( var i = 0; i < attributes.Length; i++ ) {
        //     var att = (Attribute)attributes[i];
        //     var attType = att.GetType();
        //     if ( attType.RTIsAssignableTo(attributeType) ) {
        //         if ( inherited || member.IsDefined(attType, false) ) {
        //             return att;
        //         }
        //     }
        // }
        // return null;
    }

    ///<summary>Get all attributes of type T recursively up the type hierarchy</summary>
    public static IEnumerable<T> RTGetAttributesRecursive<T>(this Type type) where T : Attribute {
        var current = type;
        while ( current != null ) {
            var att = current.RTGetAttribute<T>(false);
            if ( att != null ) {
                yield return att;
            }
            current = current.BaseType;
        }
    }

	
	public static bool RTIsGenericParameter(this Type type) {
		return type.IsGenericParameter;
	}
	
	public static bool RTIsGenericType(this Type type) {
		return type.IsGenericType;
	}

	///----------------------------------------------------------------------------------------------
	
	
	///----------------------------------------------------------------------------------------------

	private static Assembly[] _loadedAssemblies;
	private static Type[] _allTypes;
	private static Dictionary<string, Type> _typesMap;
	
    private static Assembly[] loadedAssemblies {
        get { return _loadedAssemblies != null ? _loadedAssemblies : _loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies(); }
    }

    //Alternative to Type.GetType to work with FullName instead of AssemblyQualifiedName when looking up a type by string
    //This also handles Generics and their arguments, assembly changes and namespace changes to some extend.
    public static Type GetType(string typeFullName) { return GetType(typeFullName, false, null); }
    public static Type GetType(string typeFullName, Type fallbackAssignable) { return GetType(typeFullName, true, fallbackAssignable); }
    public static Type GetType(string typeFullName, bool fallbackNoNamespace = false, Type fallbackAssignable = null) {

        if ( string.IsNullOrEmpty(typeFullName) ) {
            return null;
        }

        Type type = null;
        if ( _typesMap.TryGetValue(typeFullName, out type) ) {
            return type;
        }

        //direct look up
        type = GetTypeDirect(typeFullName);
        if ( type != null ) {
            return _typesMap[typeFullName] = type;
        }

        //handle generics now
        type = TryResolveGenericType(typeFullName, fallbackNoNamespace, fallbackAssignable);
        if ( type != null ) {
            // Logger.LogWarning(string.Format("Type with name '{0}' was resolved using a fallback resolution (Generics).", typeFullName), "Type Request");
            return _typesMap[typeFullName] = type;
        }

        // TODO: make use of DeserializeFromAttribute
        // type = TryResolveDeserializeFromAttribute(typeFullName);
        // if ( type != null ) {
        //     // Logger.LogWarning(string.Format("Type with name '{0}' was resolved using a fallback resolution (DeserializeFromAttribute).", typeFullName), "Type Request");
        //     return _typesMap[typeFullName] = type;
        // }

        //get type regardless namespace
        if ( fallbackNoNamespace ) {
            type = TryResolveWithoutNamespace(typeFullName, fallbackAssignable);
            if ( type != null ) {
                // Logger.LogWarning(string.Format("Type with name '{0}' was resolved using a fallback resolution (NoNamespace).", typeFullName), "Type Request");
                return _typesMap[typeFullName] = type;
            }
        }

        Debug.LogError(string.Format("Type with name '{0}' could not be resolved.", typeFullName));
        return _typesMap[typeFullName] = null;
    }

    //direct type look up with it's FullName
    static Type GetTypeDirect(string typeFullName) {
        var type = Type.GetType(typeFullName);
        if ( type != null ) {
            return type;
        }

        for ( var i = 0; i < loadedAssemblies.Length; i++ ) {
            var asm = loadedAssemblies[i];
            try { type = asm.GetType(typeFullName); }
            catch { continue; }
            if ( type != null ) {
                return type;
            }
        }

        return null;
    }

    //Resolve generic types by their .FullName or .ToString
    //Remark: a generic's type .FullName returns a string where it's arguments only are instead printed as AssemblyQualifiedName.
    static Type TryResolveGenericType(string typeFullName, bool fallbackNoNamespace = false, Type fallbackAssignable = null) {

        //ensure that it is a generic type implementation, not a definition
        if ( typeFullName.Contains('`') == false || typeFullName.Contains('[') == false ) {
            return null;
        }

        try //big try/catch block cause maybe there is a bug. Hopefully not.
        {
            var quoteIndex = typeFullName.IndexOf('`');
            var genericTypeDefName = typeFullName.Substring(0, quoteIndex + 2);
            var genericTypeDef = GetType(genericTypeDefName, fallbackNoNamespace, fallbackAssignable);
            if ( genericTypeDef == null ) {
                return null;
            }

            int argCount = Convert.ToInt32(typeFullName.Substring(quoteIndex + 1, 1));
            var content = typeFullName.Substring(quoteIndex + 2, typeFullName.Length - quoteIndex - 2);
            string[] split = null;
            if ( content.StartsWith("[[") ) { //this means that assembly qualified name is contained. Name was generated with FullName.
                var startIndex = typeFullName.IndexOf("[[") + 2;
                var endIndex = typeFullName.LastIndexOf("]]");
                content = typeFullName.Substring(startIndex, endIndex - startIndex);
                split = content.Split(new string[] { "],[" }, argCount, StringSplitOptions.RemoveEmptyEntries);
            } else { //this means that the name was generated with type.ToString().
                var startIndex = typeFullName.IndexOf('[') + 1;
                var endIndex = typeFullName.LastIndexOf(']');
                content = typeFullName.Substring(startIndex, endIndex - startIndex);
                split = content.Split(new char[] { ',' }, argCount, StringSplitOptions.RemoveEmptyEntries);
            }

            var argTypes = new Type[argCount];
            for ( var i = 0; i < split.Length; i++ ) {
                var subName = split[i];
                if ( !subName.Contains('`') && subName.Contains(',') ) { //remove assembly info since we work with FullName, but only if it's not yet another generic.
                    subName = subName.Substring(0, subName.IndexOf(','));
                }

                var argType = GetType(subName, true /*fallback no namespace*/);
                if ( argType == null ) {
                    return null;
                }
                argTypes[i] = argType;
            }

            return genericTypeDef.RTMakeGenericType(argTypes);
        }

        catch ( Exception e ) {
            Debug.LogError(e.Message); // Type Request Bug. Please report. :-(
            return null;
        }
    }

    //uterly slow, but only happens when we have a null type
    // static Type TryResolveDeserializeFromAttribute(string typeName) {
    //     var allTypes = GetAllTypes(true);
    //     for ( var i = 0; i < allTypes.Length; i++ ) {
    //         var t = allTypes[i];
    //         var att = t.GetCustomAttribute(typeof(Serialization.DeserializeFromAttribute), false) as Serialization.DeserializeFromAttribute;
    //         if ( att != null && att.previousTypeFullName == typeName ) {
    //             return t;
    //         }
    //     }
    //     return null;
    // }

    //fallback type look up with it's FullName. This is slow.
    static Type TryResolveWithoutNamespace(string typeName, Type fallbackAssignable = null) {

        //dont handle generic implementations this way (still handles definitions though).
        if ( typeName.Contains('`') && typeName.Contains('[') ) {
            return null;
        }

        //remove assembly info if any
        if ( typeName.Contains(',') ) {
            typeName = typeName.Substring(0, typeName.IndexOf(','));
        }

        //ensure strip namespace
        if ( typeName.Contains('.') ) {
            var dotIndex = typeName.LastIndexOf('.') + 1;
            typeName = typeName.Substring(dotIndex, typeName.Length - dotIndex);
        }

        //check all types
        var allTypes = GetAllTypes(true);
        for ( var i = 0; i < allTypes.Length; i++ ) {
            var t = allTypes[i];
            if ( t.Name == typeName && ( fallbackAssignable == null || fallbackAssignable.RTIsAssignableFrom(t) ) ) {
                return t;
            }
        }
        return null;
    }
	
	///<summary>Get every single type in loaded assemblies</summary>
	public static Type[] GetAllTypes(bool includeObsolete) {
		if ( _allTypes != null ) {
			return _allTypes;
		}

		var result = new List<Type>();
		for ( var i = 0; i < loadedAssemblies.Length; i++ ) {
			var asm = loadedAssemblies[i];
			try { result.AddRange(asm.GetExportedTypes().Where(t => includeObsolete == true || !t.RTIsDefined<System.ObsoleteAttribute>(false))); }
			catch { continue; }
		}
		return _allTypes = result.OrderBy(t => t.Namespace).ThenBy(t => t.FriendlyName()).ToArray();
	}

    ///----------------------------------------------------------------------------------------------
	
	private static Dictionary<Type, string> _typeFriendlyName;
	private static Dictionary<Type, string> _typeFriendlyNameCompileSafe;
	
	///<summary>Get a friendly name for the type</summary>
    public static string FriendlyName(this Type t, bool compileSafe = false) {

        if ( t == null ) {
            return null;
        }

        if ( !compileSafe && t.IsByRef ) {
            t = t.GetElementType();
        }

        if ( !compileSafe && t == typeof(UnityEngine.Object) ) {
            return "UnityObject";
        }

        string s;
        if ( !compileSafe && _typeFriendlyName.TryGetValue(t, out s) ) {
            return s;
        }

        if ( compileSafe && _typeFriendlyNameCompileSafe.TryGetValue(t, out s) ) {
            return s;
        }

        s = compileSafe ? t.FullName : t.Name;
        if ( !compileSafe ) {
            if ( s == "Single" ) { s = "Float"; }
            if ( s == "Single[]" ) { s = "Float[]"; }
            if ( s == "Int32" ) { s = "Integer"; }
            if ( s == "Int32[]" ) { s = "Integer[]"; }
        }

        if ( t.RTIsGenericParameter() ) {
            s = "T";
        }

        if ( t.RTIsGenericType() ) {
            s = compileSafe && !string.IsNullOrEmpty(t.Namespace) ? t.Namespace + "." + t.Name : t.Name;
            var args = t.RTGetGenericArguments();
            if ( args.Length != 0 ) {

                s = s.Replace("`" + args.Length.ToString(), "");

                s += compileSafe ? "<" : " (";
                for ( var i = 0; i < args.Length; i++ ) {
                    s += ( i == 0 ? "" : ", " ) + args[i].FriendlyName(compileSafe);
                }
                s += compileSafe ? ">" : ")";
            }
        }

        if ( compileSafe ) {
            return _typeFriendlyNameCompileSafe[t] = s;
        }
        return _typeFriendlyName[t] = s;
    }

    ///<summary>Get a friendly name for member info</summary>
    public static string FriendlyName(this MemberInfo info) {
        if ( info == null ) { return null; }
        if ( info is Type ) { return FriendlyName((Type)info); }
        var type = info.ReflectedType.FriendlyName();
        return type + '.' + info.Name;
    }
	
	
	///<summary>Returns the first argument parameter constraint. If no constraint, typeof(object) is returned.</summary>
	public static Type GetFirstGenericParameterConstraintType(this Type type) {
		if ( type == null || !type.RTIsGenericType() ) { return null; }
		type = type.GetGenericTypeDefinition();
		var arg1 = type.RTGetGenericArguments().First();
		var c1 = arg1.GetGenericParameterConstraints().FirstOrDefault();
		return c1 != null ? c1 : typeof(object);
	}

	///<summary>Returns the first argument parameter constraint. If no constraint, typeof(object) is returned.</summary>
	public static Type GetFirstGenericParameterConstraintType(this MethodInfo method) {
		if ( method == null || !method.IsGenericMethod ) { return null; }
		method = method.GetGenericMethodDefinition();
		var arg1 = method.RTGetGenericArguments().First();
		var c1 = arg1.GetGenericParameterConstraints().FirstOrDefault();
		return c1 != null ? c1 : typeof(object);
	}

}

}
