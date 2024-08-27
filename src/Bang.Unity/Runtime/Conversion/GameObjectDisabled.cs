using System;
using Bang.Components;


namespace Bang.Unity.Conversion {

	public readonly struct GameObjectDisabledComponent : IComponent, IEquatable<GameObjectDisabledComponent>
	{
		public bool Equals(GameObjectDisabledComponent other) => true;
		public override bool Equals(object obj) => obj is GameObjectDisabledComponent;
		public override int GetHashCode() => 0;
		public override string ToString() => "()";
	}
	
}
