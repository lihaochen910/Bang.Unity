using Bang.Components;
using Bang.Unity.Utilities;


namespace Bang.Unity.Messages.Physics {

	/// <summary>
	/// Message sent to the ACTOR when touching a trigger area.
	/// </summary>
	public readonly struct OnCollisionMessage : IMessage {
	
		public readonly int EntityId;
		public readonly CollisionDirection Movement;

		/// <summary>
		/// Message sent to the ACTOR when touching a trigger area.
		/// </summary>
		public OnCollisionMessage( int triggerId, CollisionDirection movement ) {
			EntityId = triggerId;
			Movement = movement;
		}
	}

}
