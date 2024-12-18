using System;
using UnityEngine;
using Object = System.Object;
using UnityEngine.Serialization;


namespace Bang.Unity.Utilities {

    [Serializable]
    public class SerializableSystemType
    {
        // /// <summary>
        // /// The max depth of serialization in the system. Matches Unity's max depth.
        // /// </summary>
        // public const int MaxSerializationDepth = 7;
        
        [SerializeField]
        private string _name;
        public string Name => _name;
        
        // public string NiceName
        // {
        //     get { return Util.PrettifyTypeName(Name); }
        // }

        [SerializeField]
        private string _assemblyQualifiedName;
        public string AssemblyQualifiedName => _assemblyQualifiedName;

        
        private Type _systemType;
        public Type SystemType {
            get {
                if ( _systemType == null ) {
                    GetSystemType();
                }

                return _systemType;
            }
        }


        private void GetSystemType() {
            _systemType = string.IsNullOrEmpty( _assemblyQualifiedName ) ? null : Type.GetType( _assemblyQualifiedName );
        }

        public SerializableSystemType( Type systemType ) {
            SetType( systemType );
        }

        public void SetType( Type systemType ) {
            // if ( systemType == null ) {
            //     throw new ArgumentNullException( nameof( systemType ) );
            // }
            _systemType = systemType;
            _name = systemType?.Name;
            _assemblyQualifiedName = systemType?.AssemblyQualifiedName;
        }

        public override bool Equals( Object obj ) {
            if ( obj is not SerializableSystemType temp ) {
                return false;
            }

            return Equals( temp );
        }

        public bool Equals( SerializableSystemType @object ) {
            if ( @object == null ) {
                return false;
            }
            return SystemType.Equals( @object.SystemType );
        }

        public static bool operator ==(SerializableSystemType a, SerializableSystemType b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (a == null || b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(SerializableSystemType a, SerializableSystemType b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return SystemType.GetHashCode();
        }

        public static implicit operator SerializableSystemType( Type type ) {
            return new SerializableSystemType( type );
        }

        public static implicit operator Type( SerializableSystemType type ) {
            return type.SystemType;
        }

    }

}
