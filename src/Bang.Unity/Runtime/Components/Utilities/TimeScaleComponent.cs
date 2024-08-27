using Bang.Components;


namespace Bang.Unity.Components {

	public readonly struct TimeScaleComponent : IComponent {
		
		public readonly float Value;

		public TimeScaleComponent( float scale ) {
			Value = scale;
		}

	}

}
