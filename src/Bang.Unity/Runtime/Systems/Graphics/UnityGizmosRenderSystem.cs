using System;
using Bang.Contexts;
using Bang.Systems;
using Drawing;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Bang.Unity.Graphics {
	
	// [Filter( ContextAccessorFilter.AnyOf, ContextAccessorKind.Read, typeof( CustomUnityOnGuiDrawComponent ))]
	[Filter]
	public class UnityGizmosRenderSystem : IStartupSystem, IExitSystem, IGizmosSystem {

		private GameObject _drawAgent;

		public void Start( Context context ) {
			_drawAgent = new GameObject( nameof( UnityGizmosRenderSystem ), typeof( BangGlobalMonoBehaviourOnGizmosDrawAgent ) );
			_drawAgent.GetComponent< BangGlobalMonoBehaviourOnGizmosDrawAgent >().SetWorld( context.World as UnityWorld );
			Object.DontDestroyOnLoad( _drawAgent );
		}
		
		public void Exit( Context context ) {
			Object.Destroy( _drawAgent );
		}

		public void DrawGizmos( Context context ) {
			// foreach ( var entity in context.Entities ) {
			// 	entity.GetCustomUnityOnGuiDraw().Draw.Invoke();
			// }
		}
		
	}


	public class BangGlobalMonoBehaviourOnGizmosDrawAgent : MonoBehaviour, IDrawGizmos {

		private WeakReference< UnityWorld > _worldRef;

		public void SetWorld( UnityWorld world ) {
			_worldRef = new WeakReference< UnityWorld >( world );
		}

		private void Start() {
#if UNITY_EDITOR
			DrawingManager.Register( this );
#endif
		}

		public virtual void DrawGizmos() {
			if ( _worldRef.TryGetTarget( out var world ) ) {
				world.DrawGizmos();
			}
		}

	}

}
