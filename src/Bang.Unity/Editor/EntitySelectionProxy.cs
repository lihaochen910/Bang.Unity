using Bang.Entities;
using UnityEngine;


namespace Bang.Unity.Editor {

	public sealed class EntitySelectionProxy : ScriptableObject {
		public Bang.World world;
		public Entity entity;
	}

}
