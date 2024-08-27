using System.Collections.Immutable;
using Bang.Components;
using Bang.Interactions;


namespace Bang.Unity.Components {

	public readonly struct InteractOnCollisionComponent : IComponent {
	
		[Tooltip( "Whether this should be activated again." )]
		public readonly bool OnlyOnce;

		[Tooltip( "Whether this will send a message once the object stop colliding." )]
		public readonly bool SendMessageOnExit;

		[Tooltip( "Whether this will send a message every frame while colliding." )]
		public readonly bool SendMessageOnStay;


		[Tooltip( "Interactions that will be triggered in addition to interactions in this entity." )]
		public readonly ImmutableArray< IInteractiveComponent > CustomEnterMessages;

		[Tooltip( "Interactions that will be triggered in addition to interactions in this entity." )]
		public readonly ImmutableArray< IInteractiveComponent > CustomExitMessages;

		[Tooltip( "Whether only a player is able to activate this." )]
		public readonly bool PlayerOnly;

		// public InteractOnCollisionComponent() {}

		public InteractOnCollisionComponent( bool playerOnly ) {
			OnlyOnce = false;
			SendMessageOnExit = false;
			SendMessageOnStay = false;
			CustomEnterMessages = ImmutableArray< IInteractiveComponent >.Empty;
			CustomExitMessages = ImmutableArray< IInteractiveComponent >.Empty;
			PlayerOnly = playerOnly;
		}

		public InteractOnCollisionComponent( bool playerOnly, bool sendMessageOnExit ) {
			OnlyOnce = false;
			SendMessageOnExit = sendMessageOnExit;
			SendMessageOnStay = false;
			CustomEnterMessages = ImmutableArray< IInteractiveComponent >.Empty;
			CustomExitMessages = ImmutableArray< IInteractiveComponent >.Empty;
			PlayerOnly = playerOnly;
		}
	
	}

}
