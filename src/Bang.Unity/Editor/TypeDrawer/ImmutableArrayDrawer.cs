using System;
using System.Collections.Immutable;
using UnityEditor;


namespace Bang.Unity.Editor {

	public class ImmutableArrayDrawer : ITypeDrawer {

		public bool CanHandlesType( Type type ) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof( ImmutableArray<> );

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			var elementType = memberType.GetGenericArguments()[0];
			if ( ( bool )memberType.GetProperty( "IsDefault" ).GetValue( value ) ) {
				value = memberType.GetField( "Empty" ).GetValue( null );
			}
			
			var elementCount = ( int )memberType.GetProperty( "Length" ).GetValue( value );
			if ( elementCount == 0 ) {
				value = DrawAddElement( value, memberName, elementType );
			}
			else {
				EditorGUILayout.LabelField( memberName );
			}
			
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = indent + 1;
			Func<object> editAction = null;
			for (var i = 0; i < elementCount; i++)
			{
				var localIndex = i;
				EditorGUILayout.BeginHorizontal();
				{
					var elementValue = memberType.GetMethod( "get_Item" ).Invoke( value, new object[] { localIndex } );
					EntityDrawer.DrawObjectMember(elementType, $"{memberName}[{localIndex}]", elementValue,
						target, (_, newValue) => value = memberType.GetMethod( "SetItem" ).Invoke( value, new [] { localIndex, newValue } ));

					var action = DrawEditActions(value, elementType, localIndex);
					if ( action != null ) {
						editAction = action;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if ( editAction != null ) {
				value = editAction();
			}

			EditorGUI.indentLevel = indent;

			return value;
		}
		
		object DrawAddElement(object array, string memberName, Type elementType)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField(memberName, "empty");
				if ( EntityDrawer.MiniButton( $"add {elementType.ToCompilableString().TypeName()}" ) ) {
					if ( EntityDrawer.CreateDefault( elementType, out var defaultValue ) ) {
						array = array.GetType().GetMethod( "Add" ).Invoke( array, new [] { defaultValue } );
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			return array;
		}
		
		static Func<object> DrawEditActions(object array, Type elementType, int index)
		{
			// ImmutableArray<int> arr = ImmutableArray< int >.Empty;
			// arr.();
			
			
			if (EntityDrawer.MiniButtonLeft("↑"))
				if (index > 0)
					return () =>
					{
						var otherIndex = index - 1;
						var other = array.GetType().GetMethod( "get_Item" ).Invoke( array, new object[] { otherIndex } );
						// array[otherIndex] = array[index];
						array = array.GetType().GetMethod( "SetItem" ).Invoke( array,
							new [] {
								otherIndex,
								array.GetType().GetMethod( "get_Item" ).Invoke( array, new object[] { index } )
							} );
						// array[index] = other;
						array = array.GetType().GetMethod( "SetItem" ).Invoke( array,
							new [] {
								index,
								other
							} );
						return array;
					};

			if ( EntityDrawer.MiniButtonMid( "↓" ) ) {
				var arrayLength = ( int )array.GetType().GetProperty( "Length" ).GetValue( array );
				if ( index < arrayLength - 1 ) {
					return () =>
					{
						var otherIndex = index + 1;
						var other = array.GetType().GetMethod( "get_Item" ).Invoke( array, new object[] { otherIndex } );
						array = array.GetType().GetMethod( "SetItem" ).Invoke( array,
							new [] {
								otherIndex,
								array.GetType().GetMethod( "get_Item" ).Invoke( array, new object[] { index } )
							} );
						array = array.GetType().GetMethod( "SetItem" ).Invoke( array,
							new [] {
								index,
								other
							} );
						return array;
					};
				}
			}

			if (EntityDrawer.MiniButtonMid("+"))
			{
				if (EntityDrawer.CreateDefault(elementType, out var defaultValue))
				{
					var insertAt = index + 1;
					return () =>
					{
						array = array.GetType().GetMethod( "Insert" ).Invoke( array, new [] { insertAt, defaultValue } );
						return array;
					};
				}
			}

			if (EntityDrawer.MiniButtonRight("-"))
			{
				var removeAt = index;
				return () =>
				{
					array = array.GetType().GetMethod( "RemoveAt" ).Invoke( array, new object[] { removeAt } );
					return array;
				};
			}

			return null;
		}
	}

}