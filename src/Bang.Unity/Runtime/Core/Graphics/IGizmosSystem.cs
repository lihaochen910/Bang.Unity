using Bang.Contexts;
using Bang.Systems;


namespace Bang.Unity.Graphics {
	
	public interface IGizmosSystem : IRenderSystem {

		/// <summary>
		/// Called before rendering starts.
		/// </summary>
		public abstract void DrawGizmos( Context context );

	}

}
