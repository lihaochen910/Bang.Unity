using Bang.Components;
using Bang.Entities;
using Bang.Interactions;


namespace Bang.Unity.Interactions {

	public readonly struct SendMessageInteraction : IInteraction {
	
		public readonly IMessage Message;

		public SendMessageInteraction( IMessage message ) => Message = message;

		public void Interact( World world, Entity interactor, Entity? interacted ) {
			interacted?.SendMessage( Message );
		}
	}
	
}
