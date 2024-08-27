using Bang.Components;


namespace Bang.Unity.Components {

	public readonly struct InteractOnButtonPressComponent : IComponent {
	
		public readonly int Priority;
		
		public InteractOnButtonPressComponent( int priority ) {
			Priority = priority;
		}
	
	}

}
