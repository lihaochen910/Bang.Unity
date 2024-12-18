using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;


namespace Bang.Unity.Utilities {

	public abstract class AttributeExtensions {
		
		// public static bool IsDefined(EditorMember element, Type attributeType) =>
		// 	Attribute.IsDefined(element.Member, attributeType);

		public static bool TryGetAttribute< T >( MemberInfo member, [NotNullWhen( true )] out T? attribute )
			where T : Attribute {
			if ( Attribute.IsDefined( member, typeof( T ) ) &&
				 Attribute.GetCustomAttribute( member, typeof( T ) ) is T customAttribute ) {
				attribute = customAttribute;
				return true;
			}

			attribute = default;
			return false;
		}

	}

}
