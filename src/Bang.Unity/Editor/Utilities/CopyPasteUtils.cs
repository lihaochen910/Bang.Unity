using System;
using System.Collections.Generic;
using Bang.Unity.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Bang.Unity.Editor.Utilities {

    public static class CopyPasteUtils {
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public static object SourceObject { get; private set; }
        public static Type SourceType { get; private set; }
        public static string SourceObjectJson { get; private set; }
        private static List< UnityEngine.Object > SourceObjectJsonReferences { get; set; }


        public static object SourceObjectCopy {
            get {
                if ( SourceObject == null ) {
                    return null;
                }

                if ( SourceObjectJsonReferences is null ) {
                    SourceObjectJsonReferences = new List< Object >();
                }
                SourceObjectJsonReferences.Clear();

                SourceObjectJson = JSONSerializer.Serialize( SourceType, SourceObject, SourceObjectJsonReferences, true );
                object newInstance = JSONSerializer.Deserialize( SourceType, SourceObjectJson, SourceObjectJsonReferences );

                EventSoftPaste?.Invoke();

                return newInstance;
            }
        }


        // EVENTS: --------------------------------------------------------------------------------

        public static event Action EventSoftCopy;
        public static event Action EventSoftPaste;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void CopyToClipboard( string text ) {
            var textEditor = new TextEditor { text = text };
            textEditor.SelectAll();
            textEditor.Copy();
        }

        public static void SoftCopy( object source, Type baseType ) {
            SourceType = baseType;
            SourceObject = source;

            EventSoftCopy?.Invoke();
        }

        public static bool CanSoftPaste( Type baseType ) {
            if ( SourceObject == null ) return false;
            return SourceType != null && baseType.IsAssignableFrom( SourceType );
        }

        public static void Duplicate(object source)
        {
            if ( source == null ) {
                return;
            }
            
            if ( SourceObjectJsonReferences is null ) {
                SourceObjectJsonReferences = new List< Object >();
            }
            SourceObjectJsonReferences.Clear();
            
            SourceType = source.GetType();
            SourceObjectJson = JSONSerializer.Serialize( source.GetType(), source, SourceObjectJsonReferences, true );

            // object newInstance = Activator.CreateInstance(source.GetType());
            // EditorJsonUtility.FromJsonOverwrite(jsonSource, newInstance);
            //
            // target.SetManaged(newInstance);
        }

        public static void ClearCopy() {
            SourceType = null;
            SourceObject = null;
            SourceObjectJson = null;
            SourceObjectJsonReferences = null;
        }

    }

}
