using System.Reflection;


namespace Bang.Unity.Utilities {

	public static class SystemObjectExtensions {
		
		public static T MemberwiseClone< T >( this T obj ) {
			if ( obj is null ) {
				return obj;
			}
			
			return ( T )obj.GetType().GetMethod( "MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic )
					  .Invoke( obj, null );
		}
		
	}

}
