using Bang.Components;
using Bang.Entities;


namespace Bang.Unity.Messages {

	/// <summary>
	/// Generic struct for interacting with an entity.
	/// </summary>
	[RuntimeOnly]
	public readonly struct InteractMessage : IMessage {
	
		public readonly Entity? Interactor;
		
		public InteractMessage( Entity interactor ) {
			Interactor = interactor;
		}
	}

}
