using System.Collections.Immutable;
using Bang.Components;


namespace Bang.Unity.Graphics {
	
	public delegate void UnityMonoBehaviourOnGuiFunction();

	// [Unique]
	// public readonly struct GlobalMonoBehaviourOnGuiComponent : IComponent {
	//
	// 	public readonly ImmutableArray< UnityMonoBehaviourOnGuiFunction > OnGuiCallbacks;
	//
	// 	public GlobalMonoBehaviourOnGuiComponent( ImmutableArray< UnityMonoBehaviourOnGuiFunction > onGuiCallbacks ) {
	// 		OnGuiCallbacks = onGuiCallbacks;
	// 	}
	//
	// 	public GlobalMonoBehaviourOnGuiComponent PushCallback( UnityMonoBehaviourOnGuiFunction callback ) {
	// 		return new GlobalMonoBehaviourOnGuiComponent( OnGuiCallbacks.Add( callback ) );
	// 	}
	//
	// }

}
