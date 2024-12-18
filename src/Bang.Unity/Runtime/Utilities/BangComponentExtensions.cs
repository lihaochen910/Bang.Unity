using Bang.Components;


namespace Bang.Unity.Utilities {

	public static class BangComponentExtensions {
		
		/// <summary>
		/// 是否这个组件是一个flag组件(类型不包含任何Field和Property)
		/// </summary>
		/// <param name="component"></param>
		public static bool IsFlagLikeComponent( this IComponent component ) {
			var componentType = component.GetType();
			var fieldCount = componentType.GetFields().Length;
			var propertyCount = componentType.GetProperties().Length;
			return fieldCount < 1 && propertyCount < 1;
			// return Unsafe.SizeOf( componentType ) < 4;
		}
		
	}

}
