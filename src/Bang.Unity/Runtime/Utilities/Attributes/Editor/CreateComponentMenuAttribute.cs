using System;


namespace Bang.Unity {

	[AttributeUsage( AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
	public sealed class CreateComponentMenuAttribute : Attribute {
		
		/// <summary>
		///   <para>The display name for this type shown in the Assets/Create menu.</para>
		/// </summary>
		public string MenuName { get; set; }

		public CreateComponentMenuAttribute( string menuName ) {
			MenuName = menuName;
		}
	}

}
