using System;


namespace Bang.Unity {

	/// <summary>
	/// Attribute for string fields that are actually targets of the entity.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public class TargetAttribute : Attribute {}
	
}
