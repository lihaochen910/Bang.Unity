using System;


namespace Bang.Unity.Editor {

	public class DefaultNullableCreator : IDefaultInstanceCreator {

		public bool CanHandlesType( Type type ) {
			return Nullable.GetUnderlyingType(type) != null;
		}

		public object CreateDefault( Type type ) {
			var argType = type.GetGenericArguments()[ 0 ];
			if ( EntityDrawer.CreateDefault( argType, out var defaultValue ) ) {
				return defaultValue;
			}

			return null;
		}
	}

}
