using System;


namespace Bang.Unity.Utilities {

    [Serializable]
    public readonly struct IdString : IEquatable< IdString > {

        public static readonly IdString Empty = new ( string.Empty );

        public readonly string String;
        public readonly int Hash;

        public IdString( string value ) {
            String = value;
            Hash = value.GetHashCode();
        }

        public override int GetHashCode() => Hash;

        public bool Equals( IdString other ) => Hash == other.Hash;

        public override bool Equals( object other ) => other is IdString otherIdString && Equals( otherIdString );

        public override string ToString() => String;

        // OPERATORS: -----------------------------------------------------------------------------

        public static bool operator ==( IdString left, IdString right ) {
            return left.Equals( right );
        }

        public static bool operator !=( IdString left, IdString right ) {
            return !left.Equals( right );
        }

        public static implicit operator string( IdString id ) {
            return id.String;
        }
        
        public static implicit operator IdString( string id ) {
            return new IdString( id );
        }

    }


    // internal static class IdStringPool {
    //
    //     private readonly static Dictionary< int, string > _pool = new ();
    //     
    // }
    
}
