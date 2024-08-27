using System.Linq;
using UnityEditor;


namespace Bang.Unity.Editor {

	[CustomEditor( typeof( EntitySelectionProxy ) ), CanEditMultipleObjects]
	public class EntitySelectionProxyEditor : UnityEditor.Editor {

		private EntitySelectionProxy owner => ( EntitySelectionProxy ) target;

		public override void OnInspectorGUI() {
			if ( targets.Length == 1 ) {
				EntityDrawer.DrawEntity( owner.entity );
			}
			else {
				var entities = targets
							   .Select( t => ( ( BangEntity )t ).Entity )
							   .ToArray();

				EntityDrawer.DrawMultipleEntities( entities );
			}

			if ( target != null ) {
				EditorUtility.SetDirty( target );
			}
		}
		
	}
	
}
