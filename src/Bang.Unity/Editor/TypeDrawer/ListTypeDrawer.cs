using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using UnityEditor;


namespace Bang.Unity.Editor {

    public class ListTypeDrawer : ITypeDrawer {
        
        public bool CanHandlesType( Type type ) =>
            type.GetInterfaces().Contains( typeof( IList ) ) &&
            type.GetGenericTypeDefinition() != typeof( ImmutableArray<> );

        public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
            var list = ( IList )value;
            var elementType = memberType.GetGenericArguments()[ 0 ];
            if ( memberType.GetGenericTypeDefinition() == typeof( ImmutableArray<> ) ) {
                if ( ( bool )memberType.GetProperty( "IsDefault" ).GetValue( list ) ) {
                    list = ( IList )memberType.GetField( "Empty" ).GetValue( null );
                }
            }

            if ( list.Count == 0 )
                list = DrawAddElement( list, memberName, elementType );
            else
                EditorGUILayout.LabelField( memberName );

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent + 1;
            Func< IList > editAction = null;
            for ( var i = 0; i < list.Count; i++ ) {
                var localIndex = i;
                EditorGUILayout.BeginHorizontal();
                {
                    EntityDrawer.DrawObjectMember( elementType, $"{memberName}[{localIndex}]", list[ localIndex ],
                        target, ( _, newValue ) => list[ localIndex ] = newValue );

                    var action = DrawEditActions( list, elementType, localIndex );
                    if ( action != null ) editAction = action;
                }
                EditorGUILayout.EndHorizontal();
            }

            if ( editAction != null ) list = editAction();

            EditorGUI.indentLevel = indent;

            return list;
        }

        static Func< IList > DrawEditActions( IList list, Type elementType, int index ) {
            if ( EntityDrawer.MiniButtonLeft( "↑" ) )
                if ( index > 0 )
                    return () => {
                        var otherIndex = index - 1;
                        var other = list[ otherIndex ];
                        list[ otherIndex ] = list[ index ];
                        list[ index ] = other;
                        return list;
                    };

            if ( EntityDrawer.MiniButtonMid( "↓" ) )
                if ( index < list.Count - 1 )
                    return () => {
                        var otherIndex = index + 1;
                        var other = list[ otherIndex ];
                        list[ otherIndex ] = list[ index ];
                        list[ index ] = other;
                        return list;
                    };

            if ( EntityDrawer.MiniButtonMid( "+" ) ) {
                if ( EntityDrawer.CreateDefault( elementType, out var defaultValue ) ) {
                    var insertAt = index + 1;
                    return () => {
                        list.Insert( insertAt, defaultValue );
                        return list;
                    };
                }
            }

            if ( EntityDrawer.MiniButtonRight( "-" ) ) {
                var removeAt = index;
                return () => {
                    list.RemoveAt( removeAt );
                    return list;
                };
            }

            return null;
        }

        IList DrawAddElement( IList list, string memberName, Type elementType ) {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField( memberName, "empty" );
                if ( EntityDrawer.MiniButton( $"add {elementType.ToCompilableString().TypeName()}" ) )
                    if ( EntityDrawer.CreateDefault( elementType, out var defaultValue ) )
                        list.Add( defaultValue );
            }
            EditorGUILayout.EndHorizontal();

            return list;
        }

    }

}
