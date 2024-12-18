using Bang.Components;


namespace Bang.Unity.Editor.Components {

	/// <summary>
	/// Component that a component has been selected in the editor.
	/// </summary>
	[DoNotPersistOnSave]
	public readonly struct IsSelectedComponent : IComponent {}

}
