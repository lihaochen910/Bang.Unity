using Bang.Components;


namespace Bang.Unity.Components {

	public enum RemoveStyle : byte {
		Destroy,
		Deactivate,
		RemoveComponents,
		None
	}


	[RuntimeOnly, DoNotPersistOnSave]
	public readonly struct DestroyAtTimeComponent : IComponent {
	
		public readonly RemoveStyle Style;
		public readonly float TimeToDestroy;

		// /// <summary>
		// /// Destroy at the end of the frame
		// /// </summary>
		// public DestroyAtTimeComponent() {
		// 	TimeToDestroy = -1;
		// }

		public DestroyAtTimeComponent( float timeToDestroy ) {
			Style = RemoveStyle.Destroy;
			TimeToDestroy = timeToDestroy;
		}

		public DestroyAtTimeComponent( RemoveStyle style, float timeToDestroy ) {
			Style = style;
			TimeToDestroy = timeToDestroy;
		}
	}

}
