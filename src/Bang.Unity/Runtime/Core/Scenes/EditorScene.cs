using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Bang.Components;
using Bang.Systems;
using Bang.Unity.Utilities;
using UnityEngine;


namespace Bang.Unity {

	public class EditorScene : Scene {

		private UnityWorld? _world;
		public override UnityWorld World => _world;


		#region Override

		public override void LoadContentImpl() {
			_world = CreateWorld();
			// GC.Collect( generation: 0, mode: GCCollectionMode.Forced, blocking: true );
		}
		
		public override void ReloadImpl() {
			_world?.Dispose();
			_world = null;
		}

		public override void ResumeImpl() {
			_world?.ActivateAllSystems();
		}

		public override void SuspendImpl() {
			_world?.DeactivateAllSystems();
		}
		
		public override async Task UnloadAsyncImpl() {
			ValueTask< bool > result = new ValueTask< bool >( true );
			await result;

			_world?.Dispose();
			_world = null;
		}
		
		#endregion
		
		
		private static List<(Type tSystem, bool isActive)>? _cachedEditorSystems = null;

	    public static List<(Type tSystem, bool isActive)> FetchEditorTypeSystems()
	    {
	        if (_cachedEditorSystems is not null)
	        {
	            return _cachedEditorSystems;
	        }
	        
	        HashSet<Type> systemsAdded = new();
	        List<(Type, bool)> systems = new();

	        void AddSystem(Type t, bool isActive)
	        {
	            if (systemsAdded.Contains(t))
	            {
	                // Already added, so skip.
	                return;
	            }

	            if (Attribute.GetCustomAttribute(t, typeof(RequiresAttribute)) is RequiresAttribute requiresAttribute)
	            {
	                foreach (Type tt in requiresAttribute.Types)
	                {
	                    AddSystem(tt, isActive);
	                }
	            }

	            systems.Add((t, isActive));
	            systemsAdded.Add(t);
	        }

	        // Fetch all the systems that are not included in the editor system.
	        foreach (Type t in ReflectionHelper.GetAllTypesWithAttributeDefinedOfType<WorldEditorAttribute>(typeof(ISystem)))
	        {
	            WorldEditorAttribute worldAttribute = (WorldEditorAttribute)t.GetCustomAttribute(typeof(WorldEditorAttribute))!;
	            bool isActive = worldAttribute.StartActive;

	            AddSystem(t, isActive);
	        }

	        foreach (Type t in ReflectionHelper.GetAllTypesWithAttributeDefinedOfType<DefaultEditorSystemAttribute>(typeof(ISystem)))
	        {
	            DefaultEditorSystemAttribute attribute = (DefaultEditorSystemAttribute)t.GetCustomAttribute(typeof(DefaultEditorSystemAttribute))!;
	            bool isActive = attribute.StartActive;

	            AddSystem(t, isActive);
	        }

	        // Start with all the editor systems enabled by default.
	        foreach (Type t in ReflectionHelper.GetAllTypesWithAttributeDefinedOfType<EditorSystemAttribute>(typeof(ISystem)))
	        {
	            AddSystem(t, isActive: true);
	        }

	        // Type[] targetAttributesForDisabledByDefault = [
	        //     typeof(StoryEditorAttribute),
	        //     typeof(DialogueEditorAttribute),
	        //     typeof(TileEditorAttribute),
	        //     typeof(PathfindEditorAttribute),
	        //     typeof(SoundEditorAttribute),
	        //     typeof(SpriteEditorAttribute),
	        //     typeof(PrefabEditorAttribute),
	        //     typeof(EditorSystemAttribute)];

			Type[] targetAttributesForDisabledByDefault = new [] { typeof( EditorSystemAttribute ) };

	        foreach (Type tAttribute in targetAttributesForDisabledByDefault)
	        {
	            foreach (Type t in ReflectionHelper.GetAllTypesWithAttributeDefinedOfType(tAttribute, typeof(ISystem)))
	            {
	                AddSystem(t, isActive: false);
	            }
	        }

	        _cachedEditorSystems = systems;
	        return systems;
	    }

		
		private UnityWorld CreateWorld() {

			List<(ISystem, bool)> systemInstances = new();

			// Actually instantiate and add each of our system types.
			foreach (var (type, isActive) in FetchEditorTypeSystems())
			{
				if (type is null)
				{
					// Likely a debug system, skip!
					Debug.LogWarning( "found null type at GameScene::CreateWorld()!" );
					continue;
				}

				if (Activator.CreateInstance(type) is ISystem system)
				{
					systemInstances.Add((system, isActive));
				}
				else
				{
					Debug.LogError($"The {type} is not a valid system!");
				}
			}
			
			// TODO: create from save??
			return new UnityWorld( systemInstances );
		}
		
	}
	
}
