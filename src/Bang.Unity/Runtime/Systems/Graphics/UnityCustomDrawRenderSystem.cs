using System;
using Bang.Contexts;
using Bang.Systems;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Bang.Unity.Graphics {
	
	[Filter]
	public class UnityCustomDrawRenderSystem : IStartupSystem, IExitSystem {

		private GameObject _drawAgent;

		public void Start( Context context ) {
			_drawAgent = new GameObject( nameof( UnityCustomDrawRenderSystem ), typeof( BangGlobalMonoBehaviourCustomDrawAgent ) );
			_drawAgent.GetComponent< BangGlobalMonoBehaviourCustomDrawAgent >().SetWorld( context.World as UnityWorld );
			Object.DontDestroyOnLoad( _drawAgent );
		}
		
		public void Exit( Context context ) {
			Object.Destroy( _drawAgent );
		}
		
	}


	public class BangGlobalMonoBehaviourCustomDrawAgent : MonoBehaviour {

		private WeakReference< UnityWorld > _worldRef;

		public void SetWorld( UnityWorld world ) {
			_worldRef = new WeakReference< UnityWorld >( world );
		}

		private void Update() {
			if ( _worldRef.TryGetTarget( out var world ) ) {
				world.Draw();
			}
		}

	}

}
