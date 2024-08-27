// using System.Collections.Immutable;
// using Bang.Components;
// using Bang.Entities;
// using Bang.Interactions;
// using UnityEngine;
//
//
// namespace Bang.Unity.Interactions {
//
// 	/// <summary>
// 	/// This will trigger an effect by placing <see cref="_prefab"/> in the world.
// 	/// </summary>
// 	public readonly struct AddEntityOnInteraction : IInteraction
// 	{
// 		public readonly GameObject Prefab;
// 		
// 		public readonly ImmutableArray<IComponent> _customComponents = ImmutableArray<IComponent>.Empty;
//
// 		// public AddEntityOnInteraction() {}
//
// 		public AddEntityOnInteraction(GameObject prefab) => Prefab = prefab;
//
// 		public void Interact(World world, Entity interactor, Entity? interacted)
// 		{
// 			Entity result = AssetServices.Create(world, Prefab);
//
// 			foreach (IComponent c in _customComponents)
// 			{
// 				// We need to guarantee that any modifiable components added here are safe.
// 				IComponent component = c is IModifiableComponent ? SerializationHelper.DeepCopyWithRuntimeGodotObject(c) : c;
// 				result.AddOrReplaceComponent(component, component.GetType());
// 			}
//
// 			// Adjust the position, if applicable.
// 			// if (interacted?.TryGetTransform() is IMurderTransformComponent transform)
// 			// {
// 			// 	result.SetTransform(transform);
// 			// }
//
// 			// Remove after triggered.
// 			interacted?.RemoveInteractive();
// 		}
// 	}
//
// }
