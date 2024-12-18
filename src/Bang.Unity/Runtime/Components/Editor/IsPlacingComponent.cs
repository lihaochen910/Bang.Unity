using Bang.Components;


namespace Bang.Unity.Editor.Components {

	/// <summary>
	/// Component for an entity that is currently being placed in the map.
	/// </summary>
	[RuntimeOnly]
	[DoNotPersistOnSave]
	public readonly struct IsPlacingComponent : IComponent {}

}
