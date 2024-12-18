using Bang.Components;
using UnityEngine;


namespace Bang.Unity.Messages {

	public readonly struct GameObjectDestroyedMessage : IMessage {

		public readonly GameObject GameObject;
		
		public GameObjectDestroyedMessage( GameObject gameObject ) {
			GameObject = gameObject;
		}

	}

}