using System;
using System.Reflection;
using Bang.Entities;
using Bang.StateMachines;
using UnityEngine;


namespace Bang.Unity.Utilities {

	public static class BangStateMachineExtensions {
		
		// public static T GetRoutine< T >( this Bang.StateMachines.StateMachine stateMachine ) where T : StateMachine {
		// 	var fieldInfo = typeof( Bang.StateMachines.StateMachine ).GetField( "_routine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
		// 	return fieldInfo.GetValue( stateMachine ) as T;
		// }

		public static void SetStateMachine< T >( this Entity e ) where T : StateMachine, new() {
			var instanceStateMachineType = typeof( StateMachineComponent<> ).MakeGenericType( typeof( T ) );
			e.SetStateMachine( Activator.CreateInstance( instanceStateMachineType ) as IStateMachineComponent );
		}

		/// <summary>
		/// Adds or replaces the component of type <see cref="Type" />.
		/// </summary>
		public static void SetStateMachine( this Entity e, Type stateMachineType ) {
			var instanceStateMachineType = typeof( StateMachineComponent<> ).MakeGenericType( stateMachineType );
			e.SetStateMachine( Activator.CreateInstance( instanceStateMachineType ) as IStateMachineComponent );
		}
		
		
		public static void SetStateMachine( this Entity e, StateMachine routine ) {
			var instanceStateMachineType = typeof( StateMachineComponent<> ).MakeGenericType( routine.GetType() );
			// routine.GetType().GetMethod( "Initialize", BindingFlags.Instance | BindingFlags.NonPublic )
			// 	   .Invoke( routine, new object[] { e.World, e } );
			
			e.AddOrReplaceComponent(
				Activator.CreateInstance( instanceStateMachineType, routine ) as IStateMachineComponent,
				BangComponentTypes.StateMachine );
			
			// e.SetStateMachine( Activator.CreateInstance( instanceStateMachineType ) as IStateMachineComponent );
		}
		
		
		public static StateMachine GetRoutine( this IStateMachineComponent stateMachineComponent, Type routineType ) {
			var fieldInfo = typeof( StateMachineComponent<> ).MakeGenericType( routineType ).GetField( "_routine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
			return fieldInfo?.GetValue( stateMachineComponent ) as StateMachine;
		}
		
		
		public static T GetRoutine< T >( this IStateMachineComponent stateMachineComponent ) where T : StateMachine, new() {
			if ( stateMachineComponent is StateMachineComponent< T > ) {
				var fieldInfo = typeof( StateMachineComponent< T > ).GetField( "_routine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
				return fieldInfo?.GetValue( stateMachineComponent ) as T;
			}

			return null;
		}
		

		public static T GetRoutine< T >( this StateMachineComponent< T > stateMachineComponent ) where T : StateMachine, new() {
			var fieldInfo = typeof( StateMachineComponent< T > ).GetField( "_routine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
			return fieldInfo?.GetValue( stateMachineComponent ) as T;
		}
		
	}
	
}
