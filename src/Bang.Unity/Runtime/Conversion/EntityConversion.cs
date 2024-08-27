using System;
using System.Collections.Generic;
using Bang.Entities;
using UnityEngine;
using Entity = Bang.Entities.Entity;


namespace Bang.Unity.Conversion {

    public static class EntityConversion {
        
        // sealed class Converter : IEntityConverter
        // {
        //     readonly List<ComponentType> componentTypes = new();
        //     readonly List<object> components = new();
        //
        //     public void AddComponent<T>(T component)
        //     {
        //         if (typeof(T).IsValueType)
        //         {
        //             componentTypes.Add(typeof(T));
        //         }
        //         else
        //         {
        //             componentTypes.Add(component.GetType());
        //         }
        //
        //         components.Add(component); // TODO: avoid boxing
        //     }
        //
        //     public Entity Convert(World world)
        //     {
        //         // world.AddEntity()
        //         var entity = world.Create(componentTypes.ToArray());
        //         foreach (var component in components)
        //         {
        //             world.Set(entity, component);
        //         }
        //         return entity;
        //     }
        // }
	    
	    public static Entity Convert(GameObject gameObject, EntityConversionOptions options)
	    {
		    return Convert(gameObject, Game.ActiveScene?.World, options);
	    }

	    public static Entity Convert(GameObject gameObject, World world)
	    {
		    return Convert(gameObject, world, EntityConversionOptions.Default);
	    }
	    
	    public static Entity Convert(GameObject gameObject, World world, EntityConversionOptions options)
        {
            var components = gameObject.GetComponents<UnityEngine.Component>();
            world ??= Game.ActiveScene?.World;
            var entity = world.AddEntity();
            
            // var converter = new Converter();

            if (options.ConversionMode == ConversionMode.SyncWithEntity)
            {
                // converter.AddComponent(new GameObjectReference(gameObject));
                // converter.AddComponent(new EntityName(gameObject.name));
                entity.SetGameObjectReference( gameObject );
                entity.SetEntityName( gameObject.name );
            }

            // for (int i = 0; i < components.Length; i++)
            // {
            //     var component = components[i];
            //
            //     if (component is BangEntity)
            //     {
            //         throw new InvalidOperationException("A GameObject that has already been synchronized with an entity cannot be converted again.");
            //     }
            //
            //     // if (component is IComponentConverter componentConverter)
            //     // {
            //     //     try
            //     //     {
            //     //         componentConverter.Convert(converter);
            //     //     }
            //     //     catch (Exception ex)
            //     //     {
            //     //         Debug.LogException(ex);
            //     //     }
            //     // }
            //     // else if (options.ConversionMode == ConversionMode.SyncWithEntity && options.ConvertHybridComponents)
            //     // {
            //     //     converter.AddComponent((object)component);
            //     // }
            // }
            
            // var entityReference = world.Reference(converter.Convert(world));

            if (options.ConversionMode == ConversionMode.ConvertAndDestroy)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                var syncWithEntity = gameObject.GetComponent<BangEntity>();
                syncWithEntity.World = world;
                syncWithEntity.UseDisabledComponent = options.UseDisabledComponent;
            }

            // OnConvert?.Invoke(entityReference, world);

            return entity;
        }

        public static bool TryGetEntity(GameObject gameObject, out Entity entity)
        {
            if (gameObject.TryGetComponent<BangEntity>(out var bangEntity))
            {
                entity = bangEntity.Entity;
                return true;
            }

            entity = default;
            return false;
        }

        public static bool TryGetGameObject(Entity entity, out GameObject gameObject)
        {
            return TryGetGameObject(entity, Game.ActiveScene?.World, out gameObject);
        }

        public static bool TryGetGameObject(Entity entity, World world, out GameObject gameObject)
        {
            if (entity.TryGetGameObjectReference() is {} gameObjectReference)
            {
                gameObject = gameObjectReference.GameObject;
                return gameObject != null;
            }

            gameObject = default;
            return false;
        }
	    
    }
    
}
