using System;
using Bang.Components;
// using Unity.Collections;


namespace Bang.Unity {

	public readonly struct EntityNameComponent : IComponent, IEquatable< EntityNameComponent > {
		
		// public readonly global::Unity.Collections.FixedString64Bytes Value;
		public readonly string Value;

		public EntityNameComponent( string value ) {
			Value = value;
		}
		
		public override int GetHashCode() {
			return Value.GetHashCode();
		}

		public override bool Equals( object obj ) {
			if ( obj is not EntityNameComponent entityName ) return false;
			return entityName.Equals( this );
		}

		public bool Equals( EntityNameComponent other ) {
			if ( other.Value is null ) {
				return Value is null;
			} 
			return other.Value.Equals( Value );
		}

		public override string ToString() {
			// var str = Value;
			// return str.ConvertToString();
			return Value;
		}

	}
	
}
