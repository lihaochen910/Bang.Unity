using System;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Bang.Unity.Graphics {

	[Filter( ContextAccessorFilter.AnyOf, ContextAccessorKind.Read, typeof( CustomUnityOnGuiDrawComponent ))]
	public class UnityOnGuiRenderSystem : IStartupSystem, IExitSystem, IGuiSystem {

		private GameObject _drawAgent;

		public void Start( Context context ) {
			_drawAgent = new GameObject( nameof( UnityOnGuiRenderSystem ), typeof( BangGlobalMonoBehaviourOnGuiDrawAgent ) );
			_drawAgent.GetComponent< BangGlobalMonoBehaviourOnGuiDrawAgent >().SetWorld( context.World as UnityWorld );
			Object.DontDestroyOnLoad( _drawAgent );
		}
		
		public void Exit( Context context ) {
			Object.Destroy( _drawAgent );
		}

		public void DrawGui( Context context ) {
			foreach ( var entity in context.Entities ) {
				entity.GetCustomUnityOnGuiDraw().Draw.Invoke();
			}
		}
		
	}


	public class BangGlobalMonoBehaviourOnGuiDrawAgent : MonoBehaviour {

		private WeakReference< UnityWorld > _worldRef;

		public void SetWorld( UnityWorld world ) {
			_worldRef = new WeakReference< UnityWorld >( world );
		}

		private void OnGUI() {
			if ( _worldRef.TryGetTarget( out var world ) ) {
				world.DrawGui();
				// foreach ( var onGuiFunction in world.GlobalMonoBehaviourOnGuiComponent.Value.OnGuiCallbacks ) {
				// 	onGuiFunction.Invoke();
				// }
			}
		}

	}
}
