using System;


namespace Bang.Unity {

	public class CustomNameAttribute : Attribute {
		
		public string Name;

		public CustomNameAttribute( string name ) {
			Name = name;
		}
	}
	
}
