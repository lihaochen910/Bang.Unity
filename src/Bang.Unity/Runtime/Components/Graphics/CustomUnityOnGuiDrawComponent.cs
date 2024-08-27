using Bang.Components;


namespace Bang.Unity.Graphics {

	[RuntimeOnly]
	[DoNotPersistOnSave]
	public readonly struct CustomUnityOnGuiDrawComponent : IComponent {

		public readonly UnityMonoBehaviourOnGuiFunction Draw;
		
		public CustomUnityOnGuiDrawComponent( UnityMonoBehaviourOnGuiFunction draw ) {
			Draw = draw;
		}

	}

}
