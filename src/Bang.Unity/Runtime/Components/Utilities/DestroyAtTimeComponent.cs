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


namespace Bang.Unity {

	public static class DestroyAtTimeEntityExtensions {

		public static void DestroyNextFrame(this global::Bang.Entities.Entity e)
		{
			e.AddOrReplaceComponent(new global::Bang.Unity.Components.DestroyAtTimeComponent(Bang.Unity.Components.RemoveStyle.Destroy, -1f), global::Bang.Entities.BangUnityComponentTypes.DestroyAtTime);
		}
		
		public static void SetDestroyAfterTime(this global::Bang.Entities.Entity e, Bang.Unity.Components.RemoveStyle style, System.Single timeToDestroy)
		{
			e.AddOrReplaceComponent(new global::Bang.Unity.Components.DestroyAtTimeComponent(style, Game.Now + timeToDestroy), global::Bang.Entities.BangUnityComponentTypes.DestroyAtTime);
		}
		
	}

}
