using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DigitalRune.Linq;
using DigitalRune.Utility;
using UnityEditor;
using ReflectionHelper = Bang.Unity.Utilities.ReflectionHelper;


namespace Bang.Unity.Editor {

	public class SystemTypeDrawer : ITypeDrawer {

		private static Dictionary< Type, Lazy< Type[] > > CachedQuery = new ();

		public bool CanHandlesType( Type type ) => type == typeof( System.Type ) || type.FullName == "System.RuntimeType";

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

			Type typeToSearch = default;
			
			var fieldInfo = target.GetType().GetField( memberName, bindingFlags );
			if ( fieldInfo != null && fieldInfo.GetAttribute< TypeOfAttribute >() is {} typeOfAttribute ) {
				typeToSearch = typeOfAttribute.Type;
			}
			
			var propertyInfo = target.GetType().GetProperty( memberName, bindingFlags );
			if ( propertyInfo != null && propertyInfo.GetAttribute< TypeOfAttribute >() is {} propertyTypeOfAttribute ) {
				typeToSearch = propertyTypeOfAttribute.Type;
			}

			if ( typeToSearch != null ) {
				if ( !CachedQuery.ContainsKey( typeToSearch ) ) {
					CachedQuery.Add( typeToSearch, new Lazy< Type[] >( () => ReflectionHelper.GetAllImplementationsOf( typeToSearch ).ToArray() ) );
				}

				var selectedIndex = CachedQuery[ typeToSearch ].Value.IndexOf( t => t == ( Type )value );
				var typeNames = CachedQuery[ typeToSearch ].Value
								.Select( t => $"{t.Name} ({t.Namespace})" )
								.ToArray();
				var index = EditorGUILayout.Popup( memberName, selectedIndex, typeNames );
				if ( index >= 0 ) {
					return CachedQuery[ typeToSearch ].Value[ index ];
				}
			}
			else {
				EditorGUILayout.LabelField( $"{memberName} need the TypeOfAttribute." );
			}

			return null;
		}

		public static Type[] GetAllImplementationsOfCached( Type type ) {
			if ( !CachedQuery.ContainsKey( type ) ) {
				CachedQuery.Add( type, new Lazy< Type[] >( () => ReflectionHelper.GetAllImplementationsOf( type ).ToArray() ) );
			}

			return CachedQuery[ type ].Value;
		}
	}

}
